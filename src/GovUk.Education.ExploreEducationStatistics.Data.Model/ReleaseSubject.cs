#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseSubject : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
    {
        public Subject Subject { get; set; } = null!;

        public Guid SubjectId { get; set; }

        public ReleaseVersion ReleaseVersion { get; set; } = null!;

        public Guid ReleaseVersionId { get; set; }

        public List<FilterSequenceEntry>? FilterSequence { get; set; }

        public List<IndicatorGroupSequenceEntry>? IndicatorSequence { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }
    }
}
