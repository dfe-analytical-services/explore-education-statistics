using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseCacheViewModel(Guid Id)
{
    /// <summary>
    /// Release Version Id
    /// </summary>
    public Guid Id { get; set; } = Id;

    public Guid ReleaseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string YearTitle { get; set; } = string.Empty;

    public string CoverageTitle { get; set; } = string.Empty;

    public string ReleaseName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; set; }

    public PartialDate? NextReleaseDate { get; set; }

    public DateTime? Published { get; set; }

    public IEnumerable<OrganisationViewModel> PublishingOrganisations { get; init; } = [];

    public List<ReleaseNoteViewModel> Updates { get; set; } = [];

    public List<ContentSectionViewModel> Content { get; set; } = [];

    public ContentSectionViewModel SummarySection { get; set; } = null!;

    public ContentSectionViewModel HeadlinesSection { get; set; } = null!;

    public List<KeyStatisticViewModel> KeyStatistics { get; set; } = [];

    public ContentSectionViewModel KeyStatisticsSecondarySection { get; set; } = null!;

    public ContentSectionViewModel? RelatedDashboardsSection { get; set; }

    public List<Common.Model.FileInfo> DownloadFiles { get; set; } = [];

    public string DataGuidance { get; set; } = string.Empty;

    public string PreReleaseAccessList { get; set; } = string.Empty;

    public List<LinkViewModel> RelatedInformation { get; set; } = [];
}
