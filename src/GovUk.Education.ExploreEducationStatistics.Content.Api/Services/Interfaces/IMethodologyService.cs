using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IMethodologyService
    {
        List<ThemeTree> GetTree();
    }
}
