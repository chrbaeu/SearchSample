using System.Collections.Generic;

namespace SearchSample.RequestModels;

public record class SearchFilter
{
    public SearchFilter() { }

    public SearchFilter(string category, string value)
    {
        Category = category;
        Values = [value];
    }

    public SearchFilter(string category, List<string> values)
    {
        Category = category;
        Values = values;
    }

    public string Category { get; init; } = "";
    public IReadOnlyCollection<string> Values { get; init; } = [];
}
