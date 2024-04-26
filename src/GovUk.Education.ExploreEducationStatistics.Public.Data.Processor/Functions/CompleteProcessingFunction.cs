using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class CompleteProcessingFunction(
    ILogger<CompleteProcessingFunction> logger,
    PublicDataDbContext publicDataDbContext)
{
    [Function(nameof(CompleteProcessing))]
    public async Task CompleteProcessing(
        [ActivityTrigger] ProcessInitialDataSetVersionContext input,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: input.DataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);

        dataSetVersionImport.Stage = DataSetVersionImportStage.Completing;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        // Any additional logic to tidy up after importing will be added here

        dataSetVersion.Status = DataSetVersionStatus.Draft;
        dataSetVersionImport.Completed = DateTimeOffset.UtcNow;
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<DataSetVersion> GetDataSetVersion(
        Guid dataSetVersionId,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersions
            .Include(dsv => dsv.Imports.Where(i => i.InstanceId == instanceId))
            .SingleAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken: cancellationToken);
    }
}
