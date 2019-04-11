using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Meta
{
    [Obsolete]
    public class CharacteristicMeta
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Group { get; set; }
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
    }
}