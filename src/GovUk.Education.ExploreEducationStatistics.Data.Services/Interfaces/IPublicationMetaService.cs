using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IPublicationMetaService
    {
        PublicationSubjectsMetaViewModel GetPublication(Guid publicationId);

        IEnumerable<ThemeMetaViewModel> GetThemes();
    }
}