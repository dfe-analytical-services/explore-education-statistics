#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

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
        ContactTelNo = contact.ContactTelNo;
    }
}
