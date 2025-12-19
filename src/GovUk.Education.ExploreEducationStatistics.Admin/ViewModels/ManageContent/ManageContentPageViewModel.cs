#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ManageContentPageViewModel
{
    public required ReleaseViewModel Release { get; init; }

    public required List<DataBlockViewModel> UnattachedDataBlocks { get; init; }

    public record ReleaseViewModel
    {
        public required Guid Id { get; init; }

        public required string Title { get; init; }

        public required string YearTitle { get; init; }

        public required string CoverageTitle { get; init; }

        public required string ReleaseName { get; init; }

        public required DateTimeOffset? Published { get; init; }

        public required string Slug { get; init; }

        public required Guid PublicationId { get; init; }

        [JsonConverter(typeof(StringEnumConverter))]
        public required ReleaseApprovalStatus ApprovalStatus { get; init; }

        public PublicationViewModel Publication { get; set; } = null!;

        public required bool LatestRelease { get; init; }

        [JsonConverter(typeof(StringEnumConverter))]
        public required ReleaseType Type { get; init; }

        public required List<OrganisationViewModel> PublishingOrganisations { get; init; }

        public required List<ReleaseNoteViewModel> Updates { get; init; }

        public required List<ContentSectionViewModel> Content { get; init; }

        public required ContentSectionViewModel SummarySection { get; init; }

        public required ContentSectionViewModel HeadlinesSection { get; init; }

        public required List<KeyStatisticViewModel> KeyStatistics { get; init; }

        public required ContentSectionViewModel KeyStatisticsSecondarySection { get; init; }

        public required ContentSectionViewModel RelatedDashboardsSection { get; init; }

        public IEnumerable<FileInfo> DownloadFiles { get; set; } = [];

        public required bool HasPreReleaseAccessList { get; init; }

        public bool HasDataGuidance => DownloadFiles.Any(file => file.Type == FileType.Data);

        public required DateOnly? PublishScheduled { get; init; }

        public required PartialDate? NextReleaseDate { get; init; }

        public required List<Link> RelatedInformation { get; init; }
    }

    public record PublicationViewModel
    {
        public required Guid Id { get; init; }

        public required string Summary { get; init; }

        public required string Title { get; init; }

        public required string Slug { get; init; }

        public required List<ReleaseSeriesItemViewModel> ReleaseSeries { get; init; }

        public required Contact Contact { get; init; }

        public required List<IdTitleViewModel> Methodologies { get; init; }

        public required ExternalMethodology? ExternalMethodology { get; init; }
    }
}

public class ReleaseNoteViewModel
{
    public Guid Id { get; set; }

    public string Reason { get; set; }

    public DateTime On { get; set; }
}
