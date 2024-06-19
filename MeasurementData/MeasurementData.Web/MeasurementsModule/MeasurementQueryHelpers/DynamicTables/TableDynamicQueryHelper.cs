using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using DataModel;
using LinqToDB;

namespace MeasurementData.MeasurementModule;

/// <summary>
/// Адаптер получения динамических данных по первичкам или витринам
/// </summary>
public class TableDynamicQueryHelper<T> : IMeasurementQueryHelper
    where T : notnull
{
    private readonly long _measurementId;
    private readonly Func<AppDataConnection, IQueryable<T>> _queriableFunc;
    private readonly Expression<Func<IGrouping<MSeriesDbSlice, T>, decimal>> _formulaFunc;
    private readonly TableDynamicOptions _options = null!;

    public TableDynamicQueryHelper(
        long measurementId,
        Func<AppDataConnection, IQueryable<T>> queriableFunc,
        Expression<Func<IGrouping<MSeriesDbSlice, T>, decimal>> formulaExpression,
        TableDynamicOptions tableOptions
    )
    {
        _queriableFunc = queriableFunc;
        _formulaFunc = formulaExpression;
        _options = tableOptions ?? throw new ArgumentNullException(nameof(tableOptions));
        _measurementId = measurementId;
    }

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

    public virtual Func<AppDataConnection, IQueryable<MSeriesDbValue>> GetQueryable(
        MeasurementDataRequest dataRequest,
        PeriodRequest periodRequest
    )
    {
        return db =>
        {
            var (predicate, values) = TableQueryBuildHelper.GetConditions(
                dataRequest,
                periodRequest,
                typeof(T),
                _options
            );

            var query = this._queriableFunc(db)
                .Where(predicate, values)
                .GroupByFilter(dataRequest, periodRequest)
                .SelectByFormula(_formulaFunc, _measurementId)
                .AsSubQuery();
            return query;
        };
    }
}
