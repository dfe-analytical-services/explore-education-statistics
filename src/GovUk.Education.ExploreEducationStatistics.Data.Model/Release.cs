using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Release
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Slug { get; set; }
        public Publication Publication { get; set; }
        public Guid PublicationId { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }
    }
}