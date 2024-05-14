using SearchSample.SearchProcessing;

namespace SearchSample.DataProvider;

public record class FilterTag(int FilterType, long Value) : ISearchFilter { }
