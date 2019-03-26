using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Models
{
    public class Publication
    {
        public Guid PublicationId { get; set; }
        public string Name { get; set; }
        public Release[] Releases { get; set; }
    }
}