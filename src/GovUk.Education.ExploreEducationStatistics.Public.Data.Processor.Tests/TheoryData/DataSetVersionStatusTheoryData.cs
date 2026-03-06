using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;

public static class DataSetVersionStatusTheoryData
{
    private static readonly List<DataSetVersionStatus> AllStatuses = EnumUtil.GetEnums<DataSetVersionStatus>();

    private static readonly List<DataSetVersionStatus> PostMappingStatuses =
    [
        DataSetVersionStatus.Finalising,
        DataSetVersionStatus.Published,
        DataSetVersionStatus.Withdrawn,
    ];

    private static readonly List<DataSetVersionStatus> PreMappingsFinalisedMappingStatuses = AllStatuses
        .Except(PostMappingStatuses)
        .ToList();

    private static readonly List<DataSetVersionStatus> DeletableStatusList =
    [
        DataSetVersionStatus.Failed,
        DataSetVersionStatus.Mapping,
        DataSetVersionStatus.Draft,
        DataSetVersionStatus.Cancelled,
    ];

    public static readonly TheoryData<DataSetVersionStatus> PostMappingStatusesTheoryData = new(PostMappingStatuses);

    public static readonly TheoryData<DataSetVersionStatus> DeletableStatuses = new(DeletableStatusList);

    public static readonly TheoryData<DataSetVersionStatus> NonDeletableStatuses = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except(DeletableStatusList)
    );

    public static readonly TheoryData<DataSetVersionStatus> StatusesExceptMapping = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except([DataSetVersionStatus.Mapping])
    );

    public static readonly TheoryData<DataSetVersionStatus> PreMappingsFinalisedStatusesTheoryData = new(
        PreMappingsFinalisedMappingStatuses
    );
}
