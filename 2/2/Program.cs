
using _2;

public class Program
{
    public async static Task Main()
    {
        await new LemmatizeService().Lemmatize();
    }
}

