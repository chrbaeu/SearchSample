using SearchSample.Interfaces;
using SearchSample.QueryParser;
using SearchSample.QueryProcessing;
using SearchSample.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample.SearchProcessing;

public static class QueryableSearchExtensions
{

    private static readonly Dictionary<Type, Type> predicateBuilderDict = new()
    {
        [typeof(EnumerableQuery)] = typeof(LinqPredicateBuilder),
    };

    public static void AddPredicateBuilder<TQueryProvider, TSearchPredicateBuilder>()
        where TQueryProvider : IQueryProvider
        where TSearchPredicateBuilder : SearchPredicateBuilder
    {
        predicateBuilderDict[typeof(TQueryProvider)] = typeof(TSearchPredicateBuilder);
    }

    public static IQueryable<T> SearchByQueryString<T>(this IQueryable<T> queryable, Expression<Func<T, string>> propertyAccessor,
        string searchQuery, SearchQueryParser searchQueryParser)
    {
        SearchPredicateBuilder searchPredicateBuilder;
        var type = queryable.Provider.GetType()?.BaseType;
        if (type is null || !predicateBuilderDict.ContainsKey(type))
        {
            type = queryable.Provider.GetType();
        }
        if (predicateBuilderDict.TryGetValue(type, out var predicateBuilderType))
        {
            searchPredicateBuilder = (SearchPredicateBuilder)Activator.CreateInstance(predicateBuilderType, [searchQueryParser.TokenizerConfig])!;
        }
        else
        {
            throw new NotSupportedException($"The query provider {queryable.Provider.GetType()} is not supported.");
        }
        var predicate = searchPredicateBuilder.CreateExpression(searchQueryParser.ParseToPostfixTokens(searchQuery), propertyAccessor);
        return queryable.Where(predicate);
    }

    public static IQueryable<TSearchData> SearchByFilters<TSearchData, TSearchDataFilterCollection>(this IQueryable<TSearchData> queryable,
        IEnumerable<SearchFilter> searchFilters)
        where TSearchData : ISearchData<TSearchDataFilterCollection>
        where TSearchDataFilterCollection : IEnumerable<ISearchFilterData>
    {
        foreach (var filter in searchFilters)
        {
            if (filter.Values.Count == 1)
            {
                var filterValue = filter.Values.First();
                queryable = queryable.Where(x => x.SearchFilters.Any(y => y.Category == filter.Category && y.Value == filterValue));
            }
            else
            {
                queryable = queryable.Where(x => x.SearchFilters.Any(y => y.Category == filter.Category && filter.Values.Contains(y.Value)));
            }
        }
        return queryable;
    }

    public static IQueryable<TSearchData> Search<TSearchData, TSearchDataFilterCollection>(this IQueryable<TSearchData> queryable,
        SearchRequest searchRequest, SearchQueryParser searchQueryParser)
        where TSearchData : ISearchData<TSearchDataFilterCollection>
        where TSearchDataFilterCollection : IEnumerable<ISearchFilterData>
    {
        queryable = SearchByQueryString(queryable, x => x.FullText, searchRequest.SearchQuery, searchQueryParser);
        queryable = SearchByFilters<TSearchData, TSearchDataFilterCollection>(queryable, searchRequest.SearchFilters);
        return queryable;
    }

    public static IEnumerable<TSearchData> WeightedSearch<TSearchData, TSearchDataFilterCollection>(this IQueryable<TSearchData> queryable,
        SearchRequest searchRequest, SearchQueryParser searchQueryParser)
        where TSearchData : ISearchData<TSearchDataFilterCollection>
        where TSearchDataFilterCollection : IEnumerable<ISearchFilterData>
    {
        var query = Search<TSearchData, TSearchDataFilterCollection>(queryable, searchRequest, searchQueryParser);
        MethodInfo methodInfo = typeof(QueryableSearchExtensions).GetMethod(nameof(CountMatchesInFirstLine), BindingFlags.Static | BindingFlags.NonPublic)!;
        var postfixTokens = searchQueryParser.ParseToPostfixTokens(searchRequest.SearchQuery);
        var weightingFunctionBuilder = new LinqWeightingFunctionBuilder(searchQueryParser.TokenizerConfig);
        var weightingFunction = weightingFunctionBuilder.CreateExpression(postfixTokens, (TSearchData x) => x.FullText, methodInfo).Compile();
        return query
            .AsEnumerable()
            .Select(item => new { Item = item, Score = weightingFunction(item) })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item);
    }

    private static int CountMatchesInFirstLine(string str, string substring)
    {
        var lengthToInspect = str.IndexOfAny(['\r', '\n']);
        int count = 0, i = 0;
        while ((i = str.IndexOf(substring, i, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            if (i > lengthToInspect) { break; }
            i += substring.Length;
            count++;
        }
        return count;
    }

}
