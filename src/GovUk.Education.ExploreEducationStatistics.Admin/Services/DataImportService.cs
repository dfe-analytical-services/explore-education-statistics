#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataImportService : IDataImportService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IDataImportRepository _dataImportRepository;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IStorageQueueService _queueService;
        private readonly IUserService _userService;

        public DataImportService(
            ContentDbContext contentDbContext,
            IDataImportRepository dataImportRepository,
            IReleaseFileService releaseFileService,
            IStorageQueueService queueService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _dataImportRepository = dataImportRepository;
            _releaseFileService = releaseFileService;
            _queueService = queueService;
            _userService = userService;
        }

        public async Task<DataImport?> GetImport(Guid fileId)
        {
            return await _dataImportRepository.GetByFileId(fileId);
        }

        public async Task<Either<ActionResult, Unit>> CancelImport(Guid releaseId, Guid fileId)
        {
            return await _releaseFileService.CheckFileExists(releaseId, fileId, FileType.Data)
                .OnSuccess(_userService.CheckCanCancelFileImport)
                .OnSuccessVoid(async file =>
                {
                    var import = await _dataImportRepository.GetByFileId(file.Id);
                    if (import != null)
                    {
                        await _queueService.AddMessageAsync(ImportsCancellingQueue, new CancelImportMessage(import.Id));
                    }
                });
        }

        public async Task DeleteImport(Guid fileId)
        {
            await _dataImportRepository.DeleteByFileId(fileId);
        }

        public async Task<bool> HasIncompleteImports(Guid releaseId)
        {
            return await _contentDbContext.ReleaseFiles
                .AsQueryable()
                .Where(rf => rf.ReleaseId == releaseId)
                .Join(_contentDbContext.DataImports,
                    rf => rf.FileId,
                    i => i.FileId,
                    (file, import) => import)
                .AnyAsync(import => import.Status != DataImportStatus.COMPLETE);
        }

        public async Task<DataImportStatusViewModel> GetImportStatus(Guid fileId)
        {
            var import = await _dataImportRepository.GetByFileId(fileId);

            if (import == null)
            {
                return DataImportStatusViewModel.NotFound();
            }

            await _contentDbContext.Entry(import)
                .Collection(i => i.Errors)
                .LoadAsync();

            return new DataImportStatusViewModel
            {
                Errors = import.Errors.Select(error => error.Message).ToList(),
                PercentageComplete = import.PercentageComplete(),
                StagePercentageComplete = import.StagePercentageComplete,
                TotalRows = import.TotalRows,
                Status = import.Status
            };
        }

        public async Task<DataImport> Import(Guid subjectId, File dataFile, File metaFile)
        {
            var import = await _dataImportRepository.Add(new DataImport
            {
                Created = DateTime.UtcNow,
                FileId = dataFile.Id,
                MetaFileId = metaFile.Id,
                SubjectId = subjectId,
                Status = DataImportStatus.QUEUED
            });

            await _queueService.AddMessageAsync(ImportsPendingQueue, new ImportMessage(import.Id));
            return import;
        }

        public async Task<DataImport> ImportZip(Guid subjectId, File dataFile, File metaFile, File zipFile)
        {
            var import = await _dataImportRepository.Add(new DataImport
            {
                Created = DateTime.UtcNow,
                FileId = dataFile.Id,
                MetaFileId = metaFile.Id,
                ZipFileId = zipFile.Id,
                SubjectId = subjectId,
                Status = DataImportStatus.QUEUED
            });

            await _queueService.AddMessageAsync(ImportsPendingQueue, new ImportMessage(import.Id));
            return import;
        }
    }
}
