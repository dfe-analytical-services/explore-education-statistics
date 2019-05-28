using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPublicationMetaService
    {
        PublicationMetaViewModel GetPublicationMeta(Guid publicationId);
    }
}