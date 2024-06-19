using APRF.Web.Common;
using DataModel;

namespace MeasurementData.MeasurementModule;

public static class MeasurementDataRepositoryExtensions
{
    public static Func<AppDataConnection, IQueryable<MSeriesDbValue>> GetQueryable(
        this IMeasurementQueryHelper repo,
        MeasurementDataRequest filter,
        long calendarLevel,
        DateTime? inDate,
        DateTime? outDate,
        PeriodRequestType type
    )
    {
        return repo.GetQueryable(
            filter,
            new PeriodRequest(inDate, outDate, calendarLevel, type)
        );
    }
}
