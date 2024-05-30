using Lucene.Net.Index;
using Lucene.Net.Search;
using SearchSample.QueryParser;
using System.Collections.Generic;
using System.Linq;

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

    public Query? ConvertToLuceneQuery(IEnumerable<string> postfixTokens)
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
                        { new MatchAllDocsQuery(), Occur.SHOULD },
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
                    Query fieldQuery;
                    if (token[0] == config.SegmentToken[0] && token[0] == token[^1])
                    {
                        fieldQuery = new TermQuery(new Term(fieldName, token.Trim(config.SegmentToken[0]))) { Boost = boost };
                    }
                    else if (token[0] == '~' && token[0] == token[^1])
                    {
                        fieldQuery = new FuzzyQuery(new Term(fieldName, token.Trim('~')), token.Count(x => x == '~') - 1) { Boost = boost / 2 };
                    }
                    else
                    {
                        fieldQuery = new WildcardQuery(new Term(fieldName, $"*{token}*")) { Boost = boost };
                    }
                    booleanQuery.Add(fieldQuery, Occur.SHOULD);
                }
                stack.Push(booleanQuery);
            }
        }
        if (stack.Count == 0) { return null; }
        return stack.Pop();
    }

}
