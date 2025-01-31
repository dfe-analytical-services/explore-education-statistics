#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class BlobInfo
    {
        public readonly string Path;

        public readonly string ContentType;

        public readonly long ContentLength;

        public readonly IDictionary<string, string> Meta;

        public readonly DateTimeOffset? Created;

        public readonly DateTimeOffset? Updated;

        public BlobInfo(
            string path,
            string contentType,
            long contentLength,
            IDictionary<string, string>? meta = null,
            DateTimeOffset? created = null,
            DateTimeOffset? updated = null)
        {
            Path = path;
            ContentType = contentType;
            ContentLength = contentLength;
            Meta = meta ?? new Dictionary<string, string>();
            Created = created;
            Updated = updated;
        }

        public string FileName => Path.Substring(Path.LastIndexOf('/') + 1);
    }
}
