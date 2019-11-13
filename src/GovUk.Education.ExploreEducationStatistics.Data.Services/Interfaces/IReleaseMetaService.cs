using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces
{
    public interface IReleaseMetaService
    {
        IEnumerable<IdLabel> GetSubjects(Guid releaseId);
    }
}