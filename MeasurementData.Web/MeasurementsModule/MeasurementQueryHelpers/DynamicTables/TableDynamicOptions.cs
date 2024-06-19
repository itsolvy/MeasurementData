using MeasurementData.Web.Common;

namespace MeasurementData.MeasurementModule;

public class TableDynamicOptions : MeasurementQueryOptions
{
    /// <summary>
    /// Типы данных, которые лежат в таблице
    /// </summary>
    public CalendarLevelType[] DataLevels { get; }

    public TableDynamicOptions(
        CalendarLevelType[] dataLevels,
        CalendarLevelType[]? supportCalendarLevels = null,
        bool supportCrossPeriodAggregation = true
    )
        : base(
            supportCalendarLevels ?? Array.Empty<CalendarLevelType>(),
            supportCrossPeriodAggregation
        )
    {
        DataLevels = dataLevels;
        if (supportCalendarLevels == null)
        {
            CalendarLevelType[] all = Enum.GetValues<CalendarLevelType>();
            if (DataLevels.Contains(CalendarLevelType.Day))
            {
                SupportCalendarLevels = all;
            }
            else
            {
                var minLevel = (long)
                    DataLevels.Where(x => x != CalendarLevelType.Day).Min(x => x);
                SupportCalendarLevels = all.Where(level => (long)level >= minLevel).ToArray();
            }
        }
    }
}
