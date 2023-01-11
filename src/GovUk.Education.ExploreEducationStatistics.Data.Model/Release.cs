using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Release
    {
        public Guid Id { get; set; }
        public DateTime? Published { get; set; }
        public string Slug { get; set; }
        public Guid PublicationId { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public int Year { get; set; }
        public ICollection<ReleaseFootnote> Footnotes { get; set; }
        public bool Live => Published.HasValue && UtcNow >= Published.Value;
        public Guid? PreviousVersionId { get; set; }
        public int Version { get; set; }
        
        public string Title =>
            TimePeriodLabelFormatter.Format(Year, TimeIdentifier, TimePeriodLabelFormat.FullLabelBeforeYear);

        public Release CreateReleaseAmendment(Guid contentAmendmentId, int amendmentVersion, Guid? amendmentPreviousVersionId)
        {
            var copy = MemberwiseClone() as Release;
            copy.Id = contentAmendmentId;
            copy.Published = null;
            copy.PreviousVersionId = amendmentPreviousVersionId;
            copy.Version = amendmentVersion;
            return copy;
        }
    }
}
