#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    // TODO: EES-2343 Remove when file sizes are stored in database
    public class ReleaseBlobInfoGetter : IReleaseService.IBlobInfoGetter
    {
        private readonly IBlobStorageService _blobStorageService;

        public ReleaseBlobInfoGetter(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<BlobInfo?> Get(ReleaseFile releaseFile)
        {
            return await _blobStorageService.FindBlob(
                containerName: BlobContainers.PrivateReleaseFiles,
                path: releaseFile.File.Path()
            );
        }
    }
}