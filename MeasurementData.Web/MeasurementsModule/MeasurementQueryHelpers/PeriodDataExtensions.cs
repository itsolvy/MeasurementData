using APRF.Web.Common;
using MeasurementData.Web.Common;

namespace MeasurementData.MeasurementModule;

public static class PeriodDataExtensions
{
    public static PeriodRequest? AddPeriod(this PeriodRequest periodRequest, int periodNum)
    {
        if (periodRequest.InDate == null || periodRequest.OutDate == null)
        {
            return null;
        }
        var period = new Period
        {
            InDate = periodRequest.InDate.Value,
            OutDate = periodRequest.OutDate.Value,
            CalendarLevelId = periodRequest.CalendarLevelId,
        };
        if (period.IsValid)
        {
            var newPeriod = period.AddCalendarLevelPeriod(periodNum);
            return new PeriodRequest(
                newPeriod.InDate,
                newPeriod.OutDate,
                newPeriod.CalendarLevelId,
                periodRequest.Type
            );
        }
        else
        {
            var delta = periodRequest.OutDate.Value - periodRequest.InDate.Value;
            return new PeriodRequest(
                periodRequest.InDate.Value.Add(delta * periodNum).AddDays(periodNum),
                periodRequest.OutDate.Value.Add(delta * periodNum).AddDays(periodNum),
                periodRequest.CalendarLevelId,
                periodRequest.Type
            );
        }
    }

    public static PeriodRequest ToPeriodDataRequest(
        this IPeriod period,
        PeriodRequestType valueType
    )
    {
        return new PeriodRequest(period.InDate, period.OutDate, period.CalendarLevelId, valueType);
    }
}
