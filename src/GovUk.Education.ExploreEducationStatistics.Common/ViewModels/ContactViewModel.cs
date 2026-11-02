using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record ContactViewModel
{
    public required string TeamName { get; init; }

    public required string TeamEmail { get; init; }

    public required string ContactName { get; init; }

    public required string? ContactTelNo { get; init; }

    public static ContactViewModel FromModel(Contact contact) =>
        new()
        {
            TeamName = contact.TeamName,
            TeamEmail = contact.TeamEmail,
            ContactName = contact.ContactName,
            ContactTelNo = contact.ContactTelNo,
        };
}
