#nullable enable

using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record ContactViewModel
{
    public string TeamName { get; init; } = string.Empty;

    public string TeamEmail { get; init; } = string.Empty;

    public string ContactName { get; init; } = string.Empty;

    public string ContactTelNo { get; init; } = string.Empty;

    public ContactViewModel()
    {
    }

    public ContactViewModel(Contact contact)
    {
        TeamName = contact.TeamName;
        TeamEmail = contact.TeamEmail;
        ContactName = contact.ContactName;
        ContactTelNo = contact.ContactTelNo ?? string.Empty;
    }
}