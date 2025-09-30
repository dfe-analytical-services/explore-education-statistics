using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.TheoryData;

public static class DataSetVersionStatusTheoryData
{
    private static readonly List<DataSetVersionStatus> AvailableStatusesList =
    [
        DataSetVersionStatus.Deprecated,
        DataSetVersionStatus.Published,
        DataSetVersionStatus.Withdrawn,
    ];

    private static readonly List<DataSetVersionStatus> UpdateableStatusesList =
    [
        DataSetVersionStatus.Draft,
        DataSetVersionStatus.Mapping,
    ];

    public static readonly TheoryData<DataSetVersionStatus> AvailableStatuses = new(
        AvailableStatusesList
    );

    public static readonly TheoryData<DataSetVersionStatus> UnavailableStatuses = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except(AvailableStatusesList)
    );

    public static readonly TheoryData<DataSetVersionStatus> UpdateableStatuses = new(
        UpdateableStatusesList
    );

    public static readonly TheoryData<DataSetVersionStatus> ReadOnlyStatuses = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except(UpdateableStatusesList)
    );

    public static readonly TheoryData<DataSetVersionStatus> StatusesExceptDraft = new(
        EnumUtil.GetEnums<DataSetVersionStatus>().Except([DataSetVersionStatus.Draft])
    );
}
