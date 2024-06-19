using DataModel;

namespace MeasurementData.MeasurementModule;

/// <summary>
/// Типовой репозиторий показателя
/// </summary>
public interface IMeasurementQueryHelper
{
    /// <summary>
    /// Проверить поддержу
    /// </summary>
    (bool canApply, string? error) CanApplyRequest(PeriodRequest periodRequest);

    /// <summary>
    /// Функция получения данных
    /// </summary>
    /// <remarks>Фактически возвращает SQL-запрос</remarks>
    Func<AppDataConnection, IQueryable<MSeriesDbValue>> GetQueryable(
        MeasurementDataRequest dataRequest,
        PeriodRequest periodRequest
    );
}
