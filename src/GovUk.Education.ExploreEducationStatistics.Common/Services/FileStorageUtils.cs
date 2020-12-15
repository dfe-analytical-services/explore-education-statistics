﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.Azure.Storage.Blob;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

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
            string filename,
            string name)
        {
            return new Dictionary<string, string>
            {
                {BlobInfoExtensions.FilenameKey, filename},
                {BlobInfoExtensions.NameKey, name}
            };
        }

        public static IDictionary<string, string> GetDataFileMetaValues(
            string name,
            string metaFileName,
            string userName,
            int numberOfRows)
        {
            return new Dictionary<string, string>
            {
                {BlobInfoExtensions.NameKey, name},
                {BlobInfoExtensions.MetaFileKey, metaFileName.ToLower()},
                {BlobInfoExtensions.UserNameKey, userName},
                {BlobInfoExtensions.NumberOfRowsKey, numberOfRows.ToString()}
            };
        }

        public static IDictionary<string, string> GetMetaDataFileMetaValues(string dataFileName)
        {
            return new Dictionary<string, string>
            {
                {BlobInfoExtensions.DataFileKey, dataFileName.ToLower()}
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

        public static bool IsBatchedDataFile(IListBlobItem blobItem, Guid releaseId)
        {
            return blobItem.Parent.Prefix.Equals(AdminReleaseBatchesDirectoryPath(releaseId));
        }

        public static bool IsMetaDataFile(CloudBlob blob)
        {
            // The meta data file contains a metadata attribute referencing it's corresponding data file
            return blob.Metadata.ContainsKey(BlobInfoExtensions.DataFileKey);
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