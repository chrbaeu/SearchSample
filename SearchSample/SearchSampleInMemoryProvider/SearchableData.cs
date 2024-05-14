using SearchSample.SearchProcessing;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchSample.DataProvider;

public record class SearchableData(Guid SourceUuid, string FullText) : ISearchData<IReadOnlyCollection<FilterTag>>
{
    [JsonIgnore]
    public IReadOnlyCollection<FilterTag> FilterTags { get; init; } = [];
}
