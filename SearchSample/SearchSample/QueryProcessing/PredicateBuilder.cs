using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SearchSample.QueryProcessing;

public abstract class PredicateBuilder(TokenizerConfig config)
{

    public Expression<Func<T, bool>> CreateExpression<T>(List<string> postfixTokens, Expression<Func<T, string>> propertyAccessor)
    {
        MemberExpression property = (MemberExpression)propertyAccessor.Body;
        return CreateExpression<T>(postfixTokens, property.Member.Name);
    }

    public Expression<Func<T, bool>> CreateExpression<T>(List<string> postfixTokens, string propertyName)
    {
        var stack = new Stack<Expression<Func<T, bool>>>();
        var param = Expression.Parameter(typeof(T), "x");
        var property = Expression.PropertyOrField(param, propertyName);
        foreach (var token in postfixTokens)
        {
            if (config.IsOperator(token))
            {
                if (token == config.NotToken)
                {
                    var expr = stack.Pop();
                    var negated = Expression.Not(expr.Body);
                    stack.Push(Expression.Lambda<Func<T, bool>>(negated, expr.Parameters));
                }
                else
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    var combined = token == config.AndToken ? Expression.AndAlso(left.Body, right.Body) : Expression.OrElse(left.Body, right.Body);
                    stack.Push(Expression.Lambda<Func<T, bool>>(combined, left.Parameters));
                }
            }
            else
            {
                var body = ContainsMethodCallBuilder(property, token);
                stack.Push(Expression.Lambda<Func<T, bool>>(body, param));
            }
        }
        return stack.Pop();
    }

    protected abstract MethodCallExpression ContainsMethodCallBuilder(MemberExpression property, string token);

    //protected virtual MethodCallExpression ContainsMethodCallBuilder(MemberExpression property, string token)
    //{
    //    MethodInfo containsMethod = typeof(SqlServerDbFunctionsExtensions)
    //            .GetMethod(nameof(SqlServerDbFunctionsExtensions.Contains), [typeof(DbFunctions), typeof(object), typeof(string)])!;
    //    return Expression.Call(null, containsMethod, Expression.Constant(EF.Functions), property, Expression.Constant(token));
    //}

    //protected virtual MethodCallExpression ContainsMethodCallBuilderY(MemberExpression property, string token)
    //{
    //    MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;
    //    return Expression.Call(property, containsMethod, Expression.Constant(token), Expression.Constant(StringComparison.OrdinalIgnoreCase));
    //}

    //protected virtual MethodCallExpression ContainsMethodCallBuilder(MemberExpression property, string token)
    //{
    //    MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
    //    return Expression.Call(property, containsMethod, Expression.Constant(token));
    //}

}
