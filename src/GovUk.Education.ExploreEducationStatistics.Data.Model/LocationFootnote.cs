using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocationFootnote
    {
        public Location Location { get; set; }
        public Guid LocationId { get; set; }
        public Footnote Footnote { get; set; }
        public Guid FootnoteId { get; set; }
    }
}
