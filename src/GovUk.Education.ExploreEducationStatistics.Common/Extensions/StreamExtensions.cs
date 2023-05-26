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

        public static byte[] ReadFully(this Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static string ReadToEnd(this Stream stream, bool leaveOpen = false)
        {
            using var reader = new StreamReader(stream, leaveOpen);
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

        public static void SeekToBeginning(this Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(offset: 0, origin: SeekOrigin.Begin);
            }
        }
    }
}
