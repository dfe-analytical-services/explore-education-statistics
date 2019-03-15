using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class ReleaseAttributeMeta
    {
        public long ReleaseId { get; set; }
        public Release Release { get; set; }
        public long AttributeMetaId { get; set; }
        public AttributeMeta AttributeMeta { get; set; }
        public string DataType { get; set; }
        public string Group { get; set; }
    }
}