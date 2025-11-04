using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublicBlobCacheService(
    IPublicBlobStorageService publicBlobStorageService,
    ILogger<PublicBlobCacheService> logger
) : BlobCacheService(publicBlobStorageService, logger), IPublicBlobCacheService { }
