namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;

public static class StreamExtensions
{
    public static Stream ToStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
        return stream;
    }
}
