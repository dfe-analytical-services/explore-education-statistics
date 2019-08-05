namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IBoundaryLevelService : IRepository<BoundaryLevel, long>
    {
        BoundaryLevel FindLatestByGeographicLevel(GeographicLevel geographicLevel);
    }
}