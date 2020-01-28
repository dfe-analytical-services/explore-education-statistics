using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class GenerateReleaseContentMessage
    {
        public IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> Releases;
    }
}