using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Release
    {
        public long Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Guid PublicationId { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }

        public Release(DateTime releaseDate, Guid publicationId)
        {
            ReleaseDate = releaseDate;
            PublicationId = publicationId;
        }
    }
}