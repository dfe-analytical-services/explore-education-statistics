using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class IndicatorFootnote
    {
        public Indicator Indicator { get; set; }
        public Guid IndicatorId { get; set; }
        public Footnote Footnote { get; set; }
        public Guid FootnoteId { get; set; }
    }
}