
namespace SearchSample.SearchProcessing;

public interface ISearchableData<TFilterTagCollection> where TFilterTagCollection : IEnumerable<IFilterTag>
{
    Guid Uuid { get; }
    string Text { get; }
    int Weight { get; }
    TFilterTagCollection FilterTags { get; }
}