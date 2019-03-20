using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class ReleaseIndicatorMeta
    {
        public long ReleaseId { get; set; }
        public Release Release { get; set; }
        public long IndicatorMetaId { get; set; }
        public IndicatorMeta IndicatorMeta { get; set; }
        public string DataType { get; set; }
        public string Group { get; set; }
    }
}