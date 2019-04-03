using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Subject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Release Release { get; set; }
        public long ReleaseId { get; set; }
        public IEnumerable<CharacteristicMeta> Characteristics { get; set; }
        public IEnumerable<IndicatorMeta> Indicators { get; set; }

        public Subject()
        {
        }

        public Subject(string name, Release release)
        {
            Name = name;
            Release = release;
        }
    }
}