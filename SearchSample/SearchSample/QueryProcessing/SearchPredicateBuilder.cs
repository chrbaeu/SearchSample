using SearchSample.QueryParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SearchSample.QueryProcessing;

public abstract class SearchPredicateBuilder(TokenizerConfig config)
{
    /// <summary>
    /// Creates an expression for searching based on the given postfix tokens and property accessor.
    /// </summary>
    /// <param name="postfixTokens">The postfix tokens representing the search query.</param>
    /// <param name="propertyAccessor">The property accessor expression.</param>
    /// <returns>An expression representing the search predicate.</returns>
    public Expression<Func<T, bool>> CreateExpression<T>(IEnumerable<string> postfixTokens, Expression<Func<T, string>> propertyAccessor)
    {
        var stack = new Stack<Expression<Func<T, bool>>>();
        var param = propertyAccessor.Parameters.First();
        var property = (MemberExpression)propertyAccessor.Body;
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

    /// <summary>
    /// Builds the method call expression for the contains method.
    /// </summary>
    /// <param name="property">The property expression.</param>
    /// <param name="searchWord">The search word.</param>
    /// <returns>The method call expression.</returns>
    protected abstract MethodCallExpression ContainsMethodCallBuilder(MemberExpression propertyExpression, string searchWord);

}
