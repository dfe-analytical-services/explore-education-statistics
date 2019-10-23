using System;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IPublicationMetaService
    {
        PublicationSubjectsMetaViewModel GetSubjectsForLatestRelease(Guid publicationId);
    }
}