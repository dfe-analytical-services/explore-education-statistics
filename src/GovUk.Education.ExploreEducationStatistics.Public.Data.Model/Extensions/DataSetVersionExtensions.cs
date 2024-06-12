namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;

public static class DataSetVersionExtensions
{
    public static bool CanBeDeleted(this DataSetVersion dataSetVersion) => dataSetVersion.Status.IsDeletableState();

    public static bool IsDeletableState(this DataSetVersionStatus status) => status is DataSetVersionStatus.Failed
        or DataSetVersionStatus.Mapping
        or DataSetVersionStatus.Draft
        or DataSetVersionStatus.Cancelled;
}
