using SearchSample.QueryParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SearchSample.SearchProcessing;

public class SearchService<TSearchableData, TFilterTagCollection>
    where TSearchableData : ISearchData<TFilterTagCollection>
    where TFilterTagCollection : IEnumerable<ISearchFilterData>
{

    private readonly SearchQueryParser searchQueryParser;
    private readonly WeightingFunctionBuilder weightingFunctionBuilder;

    public SearchService(SearchQueryParser searchQueryParser)
    {
        this.searchQueryParser = searchQueryParser;
        weightingFunctionBuilder = new(searchQueryParser.TokenizerConfig);
    }

    public IList<Guid> Search(ISearchDataProvider<TSearchableData, TFilterTagCollection> searchDataProvider, SearchRequest searchRequest)
    {
        var query = searchDataProvider.GetQueryable()
            .WhereSearchRequestIsMatched<TSearchableData, TFilterTagCollection>(searchRequest, searchQueryParser)
            .Select(x => x.SourceUuid);
        //Console.WriteLine(selectQuery.ToQueryString());
        return query.ToList();
    }

    public IList<TSearchableData> WeightedSearch(ISearchDataProvider<TSearchableData, TFilterTagCollection> searchDataProvider, SearchRequest searchRequest)
    {
        MethodInfo methodInfo = typeof(WeightingHelper).GetMethod(nameof(WeightingHelper.CountInFirstLine))!;
        var postfixTokens = searchQueryParser.ParseToPostfixTokens(searchRequest.SearchQuery);
        var weightingFunction = weightingFunctionBuilder.CreateExpression(postfixTokens, (TSearchableData x) => x.FullText, methodInfo).Compile();
        var query = searchDataProvider.GetQueryable()
            .WhereSearchRequestIsMatched<TSearchableData, TFilterTagCollection>(searchRequest, searchQueryParser);
        //Console.WriteLine(query.ToQueryString());
        return query
            .AsEnumerable()
            .Select(item => new { Item = item, Score = weightingFunction(item) })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Item)
            .ToList();
    }

}
