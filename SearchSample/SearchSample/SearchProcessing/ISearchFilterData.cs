namespace SearchSample.SearchProcessing;

public interface ISearchFilterData
{
    int FilterType { get; }
    long Value { get; }
}
