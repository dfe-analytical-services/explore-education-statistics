using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

public class HandleProcessingFailureFunction(
    ILogger<HandleProcessingFailureFunction> logger,
    PublicDataDbContext publicDataDbContext)
{
    [Function(nameof(HandleProcessingFailure))]
    public async Task HandleProcessingFailure(
        [ActivityTrigger] ProcessInitialDataSetVersionContext input,
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = await GetDataSetVersion(
            dataSetVersionId: input.DataSetVersionId,
            instanceId: instanceId,
            cancellationToken);

        dataSetVersion.Status = DataSetVersionStatus.Failed;

        var dataSetVersionImport = dataSetVersion.Imports.Single(i => i.InstanceId == instanceId);
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
