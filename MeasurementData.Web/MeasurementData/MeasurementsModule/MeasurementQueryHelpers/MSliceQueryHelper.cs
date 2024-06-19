using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Reflection;
using APRF.Web.Common.DataSource;
using DataModel;
using LinqToDB;
using Minio.DataModel;
using StackExchange.Redis;

namespace MeasurementData.MeasurementModule;

/// <summary>
/// Провайдер значений по умолчанию в связку measurement_value и measurement_value_slice
/// </summary>
internal sealed class MSliceQueryHelper : IMeasurementQueryHelper
{
    private static readonly IDictionary<FilterOperator, string> _operators = new Dictionary<
        FilterOperator,
        string
    >
    {
        { FilterOperator.Equals, "=" },
        { FilterOperator.NotEquals, "!=" },
        { FilterOperator.LessThan, "<" },
        { FilterOperator.LessThanOrEqual, "<=" },
        { FilterOperator.GreaterThan, ">" },
        { FilterOperator.GreaterThanOrEqual, ">=" },
        { FilterOperator.StartsWith, "StartsWith" },
        { FilterOperator.EndsWith, "EndsWith" },
        { FilterOperator.Contains, "Contains" },
        { FilterOperator.DoesNotContain, "Contains" }
    };

    public MSliceQueryHelper(long measurementId, MeasurementQueryOptions options)
    {
        _measurementId = measurementId;
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    private readonly long _measurementId;

    private readonly MeasurementQueryOptions _options;

    public (bool canApply, string? error) CanApplyRequest(PeriodRequest periodRequest)
    {
        if (!_options.SupportCalendarLevels.Contains(periodRequest.CalendarLevel))
        {
            return (false, $"Не поддерживаемый уровень календаря {periodRequest.CalendarLevel}");
        }
        if (!_options.SupportComplexPeriodAggregation && periodRequest.IsComplexPeriod)
        {
            return (false, $"Не поддерживается комплексный период");
        }
        return (true, null);
    }

    public Func<AppDataConnection, IQueryable<MSeriesDbValue>> GetQueryable(
        MeasurementDataRequest dataRequest,
        PeriodRequest periodRequest
    )
    {
        (string query, object?[] values) = this.BuildSlicePredicateDynamicLinq(
            dataRequest,
            periodRequest
        );
        return db =>
            db.MeasurementValues
                .Where(query, values)
                .Select(
                    x =>
                        new MSeriesDbValue
                        {
                            MeasurementId = this._measurementId,
                            CalendarLevelId = x.CalendarLevelId,
                            InDate = x.InDate,
                            OutDate = x.OutDate,
                            UpdateDate = x.UpdateDate,
                            Value = x.Value,
                            ValueTypeId = x.ValueTypeId,
                            SegmentId = x.MeasurementValueSlice.SegmentId,
                            AgencyId = x.MeasurementValueSlice.AgencyId,
                            ClientTypeId = x.MeasurementValueSlice.ClientTypeId,
                            ServiceCardId = x.MeasurementValueSlice.ServiceCardId,
                            AreaLifeId = x.MeasurementValueSlice.AreaLifeId,
                            CardLifeSituationId = x.MeasurementValueSlice.CardLifeSituationId,
                            OkvedId = x.MeasurementValueSlice.OkvedId,
                            ToolServiceId = x.MeasurementValueSlice.ToolServiceId,
                            SubjectRfId = x.MeasurementValueSlice.SubjectRfId,
                            SupportMeasuresId = x.MeasurementValueSlice.SupportMeasuresId,
                            ClientWayMoveId = x.MeasurementValueSlice.ClientWayMoveId,
                            TaxonomyId = x.MeasurementValueSlice.TaxonomyId,
                            IndicatorId = null,
                            ApplicationStatusId = null,
                            PersonAgeId = null,
                            PersonGenderId = null,
                            UserKindId = null,
                        }
                );
    }

    private (string query, object?[] vals) BuildSlicePredicateDynamicLinq(
        MeasurementDataRequest filter,
        PeriodRequest period
    )
    {
        var i = 0;
        var validFilters = filter.Filters.Where(filter => IsExists(filter.Slice)).ToList();
        List<string> subQueries = validFilters
            .Select(x => ToDynamicLinqExpression(x, i++))
            .ToList();
        var values = validFilters.Select(x => GetExpressionValue(x)).ToList();
        subQueries.AddRange(
            filter.AggregateBy
                .Where(slice => IsExists(slice))
                .Select(x => ToAggregateDynamicLinqExpression(x))
        );

        subQueries.Add($"CalendarLevelId == @{i++}");
        values.Add((long)period.CalendarLevelId);
        subQueries.Add($"MeasurementValueSlice.MeasurementId == @{i++}");
        values.Add(this._measurementId);
        if (period.InDate != null)
        {
            subQueries.Add($"InDate >= @{i++}");
            values.Add(period.InDate.Value);
        }
        if (period.OutDate != null)
        {
            subQueries.Add($"OutDate <= @{i++}");
            values.Add(period.OutDate.Value);
        }
        return (
            subQueries.Count > 1
                ? subQueries.Aggregate((x, y) => $"{x} && {y}")
                : subQueries.First(),
            values.ToArray()
        );
    }

    private static object? GetExpressionValue(SliceFilter filter)
    {
        var prop = typeof(MeasurementValueSlice).GetProperty(
            $"{filter.Slice}Id",
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
        );
        if (prop is null)
        {
            throw new InvalidOperationException($"Property {$"{filter.Slice}Id"} not found");
        }

        var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
        var isPropNullable = underlyingType is not null;
        if (isPropNullable && underlyingType == typeof(long) && filter.Value is long[] longValues)
        {
            // значения values: [1, 2] приходят, как long[], но если фильтруем по nullable полю, то возникает ошибка,
            // надо привести к long?[]
            var nullableValues = longValues.Cast<long?>().ToArray();
            return nullableValues;
        }

        return filter.Value;
    }

    private static bool IsExists(Slice slice)
    {
        return typeof(MeasurementValueSlice).GetProperty($"{slice}Id") != null;
    }

    private static string ToAggregateDynamicLinqExpression(Slice aggregateBy)
    {
        return aggregateBy switch
        {
            Slice.SubjectRf
                => $"{GetField(aggregateBy)} == {SubjectRfDictionary.RUSSIAN_FEDERATION}",
            Slice.Okved
                => $"( {GetField(aggregateBy)} == {OkvedDictionary.ALL} || {GetField(aggregateBy)} == null )",
            _ => $"{GetField(aggregateBy)} == null"
        };
    }

    private static string ToDynamicLinqExpression(SliceFilter filter, int index)
    {
        var comparison = _operators[filter.Operator];
        if (
            filter.Operator == FilterOperator.Equals
            && filter.Value != null
            && filter.Value.GetType().IsArray
        )
        {
            return $"@{index}.Contains({GetField(filter.Slice)})";
        }

        if (filter.Operator == FilterOperator.DoesNotContain)
        {
            return $"!{GetField(filter.Slice)}.{comparison}(@{index})";
        }

        return $"{GetField(filter.Slice)} {comparison} @{index}";
    }

    private static string GetField(Slice slice)
    {
        return $"MeasurementValueSlice.{slice}Id";
    }
}
