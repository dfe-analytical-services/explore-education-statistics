using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces
{
    public interface IMethodologyService
    {
        List<ThemeTree> GetTree();

        Methodology Get(Guid id);

        IEnumerable<Methodology> Get();
    }
}
