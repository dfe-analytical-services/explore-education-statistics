using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta
{
    public class CharacteristicMeta : IMeta
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Group { get; set; }
        public List<ReleaseCharacteristicMeta> ReleaseCharacteristicMetas { get; set; }
    }
}