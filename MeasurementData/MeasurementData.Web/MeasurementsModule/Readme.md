# Механизм получения данных показателей

## Общее описание работы

Общую схему работы модуля можно посмотреть [тут](https://conf.parma.ru/pages/viewpage.action?pageId=243542991)

Механизм включает в себя динамический расчет значений показателей "на лету" по формулам 
из таблиц для динамичсеких расчетов. Запрос на получение ``PeriodRequest`` данных содержит параметр 
тип ``Type`` - благодаря которому указывается как мы проводим аггрегацию по времени:
 1.  DataRow - по периодам типа CalendarLevel. И на выходе получаем временной ряд
 2.  DataValue - проводим сплошную аггрегацию по периоду запроса

Под капотом он содержит несколько концепции.
1. Общий интерфейс ``IMeasurementQueryHelper`` и общий модуль ``MeasurementDataModule`` 
для получения данных любых данных показателей
2. Конфигурирование ``MeasurementDataModule`` для сопоставления таблиц данных и формул
3. Конфирурирование построения витрин (пока не реализовано)

## Поля запроса

Запрос к данным формируется через функцию ``GetQueryable`` от БД, котрая возвращает ``IQueryable<MSeriesDbValue>``.
Аргументы функции
1. ``MeasurementDataRequest`` который говорит репозиторию, по каким полям фильтровать и\или аггрегировать данные.
2. ``PeriodRequest`` - задает временные ограничения, а так-же указывает тип аггрегации по дате

``IMeasurementQueryHelper`` могут не поддерживать запросы по некотопым уровням календаря или составному периоду. Для проверки  
поодержи есть метод ``(bool canApply,string? error) CanApplyRequest`` который проверяет возможность выполнения запроса, а в случае   
невозможности возвращает ошибку.



## БД

Таблицы и представления располагаются в схеме ``measurement_data``

Соглашение об именовании обьектов:
``sysname_name[_suffix]``
 
 где 
 1. **sysname**- суффикс системы откуда идут данные. epgu, mkgu и т.д.
 2. **suffix** - суффикс обьекта, по типу
 
**_mart**- таблица витрины

**_view** - представление 

**_upload** - таблица для первичной загрузки данных.

Как временное (постоянное?) решение некоторые показатели строятся прямо из таблиц загрузки

## Пример использования

### Конфиругирование

```csharp 
    //MeasurementsDataConfigirator.cs

    public static class MeasurementsDataConfigirator
    {
        public static void Configure()
        {
            //1 "Среднее время оказания услуг"
            MeasurementDataModule.ConfigMeasurementFromTable(
                //Id показателя
                MD.SERVICE_TIME_MEAS_SERVICES_ID,
                //Выбрать из таблицы или IQueriable
                db => db.MeasurementData.EpguAvgUploads,
                //Считать по формуле
                gr => gr.Sum(a => a.AvgTime!.Value) / gr.Sum(a => a.StatementCount!.Value));

        }
    }
```

Все показатели явно не указанные как динамичсекие будут считаться как статические

### Использование 

Например, для карточек ЖС и услуг есть репозиторий ``NextGenerationCardRepository``

Из мета-данных формируется ``MeasurementDataRequest``. В указанном ниже примере происходит фильтрация по двум полям,
по остальным разрезам происходит аггрегация

```csharp
//ServiceConst.cs
//..
                (cardId) =>
                    MeasurementDataRequest.AggregateAllExept(new[]
                    {
                        new SliceFilter(Slice.ServiceCard, cardId),
                        new SliceFilter(Slice.Taxonomy,TaxonomyDictionaries.SERVICE)
                    })
//..

```

Рассмотрим процесс получения данных на примере

```csharp
// NextGenerationCardRepository.cs

        public Task<CardDbValuesData[]> GetData(
            CardMeasurementsRequest request,
            CardMeasurementsMetaInfo metaInfo,
            CancellationToken cancellationToken
        )
        {
            //...
            //1. Задали период и уровень аггрегации по дате в рамках него
            PeriodRequest periodRequest = request.Period.ToPeriodDataRequest(PeriodRequestType.DataValue);

            //2. Запуск c ограниченным параллелизмом
            return measurements.WhenAllAsyncConcurrent(
                async mId =>
                {
                    //3. Получили репозиторий из модуля
                    var measurementRepo = MeasurementDataModule.GetQueryHelper(mId);
                    //4. Проверили, что он поддерживает нужный нам тип запроса
                    //Старый механизм статических данных, например, не поддерживает произвольный период
                    (var canApply, string? error) = measurementRepo.CanApplyRequest(periodRequest);
                    if (!canApply)
                    {
                        return new MeasurementDbValuesData(
                            mId,
                            null,
                            null,
                            null,
                            null,
                            error
                        );
                    }
                    return await _dbService.Execute(async db =>
                    {
                        var prevPeriod = request.Period.AddPeriod(-1);
                        var filter = metaInfo.GetDataFilterFunc(request.CardId);

                        var periodData = await measurementRepo
                            //5. Получили IQueryable<MSeriesDbValue> нужных нам данных
                            .GetQueryable(filter, periodRequest)(db)
                            //6. Обвесили его нужными нами условиями сверху
                            .Where(x => x.ValueTypeId == (long)MeasurementValueType.Fact)
                            .OrderByDescending(x => x.InDate)
                            //7. Выбрали данные
                            .FirstOrDefaultAsync(cancellationToken);

                        //8. Надо проверять на комплексность периода, для высчитывания динамики
                        var prevPeriodData =
                            prevPeriod != null && !periodRequest.IsComplexPeriod
                                ? await measurementRepo
                                    .GetQueryable(filter, periodRequest.AddPeriod(-1)!)(db)
                                    .Where(x => x.ValueTypeId == (long)MeasurementValueType.Fact)
                                    .OrderByDescending(x => x.InDate)
                                    .FirstOrDefaultAsync(cancellationToken)
                                : null;
                      }
                            //.... 
                }
            }

```