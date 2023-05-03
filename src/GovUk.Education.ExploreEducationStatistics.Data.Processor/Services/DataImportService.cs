#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class DataImportService : IDataImportService
    {
        private readonly IDbContextSupplier _dbContextSupplier;
        private readonly ILogger<DataImportService> _logger;

        public DataImportService(
            IDbContextSupplier dbContextSupplier,
            ILogger<DataImportService> logger)
        {
            _dbContextSupplier = dbContextSupplier;
            _logger = logger;
        }

        public async Task FailImport(Guid id, List<DataImportError> errors)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            
            var import = await contentDbContext.DataImports.SingleAsync(d => d.Id == id);
            
            if (import.Status != COMPLETE && import.Status != FAILED)
            {
                contentDbContext.Update(import);
                import.Status = FAILED;
                import.Errors.AddRange(errors);                    
                
                await contentDbContext.SaveChangesAsync();
            }
        }

        public async Task FailImport(Guid id, params string[] errors)
        {
            await FailImport(id, errors
                .Select(error => new DataImportError(error))
                .ToList());
        }

        public async Task<DataImport> GetImport(Guid id)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            return await contentDbContext.DataImports
                .AsNoTracking()
                .Include(import => import.Errors)
                .Include(import => import.File)
                .Include(import => import.MetaFile)
                .Include(import => import.ZipFile)
                .SingleAsync(import => import.Id == id);
        }

        public async Task<DataImportStatus> GetImportStatus(Guid id)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            var import = await contentDbContext.DataImports
                .AsNoTracking()
                .SingleOrDefaultAsync(i => i.Id == id);

            if (import == null)
            {
                return NOT_FOUND;
            }

            return import.Status;
        }

        public async Task Update(
            Guid id,
            int? expectedImportedRows = null,
            int? totalRows = null,
            HashSet<GeographicLevel>? geographicLevels = null,
            int? importedRows = null,
            int? lastProcessedRowIndex = null)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            var import = await contentDbContext.DataImports.SingleAsync(import => import.Id == id);
            contentDbContext.Update(import);

            import.ExpectedImportedRows = expectedImportedRows ?? import.ExpectedImportedRows;
            import.TotalRows = totalRows ?? import.TotalRows;
            import.GeographicLevels = geographicLevels ?? import.GeographicLevels;
            import.ImportedRows = importedRows ?? import.ImportedRows;
            import.LastProcessedRowIndex = lastProcessedRowIndex ?? import.LastProcessedRowIndex;

            await contentDbContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(Guid id, DataImportStatus newStatus, double percentageComplete)
        {
            await using var context = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            
            var import = await context.DataImports
                .Include(i => i.File)
                .SingleAsync(i => i.Id == id);

            var filename = import.File.Filename;

            var percentageCompleteBefore = import.StagePercentageComplete;
            var percentageCompleteAfter = (int) Math.Clamp(percentageComplete, 0, 100);

            // Ignore updating if already finished, or in the process of aborting and this status update isn't a
            // finishing status update. 
            if (import.Status.IsFinished() || (import.Status.IsAborting() && !newStatus.IsFinished()))
            {
                _logger.LogWarning(
                    "Update: {Filename} {ImportStatus} ({PercentageCompleteBefore}%) -> " +
                    "{NewStatus} ({PercentageCompleteAfter}%) ignored as this import is already in finished or " +
                    "completed state state {FinishedImportStatus}",
                    filename,
                    import.Status,
                    percentageCompleteBefore,
                    newStatus,
                    percentageCompleteAfter,
                    import.Status);
                
                return;
            }

            // Ignore updating to an equal percentage complete (after rounding) at the same status without logging it
            if (import.Status == newStatus && percentageCompleteBefore == percentageCompleteAfter)
            {
                return;
            }

            _logger.LogInformation(
                "Update: {Filename} {ImportStatus} ({PercentageCompleteBefore}%) -> {NewStatus} ({PercentageCompleteAfter}%)",
                filename,
                import.Status,
                percentageCompleteBefore,
                newStatus,
                percentageCompleteAfter);

            import.StagePercentageComplete = percentageCompleteAfter;
            import.Status = newStatus;
            context.DataImports.Update(import);
            await context.SaveChangesAsync();
        }
    }
}
