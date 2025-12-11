namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task UpdateEinTiles(Guid[] releaseVersionIdsToUpdate);
}
