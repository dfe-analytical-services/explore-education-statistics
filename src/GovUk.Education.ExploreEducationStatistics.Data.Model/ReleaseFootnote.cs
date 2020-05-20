using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseFootnote
    {
        public Footnote Footnote { get; set; }
        
        public Guid FootnoteId { get; set; }
        
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }

        public ReleaseFootnote CreateReleaseAmendment(Release amendment)
        {
            var copy = MemberwiseClone() as ReleaseFootnote;
            copy.Release = amendment;
            copy.ReleaseId = amendment.Id;
            return copy;
        }
    }
}