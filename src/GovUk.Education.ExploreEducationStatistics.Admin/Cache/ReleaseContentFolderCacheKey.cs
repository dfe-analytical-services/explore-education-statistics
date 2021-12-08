#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Cache
{
    public class ReleaseContentFolderCacheKey : IBlobCacheKey
    {
        private Guid PublicationId { get; }

        private Guid ReleaseId { get; }

        public ReleaseContentFolderCacheKey(Guid publicationId, Guid releaseId)
        {
            PublicationId = publicationId;
            ReleaseId = releaseId;
        }

        public string Key => PrivateContentReleaseParentPath(PublicationId, ReleaseId);

        public IBlobContainer Container => PrivateContent;
    }
}