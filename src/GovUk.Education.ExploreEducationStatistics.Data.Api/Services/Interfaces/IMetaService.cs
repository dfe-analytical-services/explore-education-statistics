using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IMetaService
    {
        PublicationMetaViewModel GetPublicationMeta(Guid publicationId);

        SubjectMetaViewModel GetSubjectMeta(long subjectId,
            IEnumerable<int> years = null);

        SubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query);
    }
}