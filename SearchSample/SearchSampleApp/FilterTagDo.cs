using SearchSample.SearchProcessing;

namespace SearchSampleApp;

public class FilterTagDo : IFilterTag
{
    public long Id { get; set; }
    public Guid SearchableDataUuid { get; set; }
    public Guid Type { get; set; }
    public string Value { get; set; } = "";
}
