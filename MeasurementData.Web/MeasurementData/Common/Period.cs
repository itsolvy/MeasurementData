using System.Text.Json.Serialization;
using Newtonsoft.Json.Serialization;

namespace MeasurementData.Web.Common;


public sealed class Period : ValidatableObject, IPeriod
{
    private DateTime _inDate;
    private DateTime _outDate;
    private long _calendarLevel;
    private (ValidationErrors errors, int complexNum)? _validationParseResult;

    /// <summary>
    /// Месяцы начала квартала
    /// </summary>
    private static readonly int[] _quartalMonths = { 1, 4, 7, 10 };

    #region Properties

    [NotNullEmptyDefault]
    public DateTime InDate
    {
        get => _inDate;
        set
        {
            _inDate = value;
            _validationParseResult = null;
        }
    }

    [NotNullEmptyDefault]
    public DateTime OutDate
    {
        get => _outDate;
        set
        {
            _outDate = value;
            _validationParseResult = null;
        }
    }

    //internal, чтобы оно не вошло в сваггер-схему
    [JsonIgnore]
    public CalendarLevelType CalendarLevel
    {
        get => (CalendarLevelType)_calendarLevel;
        set
        {
            _calendarLevel = (long)value;
            _validationParseResult = null;
        }
    }

    [NotNullEmptyDefault]
    public long CalendarLevelId
    {
        get => _calendarLevel;
        set
        {
            _calendarLevel = value;
            _validationParseResult = null;
        }
    }

    #region Validation Properties

    //internal, чтобы оно не вошло в сваггер-схему
    [JsonIgnore]
    public bool IsValid
    {
        get
        {
            if (_validationParseResult == null)
            {
                _validationParseResult = CheckParse();
            }
            return !_validationParseResult.Value.Item1.HasErrors;
        }
    }

    /// <summary>
    /// Признак составного периода
    /// </summary>
    [JsonIgnore]
    public bool IsComplex
    {
        get
        {
            if (_validationParseResult == null)
            {
                _validationParseResult = CheckParse();
            }
            return _validationParseResult.Value.complexNum > 1;
        }
    }

    /// <summary>
    /// Число простых периодов в периоде
    /// </summary>
    [JsonIgnore]
    public int ComplexNum
    {
        get
        {
            if (_validationParseResult == null)
            {
                _validationParseResult = CheckParse();
            }
            return _validationParseResult.Value.complexNum;
        }
    }

    #endregion

    #endregion

    public Period GetPreviosPeriod()
    {
        if (!IsValid)
        {
            throw new ValidationFailException(Validate());
        }
        if (!IsComplex)
        {
            return AddCalendarLevelPeriod(-1);
        }
        return AddCalendarLevelPeriod(-1 * ComplexNum);
    }

    /// <summary>
    /// Получить новый период, добавив к нему простой период календарного типа
    /// </summary>
    public Period AddCalendarLevelPeriod(int periodNum)
    {
        if (!IsValid)
        {
            throw new ValidationFailException(Validate());
        }
        switch (CalendarLevel)
        {
            case CalendarLevelType.Day:
                return new Period
                {
                    InDate = InDate.AddDays(1 * periodNum),
                    OutDate = OutDate.AddDays(1 * periodNum),
                    CalendarLevelId = CalendarLevelId,
                };
            case CalendarLevelType.Week:
                return new Period
                {
                    InDate = InDate.AddDays(7 * periodNum),
                    OutDate = OutDate.AddDays(7 * periodNum),
                    CalendarLevelId = CalendarLevelId,
                };
            case CalendarLevelType.Month:
                var year = InDate.Month != 1 ? InDate.Year : InDate.Year - 1;
                var month = InDate.AddMonths(1 * periodNum).Month;
                return new Period
                {
                    InDate = new DateTime(year, month, 1),
                    OutDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                    CalendarLevelId = CalendarLevelId,
                };
            case CalendarLevelType.Quartal:
                var indateQTemp = InDate.AddMonths(3 * periodNum);
                var indateQ = new DateTime(indateQTemp.Year, indateQTemp.Month, 1);
                return new Period
                {
                    InDate = indateQTemp,
                    OutDate = new DateTime(
                        indateQ.Year,
                        indateQ.Month + 2,
                        DateTime.DaysInMonth(indateQ.Year, indateQ.Month + 2)
                    ),
                    CalendarLevelId = CalendarLevelId,
                };
            case CalendarLevelType.Year:
                var yearY = InDate.Year + periodNum;
                return new Period
                {
                    InDate = new DateTime(yearY, 1, 1),
                    OutDate = new DateTime(yearY, 12, DateTime.DaysInMonth(yearY, 12)),
                    CalendarLevelId = CalendarLevelId,
                };
            default:
                throw new NotImplementedException();
        }
    }

