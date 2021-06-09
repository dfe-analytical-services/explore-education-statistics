using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyParent
    {
        public Guid Id { get; set; }

        public List<PublicationMethodology> Publications { get; set; }

        public List<Methodology> Versions { get; set; }

        public Methodology LatestVersion()
        {
            return Versions?.SingleOrDefault(mv => IsLatestVersionOfMethodology(mv.Id));
        }

        public bool IsLatestVersionOfMethodology(Guid methodologyVersionId)
        {
            return Versions != null
                   && Versions.Exists(mv => mv.Id == methodologyVersionId)
                   && Versions.All(mv => mv.PreviousVersionId != methodologyVersionId);
        }
    }
}
