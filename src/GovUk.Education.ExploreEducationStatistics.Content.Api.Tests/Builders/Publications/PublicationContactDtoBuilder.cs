using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationContactDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _contactName = "Contact name";
    private string? _contactTelNo = "Contact tel no";
    private string _teamName = "Team email";
    private string _teamEmail = "Team name";

    public PublicationContactDto Build() =>
        new()
        {
            Id = _id,
            ContactName = _contactName,
            ContactTelNo = _contactTelNo,
            TeamEmail = _teamName,
            TeamName = _teamEmail,
        };

    public PublicationContactDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublicationContactDtoBuilder WithContactName(string contactName)
    {
        _contactName = contactName;
        return this;
    }

    public PublicationContactDtoBuilder WithContactTelNo(string? contactTelNo)
    {
        _contactTelNo = contactTelNo;
        return this;
    }

    public PublicationContactDtoBuilder WithTeamEmail(string teamEmail)
    {
        _teamEmail = teamEmail;
        return this;
    }

    public PublicationContactDtoBuilder WithTeamName(string teamName)
    {
        _teamName = teamName;
        return this;
    }
}
