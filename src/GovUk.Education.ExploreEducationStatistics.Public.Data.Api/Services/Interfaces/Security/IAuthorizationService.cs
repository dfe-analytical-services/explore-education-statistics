namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;

public interface IAuthorizationService
{
    bool CanAccessUnpublishedData();
}
