namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task UpdateEinTiles(Guid[] releaseVersionIdsToUpdate, CancellationToken cancellationToken = default);
}
