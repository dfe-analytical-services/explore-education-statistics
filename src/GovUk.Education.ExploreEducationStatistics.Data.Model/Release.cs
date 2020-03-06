using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Release
    {
        public Guid Id { get; set; }
        public DateTime? Published { get; set; }
        public string Slug { get; set; }
        public Publication Publication { get; set; }
        public Guid PublicationId { get; set; }
        public IEnumerable<Subject> Subjects { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public int Year { get; set; }

        // TODO EES-1417 ReleaseDate originates from the scheduled publish date of the Content Release set in the Admin.
        // TODO (See the Release summary tab). It gets copied to the Statistics db as the ReleaseDate field.
        // TODO This live check should use an actual Published date (or flag) that is set once content is published
        // TODO by the PublishReleaseContentFunction. See method PublishingService.PublishReleaseContentAsync
        // TODO where the Published date on a Release is set in the Content db for the same Live check there.
        public bool Live => Published.HasValue && Compare(UtcNow, Published.Value) > 0;
    }
}