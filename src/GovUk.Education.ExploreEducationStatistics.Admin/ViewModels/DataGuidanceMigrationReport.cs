#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

/// <summary>
/// TODO EES-4661 Remove after the EES-4660 data guidance migration is successful
/// </summary>
public record DataGuidanceMigrationReport(
    bool DryRun,
    int ReleaseDataFilesExcludingReplacementsCount,
    HashSet<Guid> FileIdsWithNoMatchingSubject);
