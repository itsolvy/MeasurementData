using APRF.Web.Common.DataSource;

namespace MeasurementData.MeasurementModule;

///<remarks>
///Работает так: Если нет Ids - условие на не равно null. Если есть - на конкретные Ids
///</remarks>
public class SliceFilter
{
    public SliceFilter(Slice slice, long id)
    {
        Slice = slice;
        Value = id;
        Operator = FilterOperator.Equals;
    }

    public SliceFilter(Slice slice, FilterOperator @operator, object? value)
    {
        Slice = slice;
        Operator = @operator;
        Value = value;
    }

    public Slice Slice { get; set; }

    public FilterOperator Operator { get; set; }

    public object? Value { get; set; }
}
