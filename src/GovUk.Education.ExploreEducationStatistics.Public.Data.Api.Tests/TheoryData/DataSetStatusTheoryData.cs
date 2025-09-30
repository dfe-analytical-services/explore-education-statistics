using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;

public class DataSetStatusTheoryData
{
    private static readonly List<DataSetStatus> AvailableStatusesList = [
        DataSetStatus.Deprecated,
        DataSetStatus.Published,
    ];

    public static readonly TheoryData<DataSetStatus> AllStatuses = new(
        EnumUtil.GetEnums<DataSetStatus>()
    );

    public static readonly TheoryData<DataSetStatus> AvailableStatuses = new(AvailableStatusesList);

    public static readonly TheoryData<DataSetStatus> UnavailableStatuses = new(
        EnumUtil.GetEnums<DataSetStatus>().Except(AvailableStatusesList)
    );
}
