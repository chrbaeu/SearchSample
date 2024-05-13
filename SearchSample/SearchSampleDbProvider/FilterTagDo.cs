using SearchSample.SearchProcessing;
using System;

namespace SearchSampleApp.DbDataProvider;

public class FilterTagDo : IFilterTag
{
    public long Id { get; set; }
    public Guid ItemUuid { get; set; }
    public Guid FilterTypeUuid { get; set; }
    public string Value { get; set; } = "";
}