    public static Period GetCurrentMonthPeriod()
    {
        var now = DateTime.UtcNow;
        return new Period
        {
            InDate = new DateTime(now.Year, now.Month, 1),
            OutDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)),
            CalendarLevel = CalendarLevelType.Month
        };
    }

    #region validation

    public override ValidationErrors Validate()
    {
        if (_validationParseResult == null)
        {
            _validationParseResult = CheckParse();
        }
        return _validationParseResult.Value.Item1;
    }

    private (ValidationErrors errors, int ComplexNum) CheckParse()
    {
        var result = base.Validate();
        if (!IsMidnight(InDate))
        {
            result.Add(nameof(InDate), "Дата должна содержать только день");
        }
        if (!IsMidnight(OutDate))
        {
            result.Add(nameof(InDate), "Дата должна содержать только день");
        }
        if (OutDate < InDate)
        {
            result.Add(nameof(InDate), "Дата конца должна быть не меньше даты начала");
        }
        if (result.HasErrors)
        {
            return new(result, 0);
        }
        //1. Период- один день
        if (CalendarLevelId == (long)CalendarLevelType.Day)
        {
            return new(result, (int)(OutDate - InDate).TotalDays + 1);
        }
        //2. Если начинается с первого числа
        if (InDate.Day == 1)
        {
            //2.1 Проверка на год
            if (CalendarLevelId == (long)CalendarLevelType.Year)
            //&& period.OutDate.DayOfYear == (DateTime.IsLeapYear(period.InDate.Year) ? 366 : 365)
            {
                if (InDate.DayOfYear != 1)
                {
                    result.Add(
                        nameof(InDate),
                        $"Период типа {CalendarLevel} должен начинаться с 1 января"
                    );
                }
                else if (
                    OutDate.DayOfYear != (DateTime.IsLeapYear(OutDate.Year) ? 366 : 365)
                )
                {
                    result.Add(
                        nameof(InDate),
                        $"Период типа {CalendarLevel} должен заканчиваться последним днем в году"
                    );
                }
                return new(
                    result,
                    !result.HasErrors ? OutDate.Year - InDate.Year + 1 : 0
                );
            }
            //2.2 начало-конец месяца
            else if (CalendarLevelId == (long)CalendarLevelType.Month)
            {
                if (OutDate.Day != DateTime.DaysInMonth(OutDate.Year, OutDate.Month))
                {
                    result.Add(
                        nameof(InDate),
                        $"Период типа {CalendarLevel} должен заканчиваться последним днем в месяце"
                    );
                }
                return new(
                    result,
                    !result.HasErrors ? OutDate.Month - InDate.Month + 1 : 0
                );
            }
            //2.3 квартал
            else if (CalendarLevelId == (long)CalendarLevelType.Quartal)
            {
                if (!_quartalMonths.Contains(InDate.Month))
                {
                    result.Add(
                        nameof(InDate),
                        $"Период типа {CalendarLevel} должен начинаться в первый день квартала"
                    );
                }
                else if (
                    OutDate.Day != DateTime.DaysInMonth(OutDate.Year, OutDate.Month)
                    || !_quartalMonths.Contains(OutDate.Month - 2)
                )
                {
                    result.Add(
                        nameof(InDate),
                        $"Период типа {CalendarLevel} должен кончаться в последний день квартала"
                    );
                }
                return new(
                    result,
                    !result.HasErrors ? (OutDate.Month - InDate.Month) / 3 + 1 : 0
                );
            }
        }
        //3. Если неделя
        if (CalendarLevelId == (long)CalendarLevelType.Week)
        {
            if (
                InDate.DayOfWeek != DayOfWeek.Monday
                || OutDate.DayOfWeek != DayOfWeek.Sunday
            )
            {
                result.Add(
                    nameof(InDate),
                    $"Период типа {CalendarLevel} должен начинаться в понедельник, а заканчиваться в воскресенье"
                );
            }
            return new(result, (OutDate - InDate).Days / 7 + 1);
        }

        result.Add(nameof(InDate), "Невозможно получить тип периода");
        return new(result, 0);
    }

    /// <summary>
    /// Провалидировать период и попробовать получить тип периода
    /// </summary>
    /// <param name="period"></param>
    /// <returns></returns>
    public static (ValidationErrors, CalendarLevelType?) ParseValidate(Period period)
    {
        var result = new ValidationErrors();
        if (!IsMidnight(period.InDate))
        {
            result.Add(nameof(InDate), "Дата должна содержать только день");
        }
        if (!IsMidnight(period.OutDate))
        {
            result.Add(nameof(InDate), "Дата должна содержать только день");
        }
        if (period.OutDate < period.InDate)
        {
            result.Add(nameof(InDate), "Дата конца должна быть больше даты начала");
        }
        else if (
            period.InDate.Year != period.OutDate.Year
            && (period.OutDate - period.InDate).TotalDays != 6
        )
        {
            result.Add(
                nameof(InDate),
                "Период кроме недели должен начинаться и заканчиваться в одном году"
            );
        }
        if (result.HasErrors)
        {
            return new(result, null);
        }
        //1. Период- один день
        if (period.InDate == period.OutDate)
        {
            return new(result, CalendarLevelType.Day);
        }
        //2. Если начинается с первого числа
        if (period.InDate.Day == 1)
        {
            //2.1 январь- декабрь, значит год
            if (
                period.InDate.DayOfYear == 1
                && period.OutDate.DayOfYear == (DateTime.IsLeapYear(period.InDate.Year) ? 366 : 365)
            )
            {
                return new(result, CalendarLevelType.Year);
            }
            //2.2 начало-конец месяца
            else if (
                period.InDate.Month == period.OutDate.Month
                && period.OutDate.Day
                    == DateTime.DaysInMonth(period.OutDate.Year, period.OutDate.Month)
            )
            {
                return new(result, CalendarLevelType.Month);
            }
            //2.3 квартал
            else if (
                _quartalMonths.Contains(period.InDate.Month)
                && period.OutDate.Month == period.InDate.Month + 2
                && period.OutDate.Day
                    == DateTime.DaysInMonth(period.OutDate.Year, period.OutDate.Month)
            )
            {
                return new(result, CalendarLevelType.Quartal);
            }
        }
        //3. Если неделя
        if (
            period.InDate.DayOfWeek == DayOfWeek.Monday
            && period.OutDate.DayOfWeek == DayOfWeek.Sunday
            && (period.OutDate - period.InDate).Days == 6
        )
        {
            return new(result, CalendarLevelType.Week);
        }

        result.Add(nameof(period.InDate), "Невозможно получить тип периода");
        return new(result, null);
    }

    private static bool IsMidnight(DateTime date)
    {
        return date.Millisecond == 0 && date.Second == 0 && date.Minute == 0 && date.Hour == 0;
    }

    /// <summary>
    /// Создать период из точки
    /// </summary>
    public static Period GetPeriodFromPoint(DateTime date, CalendarLevelType calendarLevel)
    {
        throw new NotImplementedException();
    }

    #region Equals

    public bool Equals(IPeriod? other)
    {
        if (other is null)
        {
            return false;
        }

        return InDate == other.InDate && OutDate == other.OutDate;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as IPeriod);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InDate, OutDate, CalendarLevelId);
    }
    #endregion
    #endregion
}
