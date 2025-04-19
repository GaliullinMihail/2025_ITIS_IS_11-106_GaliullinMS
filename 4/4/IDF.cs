using System.Text;

namespace _4;

public static class IDF
{
       private static readonly string PagesInputPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\tokens";

    private static readonly string IdfPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\4\4\IDF\IDF.csv";
    
    
    private static readonly Dictionary<string, decimal> Idf = new ();
    
    public static async Task<Dictionary<string, decimal>> CreateInverseDocumentFrequency(SortedDictionary<string, SortedSet<(int,int)>> dictionary)
    {
        var countFiles = Directory.GetFiles(PagesInputPath).Length;
        
        var allWords = new HashSet<string>();
        
        for (int i = 1; i < 101; i++)
        {
            var words = (await File.ReadAllTextAsync($@"{PagesInputPath}\{i}.txt")).Split(' ');
            allWords.UnionWith(words);
        }
        
        foreach (var word in allWords)
        {
            var idf = Math.Round((decimal)Math.Log10(((double) countFiles) / dictionary[word].Count), 6);
            Idf.TryAdd(word, idf);
        }
        
        await CsvSaver.SaveCsv(IdfPath, Idf);
        
        return Idf;
    }

    public static bool TryInverseDocumentFrequency(out Dictionary<string, decimal> idf)
    {
        idf = new Dictionary<string, decimal>();
        
        if (!File.Exists(IdfPath))
        {
            return false;
        }
        
        var file = File.ReadAllLines(IdfPath);
    
        var dictinary = file
            .Skip(1)
            .Select(x => x.Split(';'))
            .ToDictionary(
                kv => kv[0],
                kv => decimal.Parse(kv[1]));
        idf = dictinary;
        
        return true;
    }
}