using System.Text;
using _3;

var path = @"D:\University\Information_Search\git\2025_ITIS_IS_11-106_GaliullinMS\3\3\BooleanSearch\BooleanSearch.txt";

var word1 = "абсолютно";
var word2 = "август";
var word3 = "августин";

var sb = new StringBuilder();

var inputs = new List<string>
{
    $"{word1} & {word2} | {word3}",
    $"{word1} | {word2} | {word3}",
    $"{word1} & {word2} & {word3}", 
    $"{word1} & !{word2}| !{word3}",
    $"{word1} | !{word2} | !{word3}",
    $"{word1}",
    $"{word2}",
    $"{word3}",
    $"{word1} & {word2}",
    $"{word1} & !{word2}",
    $"{word1} & {word2} & !{word3}",
};

var index = new SortedDictionary<string, SortedSet<(int, int)>>();

if (!InvertedIndex.TryGetIndex(out index))
{
    index = await InvertedIndex.Build();
}

foreach (var input in inputs)
{
    var check = (await BooleanSearch.Search(index, input)).ToList();
    sb.Append(input + ": ");
    sb.AppendLine(String.Join(',', check));
    sb.AppendLine();
}

File.WriteAllText(path, sb.ToString());