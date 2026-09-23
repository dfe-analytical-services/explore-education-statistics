using GovUk.Education.ExploreEducationStatistics.Content.Services.Organisations.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Organisations;

public class OrganisationDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string? _gisLogoHexCode = "GIS logo hex code";
    private string _logoFileName = "Logo file name";
    private string _title = "Title";
    private string _url = "Url";
    private bool _useGisLogo = true;

    public OrganisationDto Build() =>
        new()
        {
            Id = _id,
            GISLogoHexCode = _gisLogoHexCode,
            LogoFileName = _logoFileName,
            Title = _title,
            Url = _url,
            UseGISLogo = _useGisLogo,
        };

    public OrganisationDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrganisationDtoBuilder WithGisLogoHexCode(string? gisLogoHexCode)
    {
        _gisLogoHexCode = gisLogoHexCode;
        return this;
    }

    public OrganisationDtoBuilder WithLogoFileName(string logoFileName)
    {
        _logoFileName = logoFileName;
        return this;
    }

    public OrganisationDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public OrganisationDtoBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public OrganisationDtoBuilder WithUseGisLogo(bool useGisLogo)
    {
        _useGisLogo = useGisLogo;
        return this;
    }
}
