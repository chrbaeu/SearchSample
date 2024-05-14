using SearchSample.QueryParser;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.SearchProcessing;

public class WeightingFunctionBuilder(TokenizerConfig config)
{

    public Expression<Func<T, int>> CreateExpression<T>(IEnumerable<string> postfixTokens, Expression<Func<T, string>> propertyAccessor, MethodInfo weightFunction)
    {
        var property = (MemberExpression)propertyAccessor.Body;
        return CreateExpression<T>(postfixTokens, property.Member.Name, weightFunction);
    }

    public Expression<Func<T, int>> CreateExpression<T>(IEnumerable<string> postfixTokens, string propertyName, MethodInfo weightFunction)
    {
        var stack = new Stack<Expression<Func<T, int>>>();
        var param = Expression.Parameter(typeof(T), "x");
        foreach (var token in postfixTokens)
        {
            if (config.IsOperator(token))
            {
                if (token != config.NotToken)
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    var combined = Expression.Add(left.Body, right.Body);
                    stack.Push(Expression.Lambda<Func<T, int>>(combined, param));
                }
            }
            else
            {
                var match = Expression.Call(
                    null,
                    weightFunction,
                    Expression.PropertyOrField(param, propertyName),
                    Expression.Constant(token)
                );
                stack.Push(Expression.Lambda<Func<T, int>>(match, param));
            }
        }
        return stack.Pop();
    }

}
