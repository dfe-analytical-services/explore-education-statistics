#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Indicator
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Unit Unit { get; set; } = Unit.Number;
        public IndicatorGroup IndicatorGroup { get; set; } = null!;
        public Guid IndicatorGroupId { get; set; }
        public List<IndicatorFootnote> Footnotes { get; set; } = new();
        public int? DecimalPlaces { get; set; }
    }
}
