namespace MeasurementData.MeasurementModule;

/// <summary>
///
/// </summary>
public class MeasurementQueryOptions
{
    public MeasurementQueryOptions(
        CalendarLevelType[] supportCalendarLevelIds,
        bool supportcomplexPeriodAggregation = true
    )
    {
        SupportCalendarLevels = supportCalendarLevelIds;
        SupportComplexPeriodAggregation = supportcomplexPeriodAggregation;
    }

    /// <summary>
    /// Поддерживаемые типы периодов
    /// </summary>
    public CalendarLevelType[] SupportCalendarLevels { get; protected set; }

    /// <summary>
    /// Поддержка аггрегации по составному периоду
    /// </summary>
    public bool SupportComplexPeriodAggregation { get; protected set; }
}
