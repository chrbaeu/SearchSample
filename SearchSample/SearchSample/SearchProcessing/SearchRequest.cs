using System.Collections.Generic;

namespace SearchSample.SearchProcessing;

public record class SearchRequest(string SearchText)
{
    public IReadOnlyCollection<SearchFilter> SearchFilters { get; init; } = [];
}
