using SearchSample.SearchProcessing;

namespace SearchSample.DataProvider;

public record class SearchableData(Guid Uuid, string Text, int Weight = 1) : ISearchableData<IReadOnlyCollection<FilterTag>>
{
    public IReadOnlyCollection<FilterTag> FilterTags { get; init; } = [];
}
