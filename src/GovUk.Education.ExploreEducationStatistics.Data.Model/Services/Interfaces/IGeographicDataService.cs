using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IGeographicDataService : IDataService<GeographicData, long>
    {
        LevelMeta GetLevelMeta(long subjectId);
    }
}