using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IPublicationService
    {
        PublicationViewModel GetPublication(string slug);
    }
}