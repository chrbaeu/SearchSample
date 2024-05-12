using Microsoft.EntityFrameworkCore;
using SearchSample.QueryProcessing;
using System.Reflection;

namespace SearchSample.SearchProcessing;

public class SearchService<TSearchableData, TFilterTagCollection>(TokenizerConfig config)
    where TSearchableData : ISearchableData<TFilterTagCollection>
    where TFilterTagCollection : IEnumerable<IFilterTag>
{

    private readonly QueryStringTokenizer tokenizer = new(config);
    private readonly InfixToPostfixConverter converter = new(config);
    private readonly PredicateBuilder predicateBuilder = new(config);
    private readonly WeightingFunctionBuilder weightingFunctionBuilder = new(config);

    public IList<Guid> Search(ISearchDataProvider<TSearchableData, TFilterTagCollection> searchDataProvider, SearchRequest searchRequest)
    {
        var tokens = tokenizer.GetTokens(searchRequest.SearchText);
        var postfix = converter.InfixToPostfix(tokens);
        var predicate = predicateBuilder.CreateExpression(postfix, (TSearchableData x) => x.Text);
        var query = searchDataProvider.GetQueryable()
            .Where(predicate);
        foreach (var filter in searchRequest.FilterTag)
        {
            query = query.Where(x => x.FilterTags.Any(y => y.Type == filter.Type && y.Value == filter.Value));
        }
        return query
            .OrderByDescending(x => x.Weight)
            .Select(x => x.Uuid)
            .ToList();
    }

    public IList<TSearchableData> WeightedSearch(ISearchDataProvider<TSearchableData, TFilterTagCollection> searchDataProvider, SearchRequest searchRequest)
    {
        var tokens = tokenizer.GetTokens(searchRequest.SearchText);
        var postfix = converter.InfixToPostfix(tokens);
        var predicate = predicateBuilder.CreateExpression(postfix, (TSearchableData x) => x.Text);
        MethodInfo methodInfo = typeof(WeightingHelper).GetMethod(nameof(WeightingHelper.CountInFirstLine))!;
        var weightingFunction = weightingFunctionBuilder.CreateExpression(postfix, (TSearchableData x) => x.Text, methodInfo).Compile();
        var query = searchDataProvider.GetQueryable()
            .Where(predicate);
        foreach (var filter in searchRequest.FilterTag)
        {
            query = query.Where(x => x.FilterTags.Any(y => y.Type == filter.Type && y.Value == filter.Value));
        }
        Console.WriteLine(query.ToQueryString());
        return query
            .AsEnumerable()
            .Select(item => new { Item = item, Score = weightingFunction(item) })
            .OrderByDescending(x => x.Item.Weight + x.Score)
            .Select(x => x.Item)
            .ToList();
    }

}
