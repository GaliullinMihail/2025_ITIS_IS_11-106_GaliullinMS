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

var tfidf = await TfIdf.CreateTermFrequencyInverseDocumentFrequency(idf, tf);

while (true)
{
    Console.WriteLine("Введите фразу:");
    
    //lemmatization
    var query = Console.ReadLine()!
        .Split(" ")
        .Select(x => x.ToLower());
    
    var lemmas = await Lemmatize.LemmatizeTokens(query);
    
    //find tokens in IDF, if didn't find -> remove from list
    var tokens = lemmas
        .Select(x => x.ToString())
        .Where(x => idf.ContainsKey(x))
        .ToList();

    //calculate TF for query
    var tfForQuery = TfService.CalculateTermFrequencyForQuery(tokens); 

    //calculate TF-IDF for query
    var tfIdfForQuery = idf
        .Select(kv => new KeyValuePair<string, decimal>(
            kv.Key, 
            Math.Round(kv.Value * tfForQuery.GetValueOrDefault(kv.Key, 0), 6)))
        .ToSortedDictionary(new NaturalSortComparerForModels());

    //calculate cosine similarity
    var cosineSimilarities = new List<(string doc, decimal Score)>();
    foreach (var (document, dictionary) in tfIdf)
    {
        var cosineSimilarity = Similarity
            .CalculateCosineSimilarity(
                dictionary.Values.ToList(),
                tfIdfForQuery.Values.ToList()
            ); 
        cosineSimilarities.Add((document, cosineSimilarity));
    }

    //sort by score
    var sorted = cosineSimilarities
        .OrderByDescending(x => x.Score)
        .GroupBy(x => x.Score)
        .Select(x =>
        {
            var docNames = x.Select(d => d.doc.Value);
            return $"{string.Join(", ", docNames)}: Score = {Math.Round(x.Key, 6)}";
        })
        .ToList();
    
    //write to console
    Console.WriteLine("\nРезультаты:");
    Console.WriteLine($"{string.Join(";\n", sorted)};\n");
    
}

Console.WriteLine("a");