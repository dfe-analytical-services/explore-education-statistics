using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Utils;

public class ScreenerResponseUtility
{
    public static string GetDataSetUploadStatus(DataSetScreenerResponse? screenerResult)
    {
        if (screenerResult is null)
        {
            return nameof(DataSetUploadStatus.SCREENER_ERROR);
        }

        if (screenerResult.Passed && screenerResult.TestResults.Any(test => test.Result == TestResult.WARNING))
        {
            return nameof(DataSetUploadStatus.PENDING_REVIEW);
        }

        return !screenerResult.Passed
            ? nameof(DataSetUploadStatus.FAILED_SCREENING)
            : nameof(DataSetUploadStatus.PENDING_IMPORT);
    }
}
