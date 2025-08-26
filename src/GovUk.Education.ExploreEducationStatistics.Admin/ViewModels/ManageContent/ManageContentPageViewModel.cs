using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public class ManageContentPageViewModel
{
    public ReleaseViewModel Release { get; set; } = new();

    public List<DataBlockViewModel> UnattachedDataBlocks { get; set; } = new();

    public class ReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string YearTitle { get; set; }

        public string CoverageTitle { get; set; }

        public string ReleaseName { get; set; }

        public DateTime? Published { get; set; }

        public string Slug { get; set; }

        public Guid PublicationId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public PublicationViewModel Publication { get; set; }

        public bool LatestRelease { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; set; }

        public List<OrganisationViewModel> PublishingOrganisations { get; set; } = [];

        public List<ReleaseNoteViewModel> Updates { get; set; } = [];

        public List<ContentSectionViewModel> Content { get; set; } = [];

        public ContentSectionViewModel SummarySection { get; set; } = new();

        public ContentSectionViewModel HeadlinesSection { get; set; } = new();

        public List<KeyStatisticViewModel> KeyStatistics { get; set; } = [];

        public ContentSectionViewModel KeyStatisticsSecondarySection { get; set; } = new();

        public ContentSectionViewModel RelatedDashboardsSection { get; set; } = new();

        public IEnumerable<FileInfo> DownloadFiles { get; set; }

        public bool HasPreReleaseAccessList { get; set; }

        public bool HasDataGuidance => DownloadFiles.Any(file => file.Type == FileType.Data);

        [JsonConverter(typeof(DateTimeToDateJsonConverter))]
        public DateTime? PublishScheduled { get; set; }

        public PartialDate NextReleaseDate { get; set; }

        public List<Link> RelatedInformation { get; set; } = [];
    }

    public class PublicationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public List<ReleaseSeriesItemViewModel> ReleaseSeries { get; set; }

        public Contact Contact { get; set; }

        public List<IdTitleViewModel> Methodologies { get; set; }

        public ExternalMethodology ExternalMethodology { get; set; }
    }
}

public class ReleaseNoteViewModel
{
    public Guid Id { get; set; }

    public string Reason { get; set; }

    public DateTime On { get; set; }
}
