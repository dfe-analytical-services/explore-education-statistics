#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

// TODO - EES-6480.
public class DataSetUploadRepository(
    ContentDbContext contentDbContext,
    IPrivateBlobStorageService privateBlobStorageService,
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

            return Unit.Instance;
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
