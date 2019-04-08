using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ICharacteristicDataService : IDataService<CharacteristicData, long>
    {
        LevelMeta GetLevelMeta(long subjectId);
    }
}