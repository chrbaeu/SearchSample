using SearchSample.QueryParser;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.QueryProcessing;

public class LinqPredicateBuilder(TokenizerConfig config) : SearchPredicateBuilder(config)
{
    private readonly TokenizerConfig config = config;

    private readonly MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;

    protected override MethodCallExpression ContainsMethodCallBuilder(MemberExpression propertyExpression, string searchWord)
    {
        if (searchWord[0] == config.SegmentToken[0] && searchWord[0] == searchWord[^1])
        {
            searchWord = searchWord.Trim(config.SegmentToken[0]);
        }
        return Expression.Call(propertyExpression, containsMethod, Expression.Constant(searchWord), Expression.Constant(StringComparison.OrdinalIgnoreCase));
    }

}
