namespace SearchSample.SearchProcessing;

public interface ISearchFilter
{
    int FilterType { get; }
    long Value { get; }
}
