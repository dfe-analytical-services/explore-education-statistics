#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PrivateBlobStorageService : BlobStorageService, IPrivateBlobStorageService
{
    public PrivateBlobStorageService(
        ILogger<IBlobStorageService> logger,
        IConfiguration configuration)
        : base("CoreStorage", logger, configuration)
    {}
}
