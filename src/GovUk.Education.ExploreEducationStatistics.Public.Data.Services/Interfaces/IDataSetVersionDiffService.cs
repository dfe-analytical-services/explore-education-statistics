namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

public interface IDataSetVersionDiffService
{
    Task<bool> IsMajorVersionUpdate(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken);
}
