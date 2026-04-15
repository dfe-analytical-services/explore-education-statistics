namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record RoleViewModel
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string NormalizedName { get; init; }
}
