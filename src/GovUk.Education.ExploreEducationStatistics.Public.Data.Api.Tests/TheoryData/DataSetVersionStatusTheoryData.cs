using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;

public static class DataSetVersionStatusQueryTheoryData
{
    private static readonly List<DataSetVersionStatus> AvailableStatusesList =
    [
        DataSetVersionStatus.Published,
        DataSetVersionStatus.Deprecated,
    ];

    public static readonly TheoryData<DataSetVersionStatus> AvailableStatuses = new(
        AvailableStatusesList
    );

    public static readonly TheoryData<DataSetVersionStatus> AvailableStatusesIncludingDraft = new(
        [.. AvailableStatusesList, DataSetVersionStatus.Draft]
    );

    public static readonly TheoryData<DataSetVersionStatus> UnavailableStatuses = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except(AvailableStatusesList)
    );

    public static readonly TheoryData<DataSetVersionStatus> UnavailableStatusesExceptDraft = new(
        EnumUtil
            .GetEnums<DataSetVersionStatus>()
            .Except([.. AvailableStatusesList, DataSetVersionStatus.Draft])
    );

    public static readonly TheoryData<DataSetVersionStatus> NonPublishedStatus = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except([DataSetVersionStatus.Published])
    );
}

public static class DataSetVersionStatusViewTheoryData
{
    private static readonly List<DataSetVersionStatus> AllStatusesList =
        EnumUtil.GetEnums<DataSetVersionStatus>();

    private static readonly List<DataSetVersionStatus> AvailableStatusesList =
    [
        DataSetVersionStatus.Published,
        DataSetVersionStatus.Withdrawn,
        DataSetVersionStatus.Deprecated,
    ];

    public static readonly TheoryData<DataSetVersionStatus> AllStatuses = new(AllStatusesList);

    public static readonly TheoryData<DataSetVersionStatus> AvailableStatuses = new(
        AvailableStatusesList
    );

    public static readonly TheoryData<DataSetVersionStatus> AvailableStatusesIncludingDraft = new(
        [.. AvailableStatusesList, DataSetVersionStatus.Draft]
    );

    public static readonly TheoryData<DataSetVersionStatus> UnavailableStatuses = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except(AvailableStatusesList)
    );

    public static readonly TheoryData<DataSetVersionStatus> UnavailableStatusesExceptDraft = new(
        EnumUtil
            .GetEnums<DataSetVersionStatus>()
            .Except([.. AvailableStatusesList, DataSetVersionStatus.Draft])
    );

    public static readonly TheoryData<DataSetVersionStatus> NonPublishedStatus = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except([DataSetVersionStatus.Published])
    );
}
