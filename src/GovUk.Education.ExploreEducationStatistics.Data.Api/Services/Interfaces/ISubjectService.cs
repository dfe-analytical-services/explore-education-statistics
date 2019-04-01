using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ISubjectService : IDataService<Subject>
    {
        IEnumerable<SubjectMetaViewModel> GetSubjectMetas(Guid publicationId);

        Dictionary<string, IEnumerable<IndicatorMetaViewModel>> GetIndicatorMetas(long subjectId);

        Dictionary<string, IEnumerable<CharacteristicMetaViewModel>> GetCharacteristicMetas(long subjectId);
    }
}