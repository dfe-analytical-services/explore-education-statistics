#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class StageReleaseContentMessage
    {
        public IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> Releases;

        public StageReleaseContentMessage(IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> releases)
        {
            Releases = releases;
        }

        public override string ToString()
        {
            return $"{nameof(Releases)}: {string.Join(", ", Releases)}";
        }
    }
}
