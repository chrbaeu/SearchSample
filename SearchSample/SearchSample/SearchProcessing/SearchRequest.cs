namespace SearchSample.SearchProcessing;

public record class SearchRequest(string SearchText)
{
    public IReadOnlyCollection<IFilterTag> FilterTag { get; init; } = [];
}