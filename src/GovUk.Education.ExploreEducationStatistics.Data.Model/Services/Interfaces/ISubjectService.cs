using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IDataService<Subject, long>
    {
        Dictionary<string, IEnumerable<IndicatorMeta>> GetIndicatorMetaGroups(long subjectId);

        Dictionary<string, IEnumerable<CharacteristicMeta>> GetCharacteristicMetaGroups(long subjectId);
    }
}