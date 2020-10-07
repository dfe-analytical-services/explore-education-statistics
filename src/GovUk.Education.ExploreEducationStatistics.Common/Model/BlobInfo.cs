using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class BlobInfo
    {
        public const string NameKey = "name";

        public readonly string Path;

        public readonly string ContentType;

        public readonly string Size;

        public readonly long ContentLength;

        public readonly IDictionary<string, string> Meta;

        public readonly DateTimeOffset? Created;

        public BlobInfo(
            string path,
            string size,
            string contentType,
            long contentLength,
            IDictionary<string, string> meta,
            DateTimeOffset? created = null)
        {
            Path = path;
            Size = size;
            ContentType = contentType;
            ContentLength = contentLength;
            Meta = meta;
            Created = created;
        }

        public string Name => Meta.TryGetValue(NameKey, out var name) ? name : string.Empty;

        public string FileName => Path.Substring(Path.LastIndexOf('/') + 1);

        public string Extension => FileStorageUtils.GetExtension(FileName);
    }
}