using _3;
using _4;
using _5;

var index = new SortedDictionary<string, SortedSet<(int, int)>>();

if (!InvertedIndex.TryGetIndex(out index))
{
    index = await InvertedIndex.Build();
}

var tf = new Dictionary<string, Dictionary<string, decimal>>();
if (!TF.TryGetTermFrequency(out tf))
{
    tf = await TF.CreateTermFrequency();    
}

var idf = new Dictionary<string, decimal>();

if (!IDF.TryInverseDocumentFrequency(out idf))
{
    idf = await IDF.CreateInverseDocumentFrequency(index);
}

var tfIdf = await TfIdf.CreateTermFrequencyInverseDocumentFrequency(idf, tf);

var tranformedTfIdf = new Dictionary<string, Dictionary<string, decimal>>();

foreach (var kv in tfIdf)
{
    var outerKey = kv.Key;
    var innerDict = kv.Value;
    

    foreach (var innerPair in innerDict)
    {
        var innerKey = innerPair.Key;
        var value = innerPair.Value;

        if (!tranformedTfIdf.ContainsKey(innerKey))
        {
            tranformedTfIdf[innerKey] = new Dictionary<string, decimal>();
        }

        tranformedTfIdf[innerKey][outerKey] = value;
    }
}

while (true)
{
    Console.WriteLine("Reading....");
    var query = Console.ReadLine()!
        .Split(" ")
        .Select(x => x.ToLower());
    
    var lemmas = await Lemmatize.LemmatizeTokens(query);
    
    var tokens = lemmas
        .Select(x => x.ToString())
        .Where(x => idf.ContainsKey(x))
        .ToList();
    
    var tfForQuery = TF.CalculateForQuery(tokens); 
    
    var tfIdfForQuery = idf
        .Select(kv => new KeyValuePair<string, decimal>(
            kv.Key, 
            Math.Round(kv.Value * tfForQuery.GetValueOrDefault(kv.Key, 0), 6)))
        .ToDictionary();

    //calculate cosine similarity
    var cosineSimilarities = new List<(string doc, decimal Score)>();
    foreach (var (word, dictionary) in tranformedTfIdf)
    {
        var cosineSimilarity = Similarity
            .CalculateScore(
                dictionary.Values.ToList(),
                tfIdfForQuery.Values.ToList()
            ); 
        cosineSimilarities.Add((word, cosineSimilarity));
    }

    //sort by score
    var sorted = cosineSimilarities
        .OrderByDescending(x => x.Score)
        .GroupBy(x => x.Score)
        .Select(x =>
        {
            var docNames = x.Select(d => d.doc);
            return $"{string.Join(", ", docNames)}: Score = {Math.Round(x.Key, 6)}";
        })
        .ToList();
    
    //write to console
    Console.WriteLine("\nРезультаты:");
    Console.WriteLine($"{string.Join(";\n", sorted)};\n");
    
}

Console.WriteLine("a");