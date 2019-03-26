using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
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