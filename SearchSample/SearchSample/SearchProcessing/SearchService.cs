using SearchSample.QueryProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SearchSample.SearchProcessing;

public class SearchService<TSearchableData, TFilterTagCollection>
    where TSearchableData : ISearchableData<TFilterTagCollection>
    where TFilterTagCollection : IEnumerable<IFilterTag>
{

    private readonly TokenizerConfig config;
    private readonly PredicateBuilder predicateBuilder;
    private readonly QueryStringTokenizer tokenizer;
    private readonly InfixToPostfixConverter converter;
    private readonly WeightingFunctionBuilder weightingFunctionBuilder;

    public SearchService(TokenizerConfig config, PredicateBuilder predicateBuilder)
    {
        this.config = config;
        this.predicateBuilder = predicateBuilder;
        tokenizer = new(config);
        converter = new(config);
        weightingFunctionBuilder = new(config);
    }

    public IList<Guid> Search(ISearchDataProvider<TSearchableData, TFilterTagCollection> searchDataProvider, SearchRequest searchRequest)
    {
        var tokens = tokenizer.GetTokens(searchRequest.SearchText);
        var postfix = converter.InfixToPostfix(tokens);
        var predicate = predicateBuilder.CreateExpression(postfix, (TSearchableData x) => x.FullText);
        var query = searchDataProvider.GetQueryable()
            .Where(predicate);
        query = ApplyFilters(searchRequest, query);
        return query
            .Select(x => x.ItemUuid)
            .ToList();
    }

    public IList<TSearchableData> WeightedSearch(ISearchDataProvider<TSearchableData, TFilterTagCollection> searchDataProvider, SearchRequest searchRequest)
    {
        var tokens = tokenizer.GetTokens(searchRequest.SearchText);
        var postfix = converter.InfixToPostfix(tokens);
        var predicate = predicateBuilder.CreateExpression(postfix, (TSearchableData x) => x.FullText);
        MethodInfo methodInfo = typeof(WeightingHelper).GetMethod(nameof(WeightingHelper.CountInFirstLine))!;
        var weightingFunction = weightingFunctionBuilder.CreateExpression(postfix, (TSearchableData x) => x.FullText, methodInfo).Compile();
        var query = searchDataProvider.GetQueryable()
            .Where(predicate);
        query = ApplyFilters(searchRequest, query);
        //Console.WriteLine(query.ToQueryString());
        return query
            .AsEnumerable()
            .Select(item => new { Item = item, Score = weightingFunction(item) })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item)
            .ToList();
    }

    private static IQueryable<TSearchableData> ApplyFilters(SearchRequest searchRequest, IQueryable<TSearchableData> query)
    {
        foreach (var filter in searchRequest.SearchFilters)
        {
            if (filter.Values.Count == 1)
            {
                var filterValue = filter.Values.First();
                query = query.Where(x => x.FilterTags.Any(y => y.FilterTypeUuid == filter.FilterTypeUuid && y.Value == filterValue));
            }
            else
            {
                if (filter.Values.Count == 1)
                {
                    var filterValue = filter.Values.First();
                    query = query.Where(x => x.FilterTags.Any(y => y.FilterTypeUuid == filter.FilterTypeUuid && y.Value == filterValue));
                }
                else
                {
                    query = query.Where(x => x.FilterTags.Any(y => y.FilterTypeUuid == filter.FilterTypeUuid && filter.Values.Contains(y.Value)));
                }
            }
        }
        return query;
    }
}
