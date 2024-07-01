#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class NotifierQueueServiceClient(string connectionString)
    : AbstractQueueServiceClient(connectionString), INotifierQueueServiceClient;
