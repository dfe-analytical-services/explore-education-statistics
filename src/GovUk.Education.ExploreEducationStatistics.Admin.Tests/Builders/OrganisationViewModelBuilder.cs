#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Builders;

public class OrganisationViewModelBuilder
{
    private Guid _id = Guid.NewGuid();
    private string? _gisLogoHexCode = "GIS logo hex code";
    private string _logoFileName = "Logo file name";
    private string _title = "Title";
    private string _url = "Url";
    private bool _useGisLogo = true;

    public OrganisationViewModel Build() =>
        new()
        {
            Id = _id,
            GISLogoHexCode = _gisLogoHexCode,
            LogoFileName = _logoFileName,
            Title = _title,
            Url = _url,
            UseGISLogo = _useGisLogo,
        };

    public OrganisationViewModelBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrganisationViewModelBuilder WithGisLogoHexCode(string? gisLogoHexCode)
    {
        _gisLogoHexCode = gisLogoHexCode;
        return this;
    }

    public OrganisationViewModelBuilder WithLogoFileName(string logoFileName)
    {
        _logoFileName = logoFileName;
        return this;
    }

    public OrganisationViewModelBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public OrganisationViewModelBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public OrganisationViewModelBuilder WithUseGisLogo(bool useGisLogo)
    {
        _useGisLogo = useGisLogo;
        return this;
    }
}
