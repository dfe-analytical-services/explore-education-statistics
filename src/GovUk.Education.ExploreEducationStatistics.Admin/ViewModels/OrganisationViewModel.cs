#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record OrganisationViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }

    public static OrganisationViewModel FromOrganisation(Organisation organisation) =>
        new()
        {
            Id = organisation.Id,
            Title = organisation.Title,
            Url = organisation.Url
        };
}
