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
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ImportService : IImportService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IImportRepository _importRepository;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IStorageQueueService _queueService;
        private readonly IUserService _userService;

        public ImportService(
            ContentDbContext contentDbContext,
            IImportRepository importRepository,
            IReleaseFileRepository releaseFileRepository,
            IStorageQueueService queueService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _importRepository = importRepository;
            _releaseFileRepository = releaseFileRepository;
            _queueService = queueService;
            _userService = userService;
        }

        public async Task<ImportStatus> GetStatus(Guid fileId)
        {
            return await _importRepository.GetStatusByFileId(fileId);
        }

        public async Task<Either<ActionResult, Unit>> CancelImport(Guid releaseId, Guid fileId)
        {
            return await _releaseFileRepository.CheckFileExists(releaseId, fileId, FileType.Data)
                .OnSuccess(_userService.CheckCanCancelFileImport)
                .OnSuccessVoid(async file =>
                {
                    var import = await _importRepository.GetByFileId(file.Id);
                    await _queueService.AddMessageAsync(ImportsCancellingQueue, new CancelImportMessage(import.Id));
                });
        }

        public async Task DeleteImport(Guid fileId)
        {
            await _importRepository.DeleteByFileId(fileId);
        }

        public async Task<bool> HasIncompleteImports(Guid releaseId)
        {
            return await _contentDbContext.ReleaseFiles
                .Join(_contentDbContext.Imports,
                    rf => rf.FileId,
                    i => i.FileId,
                    (file, import) => import)
                .AnyAsync(import => import.Status != ImportStatus.COMPLETE);
        }

        public async Task<ImportViewModel> GetImport(Guid fileId)
        {
            var import = await _importRepository.GetByFileId(fileId);

            if (import == null)
            {
                return ImportViewModel.NotFound();
            }

            return new ImportViewModel
            {
                Errors = import.Errors.Select(error => error.Message).ToList(),
                PercentageComplete = import.PercentageComplete(),
                StagePercentageComplete = import.StagePercentageComplete,
                NumberOfRows = import.Rows,
                Status = import.Status
            };
        }

        public async Task Import(Guid subjectId, File dataFile, File metaFile, IFormFile formFile)
        {
            var import = await _importRepository.Add(new Import
            {
                Created = DateTime.UtcNow,
                FileId = dataFile.Id,
                MetaFileId = metaFile.Id,
                SubjectId = subjectId,
                Rows = CalculateNumberOfRows(formFile.OpenReadStream()),
                Status = ImportStatus.QUEUED,
                Migrated = false
            });

            await _queueService.AddMessageAsync(ImportsPendingQueue, new ImportMessage(import.Id));
        }

        public async Task ImportZip(Guid subjectId, File dataFile, File metaFile, File zipFile)
        {
            var import = await _importRepository.Add(new Import
            {
                Created = DateTime.UtcNow,
                FileId = dataFile.Id,
                MetaFileId = metaFile.Id,
                ZipFileId = zipFile.Id,
                SubjectId = subjectId,
                Status = ImportStatus.QUEUED,
                Migrated = false
            });

            await _queueService.AddMessageAsync(ImportsPendingQueue, new ImportMessage(import.Id));
        }
    }
}
