#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

// TODO - EES-6480.
public class DataSetUploadRepository(
    ContentDbContext contentDbContext,
    IPrivateBlobStorageService privateBlobStorageService,
    IDataSetScreenerClient dataSetScreenerClient,
    IMapper mapper
) : IDataSetUploadRepository
{
    public async Task<Either<ActionResult, List<DataSetUploadViewModel>>> ListAll(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .DataSetUploads.Where(uploads => uploads.ReleaseVersionId == releaseVersionId)
            .Select(entity => mapper.Map<DataSetUploadViewModel>(entity))
            .ToListAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, Unit>> Delete(
        Guid releaseVersionId,
        Guid dataSetUploadId,
        CancellationToken cancellationToken = default
    )
    {
        return await contentDbContext
            .DataSetUploads.SingleOrNotFoundAsync(
                upload => upload.ReleaseVersionId == releaseVersionId && upload.Id == dataSetUploadId,
                cancellationToken
            )
            .OnSuccessDo(async dataSetUpload =>
            {
                contentDbContext.Remove(dataSetUpload);
                await contentDbContext.SaveChangesAsync(cancellationToken);
            })
            .OnSuccessVoid(async dataSetUpload =>
            {
                await privateBlobStorageService.DeleteBlob(PrivateReleaseTempFiles, dataSetUpload.DataFilePath);
                await privateBlobStorageService.DeleteBlob(PrivateReleaseTempFiles, dataSetUpload.MetaFilePath);
                await DeleteProgressFiles([dataSetUpload], cancellationToken);
            });
    }

    public async Task<Either<ActionResult, Unit>> DeleteAll(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var uploads = await contentDbContext
                .DataSetUploads.Where(d => d.ReleaseVersionId == releaseVersionId)
                .ToListAsync(cancellationToken);

            await uploads
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async upload =>
                    {
                        await privateBlobStorageService.DeleteBlob(PrivateReleaseTempFiles, upload.DataFilePath);
                        await privateBlobStorageService.DeleteBlob(PrivateReleaseTempFiles, upload.MetaFilePath);
                    },
                    cancellationToken
                );

            contentDbContext.DataSetUploads.RemoveRange(uploads);
            await contentDbContext.SaveChangesAsync(cancellationToken);
            await DeleteProgressFiles(uploads, cancellationToken);

            return Unit.Instance;
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }

    /// <summary>
    /// Deletes progress and completion files for data sets that are still undergoing screening.
    /// </summary>
    /// <remarks>
    /// Any data sets that were still undergoing screening will have progress and completion report
    /// files held by the Screener API. Those files are otherwise only cleaned up when screening
    /// reaches a completed or failed state, so ask the Screener API to delete them here to avoid
    /// leaving them orphaned. Uploads that have already reached a post-screening status have had
    /// theirs deleted by DataSetScreenerService.
    /// </remarks>
    private async Task DeleteProgressFiles(List<DataSetUpload> uploads, CancellationToken cancellationToken)
    {
        var screeningDataSetIds = uploads
            .Where(upload => upload.ScreeningStatus == DataSetUploadScreeningStatus.Screening)
            .Select(upload => upload.Id)
            .ToList();

        if (screeningDataSetIds.Count > 0)
        {
            await dataSetScreenerClient.DeleteScreenerProgressAndCompletionFiles(
                dataSetIds: screeningDataSetIds,
                cancellationToken: cancellationToken
            );
        }
    }
}
