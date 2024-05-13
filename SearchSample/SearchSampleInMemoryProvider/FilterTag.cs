using SearchSample.SearchProcessing;
using System;

namespace SearchSample.DataProvider;

public record class FilterTag(Guid FilterTypeUuid, string Value) : IFilterTag { }
