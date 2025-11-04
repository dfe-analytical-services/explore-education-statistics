using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetScreenerClient
{
    Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetRequest,
        CancellationToken cancellationToken
    );
}
