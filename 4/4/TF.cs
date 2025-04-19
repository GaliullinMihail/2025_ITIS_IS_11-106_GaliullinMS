using System.Text;

namespace _4;

public static class TF
{
    private static readonly string PagesInputPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\tokens";

    private static readonly string TFPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\4\4\TF\TF.csv";
    
    private static Dictionary<int, string[]> Contents = new Dictionary<int, string[]>();
    
    private static readonly Dictionary<string, Dictionary<string, decimal>> Tf = new ();
    
    public static Dictionary<string, decimal> CalculateForQuery(IEnumerable<string> tokens)
    {
        var list = tokens.ToList();
        var dict = list
            .GroupBy(x => x)
            .ToDictionary(
                kv => kv.Key,
                kv => Math.Round((decimal) kv.Count() / list.Count, 6));
        
        return dict;
    }
    
    public static async Task<Dictionary<string, Dictionary<string, decimal>>> CreateTermFrequency()
    {
        var allWords = new HashSet<string>();
        var txtFiles = new string[100];
        //прочитать content
        for (int i = 1; i < 101; i++)
        {
            var words = (await File.ReadAllTextAsync($@"{PagesInputPath}\{i}.txt")).Split(' ');
            Contents.Add(i, words);
            allWords.UnionWith(words);
            txtFiles[i - 1] = $"{i}.txt";
        }
        
        foreach (var word in allWords)
        {
            if (Tf.ContainsKey(word))
                continue;
            
            Tf.Add(word, new Dictionary<string, decimal>());
            
            for (int j = 1; j < 101; j++)
            {
                var frequency = Math.Round((decimal) Contents[j].Where(x => x.Equals(word)).Count() / Contents[j].Length, 6);
                Tf[word].Add($"{j}.txt", frequency);
            }
        }
        
        await CsvSaver.SaveCsv(TFPath, Tf, txtFiles);
        return Tf;
    }

    public static bool TryGetTermFrequency(out Dictionary<string, Dictionary<string, decimal>> tf)
    {
        tf = new Dictionary<string, Dictionary<string, decimal>>();

        if (!File.Exists(TFPath))
        {
            return false;
        }

        
        var file = File.ReadAllLines(TFPath);

        var documents = file[0].Split(';')[1..];

        foreach (var line in file.Skip(1))
        {
            var content = line.Split(';').ToList();
            
            var word = content[0];
            
            var values = content.Skip(1).Select(decimal.Parse).Zip(documents);

            var frequencies = values.ToDictionary(
                kv => kv.Second,
                kv => kv.First);
            
            tf.TryAdd(word, frequencies);
        }

        return true;
    }
}