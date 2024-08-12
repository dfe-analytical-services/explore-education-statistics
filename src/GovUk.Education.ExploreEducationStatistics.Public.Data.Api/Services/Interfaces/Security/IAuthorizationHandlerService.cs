namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;

public interface IAuthorizationHandlerService
{
    bool CanAccessUnpublishedData();
}
