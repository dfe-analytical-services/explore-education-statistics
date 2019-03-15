using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class ReleaseCharacteristicMeta
    {
        public long ReleaseId { get; set; }
        public Release Release { get; set; }
        public long CharacteristicMetaId { get; set; }
        public CharacteristicMeta CharacteristicMeta { get; set; }
        public string DataType { get; set; }
    }
}