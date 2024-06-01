using System.Collections.Generic;
using System.Linq;

namespace SearchSample.QueryParser;

public class SearchQueryParser
{
    private readonly QueryStringTokenizer tokenizer;
    private readonly InfixToPostfixConverter infixToPostfixConverter;

    /// <summary>
    /// Gets the tokenizer configuration used by the query parser.
    /// </summary>
    public TokenizerConfig TokenizerConfig { get; }

    /// <summary>
    /// Gets the synonym handler used by the query parser.
    /// </summary>
    public SynonymHandler SynonymHandler { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchQueryParser"/> class.
    /// </summary>
    public SearchQueryParser() : this(new TokenizerConfig()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchQueryParser"/> class with the specified tokenizer configuration.
    /// </summary>
    /// <param name="config">The tokenizer configuration.</param>
    public SearchQueryParser(TokenizerConfig config)
    {
        tokenizer = new QueryStringTokenizer(config);
        infixToPostfixConverter = new InfixToPostfixConverter(config);
        TokenizerConfig = config;
        SynonymHandler = new SynonymHandler(config);
    }

    /// <summary>
    /// Parses the specified query string and returns a list of parsed tokens in postfix notation.
    /// </summary>
    /// <param name="query">The query string to parse.</param>
    /// <returns>A list of parsed tokens in postfix notation.</returns>
    public IList<string> ParseToPostfixTokens(string query)
    {
        return infixToPostfixConverter.InfixToPostfix(SynonymHandler.InsertSynonymConditions(tokenizer.GetTokens(query))).Select(x => x.ToLowerInvariant()).ToList();
    }

    public IList<string> GetSearchWords(string query)
    {
        List<string> searchTerms = [];
        foreach (var token in ParseToPostfixTokens(query))
        {
            if (token == TokenizerConfig.NotToken)
            {
                searchTerms.RemoveAt(searchTerms.Count - 1);
            }
            else if (token != TokenizerConfig.AndToken && token != TokenizerConfig.OrToken)
            {
                if (token[0] == TokenizerConfig.SegmentToken[0] && token[0] == token[^1])
                {
                    searchTerms.Add(token.Trim(TokenizerConfig.SegmentToken[0]));
                }
                else
                {
                    searchTerms.Add(token);
                }
            }
        }
        return searchTerms;
    }
}
