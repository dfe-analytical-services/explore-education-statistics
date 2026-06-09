#nullable enable
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache;

public class PublicationCacheService(IPublicBlobStorageService publicBlobStorageService) : IPublicationCacheService
{
    public async Task RemovePublication(string publicationSlug)
    {
        await publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            options: new IBlobStorageService.DeleteBlobsOptions
            {
                IncludeRegex = new Regex($"^publications/{publicationSlug}/.+$"),
            }
        );
    }
}
