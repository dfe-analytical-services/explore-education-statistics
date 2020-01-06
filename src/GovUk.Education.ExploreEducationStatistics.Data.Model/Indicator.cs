using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Indicator
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public Unit Unit { get; set; }
        public IndicatorGroup IndicatorGroup { get; set; }
        public Guid IndicatorGroupId { get; set; }
        public ICollection<IndicatorFootnote> Footnotes { get; set; }
    }
}