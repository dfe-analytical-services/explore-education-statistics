using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetScreenerClient
{
    Task<DataSetScreenerResult> ScreenDataSet(
        DataSetScreenerRequest dataSetRequest,
        CancellationToken cancellationToken);
}
