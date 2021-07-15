using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyParent
    {
        public Guid Id { get; set; }

        public List<PublicationMethodology> Publications { get; set; }

        // TODO SOW4 EES-2375 Move slug to parent
        // Need to sort migration for this field
        [NotMapped]
        public string Slug { get; set; }

        public List<Methodology> Versions { get; set; }

        public Methodology LatestVersion()
        {
            if (Versions == null)
            {
                throw new ArgumentException("Methodology must be hydrated with Versions to get the latest version");
            }

            return Versions.SingleOrDefault(mv => IsLatestVersionOfMethodology(mv.Id));
        }

        public bool IsLatestVersionOfMethodology(Guid methodologyVersionId)
        {
            if (Versions == null)
            {
                throw new ArgumentException("Methodology must be hydrated with Versions to test the latest version");
            }

            return Versions.Exists(mv => mv.Id == methodologyVersionId)
                   && Versions.All(mv => mv.PreviousVersionId != methodologyVersionId);
        }
    }
}
