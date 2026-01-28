namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public record ReleaseVersionsMigrationReportDto
{
    public required bool DryRun { get; init; }
}
