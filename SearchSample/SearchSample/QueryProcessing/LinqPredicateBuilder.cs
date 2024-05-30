using SearchSample.QueryParser;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.QueryProcessing;

public class LinqPredicateBuilder(TokenizerConfig config) : SearchPredicateBuilder(config)
{
    private readonly TokenizerConfig config = config;

    private readonly MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;

    protected override MethodCallExpression ContainsMethodCallBuilder(MemberExpression property, string token)
    {
        if (token[0] == config.SegmentToken[0] && token[0] == token[^1])
        {
            token = token.Trim(config.SegmentToken[0]);
        }
        return Expression.Call(property, containsMethod, Expression.Constant(token), Expression.Constant(StringComparison.OrdinalIgnoreCase));
    }

}
