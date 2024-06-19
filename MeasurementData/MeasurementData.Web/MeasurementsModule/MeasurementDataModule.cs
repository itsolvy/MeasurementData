using System.Linq.Expressions;
using DataModel;
using LinqToDB;

namespace MeasurementData.MeasurementModule;

public static class MeasurementDataModule
{
    private static readonly Dictionary<long, Func<IMeasurementQueryHelper>> _repoFunctions =
        new();

    public static readonly MeasurementQueryOptions DefaultSliceOptions =
        new MeasurementQueryOptions(
            new[]
            {
                CalendarLevelType.Day,
                CalendarLevelType.Month,
                CalendarLevelType.Quartal,
                CalendarLevelType.Year,
                CalendarLevelType.Week,
            },
            false
        );

    private static readonly TableDynamicOptions DefaultDynamicOptions = new TableDynamicOptions(
        new[] { CalendarLevelType.Day },
        supportCrossPeriodAggregation: true
    );

    public static bool IsDynamic(long measurementId)
    {
        return _repoFunctions.ContainsKey(measurementId);
    }

    public static IMeasurementQueryHelper GetQueryHelper(long measurementId)
    {
        if (
            _repoFunctions!.TryGetValue(measurementId, out Func<IMeasurementQueryHelper>? value)
        )
        {
            return value!();
        }
        return new MSliceQueryHelper(measurementId, DefaultSliceOptions);
    }

    public static void ConfigFromTable<T>(
        long measurementId,
        Func<AppDataConnection, ITable<T>> queriableFunc,
        Expression<Func<IGrouping<MSeriesDbSlice, T>, decimal>> formulaExpression,
        TableDynamicOptions? options = null
    )
        where T : notnull
    {
        if (_repoFunctions.ContainsKey(measurementId))
        {
            throw new InvalidOperationException(
                $"Показатель {measurementId} уже отконфигирурован"
            );
        }
        _repoFunctions[measurementId] = () =>
            new TableDynamicQueryHelper<T>(
                measurementId,
                queriableFunc,
                formulaExpression,
                options ?? DefaultDynamicOptions
            );
    }

    public static void ConfigFromQueryable<T>(
        long measurementId,
        Func<AppDataConnection, IQueryable<T>> queriableFunc,
        Expression<Func<IGrouping<MSeriesDbSlice, T>, decimal>> formulaExpression,
        TableDynamicOptions? options = null
    )
        where T : notnull
    {
        if (_repoFunctions.ContainsKey(measurementId))
        {
            throw new InvalidOperationException(
                $"Показатель {measurementId} уже отконфигирурован"
            );
        }
        _repoFunctions[measurementId] = () =>
            new TableDynamicQueryHelper<T>(
                measurementId,
                queriableFunc,
                formulaExpression,
                options ?? DefaultDynamicOptions
            );
    }

    public static void ConfigFromExpression<T>(
        long measurementId,
        Func<AppDataConnection, ITable<T>> queriableFunc,
        Func<
            IQueryable<IGrouping<MSeriesDbSlice, T>>,
            IQueryable<MSeriesDbValue>
        > calculationExpression,
        TableDynamicOptions? options = null
    )
        where T : notnull
    {
        if (_repoFunctions.ContainsKey(measurementId))
        {
            throw new InvalidOperationException(
                $"Показатель {measurementId} уже отконфигирурован"
            );
        }
        _repoFunctions[measurementId] = () =>
            new ExpressionTableDynamicQueryHelper<T>(
                queriableFunc,
                calculationExpression,
                options ?? DefaultDynamicOptions
            );
    }

    public static void ConfigFromRepo(
        long measurementId,
        Func<IMeasurementQueryHelper> repoFunc
    )
    {
        if (_repoFunctions.ContainsKey(measurementId))
        {
            throw new InvalidOperationException(
                $"Показатель {measurementId} уже отконфигирурован"
            );
        }
        _repoFunctions[measurementId] = () => repoFunc();
    }
}
