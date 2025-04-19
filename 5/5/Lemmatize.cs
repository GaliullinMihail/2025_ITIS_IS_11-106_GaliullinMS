
using System.Diagnostics;
using System.Text;

namespace _5;

public static class Lemmatize
{
    private static readonly string MystemExePath =
        @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\2\2\mystem.exe";
    
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
    
    static Lemmatize()
    {
        StemProcess.Start();
    }
    
    public static async Task<IEnumerable<string>> LemmatizeTokens(IEnumerable<string> tokens)
    {
        List<string> outputTokens = [];
        foreach (var token in tokens)
        {
            if (StemProcess.HasExited)
                StemProcess.Start();

            await StemProcess.StandardInput.WriteLineAsync(token);
            var result = await StemProcess.StandardOutput.ReadLineAsync();
            var outputToken = result?.Replace("?", "") ?? token;
            outputTokens.Add(outputToken);
        }

        return outputTokens;
    }
}