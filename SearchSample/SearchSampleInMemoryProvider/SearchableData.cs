using SearchSample.SearchProcessing;
using System;
using System.Collections.Generic;

namespace SearchSample.DataProvider;

public record class SearchableData(Guid ItemUuid, string FullText) : ISearchableData<IReadOnlyCollection<FilterTag>>
{
    public IReadOnlyCollection<FilterTag> FilterTags { get; init; } = [];
}
