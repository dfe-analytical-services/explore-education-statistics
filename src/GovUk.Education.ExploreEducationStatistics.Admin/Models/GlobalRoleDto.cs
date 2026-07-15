namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record GlobalRoleDto
{
    public required Guid Id { get; init; }

    public required GlobalRoles.Role Role { get; init; }

    public required string Name { get; init; }

    public required string NormalizedName { get; init; }
}
