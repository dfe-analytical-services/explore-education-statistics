using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Models
{
    public class Release
    {
        public Guid PublicationId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Name { get; set; }
        public Subject[] Subjects { get; set; }
    }
}