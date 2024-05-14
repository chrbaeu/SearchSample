using SearchSample.QueryParser;
using SearchSample.QueryProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

    public static IQueryable<TSearchData> WhereSearchRequestIsMatched<TSearchData, TFilterDataCollection>(this IQueryable<TSearchData> queryable,
        SearchRequest searchRequest, SearchQueryParser searchQueryParser)
        where TSearchData : ISearchData<TFilterDataCollection>
        where TFilterDataCollection : IEnumerable<ISearchFilterData>
    {
        queryable = WhereSearchQueryIsMatched(queryable, x => x.FullText, searchRequest.SearchQuery, searchQueryParser);
        queryable = WhereSearchFiltersAreMatched<TSearchData, TFilterDataCollection>(queryable, searchRequest.SearchFilters);
        return queryable;
    }

    public static IQueryable<T> WhereSearchQueryIsMatched<T>(this IQueryable<T> queryable, Expression<Func<T, string>> propertyAccessor,
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

    public static IQueryable<TSearchData> WhereSearchFiltersAreMatched<TSearchData, TFilterDataCollection>(this IQueryable<TSearchData> queryable,
        IEnumerable<SearchFilter> searchFilters)
        where TSearchData : ISearchData<TFilterDataCollection>
        where TFilterDataCollection : IEnumerable<ISearchFilterData>
    {
        foreach (var filter in searchFilters)
        {
            if (filter.Values.Count == 1)
            {
                var filterValue = filter.Values.First();
                queryable = queryable.Where(x => x.FilterTags.Any(y => y.FilterType == filter.FilterType && y.Value == filterValue));
            }
            else
            {
                queryable = queryable.Where(x => x.FilterTags.Any(y => y.FilterType == filter.FilterType && filter.Values.Contains(y.Value)));
            }
        }
        return queryable;
    }

}
