using System.Collections.Generic;

namespace SearchSample.QueryProcessing;

public class SqlQueryConditionBuilder(TokenizerConfig config)
{

    public string ConvertToSql(List<string> postfixTokens, string columnName)
    {
        Stack<string> stack = new();
        foreach (var token in postfixTokens)
        {
            if (config.IsOperator(token))
            {
                if (token == config.NotToken)
                {
                    string operand = stack.Pop();
                    stack.Push($"NOT ({operand})");
                }
                else
                {
                    string right = stack.Pop();
                    string left = stack.Pop();
                    string op = token == config.AndToken ? "AND" : "OR";
                    stack.Push($"({left} {op} {right})");
                }
            }
            else
            {
                stack.Push($"{columnName} LIKE '%{token}%'");
            }
        }
        return stack.Pop();
    }

}
