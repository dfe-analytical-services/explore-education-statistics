using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class BlobInfo
    {
        public const string FilenameKey = "filename";
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

        /// <summary>
        /// In the case of the Public API's which have no access to the Content Db,
        /// this is the only method of retrieving the filename
        /// </summary>
        public string FileName =>
            Meta.TryGetValue(FilenameKey, out var filename) ? filename : Path.Substring(Path.LastIndexOf('/') + 1);
    }
}