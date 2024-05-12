namespace SearchSample.SearchProcessing;

public static class WeightingHelper
{

    public static int CountInFirstLine(string str, string substring)
    {
        var lengthToInspect = str.IndexOfAny(['\r', '\n']);
        int count = 0;
        int i = 0;
        while ((i = str.IndexOf(substring, i, StringComparison.Ordinal)) != -1)
        {
            if (i > lengthToInspect)
            {
                break;
            }
            i += substring.Length;
            count++;
        }
        return count;
    }

    public static int CountOccurrences(string str, string substring)
    {
        int count = 0;
        int i = 0;
        while ((i = str.IndexOf(substring, i, StringComparison.Ordinal)) != -1)
        {
            i += substring.Length;
            count++;
        }
        return count;
    }

}
