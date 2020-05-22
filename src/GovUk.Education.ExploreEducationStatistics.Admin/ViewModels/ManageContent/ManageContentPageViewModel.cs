using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class ManageContentPageViewModel
    {
        public ReleaseViewModel Release { get; set; } = new ReleaseViewModel();
        
        public List<DataBlock> AvailableDataBlocks { get; set; } = new List<DataBlock>();
    }
    
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
        public ReleaseStatus Status { get; set; }

        public PublicationViewModel Publication { get; set; }
        
        public bool LatestRelease { get; set; } 
        
        public ReleaseType Type { get; set; }

        public List<ReleaseNoteViewModel> Updates { get; set; } = new List<ReleaseNoteViewModel>();
        
        public List<ContentSectionViewModel> Content { get; set; } = new List<ContentSectionViewModel>();
        
        public ContentSectionViewModel SummarySection { get; set; } = new ContentSectionViewModel();
        
        public ContentSectionViewModel HeadlinesSection { get; set; } = new ContentSectionViewModel();
        
        public ContentSectionViewModel KeyStatisticsSection { get; set; } = new ContentSectionViewModel();
        
        public ContentSectionViewModel KeyStatisticsSecondarySection { get; set; } = new ContentSectionViewModel();

        public IEnumerable<FileInfo> DownloadFiles { get; set; }
        
        public DateTime? PublishScheduled { get; set; }

        public PartialDate NextReleaseDate { get; set; }
        
        public List<Link> RelatedInformation { get; set; } = new List<Link>();
    }

    public class PublicationViewModel
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }
        
        public string DataSource { get; set; }
        
        public string Summary { get; set; }

        public List<PreviousReleaseViewModel> OtherReleases { get; set; }
        
        public List<LegacyReleaseViewModel> LegacyReleases { get; set; }

        public TopicViewModel Topic { get; set; }
        
        public Contact Contact { get; set; }
        
        public MethodologyTitleViewModel Methodology { get; set; }
        
        public ExternalMethodology ExternalMethodology { get; set; }
    }

    public class KeyStatisticsViewModel
    {
        public List<DataBlock> KeyIndicators { get; set; }
        
        public ContentSectionViewModel KeyStatisticsContent { get; set; }
    }

    public class ReleaseNoteViewModel
    {
        public Guid Id { get; set; }
        
        public string Reason { get; set; }
        
        public DateTime On { get; set; }
    }
    
    public class PreviousReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }
    }
    
    public class LegacyReleaseViewModel
    {
        public Guid Id { get; set; }
        
        public string Description { get; set; }

        public string Url { get; set; }
    }

    public class ThemeViewModel
    {
        public string Title { get; set; }
    }
    
    public class TopicViewModel
    {
        public ThemeViewModel Theme;
    }
}