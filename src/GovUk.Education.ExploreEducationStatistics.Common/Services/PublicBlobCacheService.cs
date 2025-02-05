#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublicBlobCacheService(
    IPublicBlobStorageService publicBlobStorageService,
    ILogger<BlobCacheService> logger) 
    : BlobCacheService(publicBlobStorageService, logger), IPublicBlobCacheService
{
}
