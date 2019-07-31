namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IGeoJsonService
    {
        GeoJson Find(long boundaryLevelId, string code);
    }
}