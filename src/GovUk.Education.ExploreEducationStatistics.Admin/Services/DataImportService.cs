#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DataImportService(
    ContentDbContext contentDbContext,
    IDataImportRepository dataImportRepository,
    IDataProcessorClient dataProcessorClient,
    IReleaseFileService releaseFileService,
    IUserService userService
) : IDataImportService
{
    public async Task<DataImport?> GetImport(Guid fileId)
    {
        return await dataImportRepository.GetByFileId(fileId);
    }

    public async Task<Either<ActionResult, Unit>> CancelImport(Guid releaseVersionId, Guid fileId)
    {
        return await releaseFileService
            .CheckFileExists(releaseVersionId: releaseVersionId, fileId: fileId, FileType.Data)
            .OnSuccess(userService.CheckCanCancelFileImport)
            .OnSuccessVoid(async file =>
            {
                var import = await dataImportRepository.GetByFileId(file.Id);
                if (import != null)
                {
                    await dataProcessorClient.CancelImport(import.Id);
                }
            });
    }

    public async Task DeleteImport(Guid fileId)
    {
        await dataImportRepository.DeleteByFileId(fileId);
    }

    public async Task<bool> HasIncompleteImports(Guid releaseVersionId)
    {
        return await contentDbContext
            .ReleaseFiles.AsQueryable()
            .Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Join(contentDbContext.DataImports, rf => rf.FileId, i => i.FileId, (file, import) => import)
            .AnyAsync(import => import.Status != DataImportStatus.COMPLETE);
    }

    public async Task<DataImportStatusViewModel> GetImportStatus(Guid fileId)
    {
        var import = await dataImportRepository.GetByFileId(fileId);

        if (import == null)
        {
            return DataImportStatusViewModel.NotFound();
        }

        await contentDbContext.Entry(import).Collection(i => i.Errors).LoadAsync();

        return new DataImportStatusViewModel
        {
            Errors = import.Errors.Select(error => error.Message).ToList(),
            PercentageComplete = import.PercentageComplete(),
            StagePercentageComplete = import.StagePercentageComplete,
            TotalRows = import.TotalRows,
            Status = import.Status,
        };
    }

    public async Task<DataImport> Import(Guid subjectId, File dataFile, File metaFile)
    {
        var import = await dataImportRepository.Add(
            new DataImport
            {
                Created = DateTime.UtcNow,
                FileId = dataFile.Id,
                MetaFileId = metaFile.Id,
                SubjectId = subjectId,
                Status = DataImportStatus.QUEUED,
            }
        );

        await dataProcessorClient.Import(import.Id);
        return import;
    }
}
