using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using DataModel;
using LinqToDB;
using TView = DataModel.MeasurementDataSchema.EpguCsiUploadCrossYearLsView;

namespace MeasurementData.MeasurementModule;

public class ExpressionTableDynamicQueryHelper<T> : IMeasurementQueryHelper
    where T : notnull
{
    private readonly Func<AppDataConnection, IQueryable<T>> _queriableFunc;
    private readonly Func<
        IQueryable<IGrouping<MSeriesDbSlice, T>>,
        IQueryable<MSeriesDbValue>
    > _calculationExpression;
    private readonly TableDynamicOptions _options;

    public ExpressionTableDynamicQueryHelper(
        Func<AppDataConnection, IQueryable<T>> queriableFunc,
        Func<
            IQueryable<IGrouping<MSeriesDbSlice, T>>,
            IQueryable<MSeriesDbValue>
        > calculationExpression,
        TableDynamicOptions tableOptions
    )
    {
        _queriableFunc = queriableFunc;
        _calculationExpression = calculationExpression;
        _options = tableOptions ?? throw new ArgumentNullException(nameof(tableOptions));
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

    public Func<AppDataConnection, IQueryable<MSeriesDbValue>> GetQueryable(
        MeasurementDataRequest dataRequest,
        PeriodRequest periodRequest
    )
    {
        (var canApply, string? error) = CanApplyRequest(periodRequest);
        return db =>
        {
            var utcNow = DateTime.UtcNow;

            var (predicate, values) = TableQueryBuildHelper.GetConditions(
                dataRequest,
                periodRequest,
                typeof(TView),
                this._options
            );
            var query = this._queriableFunc(db)
                .Where(predicate, values)
                .GroupByFilter(dataRequest, periodRequest);

            return _calculationExpression(query);
        };
    }
}
