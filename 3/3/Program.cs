using System.Text;
using _3;

var path = @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\3\3\BooleanSearch\BooleanSearch.md";

var word1 = "абзац";
var word2 = "педиатрия";
var word3 = "переговоры";

var sb = new StringBuilder();

var inputs = new List<string>
{
    $"{word1} & {word2} | {word3}",
    $"{word1} | {word2} | {word3}",
    $"{word1} & {word2} & {word3}",
    $"{word1} & !{word2}| !{word3}",
    $"{word1} | !{word2} | !{word3}"
};

var index = await InvertedIndex.Build();

foreach (var input in inputs)
{
    var check = await BooleanSearch.Search(index, input);
    sb.Append(input + ": ");
    sb.AppendLine(String.Join(',', check));
    sb.AppendLine();
}

File.WriteAllText(path, sb.ToString());