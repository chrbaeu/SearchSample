using Microsoft.EntityFrameworkCore;
using SearchSample.QueryParser;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.QueryProcessing;

public class SqlServerPredicateBuilder(TokenizerConfig config) : SearchPredicateBuilder(config)
{

    private readonly MethodInfo containsMethod = typeof(SqlServerDbFunctionsExtensions)
        .GetMethod(nameof(SqlServerDbFunctionsExtensions.Contains), [typeof(DbFunctions), typeof(object), typeof(string)])!;

    protected override MethodCallExpression ContainsMethodCallBuilder(MemberExpression propertyExpression, string token)
    {
        return Expression.Call(null, containsMethod, Expression.Constant(EF.Functions), propertyExpression, Expression.Constant($"\"*{token}*\""));
    }

}
