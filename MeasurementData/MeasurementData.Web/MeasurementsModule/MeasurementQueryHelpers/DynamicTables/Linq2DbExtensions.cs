using System.Linq.Dynamic.Core;
using LinqToDB;
using LinqToDB.SqlQuery;
using LinqToDB.Common;
using static LinqToDB.Sql;
using PN = LinqToDB.ProviderName;
using LinqToDB.Expressions;

namespace MeasurementData.MeasurementModule;

public static class Linq2DbExtensions
{
#pragma warning disable IDE0060 // Удалите неиспользуемый параметр

    /// <summary>
    /// Получить выражение для колонки аггрегации
    /// </summary>
    [Sql.Extension(
        "",
        BuilderType = typeof(SliceColumnExpressionBuilder<long>),
        ServerSideOnly = true
    )]
    public static long FromSliceFieldLong<T>(
        T entry,
        Slice subjectRf,
        Slice[] aggregateBy,
        long def = default
    )
    {
        throw new NotImplementedException(
            "Какой-то не тот вызов. Это должно выполняться на сервере"
        );
    }

    /// <summary>
    /// Получить выражение для колонки аггрегации
    /// </summary>
    [Sql.Extension(
        "",
        BuilderType = typeof(SliceColumnExpressionBuilder<long?>),
        ServerSideOnly = true
    )]
    public static long? FromSliceFieldNullableLong<T>(
        T entry,
        Slice subjectRf,
        Slice[] aggregateBy,
        long? def = default
    )
    {
        throw new NotImplementedException(
            "Какой-то не тот вызов. Это должно выполняться на сервере"
        );
    }

    /// <summary>
    /// Взять колоночное значение
    /// </summary>
    /// <param name="entry">Сущность БД</param>
    /// <param name="sql_name">Имя колонки в SQL</param>
    [Sql.Extension("", BuilderType = typeof(ColumnExpressionBuilder), ServerSideOnly = true)]
    internal static T Column<T>(object? entry, string sql_name)
    {
        throw new NotImplementedException(
            "Какой-то не тот вызов. Это должно выполняться на сервере"
        );
    }

    /// <summary>
    /// Взять константу
    /// </summary>
    [Sql.Extension(
        "",
        BuilderType = typeof(ConstExpressionBuilder<DateTime>),
        ServerSideOnly = true
    )]
    internal static DateTime Const(object? entry, DateTime value)
    {
        throw new NotImplementedException(
            "Какой-то не тот вызов. Это должно выполняться на сервере"
        );
    }

    /// <summary>
    /// Получить преобразованную дату
    /// </summary>
    [Extension(
        PN.PostgreSQL,
        "date_trunc({part}, {date})",
        ServerSideOnly = true,
        PreferServerSide = true,
        BuilderType = typeof(DateTruncBuilderPostgreSQL)
    )]
    public static DateTime? DateTrunc(
        [SqlQueryDependent] Sql.DateParts part,
        [ExprParameter] DateTime date
    )
    {
        throw new NotImplementedException(
            "Какой-то не тот вызов. Это должно выполняться на сервере"
        );
    }

#pragma warning restore IDE0060 // Удалите неиспользуемый параметр

    private sealed class ColumnExpressionBuilder : Sql.IExtensionCallBuilder
    {
        public void Build(Sql.ISqExtensionBuilder builder)
        {
            var sqlName = builder.GetValue<string>(1);
            if (builder.GetExpression(0) is not SqlField field)
            {
                throw new InvalidOperationException("Entity required as parameter");
            }
            SqlTable table = (SqlTable)field.Table!;
            var sqlField =
                table.Fields.SingleOrDefault(x => x.PhysicalName == sqlName)
                ?? throw new InvalidOperationException(
                    $"sql поле {sqlName} не найдено для {field.ElementType}"
                );

            builder.ResultExpression = sqlField;
        }
    }

    private sealed class ConstExpressionBuilder<T> : Sql.IExtensionCallBuilder
    {
        public void Build(Sql.ISqExtensionBuilder builder)
        {
            var value = builder.GetValue<T>(1);
            if (builder.GetExpression(0) is not SqlField field)
            {
                throw new InvalidOperationException("Entity required as parameter");
            }
            builder.ResultExpression = new SqlValue(new DbDataType(typeof(DateTime)), value);
        }
    }

    //https://github.com/linq2db/linq2db/issues/1083
    //https://github.com/linq2db/linq2db/issues/2817
    //фича появилась в
    //https://github.com/linq2db/linq2db/pull/964
    /// <summary>
    /// Строит выражение для аггрегации с учетом того, что колонки может не существовать
    /// </summary>
    private sealed class SliceColumnExpressionBuilder<T> : Sql.IExtensionCallBuilder
    {
        public void Build(Sql.ISqExtensionBuilder builder)
        {
            var slice = builder.GetValue<Slice>(1);
            var aggregateBy = builder.GetValue<Slice[]>(2);
            var defValue = builder.GetValue<T>(3);
            if (builder.GetExpression(0) is not SqlField field)
            {
                throw new InvalidOperationException("Entity required as parameter");
            }
            var fieldName = $"{slice}Id";
            SqlTable table = (SqlTable)field.Table!;
            var sqlField = table.Fields.SingleOrDefault(x => x.Name == fieldName);
            if (sqlField == null || aggregateBy.Contains(slice))
            {
                builder.ResultExpression = new SqlValue(new DbDataType(typeof(T)), defValue);
            }
            else
            {
                builder.ResultExpression = sqlField;
            }
        }
    }

    private sealed class DateTruncBuilderPostgreSQL : Sql.IExtensionCallBuilder
    {
        public void Build(ISqExtensionBuilder builder)
        {
            string? partStr = null;
            var part = builder.GetValue<DateParts>("part");
            switch (part)
            {
                case DateParts.Year:
                    partStr = "'year'";
                    break;
                case DateParts.Quarter:
                    partStr = "'quarter'";
                    break;
                case DateParts.Month:
                    partStr = "'month'";
                    break;
                case DateParts.Day:
                    partStr = "'day'";
                    break;
                case DateParts.Week:
                    partStr = "'week'";
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected datepart: {part}");
            }

            builder.AddExpression("part", partStr);
        }
    }
}
