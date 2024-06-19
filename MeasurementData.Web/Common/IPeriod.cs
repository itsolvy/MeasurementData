using System.ComponentModel.DataAnnotations;

namespace MeasurementData.Web.Common;

/// <summary>
/// Интерфейс, обозначающий период конкретных данных: C-По+ по ним вычисляется тип периода
/// </summary>
public interface IPeriod
{
    /// <summary>
    /// Дата начала периода
    /// </summary>
    DateTime InDate { get; }

    /// <summary>
    /// Дата конца периода
    /// </summary>
    DateTime OutDate { get; }

    /// <summary>
    /// Тип календарного уровня периода
    /// </summary>
    public long CalendarLevelId { get; }

    /// <summary>
    /// Признак валидности периода
    /// </summary>
    public bool IsValid { get; }
}
