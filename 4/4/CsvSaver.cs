using System.Text;

namespace _4;

public static class CsvSaver
{
    public static async Task SaveCsv(string path, Dictionary<string, Dictionary<string, decimal>> dictionary, IEnumerable<string> fileNames)
    {
        if(File.Exists(path))
            return;

        var sorted = fileNames
            .Select(Path.GetFileName);
        
        List<string> lines = [$"terms;{string.Join(";", sorted)}"];

        lines.AddRange(dictionary.Select(kv => $"{kv.Key};{string.Join(";", kv.Value.Values)}"));

        await File.WriteAllLinesAsync(path, lines, Encoding.UTF8);
    }
    
    public static async Task SaveCsv(string path, Dictionary<string, decimal> dictionary)
    {
        if(File.Exists(path))
            return;

        List<string> lines = ["terms;values"];
        lines.AddRange(dictionary.Select(kv => $"{kv.Key};{kv.Value}"));

        await File.WriteAllLinesAsync(path, lines, Encoding.UTF8);
        Console.WriteLine($"Документ сохранен по пути: {path}");
    }
}