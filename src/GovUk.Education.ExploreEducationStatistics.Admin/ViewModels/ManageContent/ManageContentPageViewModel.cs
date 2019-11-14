using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class ManageContentPageViewModel
    {
        public ReleaseViewModel Release { get; set; } = new ReleaseViewModel();
        
        public List<BasicLink> PreviousReleases { get; set; } = new List<BasicLink>();

        public List<ReleaseNoteViewModel> ReleaseNotes { get; set; } = new List<ReleaseNoteViewModel>();

        public List<BasicLink> RelatedInformation { get; set; } = new List<BasicLink>();
        
        public ContentSectionViewModel IntroductionSection { get; set; } = new ContentSectionViewModel();
        
        public List<ContentSectionViewModel> ContentSections { get; set; } = new List<ContentSectionViewModel>();
    }
    
    public class ReleaseViewModel
    {
        public Guid Id { get; set; }
        
        public Guid? TypeId { get; set; }
        
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }
        
        public DateTime? PublishScheduled { get; set; }
        
        public PartialDate NextReleaseDate { get; set; }
     
        public string Title { get; set; }

        public string YearTitle { get; set; }

        public string CoverageTitle { get; set; }

        public string ReleaseName { get; set; }

        public string Slug { get; set; }

        public string PublicationTitle { get; set; }

        public Guid PublicationId { get; set; }
        
        public Contact Contact { get; set; }
    }

    public class ReleaseNoteViewModel
    {
        public string Content { get; set; }
        
        public DateTime PublishedDate { get; set; }
    }

    public class ContentSectionViewModel
    {
        public Guid Id { get; set; }
        
        public int Order { get; set; }

        public string Heading { get; set; }

        public string Caption { get; set; }

        public List<IContentBlock> Content { get; set; }
    }
}