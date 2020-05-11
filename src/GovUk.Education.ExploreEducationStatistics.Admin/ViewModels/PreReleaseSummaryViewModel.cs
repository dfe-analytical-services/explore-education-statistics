namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class PreReleaseSummaryViewModel
    {
        public string PublicationTitle { get; }

        public string ReleaseTitle { get; }

        public string ContactEmail { get; }

        public PreReleaseSummaryViewModel(string publicationTitle, string releaseTitle, string contactEmail)
        {
            PublicationTitle = publicationTitle;
            ReleaseTitle = releaseTitle;
            ContactEmail = contactEmail;
        }
    }
}