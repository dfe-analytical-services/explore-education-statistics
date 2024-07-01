using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services;

public class DataProcessorQueueServiceClient(string connectionString)
    : AbstractQueueServiceClient(connectionString), IDataProcessorQueueServiceClient;
