using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public static class MimeTypes
    {
        private static readonly List<FileType> types;

        static MimeTypes()
        {
            types = new List<FileType> {ZIP, TXT_UTF8, TXT_UTF16_BE, TXT_UTF16_LE, TXT_UTF32_BE, TXT_UTF32_LE};
        }

        private static readonly FileType TXT_UTF8 = new FileType(new byte?[] { 0xEF, 0xBB, 0xBF }, "txt", "text/plain");
        private static readonly FileType TXT_UTF16_BE = new FileType(new byte?[] { 0xFE, 0xFF }, "txt", "text/plain");
        private static readonly FileType TXT_UTF16_LE = new FileType(new byte?[] { 0xFF, 0xFE }, "txt", "text/plain");
        private static readonly FileType TXT_UTF32_BE = new FileType(new byte?[] { 0x00, 0x00, 0xFE, 0xFF }, "txt", "text/plain");
        private static readonly FileType TXT_UTF32_LE = new FileType(new byte?[] { 0xFF, 0xFE, 0x00, 0x00 }, "txt", "text/plain");
        private static readonly FileType ZIP = new FileType(new byte?[] { 0x50, 0x4B, 0x03, 0x04 }, "zip", "application/x-compressed");
        
        // default number of bytes to read from a file
        private const int MaxHeaderSize = 560;  // some file formats have headers offset to 512 bytes

        public static bool IsTextFile(this Stream stream)
        {
            bool isTextFile = false;
            var fileName = Path.GetTempFileName();

            try
            {
                using (var fileStream = File.Create(fileName))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }
                isTextFile = IsTextFile(new FileInfo(fileName));
            }
            finally
            {
                File.Delete(fileName);
            }

            return isTextFile;
        }

        private static bool IsTextFile(this FileInfo file)
        {
            return IsTextFile(() => ReadFileHeader(file, Math.Min((int)file.Length, MaxHeaderSize)));
        }

        private static bool IsTextFile(Func<byte[]> fileHeaderReadFunc)
        {
            // read first n-bytes from the file
            byte[] fileHeader = fileHeaderReadFunc();

            // checking if it's binary (not really exact, but should do the job)
            // shouldn't work with UTF-16 OR UTF-32 files
            if (fileHeader.All(b => b != 0))
            {
                return true;
            }

            // compare the file header to the stored file headers
            foreach (var type in types)
            {
                var matchingCount = GetFileMatchingCount(fileHeader, type);
                if (matchingCount == type.Header.Length)
                {
                    return type.Mime.StartsWith("text") || type.Mime.StartsWith("txt");
                }
            }
            return false;
        }
        
        private static int GetFileMatchingCount(byte[] fileHeader, FileType type)
        {
            var matchingCount = 0;
            for (var i = 0; i < type.Header.Length; i++)
            {
                // if file offset is not set to zero, we need to take this into account when comparing.
                // if byte in type.header is set to null, means this byte is variable, ignore it
                if (type.Header[i] != null && type.Header[i] != fileHeader[i + type.HeaderOffset])
                {
                    // if one of the bytes does not match, move on to the next type
                    matchingCount = 0;
                    break;
                }
                matchingCount++;
            }
            return matchingCount;
        }

        private static byte[] ReadFileHeader(FileInfo file, int maxHeaderSize)
        {
            var header = new byte[maxHeaderSize];
            try
            {
                using (var fsSource = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    // read first symbols from file into array of bytes.
                    fsSource.Read(header, 0, maxHeaderSize);
                }
            }
            catch (Exception e) // file could not be found/read
            {
                throw new ApplicationException("Could not read file : " + e.Message);
            }
            return header;
        }
    }
}