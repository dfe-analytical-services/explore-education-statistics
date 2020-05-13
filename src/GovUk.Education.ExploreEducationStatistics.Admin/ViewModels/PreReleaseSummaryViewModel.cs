namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class PreReleaseSummaryViewModel
    {
        public string PublicationSlug { get; }

        public string ReleaseSlug { get; }

        public string PublicationTitle { get; }

        public string ReleaseTitle { get; }

        public string ContactEmail { get; }

        public PreReleaseSummaryViewModel()
        {
        }

        public PreReleaseSummaryViewModel(
            string publicationSlug,
            string publicationTitle,
            string releaseSlug,
            string releaseTitle,
            string contactEmail)
        {
            PublicationSlug = publicationSlug;
            ReleaseSlug = releaseSlug;
            PublicationTitle = publicationTitle;
            ReleaseTitle = releaseTitle;
            ContactEmail = contactEmail;
        }
    }
}
