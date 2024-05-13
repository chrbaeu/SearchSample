using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.QueryProcessing;

public class LinqPredicateBuilder(TokenizerConfig config, StringComparison stringComparison) : PredicateBuilder(config)
{

    private readonly MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;

    protected override MethodCallExpression ContainsMethodCallBuilder(MemberExpression property, string token)
    {
        return Expression.Call(property, containsMethod, Expression.Constant(token), Expression.Constant(stringComparison));
    }

}
