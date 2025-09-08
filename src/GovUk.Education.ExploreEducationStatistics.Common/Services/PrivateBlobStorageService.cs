#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PrivateBlobStorageService(
    string connectionString,
    ILogger<IBlobStorageService> logger,
    IBlobSasService sasService
) : BlobStorageService(connectionString, logger, sasService), IPrivateBlobStorageService;
