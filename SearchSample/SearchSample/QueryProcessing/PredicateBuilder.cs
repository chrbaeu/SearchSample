using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.QueryProcessing;

public class PredicateBuilder(TokenizerConfig config)
{

    public Expression<Func<T, bool>> CreateExpression<T>(List<string> postfixTokens, Expression<Func<T, string>> propertyAccessor)
    {
        MemberExpression property = (MemberExpression)propertyAccessor.Body;
        return CreateExpression<T>(postfixTokens, property.Member.Name);
    }

    public Expression<Func<T, bool>> CreateExpression<T>(List<string> postfixTokens, Expression<Func<T, string>> propertyAccessor, MethodInfo containsMethod)
    {
        MemberExpression property = (MemberExpression)propertyAccessor.Body;
        return CreateExpression<T>(postfixTokens, property.Member.Name, containsMethod);
    }

    public Expression<Func<T, bool>> CreateExpression<T>(List<string> postfixTokens, string propertyName)
    {
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        return CreateExpression<T>(postfixTokens, propertyName, containsMethod);
    }

    public Expression<Func<T, bool>> CreateExpression<T>(List<string> postfixTokens, string propertyName, MethodInfo containsMethod)
    {
        var stack = new Stack<Expression<Func<T, bool>>>();
        var param = Expression.Parameter(typeof(T), "x");
        var property = Expression.PropertyOrField(param, propertyName);
        foreach (var token in postfixTokens)
        {
            if (IsOperator(token))
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
                var body = Expression.Call(property, containsMethod, Expression.Constant(token));
                stack.Push(Expression.Lambda<Func<T, bool>>(body, param));
            }
        }
        return stack.Pop();
    }

    private bool IsOperator(string token) => token == config.AndToken || token == config.OrToken || token == config.NotToken;

}
