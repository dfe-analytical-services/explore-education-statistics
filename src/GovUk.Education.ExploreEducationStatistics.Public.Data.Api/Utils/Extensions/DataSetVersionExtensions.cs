using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils.Extensions;

public static class DataSetVersionExtensions
{
    public static DataSetVersionType VersionType(this DataSetVersion dataSetVersion)
    {
        if (dataSetVersion.VersionMinor == 0)
        {
            return DataSetVersionType.Major;
        }

        return DataSetVersionType.Minor;
    }
}
