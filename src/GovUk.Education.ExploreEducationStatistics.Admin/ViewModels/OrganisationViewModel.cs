#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record OrganisationViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }

    public bool UseGISLogo { get; set; }

    public string? GISLogoHexCode { get; set; }

    public required string LogoFileName { get; set; }

    public static OrganisationViewModel FromOrganisation(Organisation organisation) =>
        new()
        {
            Id = organisation.Id,
            Title = organisation.Title,
            Url = organisation.Url,
            UseGISLogo = organisation.UseGISLogo,
            GISLogoHexCode = organisation.GISLogoHexCode,
            LogoFileName = organisation.LogoFileName,
        };
}
