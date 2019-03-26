using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
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