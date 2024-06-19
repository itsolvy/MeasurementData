using APRF.Web.Common;
using MeasurementData.Web.Common;

namespace MeasurementData.MeasurementModule;

public class MSeriesDbValue : MSeriesDbSlice, IPeriod
{
    public long MeasurementId { get; set; }

    /// <summary>
    /// Значение в базовых ЕИ
    /// </summary>
    public decimal Value { get; set; }
    public long TaxonomyId { get; set; }

    public bool IsValid => true;
}

public class MSeriesDbSlice
{
    public long SubjectRfId { get; set; }
    public long? ServiceCardId { get; set; }
    public long? CardLifeSituationId { get; set; }
    public long? ToolServiceId { get; set; }
    public long? ClientWayMoveId { get; set; }
    public long? AgencyId { get; set; }
    public long? SegmentId { get; set; }
    public long? SupportMeasuresId { get; set; }
    public long? AreaLifeId { get; set; }
    public long? ClientTypeId { get; set; }
    public long? OkvedId { get; set; }
    public long? PersonGenderId { get; set; }
    public long? IndicatorId { get; set; }
    public long? UserKindId { get; set; }
    public long? ApplicationStatusId { get; set; }
    public long? PersonAgeId { get; set; }

    public DateTime InDate { get; set; }
    public DateTime OutDate { get; set; }

    public DateTime UpdateDate { get; set; }
    public long CalendarLevelId { get; set; }
}
