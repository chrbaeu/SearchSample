using SearchSample.SearchProcessing;

namespace SearchSample.DataProvider;

public record class FilterTag(Guid Type, string Value) : IFilterTag { }
