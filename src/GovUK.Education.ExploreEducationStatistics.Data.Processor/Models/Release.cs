namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    using System;

    public class Release
    {
        public Guid PublicationId { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Name { get; set; }

        public Subject[] Subjects { get; set; }
    }
}