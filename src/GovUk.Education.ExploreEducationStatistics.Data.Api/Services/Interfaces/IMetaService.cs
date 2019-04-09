using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IMetaService
    {
        PublicationMetaViewModel GetPublicationMeta(Guid publicationId);

        SubjectMetaViewModel GetSubjectMeta(long subjectId);
    }
}