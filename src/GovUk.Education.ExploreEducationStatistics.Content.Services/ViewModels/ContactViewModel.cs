#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record ContactViewModel(
    string TeamName,
    string TeamEmail,
    string ContactName,
    string ContactTelNo)
{
    public ContactViewModel(Contact contact) : this(
        TeamName: contact.TeamName,
        TeamEmail: contact.TeamEmail,
        ContactName: contact.ContactName,
        ContactTelNo: contact.ContactTelNo
    )
    {
    }
}
