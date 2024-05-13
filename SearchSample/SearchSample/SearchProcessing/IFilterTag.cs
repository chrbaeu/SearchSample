
using System;

namespace SearchSample.SearchProcessing;

public interface IFilterTag
{
    Guid FilterTypeUuid { get; }
    string Value { get; }
}
