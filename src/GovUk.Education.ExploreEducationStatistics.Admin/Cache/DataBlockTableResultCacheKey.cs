#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Cache
{
    public class DataBlockTableResultCacheKey : IBlobCacheKey
    {
        private Guid ReleaseId { get; }
        private Guid DataBlockId { get; }

        public DataBlockTableResultCacheKey(ReleaseContentBlock releaseContentBlock)
        {
            if (!releaseContentBlock.ContentBlock.GetType().IsAssignableFrom(typeof(DataBlock)))
            {
                throw new ArgumentException(
                    $"Attempting to build key with incorrect type of content block - " +
                    $"{releaseContentBlock.ContentBlock.GetType()}");
            }

            var release = releaseContentBlock.Release;
            ReleaseId = release.Id;
            DataBlockId = releaseContentBlock.ContentBlockId;
        }

        public IBlobContainer Container => PrivateContent;

        public string Key => PrivateContentDataBlockPath(
            ReleaseId,
            DataBlockId
        );
    }
}
