#nullable enable
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class StreamExtensions
    {
        public static void WriteText(this Stream stream, string text)
        {
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.Write(text);
            writer.Flush();
        }

        public static string ReadToEnd(this Stream stream)
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return reader.ReadToEnd();
        }
    }
}