
namespace SearchSample.SearchProcessing;

public interface IFilterTag
{
    Guid Type { get; }
    string Value { get; }
}