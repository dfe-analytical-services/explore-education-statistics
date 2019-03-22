using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IContentService
    {
        List<ThemeTree> GetContentTree();
    }
}