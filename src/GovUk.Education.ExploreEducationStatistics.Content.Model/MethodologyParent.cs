using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyParent
    {
        public Guid Id { get; set; }

        public List<PublicationMethodology> Publications { get; set; }

        [Required]
        public string OwningPublicationTitle { get; set; }

        [Required]
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
