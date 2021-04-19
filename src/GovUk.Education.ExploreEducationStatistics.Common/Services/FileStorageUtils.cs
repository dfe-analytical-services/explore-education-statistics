using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStorageUtils
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum FileSizeUnit : byte
        {
            B,
            Kb,
            Mb,
            Gb,
            Tb
        }

        public static IDictionary<string, string> GetMetaValuesReleaseDateTime(
            DateTime releaseDateTime)
        {
            return new Dictionary<string, string>
            {
                {
                    BlobInfoExtensions.ReleaseDateTimeKey, releaseDateTime.ToString("o", CultureInfo.InvariantCulture)
                }
            };
        }

        public static IDictionary<string, string> GetDataFileMetaValues(
            string metaFileName,
            int numberOfRows)
        {
            return new Dictionary<string, string>
            {
                {BlobInfoExtensions.MetaFileKey, metaFileName.ToLower()},
                {BlobInfoExtensions.NumberOfRowsKey, numberOfRows.ToString()}
            };
        }

        public static string GetExtension(string fileName)
        {
            return Path.GetExtension(fileName)?.TrimStart('.') ?? string.Empty;
        }

        public static string GetSize(long contentLength)
        {
            var fileSize = contentLength;
            var unit = FileSizeUnit.B;
            while (fileSize >= 1024 && unit < FileSizeUnit.Tb)
            {
                fileSize /= 1024;
                unit++;
            }

            return $"{fileSize:0.##} {unit}";
        }

        public static int CalculateNumberOfRows(Stream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                var numberOfLines = 0;
                while (reader.ReadLine() != null)
                {
                    ++numberOfLines;
                }

                return numberOfLines;
            }
        }
    }
}
