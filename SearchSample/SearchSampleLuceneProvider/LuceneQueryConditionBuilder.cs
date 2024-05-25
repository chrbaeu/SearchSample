﻿using Lucene.Net.Index;
using Lucene.Net.Search;
using SearchSample.QueryParser;
using System.Collections.Generic;

namespace SearchSampleLuceneProvider;

public class LuceneQueryConditionBuilder
{
    private readonly TokenizerConfig config;
    private readonly List<(string FieldName, float boost)> matchFields;

    public LuceneQueryConditionBuilder(TokenizerConfig config, List<(string FieldName, float boost)> matchFields)
    {
        this.config = config;
        this.matchFields = matchFields;
    }

    public Query ConvertToLuceneQuery(IEnumerable<string> postfixTokens)
    {
        Stack<Query> stack = new();
        foreach (var token in postfixTokens)
        {
            if (config.IsOperator(token))
            {
                if (token == config.NotToken)
                {
                    Query operand = stack.Pop();
                    var booleanQuery = new BooleanQuery
                    {
                        { operand, Occur.MUST_NOT }
                    };
                    stack.Push(booleanQuery);
                }
                else
                {
                    Query right = stack.Pop();
                    Query left = stack.Pop();
                    var booleanQuery = new BooleanQuery();
                    if (token == config.AndToken)
                    {
                        booleanQuery.Add(left, Occur.MUST);
                        booleanQuery.Add(right, Occur.MUST);
                    }
                    else
                    {
                        booleanQuery.Add(left, Occur.SHOULD);
                        booleanQuery.Add(right, Occur.SHOULD);
                    }
                    stack.Push(booleanQuery);
                }
            }
            else
            {
                var booleanQuery = new BooleanQuery();
                foreach (var (fieldName, boost) in matchFields)
                {
                    var fieldQuery = new WildcardQuery(new Term(fieldName, $"*{token}*")) { Boost = boost };
                    booleanQuery.Add(fieldQuery, Occur.SHOULD);
                }
                stack.Push(booleanQuery);
            }
        }
        return stack.Pop();
    }

}
