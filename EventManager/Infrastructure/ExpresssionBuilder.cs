using System.Linq.Expressions;

namespace EventManager.Infrastructure;

public static class ExpressionBuilder
{
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T), "x");

        var visitorL = new ParameterReplacer(left.Parameters[0], param);
        var visitorR = new ParameterReplacer(right.Parameters[0], param);

        var body = Expression.AndAlso(visitorL.Visit(left.Body), visitorR.Visit(right.Body));
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
