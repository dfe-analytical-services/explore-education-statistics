using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;

public static class DataSetVersionStatusTheoryData
{
    private static readonly List<DataSetVersionStatus> DeletableStatusList =
    [
        DataSetVersionStatus.Failed,
        DataSetVersionStatus.Mapping,
        DataSetVersionStatus.Draft,
        DataSetVersionStatus.Cancelled,
    ];

    public static readonly TheoryData<DataSetVersionStatus> DeletableStatuses = new(
        DeletableStatusList
    );

    public static readonly TheoryData<DataSetVersionStatus> NonDeletableStatuses = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except(DeletableStatusList)
    );

    public static readonly TheoryData<DataSetVersionStatus> StatusesExceptMapping = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except([DataSetVersionStatus.Mapping])
    );
}
