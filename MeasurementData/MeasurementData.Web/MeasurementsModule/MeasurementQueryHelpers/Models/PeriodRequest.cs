using APRF.Web.Common;
using DataModel;

namespace MeasurementData.MeasurementModule;

public class PeriodRequest
{
    /// <summary>
    /// Дата с
    /// </summary>
    public DateTime? InDate { get; }

    /// <summary>
    /// Дата по
    /// </summary>
    public DateTime? OutDate { get; }

    /// <summary>
    /// По уровню календаря
    /// </summary>
    public long CalendarLevelId { get; }

    public CalendarLevelType CalendarLevel
    {
        get => (CalendarLevelType)CalendarLevelId;
    }

    /// <summary>
    /// Тип аггрегации по времени: Временной ряд/Значение
    /// </summary>
    public PeriodRequestType Type { get; }

    public bool IsComplexPeriod
    {
        get
        {
            if (_IsComplexPeriod == null)
            {
                if (InDate == null || OutDate == null)
                {
                    _IsComplexPeriod = false;
                }
                else
                {
                    var period = new Period
                    {
                        InDate = InDate!.Value,
                        OutDate = OutDate!.Value,
                        CalendarLevelId = CalendarLevelId
                    };
                    _IsComplexPeriod = period.IsValid && period.IsComplex;
                }
            }
            return _IsComplexPeriod.Value;
        }
    }
    private bool? _IsComplexPeriod = null;

    public PeriodRequest(
        DateTime? inDate,
        DateTime? outDate,
        long calendarLevelId,
        PeriodRequestType type
    )
    {
        Type = type;
        CalendarLevelId = calendarLevelId;
        InDate = inDate;
        OutDate = outDate;
    }
}

public enum PeriodRequestType
{
    /// <summary>
    /// Временной ряд
    /// </summary>
    DataRow = 0,

    /// <summary>
    /// Значение
    /// </summary>
    DataValue = 1
}
