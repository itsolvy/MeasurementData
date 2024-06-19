using System.Linq;
using LinqToDB;
using MD = APRF.Web.MeasurementDictionary;

namespace MeasurementData.MeasurementModule;

public static class MeasurementsDataConfigirator
{
    //Настройки для таблицы уникальных пользователей
    //Суть: можно проводить аггрегацию только в рамках одинаковых по типу и дате периодов
    private static readonly TableDynamicOptions _uniqueUsersTableOptions =
        new TableDynamicOptions(
            new[]
            {
                CalendarLevelType.Day,
                CalendarLevelType.Week,
                CalendarLevelType.Month,
                CalendarLevelType.Quartal,
                CalendarLevelType.Year
            },
            new[]
            {
                CalendarLevelType.Day,
                CalendarLevelType.Week,
                CalendarLevelType.Month,
                CalendarLevelType.Quartal,
                CalendarLevelType.Year
            },
            false
        );

    public static void Configure()
    {
        //1
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICE_TIME_MEAS_SERVICES_ID,
            db => db.MeasurementData.EpguAvgUploads,
            gr => gr.Sum(a => a.AvgTime * a.StatementCount) / gr.Sum(a => a.StatementCount)
        );
        //2 - представление
        //TODO: В витрину?
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_TIME_MEAS_ID,
            db => db.MeasurementData.EpguAvgUploadLsViews,
            gr =>
                gr.Sum(a => a.AvgTime!.Value * a.StatementCount!.Value)
                / gr.Sum(a => a.StatementCount!.Value)
        );

        //10
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_CONSUMER_MEAS_ID,
            db => db.MeasurementData.EpguUniqueUsers,
            gr => gr.Sum(ue => ue.ServiceConsumers!.Value),
            _uniqueUsersTableOptions
        );
        //11
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_CONSUMERS_MEAS_ID,
            db => db.MeasurementData.EpguUniqueUsers,
            gr => gr.Sum(ue => ue.LifeSituationConsumers!.Value),
            _uniqueUsersTableOptions
        );

        //15
        //TODO: В витрину?
        MeasurementDataModule.ConfigFromExpression(
            MD.SERVICES_CSI_YEAR_DYNAMICS_ID,
            db => db.MeasurementData.EpguCsiUploadCrossYearViews,
            query =>
                query
                    .Select(
                        gr =>
                            new
                            {
                                gr.Key,
                                val1 = gr.Sum(
                                    ecu => ecu.ServiceCustomerCount * ecu.ServiceCsiIndex
                                ),
                                val2 = gr.Sum(ecu => ecu.ServiceCustomerCount),
                                val3 = gr.Sum(ecul => ecul.LastServiceCustomerCount),
                                val4 = gr.Sum(
                                    ecul =>
                                        ecul.LastServiceCustomerCount * ecul.LastServiceCsiIndex
                                ),
                            }
                    )
                    .Where(
                        gr =>
                            gr.val1 != null
                            && gr.val2 != null
                            && gr.val3 != null
                            && gr.val4 != null
                    )
                    .Select(
                        group =>
                            new MSeriesDbValue
                            {
                                SubjectRfId = group.Key.SubjectRfId,
                                ServiceCardId = group.Key.ServiceCardId,
                                CardLifeSituationId = group.Key.CardLifeSituationId,
                                ToolServiceId = group.Key.ToolServiceId,
                                ClientWayMoveId = group.Key.ClientWayMoveId,
                                AgencyId = group.Key.AgencyId,
                                SegmentId = group.Key.SegmentId,
                                SupportMeasuresId = group.Key.SupportMeasuresId,
                                AreaLifeId = group.Key.AreaLifeId,
                                ClientTypeId = group.Key.ClientTypeId,
                                OkvedId = group.Key.OkvedId,
                                PersonGenderId = group.Key.PersonGenderId,
                                IndicatorId = group.Key.IndicatorId,
                                UserKindId = group.Key.UserKindId,
                                ApplicationStatusId = group.Key.ApplicationStatusId,
                                PersonAgeId = group.Key.PersonAgeId,
                                CalendarLevelId = group.Key.CalendarLevelId,
                                InDate = group.Key.InDate,
                                OutDate = group.Key.OutDate,
                                UpdateDate = DateTime.UtcNow,
                                Value =
                                    (
                                        group.val1!.Value
                                            * group.val3!.Value
                                            / group.val2!.Value
                                            / group.val4!.Value
                                        - 1
                                    ) * 100,
                                ValueTypeId = (long)MeasurementValueType.Fact
                            }
                    )
        );

        //16
        //TODO: В витрину?
        MeasurementDataModule.ConfigFromExpression(
            MD.LIFESITUATION_CSI_YEAR_DYNAMICS_ID,
            db => db.MeasurementData.EpguCsiUploadCrossYearLsViews,
            query =>
                query
                    .Select(
                        gr =>
                            new
                            {
                                gr.Key,
                                val1 = gr.Sum(
                                    ecu => ecu.ServiceCustomerCount * ecu.ServiceCsiIndex
                                ),
                                val2 = gr.Sum(ecu => ecu.ServiceCustomerCount),
                                val3 = gr.Sum(ecul => ecul.LastServiceCustomerCount),
                                val4 = gr.Sum(
                                    ecul =>
                                        ecul.LastServiceCustomerCount * ecul.LastServiceCsiIndex
                                ),
                            }
                    )
                    .Where(
                        gr =>
                            gr.val1 != null
                            && gr.val2 != null
                            && gr.val3 != null
                            && gr.val4 != null
                    )
                    .Select(
                        group =>
                            new MSeriesDbValue
                            {
                                SubjectRfId = group.Key.SubjectRfId,
                                ServiceCardId = group.Key.ServiceCardId,
                                CardLifeSituationId = group.Key.CardLifeSituationId,
                                ToolServiceId = group.Key.ToolServiceId,
                                ClientWayMoveId = group.Key.ClientWayMoveId,
                                AgencyId = group.Key.AgencyId,
                                SegmentId = group.Key.SegmentId,
                                SupportMeasuresId = group.Key.SupportMeasuresId,
                                AreaLifeId = group.Key.AreaLifeId,
                                ClientTypeId = group.Key.ClientTypeId,
                                OkvedId = group.Key.OkvedId,
                                PersonGenderId = group.Key.PersonGenderId,
                                IndicatorId = group.Key.IndicatorId,
                                UserKindId = group.Key.UserKindId,
                                ApplicationStatusId = group.Key.ApplicationStatusId,
                                PersonAgeId = group.Key.PersonAgeId,
                                CalendarLevelId = group.Key.CalendarLevelId,
                                InDate = group.Key.InDate,
                                OutDate = group.Key.OutDate,
                                UpdateDate = DateTime.UtcNow,
                                Value =
                                    (
                                        group.val1!.Value
                                            * group.val3!.Value
                                            / group.val2!.Value
                                            / group.val4!.Value
                                        - 1
                                    ) * 100,
                                ValueTypeId = (long)MeasurementValueType.Fact,
                                MeasurementId = MD.LIFESITUATION_CSI_YEAR_DYNAMICS_ID
                            }
                    )
        );

        //19  - представление
        //TODO: В витрину?
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_MIN_SERVICES_CSI,
            db => db.MeasurementData.EpguCsiUploadLsViews,
            gr => gr.Min(x => x.ServiceMinCsiIndex!.Value)
        );

        //20
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_CSI_INDEX,
            db => db.MeasurementData.EpguCsiUploads,
            gr =>
                gr.Sum(ecu => ecu.ServiceCustomerCount * ecu.ServiceCsiIndex)
                / gr.Sum(ecu => ecu.ServiceCustomerCount)
        );

        //21 - представление
        // TODO: В витрину ?
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_CSI_INDEX,
            db => db.MeasurementData.EpguCsiUploadLsViews,
            gr =>
                gr.Sum(ecu => ecu.ServiceCustomerCount!.Value * ecu.ServiceCsiIndex!.Value)
                / gr.Sum(ecu => ecu.ServiceCustomerCount!.Value)
        );

        //45
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_PROCESSING_ERRORS_MEAS_ID,
            db => db.MeasurementData.EpguErrorsUploads,
            gr => gr.Sum(ecu => ecu.ServiceProcessingErrors!.Value)
        );

        //46
        MeasurementDataModule.ConfigFromTable(
            MD.STATEMENTS_SENDING_MEAS_ID,
            db => db.MeasurementData.EpguOrdersUploads,
            gr => gr.Sum(ecu => ecu.StatementSending)
        );

        //47
        MeasurementDataModule.ConfigFromTable(
            MD.STATEMENTS_DRAFT_MEAS_ID,
            db => db.MeasurementData.EpguDraftsUploads,
            gr => gr.Sum(ecu => ecu.StatementDrafts)
        );

        //48
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_ERRORS_MEAS_ID,
            db => db.MeasurementData.EpguErrorsUploads,
            gr => gr.Sum(ecu => ecu.ServiceErrors!.Value)
        );

        //51
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_CSI_MIN_INDEX_ID,
            db => db.MeasurementData.EpguCsiUploads,
            gr => gr.Min(ecu => ecu.ServiceMinCsiIndex)
        );

        //53
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_SATISFACTION_LEVEL_QUALITY_ID,
            db => db.MeasurementData.MkguRateDataMartLsViews,
            gr =>
                gr.Sum(mart => mart.Rate!.Value > 3 ? mart.Count!.Value : 0)
                / gr.Sum(mart => mart.Count!.Value)
                * 100
        );

        //59
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_SHARE_OF_ERRORS_STATUSES_253_22_24_30_5_ETC,
            db => db.MeasurementData.EpguServiceErrorsUploadLsViews,
            gr => gr.Sum(ecu => ecu.HasError!.Value) / gr.Count()
        );

        //63- представление
        // TODO: В витрину ?
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_NUMBERS_OF_ERRORS_ID,
            db => db.MeasurementData.EpguErrorsUploadLsViews,
            gr => gr.Sum(eeu => eeu!.ServiceProcessingErrors!.Value)
        );

        //64- представление
        // TODO: В витрину ?
        MeasurementDataModule.ConfigFromQueryable(
            MD.LIFESITUATION_SHARE_OF_SERVICES_ONLINE_ID,
            db =>
                db.MeasurementData.EpguOrdersUploadLsViews.Where(
                    eou => eou.ServiceOnlineProvided != null || eou.ServiceProvided != null
                ),
            gr =>
                gr.Sum(eou => eou.ServiceOnlineProvided!.Value)
                * 100
                / (
                    gr.Sum(eou => eou.ServiceOnlineProvided!.Value)
                    + gr.Sum(eou => eou.ServiceProvided!.Value)
                )
        );

        //65- представление
        // TODO: В витрину ?
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_SHARE_OF_SERVICES_DECLINE_ID,
            db => db.MeasurementData.EpguOrdersUploadLsViews,
            gr =>
                gr.Sum(eou => eou.ServiceStatuses4Or41!.Value)
                * 100
                / (
                    gr.Sum(eou => eou.ServiceStatuses4Or41!.Value)
                    + gr.Sum(eou => eou.ServiceOnlineProvided!.Value)
                    + gr.Sum(eou => eou.ServiceProvided!.Value)
                )
        );

        //70- представление
        // TODO: В витрину ?
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_SHARE_OF_SERVICES_SUCESS_ID,
            db => db.MeasurementData.EpguOrdersUploadLsViews,
            gr =>
                (
                    gr.Sum(eou => eou.ServiceOnlineProvided!.Value)
                    + gr.Sum(eou => eou.ServiceProvidedSuccess!.Value)
                    + gr.Sum(eou => eou.ServiceProvided!.Value)
                )
                / gr.Sum(eou => eou.StatementSending!.Value)
                * 100
        );

        //75
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_SHARE_OF_SERVICES_DECLINE_ID,
            db => db.MeasurementData.EpguOrdersUploads,
            gr =>
                gr.Sum(eou => eou.ServiceStatuses4Or41)
                * 100
                / (
                    gr.Sum(eou => eou.ServiceStatuses4Or41)
                    + gr.Sum(eou => eou.ServiceOnlineProvided)
                    + gr.Sum(eou => eou.ServiceProvided)
                )
        );

        //76
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_SHARE_OF_SERVICES_ONLINE_ID,
            db => db.MeasurementData.EpguOrdersUploads,
            gr =>
                gr.Sum(eou => eou.ServiceOnlineProvided)
                * 100
                / (
                    gr.Sum(eou => eou.ServiceOnlineProvided)
                    + gr.Sum(eou => eou.ServiceProvided)
                )
        );

        //77
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_SHARE_OF_SERVICES_SUCESS_ID,
            db => db.MeasurementData.EpguOrdersUploads,
            gr =>
                (
                    gr.Sum(eou => eou.ServiceOnlineProvided)
                    + gr.Sum(eou => eou.ServiceProvidedSuccess)
                    + gr.Sum(eou => eou.ServiceProvided)
                )
                / gr.Sum(eou => eou.StatementSending)
                * 100
        );

        //78 - представление
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_COUNT_ONLINE_NOT_SEND_ID,
            db => db.MeasurementData.EpguDraftsUploadLsViews,
            gr => gr.Sum(x => x.StatementDrafts!.Value)
        );

        //82
        MeasurementDataModule.ConfigFromTable(
            MD.SERVICES_SHARE_OF_ERRORS,
            db => db.MeasurementData.EpguOrdersUploads,
            gr =>
                gr.Sum(
                    x =>
                        x.StatementSending
                        - x.ServiceStatuses4Or41
                        - x.ServiceOnlineProvided
                        - x.ServiceProvided
                        - x.ServiceProvidedSuccess
                ) / gr.Sum(x => x.StatementSending)
        );

        //83 - представление
        MeasurementDataModule.ConfigFromTable(
            MD.LIFESITUATION_SHARE_OF_ERRORS,
            db => db.MeasurementData.EpguOrdersUploadLsViews,
            gr =>
                gr.Sum(
                    x =>
                        x.StatementSending!.Value
                        - x.ServiceStatuses4Or41!.Value
                        - x.ServiceOnlineProvided!.Value
                        - x.ServiceProvided!.Value
                        - x.ServiceProvidedSuccess!.Value
                ) / gr.Sum(x => x.StatementSending!.Value)
        );
    }
}
