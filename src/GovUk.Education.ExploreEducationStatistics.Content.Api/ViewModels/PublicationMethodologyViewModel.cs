using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels
{
    public class PublicationMethodologyViewModel
    {
        public ExternalMethodologyViewModel ExternalMethodology { get; set; }

        public MethodologySummaryViewModel Methodology { get; set; }
    }
}
