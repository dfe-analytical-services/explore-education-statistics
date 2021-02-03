using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImportService : IImportService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ILogger<ImportService> _logger;

        public ImportService(
            ContentDbContext contentDbContext,
            ILogger<ImportService> logger)
        {
            _contentDbContext = contentDbContext;
            _logger = logger;
        }

        public async Task FailImport(Guid id, List<ImportError> errors)
        {
            var import = await GetImport(id);
            if (import.Status != COMPLETE && import.Status != FAILED)
            {
                _contentDbContext.Update(import);
                import.Status = FAILED;
                import.Errors.AddRange(errors);
                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task FailImport(Guid id, params string[] errors)
        {
            await FailImport(id, errors.Select(error => new ImportError(error)).ToList());
        }

        public async Task<Import> GetImport(Guid id)
        {
            return await _contentDbContext.Imports
                .Include(import => import.Errors)
                .Include(import => import.File)
                .Include(import => import.MetaFile)
                .Include(import => import.ZipFile)
                .SingleAsync(import => import.Id == id);
        }

        public async Task<ImportStatus> GetImportStatus(Guid id)
        {
            var import = await _contentDbContext.Imports.FindAsync(id);

            if (import == null)
            {
                return NOT_FOUND;
            }

            return import.Status;
        }

        public async Task Update(Guid id, int rowsPerBatch, int totalRows, int numBatches)
        {
            var import = await _contentDbContext.Imports.FindAsync(id);
            _contentDbContext.Update(import);

            import.RowsPerBatch = rowsPerBatch;
            import.TotalRows = totalRows;
            import.NumBatches = numBatches;

            await _contentDbContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(Guid id, ImportStatus newStatus, double percentageComplete)
        {
            await ExecuteWithExclusiveLock(
                _contentDbContext,
                $"LockForImport-{id}",
                async context =>
                {
                    var import = await context.Imports
                        .Include(i => i.File)
                        .SingleAsync(i => i.Id == id);

                    var filename = import.File.Filename;

                    var percentageCompleteBefore = import.StagePercentageComplete;
                    var percentageCompleteAfter = (int) Math.Clamp(percentageComplete, 0, 100);

                    // Ignore updating if already finished
                    if (import.Status.IsFinished())
                    {
                        _logger.LogWarning(
                            $"Update: {filename} {import.Status} ({percentageCompleteBefore}%) -> " +
                            $"{newStatus} ({percentageCompleteAfter}%) ignored as this import is already finished");
                        return;
                    }

                    // Ignore updating if already aborting and the new state is not aborting or finishing
                    if (import.Status.IsAborting() && !newStatus.IsFinishedOrAborting())
                    {
                        _logger.LogWarning(
                            $"Update: {filename} {import.Status} ({percentageCompleteBefore}%) -> " +
                            $"{newStatus} ({percentageCompleteAfter}%) ignored as this import is already aborting or is finished");
                        return;
                    }

                    // Ignore updates if attempting to downgrade from a normal importing state to a lower normal importing state,
                    // or if the percentage is being set lower or the same as is currently and is the same state
                    if (!newStatus.IsFinishedOrAborting() &&
                        (import.Status.CompareTo(newStatus) > 0 ||
                         import.Status == newStatus && percentageCompleteBefore > percentageCompleteAfter))
                    {
                        _logger.LogWarning(
                            $"Update: {filename} {import.Status} ({percentageCompleteBefore}%) -> " +
                            $"{newStatus} ({percentageCompleteAfter}%) ignored");
                        return;
                    }

                    // Ignore updating to an equal percentage complete (after rounding) at the same status without logging it
                    if (import.Status == newStatus && percentageCompleteBefore == percentageCompleteAfter)
                    {
                        return;
                    }

                    _logger.LogInformation(
                        $"Update: {filename} {import.Status} ({percentageCompleteBefore}%) -> {newStatus} ({percentageCompleteAfter}%)");

                    import.StagePercentageComplete = percentageCompleteAfter;
                    import.Status = newStatus;
                    context.Imports.Update(import);
                    await context.SaveChangesAsync();
                }
            );
        }
    }
}