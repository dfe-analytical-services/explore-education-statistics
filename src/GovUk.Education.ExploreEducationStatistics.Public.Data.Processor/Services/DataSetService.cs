using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class DataSetService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionService dataSetVersionService
) : IDataSetService
{
    public async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateDataSet(
        DataSetCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.RequireTransaction(async () =>
            await GetReleaseFile(request.ReleaseFileId, cancellationToken)
                .OnSuccess(releaseFile => CreateDataSet(releaseFile, cancellationToken))
                .OnSuccess(dataSet => dataSetVersionService
                    .CreateInitialVersion(
                        dataSetId: dataSet.Id,
                        releaseFileId: request.ReleaseFileId,
                        instanceId: instanceId,
                        cancellationToken)
                    .OnSuccess(dataSetVersionId => (dataSet.Id, dataSetVersionId))));
    }

    private async Task<DataSet> CreateDataSet(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken)
    {
        var dataSet = new DataSet
        {
            Status = DataSetStatus.Draft,
            Title = releaseFile.Name!,
            Summary = releaseFile.Summary ?? "",
            PublicationId = releaseFile.ReleaseVersion.PublicationId
        };

        publicDataDbContext.Add(dataSet);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return dataSet;
    }

    private async Task<Either<ActionResult, ReleaseFile>> GetReleaseFile(
        Guid releaseFileId,
        CancellationToken cancellationToken)
    {
        var releaseFile = await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion)
            .FirstOrDefaultAsync(rf => rf.Id == releaseFileId, cancellationToken);

        return releaseFile is null
            ? ValidationUtils.NotFoundResult<ReleaseFile, Guid>(
                releaseFileId,
                nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst())
            : releaseFile;
    }
}
