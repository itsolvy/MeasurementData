using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Expressions;
using System;
using System.Data.SqlTypes;
using Sql2 = MeasurementData.MeasurementModule.Linq2DbExtensions;
using LinqToDB.SqlQuery;
using LinqToDB.Common;
using MeasurementData.Web.Common;

namespace MeasurementData.MeasurementModule;

public static class TableQueryBuildHelper
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

    public static (string predicate, object?[] values) GetConditions(
        MeasurementDataRequest dataRequest,
        PeriodRequest periodRequest,
        Type type,
        TableDynamicOptions options
    )
    {
        var predicate = string.Empty;
        List<object?> values = new();
        var flatFilters = new List<string>();
        var i = 0;
        // обвесили фильтрами
        if (dataRequest.Filters != null && dataRequest.Filters.Any())
        {
            var validFilterAndValues = dataRequest.Filters
                .Where(f => IsValid(f.Slice, type))
                .Select(f => GetFilterAndValue(f, type, i++))
                .ToList();
            // собираем все фильтры
            flatFilters = validFilterAndValues.Select(x => x.filter).ToList();

            // собираем значения фильтров
            values = validFilterAndValues.Select(x => x.value).ToList();
        }
        if (periodRequest.InDate != null)
        {
            flatFilters.Add($"InDate >= @{i++}");
            values.Add(periodRequest.InDate.Value);
        }
        if (periodRequest.OutDate != null)
        {
            flatFilters.Add($"OutDate <= @{i++}");
            values.Add(periodRequest.OutDate.Value);
        }
        //Ищем максимальный из возможных
        long fromCalendarLevel = (long)
            options.DataLevels
                .Where(dl => (long)dl <= (long)periodRequest.CalendarLevelId)
                .Max(x => x);
        flatFilters.Add($"CalendarLevelId == {fromCalendarLevel}");

        if (flatFilters.Count == 1)
        {
            predicate = flatFilters.Single();
        }
        else if (flatFilters.Count > 1)
        {
            predicate = flatFilters.Aggregate((x, y) => $"{x} && {y}");
        }

        return (predicate, values.ToArray());
    }

    private static bool IsValid(Slice slice, Type type)
    {
        string fieldName = $"{slice}Id";
        return type.GetProperty(fieldName) != null;
    }

    private static (string filter, object? value) GetFilterAndValue(
        SliceFilter filter,
        Type type,
        int index
    )
    {
        string fieldName = $"{filter.Slice}Id";
        var comparison = _operators[filter.Operator];
        if (
            filter.Operator == FilterOperator.Equals
            && filter.Value != null
            && filter.Value.GetType().IsArray
        )
        {
            return (
                $"@{index}.Contains({fieldName})",
                ConvertArray(
                    filter.Value,
                    type.GetProperty(fieldName)?.PropertyType
                        ?? throw new InvalidOperationException($"Свойство {fieldName} не найденно")
                )
            );
        }

        if (filter.Operator == FilterOperator.DoesNotContain)
        {
            return ($"!{fieldName}.{comparison}(@{index})", filter.Value);
        }

        return ($"{fieldName} {comparison} @{index}", filter.Value);
    }

    private static object? ConvertArray(object array, Type propertyType)
    {
        var arrayType = array.GetType();
        if (!arrayType.IsArray)
        {
            throw new ArgumentException(nameof(array));
        }
        if (propertyType == typeof(long) && arrayType.GetElementType() == typeof(long?))
        {
            return (array as long?[])!.Select(x => x!.Value).ToArray();
        }
        else if (propertyType == typeof(long?) && arrayType.GetElementType() == typeof(long))
        {
            return (array as long[])!.Select(x => (long?)x).ToArray();
        }
        return array;
    }

    public static IQueryable<MSeriesDbValue> SelectByFormula<T>(
        this IQueryable<IGrouping<MSeriesDbSlice, T>> query,
        Expression<Func<IGrouping<MSeriesDbSlice, T>, decimal>> valueFunc,
        long meausurementId
    )
    {
        var utcNow = DateTime.UtcNow;

        Expression<Func<IGrouping<MSeriesDbSlice, T>, decimal, MSeriesDbValue>> exp = (
            IGrouping<MSeriesDbSlice, T> group,
            decimal funcPlaceholder
        ) =>
            new MSeriesDbValue
            {
                SubjectRfId = group.Key.SubjectRfId,
                ServiceCardId = group.Key.ServiceCardId,
                CardLifeSituationId = group.Key.CardLifeSituationId,
                ToolServiceId = group.Key.ToolServiceId,
                ClientWayMoveId = group.Key.ClientWayMoveId,
                AgencyId = group.Key.AgencyId,
                SegmentId = group.Key.SegmentId,
                SupportMeasuresId = group.Key.SupportMeasuresId,
                AreaLifeId = group.Key.AreaLifeId,
                ClientTypeId = group.Key.ClientTypeId,
                OkvedId = group.Key.OkvedId,
                PersonGenderId = group.Key.PersonGenderId,
                IndicatorId = group.Key.IndicatorId,
                UserKindId = group.Key.UserKindId,
                ApplicationStatusId = group.Key.ApplicationStatusId,
                PersonAgeId = group.Key.PersonAgeId,
                CalendarLevelId = group.Key.CalendarLevelId,
                InDate = group.Key.InDate,
                OutDate = group.Key.OutDate,
                UpdateDate = utcNow,
                Value = funcPlaceholder,
                ValueTypeId = (long)MeasurementValueType.Fact,
                MeasurementId = meausurementId
            };

        //var ddd= Expression.Invoke(exp, new[] { exp.Parameters.First(), Expression.Invoke(valueFunc,null) } )

        var selectExpression = exp.ReplaceParameterByExpession(valueFunc);
        return query.Select(selectExpression);
    }

    public static IQueryable<IGrouping<MSeriesDbSlice, T>> GroupByFilter<T>(
        this IQueryable<T> query,
        MeasurementDataRequest filter,
        PeriodRequest periodRequest
    )
    {
        var calendarLevel = periodRequest.CalendarLevelId;

        //return query.GroupBy(
        Expression<Func<T, DateTime, DateTime, MSeriesDbSlice>> linqExpr = (dt, indate, outdate) =>
            new MSeriesDbSlice
            {
                SubjectRfId = Sql2.FromSliceFieldLong(
                    dt,
                    Slice.SubjectRf,
                    filter.AggregateBy,
                    SubjectRfDictionary.RUSSIAN_FEDERATION
                ),
                ServiceCardId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.ServiceCard,
                    filter.AggregateBy,
                    (long?)null
                ),
                CardLifeSituationId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.CardLifeSituation,
                    filter.AggregateBy,
                    (long?)null
                ),
                ToolServiceId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.ToolService,
                    filter.AggregateBy,
                    (long?)null
                ),
                ClientWayMoveId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.ClientWayMove,
                    filter.AggregateBy,
                    (long?)null
                ),
                AgencyId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.Agency,
                    filter.AggregateBy,
                    (long?)null
                ),
                SegmentId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.Segment,
                    filter.AggregateBy,
                    (long?)null
                ),
                SupportMeasuresId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.SupportMeasures,
                    filter.AggregateBy,
                    (long?)null
                ),
                AreaLifeId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.AreaLife,
                    filter.AggregateBy,
                    (long?)null
                ),
                ClientTypeId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.ClientType,
                    filter.AggregateBy,
                    (long?)null
                ),
                OkvedId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.Okved,
                    filter.AggregateBy,
                    (long?)null
                ),
                PersonGenderId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.PersonGender,
                    filter.AggregateBy,
                    (long?)null
                ),
                IndicatorId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.Indicator,
                    filter.AggregateBy,
                    (long?)null
                ),
                UserKindId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.UserKindId,
                    filter.AggregateBy,
                    (long?)null
                ),
                ApplicationStatusId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.ApplicationStatus,
                    filter.AggregateBy,
                    (long?)null
                ),
                PersonAgeId = Sql2.FromSliceFieldNullableLong(
                    dt,
                    Slice.PersonAge,
                    filter.AggregateBy,
                    (long?)null
                ),
                InDate = indate, // Sql.ToSql(Sql.Expr<DateTime>(inDateSql)), //InDateGroupByExpr(dt, periodRequest),
                OutDate = outdate, //Sql.ToSql(Sql.Expr<DateTime>(outDateSql)), //DateTime.UtcNow,//OutDateGroupByExpr(dt, periodRequest),
                CalendarLevelId = calendarLevel,
            };
        //строим выражение
        var exp = linqExpr.ReplaceParameterByExpession<T, DateTime, DateTime, MSeriesDbSlice>(
            GetInDateExpression<T>(periodRequest), //Sql2.Column<DateTime>(e, "in_date"),
            GetOutDateExpression<T>(periodRequest) //e => (DateTime)Sql.DateAdd(Sql.DateParts.Month, 1, Sql2.Column<DateTime>(e, "out_date"))!
        );

        return query.GroupBy(exp);
    }

    private static Expression<Func<T, DateTime>> GetInDateExpression<T>(PeriodRequest periodRequest)
    {
        return periodRequest switch
        {
            { IsComplexPeriod: true }
                =>
                //надо обмануть оптимизатор Linq2Db. Иначе он оптимизирует запррос не туда. Версия Linq2Db 5.4.0
                dt =>
                    (DateTime)
                        Sql2.DateTrunc(
                            Sql.DateParts.Day,
                            Sql2.Const(dt, periodRequest.InDate!.Value)
                        )!,
            { CalendarLevelId: (long)CalendarLevelType.Week }
                => dt =>
                    (DateTime)
                        Sql2.DateTrunc(Sql.DateParts.Week, Sql2.Column<DateTime>(dt, "in_date"))!,
            { CalendarLevelId: (long)CalendarLevelType.Day }
                => dt =>
                    (DateTime)
                        Sql2.DateTrunc(Sql.DateParts.Day, Sql2.Column<DateTime>(dt, "in_date"))!,
            { CalendarLevelId: (long)CalendarLevelType.Month }
                => dt =>
                    (DateTime)
                        Sql2.DateTrunc(Sql.DateParts.Month, Sql2.Column<DateTime>(dt, "in_date"))!,
            { CalendarLevelId: (long)CalendarLevelType.Quartal }
                => dt =>
                    (DateTime)
                        Sql2.DateTrunc(
                            Sql.DateParts.Quarter,
                            Sql2.Column<DateTime>(dt, "in_date")
                        )!,
            { CalendarLevelId: (long)CalendarLevelType.Year }
                => dt =>
                    (DateTime)
                        Sql2.DateTrunc(Sql.DateParts.Year, Sql2.Column<DateTime>(dt, "in_date"))!,
            _ => throw new NotImplementedException()
        };
    }

    private static Expression<Func<T, DateTime>> GetOutDateExpression<T>(
        PeriodRequest periodRequest
    )
    {
        return periodRequest switch
        {
            { IsComplexPeriod: true }
                =>
                //надо обмануть оптимизатор Linq2Db. Иначе он оптимизирует запррос не туда. Версия Linq2Db 5.4.0
                dt =>
                    (DateTime)
                        Sql2.DateTrunc(
                            Sql.DateParts.Day,
                            Sql2.Const(dt, periodRequest.OutDate!.Value)
                        )!,
            { CalendarLevelId: (long)CalendarLevelType.Day }
                => dt =>
                    (DateTime)
                        Sql2.DateTrunc(Sql.DateParts.Day, Sql2.Column<DateTime>(dt, "out_date"))!,
            { CalendarLevelId: (long)CalendarLevelType.Week }
                => dt =>
                    (DateTime)
                        Sql.DateAdd(
                            Sql.DateParts.Day,
                            -1,
                            Sql.DateAdd(
                                Sql.DateParts.Week,
                                1,
                                Sql2.DateTrunc(
                                    Sql.DateParts.Week,
                                    Sql2.Column<DateTime>(dt, "out_date")
                                )
                            )
                        )!,
            { CalendarLevelId: (long)CalendarLevelType.Month }
                => dt =>
                    (DateTime)
                        Sql.DateAdd(
                            Sql.DateParts.Day,
                            -1,
                            Sql.DateAdd(
                                Sql.DateParts.Month,
                                1,
                                Sql2.DateTrunc(
                                    Sql.DateParts.Month,
                                    Sql2.Column<DateTime>(dt, "out_date")
                                )
                            )
                        )!,
            { CalendarLevelId: (long)CalendarLevelType.Quartal }
                => dt =>
                    (DateTime)
                        Sql.DateAdd(
                            Sql.DateParts.Day,
                            -1,
                            Sql.DateAdd(
                                Sql.DateParts.Month,
                                3,
                                Sql2.DateTrunc(
                                    Sql.DateParts.Quarter,
                                    Sql2.Column<DateTime>(dt, "out_date")
                                )
                            )
                        )!,
            { CalendarLevelId: (long)CalendarLevelType.Year }
                => dt =>
                    (DateTime)
                        Sql.DateAdd(
                            Sql.DateParts.Day,
                            -1,
                            Sql.DateAdd(
                                Sql.DateParts.Year,
                                1,
                                Sql2.DateTrunc(
                                    Sql.DateParts.Year,
                                    Sql2.Column<DateTime>(dt, "out_date")
                                )
                            )
                        )!,
            _ => throw new NotImplementedException()
        };
        ;
    }
}
