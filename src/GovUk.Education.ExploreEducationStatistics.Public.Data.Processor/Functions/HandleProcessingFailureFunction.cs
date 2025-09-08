using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class HandleProcessingFailureFunction(PublicDataDbContext publicDataDbContext)
    : BaseProcessDataSetVersionFunction(publicDataDbContext)
{
    private readonly PublicDataDbContext _publicDataDbContext = publicDataDbContext;

    [Function(ActivityNames.HandleProcessingFailure)]
    public async Task HandleProcessingFailure([ActivityTrigger] Guid instanceId, CancellationToken cancellationToken)
    {
        var dataSetVersionImport = await GetDataSetVersionImport(instanceId, cancellationToken);
        var dataSetVersion = dataSetVersionImport.DataSetVersion;

        dataSetVersion.Status = DataSetVersionStatus.Failed;
        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await _publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
