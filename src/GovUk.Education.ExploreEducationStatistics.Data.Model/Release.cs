using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using static System.DateTime;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    // TODO EES-3763 - Read Replica cleanup - remove unused "Published" and "PreviousVersionId" columns from
    // statistics' "Release" table. 
    public class Release
    {
        public Guid Id { get; set; }
        public string Slug { get; set; }
        public Guid PublicationId { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public int Year { get; set; }
        public ICollection<ReleaseFootnote> Footnotes { get; set; }

        public string Title =>
            TimePeriodLabelFormatter.Format(Year, TimeIdentifier, TimePeriodLabelFormat.FullLabelBeforeYear);

        public Release CreateReleaseAmendment(Guid contentAmendmentId, Guid? amendmentPreviousVersionId)
        {
            var copy = MemberwiseClone() as Release;
            copy.Id = contentAmendmentId;
            return copy;
        }
    }
}
