﻿using SearchSample.SearchProcessing;
using System;

namespace SearchSampleApp.DbDataProvider;

public class FilterTagDo : ISearchFilterData
{
    public long Id { get; set; }
    public Guid ItemUuid { get; set; }
    public int FilterType { get; set; }
    public long Value { get; set; }
}
