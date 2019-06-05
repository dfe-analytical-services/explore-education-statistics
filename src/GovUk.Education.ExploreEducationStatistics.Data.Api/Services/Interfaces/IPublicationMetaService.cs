using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IPublicationMetaService
    {
        PublicationSubjectsMetaViewModel GetPublication(Guid publicationId);

        IEnumerable<ThemeMetaViewModel> GetThemes();
    }
}