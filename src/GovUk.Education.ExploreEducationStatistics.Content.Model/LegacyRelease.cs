using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class LegacyRelease
    {
        public Guid Id { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        [Required]
        public int Order { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }

        public LegacyRelease CreateCopy()
        {
            return MemberwiseClone() as LegacyRelease;
        }
    }
}