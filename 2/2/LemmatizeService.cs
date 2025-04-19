using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace _2;

public class LemmatizeService
{
    private static readonly string MystemExePath = @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\mystem.exe";

    private static readonly string PagesTemporaryOutputPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\tmp";
    
    private static readonly string PagesOutputPath ="D:\\University\\Information_Search\\git\\2025_ITIS_IS_11-106_GaliullinMS\\2\\2\\tokens";

    private static readonly string PagesInputPath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\1\pages";

    private static readonly string StopWordsPath = @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\stopwords.txt";

    public async Task Lemmatize()
    {
        if (!File.Exists(MystemExePath))
        {
            Console.WriteLine("Stemfile not exists");
            return;
        }

        if (!File.Exists(StopWordsPath))
        {
            Console.WriteLine("Stop Words not exists");
            return;
        }

        if (!Directory.Exists(PagesTemporaryOutputPath) || !Directory.Exists(PagesInputPath))
        {
            Console.WriteLine("Input/Output directories not exist");
            return;
        }
        
        LemmatizeTemporary();
        
        var stopWords = new HashSet<string>(await File.ReadAllLinesAsync(StopWordsPath));
        await Filter(stopWords);
    }

    private static void LemmatizeTemporary()
    {
        for (var page = 1; page <= 100; page++)
        {
            var inputFilePath = $@"{PagesInputPath}\{page}.txt";
            if (!File.Exists(inputFilePath))
            {
                return;
            }
            
            var outputFilePath = $@"{PagesTemporaryOutputPath}\{page}.txt";
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            using var process = new Process();
            
            process.StartInfo = new ProcessStartInfo
            {
                FileName = MystemExePath,
                Arguments = $"-nl {inputFilePath} {outputFilePath}"
            };;

            process.Start();
            process.WaitForExit();
        }
    }

    public async Task Filter(HashSet<string> stopWords)
    {
        for (var page = 1; page <= 100; page++)
        {
            var inputFilePath = $@"{PagesTemporaryOutputPath}\{page}.txt";
            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($@"Input file not exists {page}.txt");
                continue;
            }
            
            var outputFilePath = $@"{PagesOutputPath}\{page}.txt";
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            var inputContent = await File.ReadAllLinesAsync(inputFilePath);
            
            var filteredContent = inputContent
                .Where(line => !stopWords.Contains(line))
                .Select(x => x.Replace("?", string.Empty).Replace('\n', ' ').Split('|').First());
            
            await File.WriteAllTextAsync(outputFilePath, String.Join(" ",filteredContent), encoding: Encoding.UTF8);
        }
    }
    

}