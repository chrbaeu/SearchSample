using System;
using System.Collections.Generic;

namespace SearchSample.QueryParser;

public class SynonymHandler(TokenizerConfig config)
{

    private readonly Dictionary<string, HashSet<string>> synonymDict = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds a synonym for a word.
    /// </summary>
    /// <param name="word">The word to add a synonym for.</param>
    /// <param name="synonym">The synonym for the word.</param>
    public void AddSynonym(string word, string synonym)
    {
        SetSynonym(word, synonym);
        SetSynonym(synonym, word);

        void SetSynonym(string word, string synonym)
        {
            if (!synonymDict.TryGetValue(word, out var synonyms))
            {
                synonymDict[word] = synonyms = new(StringComparer.OrdinalIgnoreCase);
            }
            synonyms.Add(synonym);
        }
    }

    /// <summary>
    /// Inserts synonym conditions into the tokens.
    /// </summary>
    /// <param name="tokens">The tokens to insert synonym conditions into.</param>
    /// <returns>The tokens with synonym conditions inserted.</returns>
    public IEnumerable<string> InsertSynonymConditions(IEnumerable<string> tokens)
    {
        foreach (var token in tokens)
        {
            if (synonymDict.TryGetValue(token, out var synonyms))
            {
                yield return config.OpeningBracketToken;
                yield return token;
                foreach (var synonym in synonyms)
                {
                    yield return config.OrToken;
                    yield return synonym;
                }
                yield return config.ClosingBracketToken;
            }
            else
            {
                yield return token;
            }
        }
    }

}
