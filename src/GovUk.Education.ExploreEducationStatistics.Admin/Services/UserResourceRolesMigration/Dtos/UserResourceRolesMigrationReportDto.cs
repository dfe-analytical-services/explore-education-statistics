#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration.Dtos;

/// <summary>
/// TODO EES-XXXX Remove after the User Resource Roles migration is complete.
/// </summary>
public record UserResourceRolesMigrationReportDto
{
    public required bool DryRun { get; init; }
    public required int NumberOfDrafterRolesRemoved { get; init; }
    public required int NumberOfDrafterRolesCreated { get; init; }
    public required int NumberOfApproverRolesRemoved { get; init; }
    public required int NumberOfApproverRolesCreated { get; init; }
}
