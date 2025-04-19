namespace _4;

public class TfIdf
{
    
    private static Dictionary<string, Dictionary<string, decimal>> tfIdfDictionary = new ();
    
    private static readonly string PagesInputPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\tokens";
    
    private static readonly string TfIdfPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\4\4\TFIDF\TFIDF.csv";
    
    public static async Task<Dictionary<string, Dictionary<string, decimal>>> CreateTermFrequencyInverseDocumentFrequency(Dictionary<string, decimal> idf,
        Dictionary<string, Dictionary<string, decimal>> tf)
    {
        foreach (var kv in tf)
        {
            var frequency = idf[kv.Key];
            var tfIdf = kv.Value.ToDictionary(
                x => x.Key,
                x => Math.Round(x.Value * frequency, 6));

            tfIdfDictionary.TryAdd(kv.Key, tfIdf);
        }
        var txtFiles = new string[100];
        for (int i = 1; i < 101; i++)
        {
            txtFiles[i - 1] = $"{i}.txt";
        }
        
        await CsvSaver.SaveCsv(TfIdfPath, tfIdfDictionary, txtFiles);

        return tfIdfDictionary;
    }
}