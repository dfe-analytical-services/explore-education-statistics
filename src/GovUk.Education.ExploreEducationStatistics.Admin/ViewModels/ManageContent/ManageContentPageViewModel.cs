using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class ManageContentPageViewModel
    {
        public ReleaseViewModel Release { get; set; } = new ReleaseViewModel();

        public List<BasicLink> RelatedInformation { get; set; } = new List<BasicLink>();
        
        public ContentSectionViewModel IntroductionSection { get; set; } = new ContentSectionViewModel();
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

        public string Summary { get; set; }

        public Guid PublicationId { get; set; }

        public PublicationViewModel Publication { get; set; }
        
        public bool LatestRelease { get; set; } 
        
        public ReleaseType Type { get; set; }

        public List<ReleaseNoteViewModel> Updates { get; set; } = new List<ReleaseNoteViewModel>();
        
        public List<ContentSectionViewModel> Content { get; set; } = new List<ContentSectionViewModel>();
        
        public KeyStatisticsViewModel KeyStatistics { get; set; }
        
        // dataFiles
        
        // downloadFiles
        
        // others...
        
//        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
//        public TimeIdentifier TimePeriodCoverage { get; set; }
        
// TODO
        public DateTime? PublishScheduled { get; set; }
        
// TODO
        public PartialDate NextReleaseDate { get; set; }
    }

// TODO
    public class PublicationViewModel
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        // needed?
// TODO
        public string Slug { get; set; }

        // needed?
        public string Description { get; set; }
        
        // needed?
        public string DataSource { get; set; }
        
        // needed?
        public string Summary { get; set; }
        
        // needed?
        public DateTime? NextUpdate { get; set; }
        
        public List<PreviousReleaseViewModel> Releases { get; set; }
        
        public List<BasicLink> LegacyReleases { get; set; }

        public TopicViewModel Topic { get; set; }
        
        public Contact Contact { get; set; }
    }

    public class KeyStatisticsViewModel
    {
        public List<DataBlock> KeyIndicators { get; set; }
        
        public ContentSectionViewModel KeyStatisticsContent { get; set; }
    }

    public class ReleaseNoteViewModel
    {
        public Guid Id { get; set; }
        
        // was content
        public string Reason { get; set; }
        
        // was publisheddate
        public DateTime On { get; set; }
    }
    
    public class PreviousReleaseViewModel
    {
        public Guid Id { get; set; }
        
        public string ReleaseName { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }
    }

    public class ContentSectionViewModel
    {
        public Guid Id { get; set; }
        
        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public List<IContentBlock> Content { get; set; }
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