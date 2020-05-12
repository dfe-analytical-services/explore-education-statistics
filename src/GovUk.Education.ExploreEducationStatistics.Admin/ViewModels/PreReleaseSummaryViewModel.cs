namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class PreReleaseSummaryViewModel
    {
        public string PublicationTitle { get; }

        public string ReleaseTitle { get; }

        public PreReleaseSummaryViewModel(string publicationTitle, string releaseTitle)
        {
            PublicationTitle = publicationTitle;
            ReleaseTitle = releaseTitle;
        }
    }
}