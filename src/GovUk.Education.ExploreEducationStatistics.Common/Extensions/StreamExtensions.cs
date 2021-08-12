#nullable enable
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class StreamExtensions
    {
        public static void WriteText(this Stream stream, string text)
        {
            using var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
        }

        public static string ReadToEnd(this Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}