using System.Diagnostics;
using System.Text;

namespace _3;

public class BooleanSearch
{
    private static readonly string MystemExePath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\mystem.exe";
    
    private static IEnumerable<int> files;
    
    private static readonly Process StemProcess = new Process()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = MystemExePath,
            Arguments = "-nld",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardInputEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8
        }
    };
    
    static BooleanSearch()
    {
        StemProcess.Start();
        files = new List<int>();
        for (var i = 0; i < 101; i++)
        {
            files.Append(i);
        }
    }
    
    public static async Task<IEnumerable<int>> Search(SortedDictionary<string, SortedSet<(int, int)>> dictionary, string input)
    {
        var tokenizedInput = TokenizeInput(input);
        var orderInput = GetPriorities(tokenizedInput);
        var docs = await SearchDocs(orderInput, dictionary);
        return docs;
    }
    
    private static async Task<IEnumerable<int>> SearchDocs(IEnumerable<string> tokens, SortedDictionary<string, SortedSet<(int, int)>> dictionary)
    {
        var stack = new Stack<IEnumerable<int>>();
        
        foreach (var token in tokens)
        {
            switch (token)
            {
                case "&":
                {
                    stack.Push(stack.Pop().Intersect(stack.Pop()));
                    break;
                }
                case "|":
                {
                    stack.Push(stack.Pop().Union(stack.Pop()));
                    break;
                }
                case "!":
                {
                    stack.Push(files.Except(stack.Pop()));
                    break;
                }
                default:
                {
                    var constant = await Constant(token, dictionary);
                    stack.Push(constant);
                    break;
                }
            }
        }

        return stack.Pop();
    }
    
    private static List<string> GetPriorities(IEnumerable<string> data)
    {
        var output = new List<string>();
        var stack = new Stack<string>();
        var priorities = new Dictionary<string, int>
        {
            ["!"] = 3,
            ["&"] = 2,
            ["|"] = 1
        };

        foreach (var token in data)
        {
            if (token is "&" or "|" or "!")
            {
                while (stack.Count > 0 &&
                       priorities.TryGetValue(stack.Peek(), out var priority) &&
                       priority >= priorities[token])
                    output.Add(stack.Pop());
                stack.Push(token);
            }
            else 
                output.Add(token);
        }

        while (stack.Count > 0)
            output.Add(stack.Pop());

        return output;
    }
    
    private static string[] TokenizeInput(string input)
    {
        var filtered = input
            .Replace("ИЛИ", " | ")
            .Replace("И", " & ")
            .Replace("!", " ! ")
            .Replace("НЕ", " ! ")
            .ToLower();
        return filtered.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    }
    
    private static async Task<string?> LemmatizeInput(string input)
    {
        if (StemProcess.HasExited)
            StemProcess.Start();
        
        await StemProcess.StandardInput.WriteLineAsync(input);
        
        return await StemProcess.StandardOutput.ReadLineAsync();
    }
    
    private static async Task<IEnumerable<int>> Constant(string token, SortedDictionary<string, SortedSet<(int, int)>> dictionary)
    {
        var input = await LemmatizeInput(token);
        if (input == null)
            return null;
        return !dictionary.TryGetValue(input, out var value) ? [] : value.Select(x => x.Item1);
    }
    
}