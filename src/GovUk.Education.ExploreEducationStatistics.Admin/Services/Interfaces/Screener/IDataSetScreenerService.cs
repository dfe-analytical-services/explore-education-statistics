using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;

/// <summary>
/// This interface acts as a facade behind which the Screener API is
/// interacted with using a mix of communication techniques, such as queue
/// triggers and HTTP requests.
/// </summary>
public interface IDataSetScreenerService
{
    Task<DataSetScreenerResponse> ScreenDataSet(
        DataSetScreenerRequest dataSetScreenerRequest,
        CancellationToken cancellationToken
    );

    Task StartScreening(DataSetStartScreeningRequest dataSetScreenRequest, CancellationToken cancellationToken);

    // ReSharper disable once UnusedMemberInSuper.Global
    /// <summary>
    /// This method will find data sets that are currently undergoing screening, and will request
    /// progress updates for them from the Screener API.  To prevent excessive numbers of checks,
    /// this method will also exclude any data sets that had their progress updated very recently.
    /// </summary>
    Task<List<DataSetScreenerProgressResponse>> UpdateScreeningProgress(CancellationToken cancellationToken);

    /// <summary>
    /// Get the progress of all data sets undergoing screening for a given ReleaseVersion.
    /// </summary>
    Task<Either<ActionResult, List<ScreenerProgressWithDataSetUploadIdViewModel>>> GetScreenerProgress(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    );
}
