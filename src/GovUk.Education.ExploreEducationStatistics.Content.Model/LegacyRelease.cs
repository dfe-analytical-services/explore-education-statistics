using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class LegacyRelease
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }

        public Guid PublicationId { get; set; }

        public LegacyRelease CreateCopy()
        {
            return MemberwiseClone() as LegacyRelease;
        }
    }
}