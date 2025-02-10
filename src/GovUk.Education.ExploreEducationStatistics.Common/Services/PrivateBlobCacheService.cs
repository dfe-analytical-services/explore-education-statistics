#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PrivateBlobCacheService(
    IPrivateBlobStorageService privateBlobStorageService,
    ILogger<PrivateBlobCacheService> logger) 
    : BlobCacheService(privateBlobStorageService, logger), IPrivateBlobCacheService
{
}
