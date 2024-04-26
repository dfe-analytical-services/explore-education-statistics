namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public interface IDataSetService
{
    Task<Guid> CreateDataSetVersion(Guid releaseFileId,
        Guid? dataSetId = null,
        CancellationToken cancellationToken = default);
}
