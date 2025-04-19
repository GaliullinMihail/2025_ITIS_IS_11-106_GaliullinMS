using System.Text;

namespace _3;

public static class InvertedIndex
{
    private static readonly string PagesInputPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\tokens";

    private static Dictionary<int, string[]> Contents = new Dictionary<int, string[]>();

    private static readonly string IndexPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\3\3\InvertedIndex\index.txt";
        
    public static async Task<SortedDictionary<string, SortedSet<(int,int)>>> Build()
    {
        if (!Directory.Exists(PagesInputPath))
        {
            Console.WriteLine("Directory doesn't exist!");
            return null;
        }
        
        for (int i = 1; i < 101; i++)
        {
            Contents.Add(i, (await File.ReadAllTextAsync($@"{PagesInputPath}\{i}.txt")).Split(' '));
        }
        
        var indexDictionary = new SortedDictionary<string, SortedSet<(int, int)>>();

        for (int i = 1; i < 101; i++)
        {
            foreach (var word in Contents[i])
            {
                if(indexDictionary.ContainsKey(word))
                    continue;
                
                for (var j = 1; j < 101; j++)
                {
                    var idxPos = GetPosition(word, Contents[j]);

                    if (idxPos == -1)
                        continue;
                    
                    if (!indexDictionary.ContainsKey(word))
                    {
                        indexDictionary.Add(word, new SortedSet<(int, int)>());
                    }
                    
                    indexDictionary[word].Add((j, idxPos));
                }
            }
        }
        
        SaveIndex(indexDictionary);

        return indexDictionary;

    }


    private static int GetPosition(string word, string[] content)
    {
        return Array.IndexOf(content, word);
    }

    private static void SaveIndex(SortedDictionary<string, SortedSet<(int, int)>> index)
    {
        if(File.Exists(IndexPath))
            return;
        
        StringBuilder sb = new StringBuilder();

        foreach (var key in index.Keys)
        {
            sb.Append($"{key} : {String.Join(',', index[key])}");
            sb.AppendLine();
        }
        
        
        File.WriteAllText(IndexPath, sb.ToString());
    }

    public static bool TryGetIndex(out SortedDictionary<string, SortedSet<(int, int)>> index)
    {
        index = new SortedDictionary<string, SortedSet<(int, int)>>();
        if (!File.Exists(IndexPath))
        {
            return false;
        }
        
        var file = File.ReadAllLines(IndexPath);

        foreach (var line in file)
        {
            var colonIndex = line.IndexOf(':');
            
            string word = line.Substring(0, colonIndex).Trim();
            
            string numbersPart = line.Substring(colonIndex + 1).Trim();
            
            var numbers = new SortedSet<(int, int)>();
            var numberPairs = numbersPart.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => s.Contains(','))
                .Select(s => s.Split(','))
                .Where(parts => parts.Length == 2);
            
            foreach (var pair in numberPairs)
            {
                if (int.TryParse(pair[0].Trim(), out int firstNumber) && int.TryParse(pair[1].Trim(), out int secondNumber))
                {
                    numbers.Add((firstNumber, secondNumber));
                }
            }
            
            if (numbers.Count > 0)
            {
                index[word] = numbers;
            }
        }

        return true;
    }
}