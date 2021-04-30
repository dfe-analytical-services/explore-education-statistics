using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class TimePeriodFootnote
    {
        public int Year { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public Footnote Footnote { get; set; }
        public Guid FootnoteId { get; set; }
    }
}
