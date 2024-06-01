using SearchSample.QueryParser;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.QueryProcessing;

public class SqlitePredicateBuilder(TokenizerConfig config) : SearchPredicateBuilder(config)
{

    private readonly MethodInfo containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
    //private readonly MethodInfo containsMethod = typeof(DbFunctionsExtensions)
    //    .GetMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])!;


    protected override MethodCallExpression ContainsMethodCallBuilder(MemberExpression propertyExpression, string token)
    {
        //return Expression.Call(null, containsMethod, Expression.Constant(EF.Functions), Expression.Constant(property.Member.Name), Expression.Constant($"%{token}%"));
        return Expression.Call(propertyExpression, containsMethod, Expression.Constant(token));
    }

}
