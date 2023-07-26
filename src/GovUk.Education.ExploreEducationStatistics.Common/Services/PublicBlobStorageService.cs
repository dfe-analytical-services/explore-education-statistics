#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublicBlobStorageService : BlobStorageService, IPublicBlobStorageService
{
    public PublicBlobStorageService(
        ILogger<IBlobStorageService> logger,
        IConfiguration configuration)
        : base("PublicStorage", logger, configuration)
    {}
}
