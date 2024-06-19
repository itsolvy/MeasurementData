using System.ComponentModel.DataAnnotations;
using APRF.Web.Common;

namespace MeasurementData.MeasurementModule;

public class MeasurementDataRequest
{
    /// <summary>
    /// Аггрегировать по разрезам
    /// </summary>

    public Slice[] AggregateBy { get; set; } = Array.Empty<Slice>();

    /// <summary>
    /// Отфильтрловать данные
    /// </summary>
    public SliceFilter[] Filters { get; set; } = Array.Empty<SliceFilter>();

    /// <summary>
    /// Получить Выборку, где аггрегировать по всему, кроме фильтров и указанных разрезов
    /// </summary>
    public static MeasurementDataRequest AggregateAllExept(
        SliceFilter[] sliceFilters,
        params Slice[]? exeptSlices
    )
    {
        var slices = sliceFilters.Select(x => x.Slice).ToArray();
        var aggregateBySlices = ((Slice[])Enum.GetValues(typeof(Slice)))
            .Where(s => !slices.Contains(s))
            .Where(s => exeptSlices == null || !exeptSlices.Contains(s))
            .ToArray();
        return new MeasurementDataRequest
        {
            Filters = sliceFilters,
            AggregateBy = aggregateBySlices
        };
    }
}
