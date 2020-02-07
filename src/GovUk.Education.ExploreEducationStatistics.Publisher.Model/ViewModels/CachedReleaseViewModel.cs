using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class CachedReleaseViewModel
    {
        public CachedReleaseViewModel()
        {
        }

        protected CachedReleaseViewModel(Guid id, string title, string yearTitle, string coverageTitle, string releaseName, DateTime? published, string slug, ReleaseTypeViewModel type, List<ReleaseNoteViewModel> updates, List<ContentSectionViewModel> content, ContentSectionViewModel summarySection, ContentSectionViewModel headlinesSection, ContentSectionViewModel keyStatisticsSection, ContentSectionViewModel keyStatisticsSecondarySection, List<FileInfo> downloadFiles, List<LinkViewModel> relatedInformation)
        {
            Id = id;
            Title = title;
            YearTitle = yearTitle;
            CoverageTitle = coverageTitle;
            ReleaseName = releaseName;
            Published = published;
            Slug = slug;
            Type = type;
            Updates = updates;
            Content = content;
            SummarySection = summarySection;
            HeadlinesSection = headlinesSection;
            KeyStatisticsSection = keyStatisticsSection;
            KeyStatisticsSecondarySection = keyStatisticsSecondarySection;
            DownloadFiles = downloadFiles;
            RelatedInformation = relatedInformation;
        }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public string YearTitle { get; set; }
        
        public string CoverageTitle { get; set; }
        
        public string ReleaseName { get; set; }
        
        public DateTime? Published { get; set; }

        public string Slug { get; set; }

        public ReleaseTypeViewModel Type { get; set; }

        public List<ReleaseNoteViewModel> Updates { get; set; }
        
        public List<ContentSectionViewModel> Content { get; set; }
        
        public ContentSectionViewModel SummarySection { get; set; }
        
        public ContentSectionViewModel HeadlinesSection { get; set; }
        
        public ContentSectionViewModel KeyStatisticsSection { get; set; }
        
        public ContentSectionViewModel KeyStatisticsSecondarySection { get; set; }
        
        public List<FileInfo> DownloadFiles { get; set; }
        
        public List<LinkViewModel> RelatedInformation { get; set; }
    }
}