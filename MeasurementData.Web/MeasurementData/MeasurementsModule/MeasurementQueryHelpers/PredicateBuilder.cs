using System.Linq.Expressions;

namespace MeasurementData.MeasurementModule;

/// https://www.albahari.com/nutshell/predicatebuilder.aspx
public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>()
    {
        return f => true;
    }

    public static Expression<Func<T, bool>> False<T>()
    {
        return f => false;
    }

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2
    )
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(expr1.Body, invokedExpr),
            expr1.Parameters
        );
    }

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2
    )
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(expr1.Body, invokedExpr),
            expr1.Parameters
        );
    }

    public static Expression<Func<TElement, TResult>> ReplaceParameterByExpession<
        TElement,
        TValue1,
        TResult
    >(
        this Expression<Func<TElement, TValue1, TResult>> inputExpression,
        Expression<Func<TElement, TValue1>> tValue1Expression
    )
    {
        //тут нужный нам кусочек Body
        var expressionTemp =
            new Replacer(tValue1Expression.Parameters[0], inputExpression.Parameters[0]).Visit(
                tValue1Expression
            ) as Expression<Func<TElement, TValue1>>;

        var replacer = new Replacer(inputExpression.Parameters[1], expressionTemp!.Body);
        var body = replacer.Visit(inputExpression.Body);
        return Expression.Lambda<Func<TElement, TResult>>(body!, inputExpression.Parameters[0]);
    }

    public static Expression<Func<TElement, TResult>> ReplaceParameterByExpession<
        TElement,
        TValue1,
        TValue2,
        TResult
    >(
        this Expression<Func<TElement, TValue1, TValue2, TResult>> inputExpression,
        Expression<Func<TElement, TValue1>> innerExpression1,
        Expression<Func<TElement, TValue1>> innerExpression2
    )
    {
        //тут нужный нам кусочек Body
        var expressionTemp1 =
            new Replacer(innerExpression1.Parameters[0], inputExpression.Parameters[0]).Visit(
                innerExpression1
            ) as Expression<Func<TElement, TValue1>>;

        var expressionTemp2 =
            new Replacer(innerExpression2.Parameters[0], inputExpression.Parameters[0]).Visit(
                innerExpression2
            ) as Expression<Func<TElement, TValue2>>;

        var body1 = new Replacer(inputExpression.Parameters[1], expressionTemp1!.Body).Visit(
            inputExpression.Body
        );
        var body2 = new Replacer(inputExpression.Parameters[2], expressionTemp2!.Body).Visit(body1);
        return Expression.Lambda<Func<TElement, TResult>>(body2!, inputExpression.Parameters[0]);
    }

    private sealed class Replacer : ExpressionVisitor
    {
        private readonly Expression _from,
            _to;

        public Replacer(Expression from, Expression to)
        {
            _from = from;
            _to = to;
        }

        public override Expression? Visit(Expression? node) =>
            node == _from ? _to : base.Visit(node);
    }

    //public static Example<Func<T1, bool>> SubInser<T1,T2>(this Expression<Func<T2, bool>> expr1,T)
}
