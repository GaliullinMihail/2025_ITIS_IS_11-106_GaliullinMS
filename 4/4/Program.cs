// See https://aka.ms/new-console-template for more information

using _3;
using _4;

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
