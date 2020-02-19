using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Release
    {
        public Guid Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Slug { get; set; }
        public Publication Publication { get; set; }
        public Guid PublicationId { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public int Year { get; set; }

        public bool Live => Compare(UtcNow, ReleaseDate) > 0;
    }
}