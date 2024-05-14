using System.Collections.Generic;

namespace SearchSample.RequestModels;

public record class SearchRequest
{
    public SearchRequest() { }

    public SearchRequest(string searchQuery)
    {
        SearchQuery = searchQuery;
    }

    public string SearchQuery { get; init; } = "";
    public IReadOnlyCollection<SearchFilter> SearchFilters { get; init; } = [];
}
