using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseFootnote
    {
        public Footnote Footnote { get; set; }
        
        public Guid FootnoteId { get; set; }
        
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }
    }
}