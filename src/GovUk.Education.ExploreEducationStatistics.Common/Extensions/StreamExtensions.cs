#nullable enable
using System;
using System.IO;
using System.Security.Cryptography;

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

        public static string ComputeMd5Hash(this Stream stream)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return BitConverter.ToString(hash)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}