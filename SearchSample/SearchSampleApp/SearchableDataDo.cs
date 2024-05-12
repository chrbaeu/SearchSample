using SearchSample.SearchProcessing;

namespace SearchSampleApp;

public class SearchableDataDo : ISearchableData<List<FilterTagDo>>
{
    public SearchableDataDo(Guid uuid, string text, int weight = 1)
    {
        Uuid = uuid;
        Text = text;
        Weight = weight;
    }

    public Guid Uuid { get; set; } = Guid.Empty;
    public string Text { get; set; } = "";
    public int Weight { get; set; }
    public virtual List<FilterTagDo> FilterTags { get; set; } = [];
}
