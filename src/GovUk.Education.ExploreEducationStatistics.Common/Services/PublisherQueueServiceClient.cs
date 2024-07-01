#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class PublisherQueueServiceClient(string connectionString)
    : AbstractQueueServiceClient(connectionString), IPublisherQueueServiceClient;
