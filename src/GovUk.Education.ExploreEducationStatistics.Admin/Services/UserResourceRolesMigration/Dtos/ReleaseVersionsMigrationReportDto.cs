#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration.Dtos;

/// <summary>
/// TODO EES-XXXX Remove after the User Resource Roles migration is complete.
/// </summary>
public record ThingDto
{
    public required bool DryRun { get; init; }
}
