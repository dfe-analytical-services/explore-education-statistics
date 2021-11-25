#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Cache
{
    public class AdminDataBlockTableResultCacheKey : IBlobCacheKey
    {
        private Guid PublicationId { get; }
        private Guid ReleaseId { get; }
        private Guid DataBlockId { get; }

        public AdminDataBlockTableResultCacheKey(ReleaseContentBlock releaseContentBlock)
        {
            if (releaseContentBlock.ContentBlock.GetType().IsAssignableFrom(typeof(DataBlock)))
            {
                throw new ArgumentException(
                    "Attempting to build key with incorrect type of content block");
            }

            var release = releaseContentBlock.Release;
            PublicationId = release.Publication.Id;
            ReleaseId = release.Id;
            DataBlockId = releaseContentBlock.ContentBlockId;
        }

        public IBlobContainer Container => BlobContainers.PrivateContent;

        public string Key => PrivateContentDataBlockPath(
            PublicationId,
            ReleaseId,
            DataBlockId
        );
    }
}
