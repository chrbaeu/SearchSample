using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.QueryProcessing;

public class SqlServerPredicateBuilder(TokenizerConfig config) : PredicateBuilder(config)
{

    private readonly MethodInfo containsMethod = typeof(SqlServerDbFunctionsExtensions)
        .GetMethod(nameof(SqlServerDbFunctionsExtensions.Contains), [typeof(DbFunctions), typeof(object), typeof(string)])!;

    protected override MethodCallExpression ContainsMethodCallBuilder(MemberExpression property, string token)
    {
        return Expression.Call(null, containsMethod, Expression.Constant(EF.Functions), property, Expression.Constant(token));
    }

}
