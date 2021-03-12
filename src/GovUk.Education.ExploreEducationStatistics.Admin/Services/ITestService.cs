using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    // TODO EES-1991 Rename this or merge it into another service
    public interface ITestService
    {
        public List<Guid> GetImages(string content);
    }
}
