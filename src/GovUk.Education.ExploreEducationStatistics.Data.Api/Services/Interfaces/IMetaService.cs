using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IMetaService
    {
        PublicationMetaViewModel GetPublicationMeta(Guid publicationId);

        SubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query);
    }
}