#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

/// <summary>
/// TODO EES-4661 Remove after the EES-4660 data guidance migration is successful
/// </summary>
public class DataGuidanceMigrationService : IDataGuidanceMigrationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IUserService _userService;

    public DataGuidanceMigrationService(ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IUserService userService)
    {
        _contentDbContext = contentDbContext;
        _statisticsDbContext = statisticsDbContext;
        _userService = userService;
    }

    public async Task<Either<ActionResult, DataGuidanceMigrationReport>> MigrateDataGuidance(bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        return await _userService.CheckIsBauUser()
            .OnSuccess(async () =>
            {
                var releaseDataFilesExcludingReplacements = await _contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .Where(rf => rf.File.Type == FileType.Data
                                 && rf.File.ReplacingId == null)
                    .ToListAsync(cancellationToken);

                var fileIdsWithNoMatchingSubject = new HashSet<Guid>();

                await releaseDataFilesExcludingReplacements
                    .ToAsyncEnumerable()
                    .ForEachAwaitWithCancellationAsync(async (ReleaseFile rf, CancellationToken ct) =>
                    {
                        var releaseSubject = await _statisticsDbContext.ReleaseSubject
                            .SingleOrDefaultAsync(rs => rs.ReleaseId == rf.ReleaseId
                                                        && rs.SubjectId == rf.File.SubjectId,
                                ct);

                        if (releaseSubject == null)
                        {
                            fileIdsWithNoMatchingSubject.Add(rf.FileId);
                        }
                        else
                        {
                            rf.Summary = releaseSubject.DataGuidance;
                        }
                    }, cancellationToken);

                if (!dryRun)
                {
                    await _contentDbContext.SaveChangesAsync(cancellationToken);
                }

                return new DataGuidanceMigrationReport(
                    dryRun,
                    ReleaseDataFilesExcludingReplacementsCount: releaseDataFilesExcludingReplacements.Count,
                    fileIdsWithNoMatchingSubject
                );
            });
    }
}
