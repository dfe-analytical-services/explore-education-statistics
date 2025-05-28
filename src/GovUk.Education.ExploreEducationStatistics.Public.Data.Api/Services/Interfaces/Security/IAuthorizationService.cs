using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;

public interface IAuthorizationService
{
    bool CanAccessUnpublishedData();

    Task<bool> RequestHasValidPreviewToken(DataSet dataSet);

    Task<bool> RequestHasValidPreviewToken(DataSetVersion dataSetVersion);
}
