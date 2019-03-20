using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta
{
    public class IndicatorMeta : IMeta
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public Unit Unit { get; set; }
        public bool KeyIndicator { get; set; }
        public List<ReleaseIndicatorMeta> ReleaseIndicatorMetas { get; set; }
    }
}