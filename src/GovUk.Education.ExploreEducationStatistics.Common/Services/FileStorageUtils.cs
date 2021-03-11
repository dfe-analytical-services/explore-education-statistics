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

        public static IDictionary<string, string> GetAllFilesZipMetaValues(
            string name,
            DateTime releaseDateTime)
        {
            return new Dictionary<string, string>
            {
                {
                    BlobInfoExtensions.NameKey, name
                },
                {
                    BlobInfoExtensions.ReleaseDateTimeKey, releaseDateTime.ToString("o", CultureInfo.InvariantCulture)
                }
            };
        }

        public static IDictionary<string, string> GetAncillaryFileMetaValues(
            string name,
            DateTime? releaseDateTime = null)
        {
            var values = new Dictionary<string, string>
            {
                {BlobInfoExtensions.NameKey, name}
            };

            if (releaseDateTime.HasValue)
            {
                values.Add(BlobInfoExtensions.ReleaseDateTimeKey, 
                    releaseDateTime.Value.ToString("o", CultureInfo.InvariantCulture));
            }

            return values;
        }

        public static IDictionary<string, string> GetDataFileMetaValues(
            string name,
            string metaFileName,
            int numberOfRows)
        {
            return new Dictionary<string, string>
            {
                {BlobInfoExtensions.NameKey, name},
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

        public static int GetNumBatches(int rows, int rowsPerBatch)
        {
            return (int) Math.Ceiling(rows / (double) rowsPerBatch);
        }
    }
}
