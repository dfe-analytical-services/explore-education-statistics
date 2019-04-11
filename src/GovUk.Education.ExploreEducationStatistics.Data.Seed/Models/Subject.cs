using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Models
{
    public class Subject
    {
        public CharacteristicMeta[] CharacteristicMetas { get; set; }
        public DataCsvFilename Filename { get; set; }
        public MetaGroup<IndicatorMeta>[] IndicatorMetas { get; set; }
        public string Name { get; set; }
    }
}