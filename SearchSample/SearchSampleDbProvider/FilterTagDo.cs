using SearchSample.Interfaces;
using System;

namespace SearchSampleApp.DbDataProvider;

public class FilterTagDo : ISearchFilterData
{
    public long Id { get; set; }
    public Guid ItemUuid { get; set; }
    public string Category { get; set; } = "";
    public string Value { get; set; } = "";
}
