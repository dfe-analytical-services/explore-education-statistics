using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    /**
     * Temporary service for migrating Import rows in Azure Table Storage to DataImports in the database
     */
    public class MigrateImportsService : IMigrateImportsService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ITableStorageService _tableStorageService;
        private readonly IUserService _userService;
        private readonly ILogger<MigrateImportsService> _logger;

        public MigrateImportsService(ContentDbContext contentDbContext,
            ITableStorageService tableStorageService,
            IUserService userService,
            ILogger<MigrateImportsService> logger)
        {
            _contentDbContext = contentDbContext;
            _tableStorageService = tableStorageService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, Unit>> MigrateImports()
        {
            return await _userService.CheckCanRunMigrations()
                .OnSuccess(CheckFirstRun)
                .OnSuccessVoid(async () =>
                {
                    var tableImports = await GetTableImports();
                    _logger.LogInformation($"Found {tableImports.Count} table imports");

                    var dataImports = await tableImports.SelectAsync(async tableImport =>
                        await GetDataImport(tableImport));

                    _logger.LogInformation("Saving database imports");
                    await _contentDbContext.DataImports.AddRangeAsync(dataImports);
                    await _contentDbContext.SaveChangesAsync();

                    _logger.LogInformation($"Table import migration complete");
                });
        }

        private async Task<Either<ActionResult, Unit>> CheckFirstRun()
        {
            if (await _contentDbContext.DataImports.AnyAsync(import => import.Migrated))
            {
                return ValidationActionResult(DataFileImportsMigrationAlreadyRun);
            }

            return Unit.Instance;
        }

        private async Task<List<TableImport>> GetTableImports()
        {
            var imports = (await _tableStorageService.ExecuteQueryAsync(
                tableName: "imports",
                query: new TableQuery<TableImport>())).ToList();

            // Filter out failed imports with no message. Lots of these exist in the Dev environment.
            return imports.Where(import => 
                    !(import.DataImportStatus == FAILED && import.Message.IsNullOrEmpty()))
                .ToList();
        }

        private async Task<DataImport> GetDataImport(TableImport tableImport)
        {
            var timestamp = tableImport.Timestamp.UtcDateTime;
            var errors = GetDataImportErrors(timestamp, tableImport.ImportErrors);
            var message = tableImport.ImportMessage;

            var file = await GetFile(tableImport.ReleaseId, message.SubjectId, FileType.Data, message.DataFileName);
            var metaFile = await GetFile(tableImport.ReleaseId, message.SubjectId, Metadata, message.MetaFileName);

            if (!message.ArchiveFileName.IsNullOrEmpty() && !file.SourceId.HasValue)
            {
                _logger.LogWarning("Zip file not found while migrating imports " +
                                   $"for Release: {tableImport.ReleaseId}, Subject: {message.SubjectId}. " +
                                   $"Expected file {file.Id} to have a SourceId for filename: {message.ArchiveFileName}");
            }

            return new DataImport
            {
                FileId = file.Id,
                MetaFileId = metaFile.Id,
                ZipFileId = file.SourceId,
                SubjectId = message.SubjectId,
                Errors = errors,
                StagePercentageComplete = tableImport.DataImportStatus == COMPLETE ? 100 : tableImport.PercentageComplete,
                Status = tableImport.DataImportStatus,
                Created = timestamp,
                NumBatches = message.NumBatches,
                Rows = tableImport.NumberOfRows,
                RowsPerBatch = message.RowsPerBatch,
                TotalRows = message.TotalRows,
                Migrated = true
            };
        }

        private static List<DataImportError> GetDataImportErrors(DateTime created,
            IEnumerable<TableImportError> tableImportErrors)
        {
            return tableImportErrors.Select(tableImportError => new DataImportError(tableImportError.Message)
            {
                Created = created
            }).ToList();
        }

        private async Task<File> GetFile(Guid releaseId, Guid subjectId, FileType type, string expectedFilename)
        {
            var file = await _contentDbContext.Files
                .SingleAsync(f => f.Type == type
                                  && f.ReleaseId == releaseId
                                  && f.SubjectId == subjectId);

            if (file.Filename != expectedFilename)
            {
                _logger.LogWarning("Unexpected file returned while migrating imports " +
                                   $"for Release: {releaseId}, Subject: {subjectId}. " +
                                   $"Expected: {expectedFilename}, found: {file.Filename}");
            }

            return file;
        }
    }
}
