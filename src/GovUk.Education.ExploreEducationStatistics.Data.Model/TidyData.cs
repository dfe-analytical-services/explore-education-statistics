using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public abstract class TidyData : ITidyData
    {
        public long Id { get; set; }
        public Subject Subject { get; set; }
        public long SubjectId { get; set; }
        public LevelComposite Level { get; set; }
        public long LevelId { get; set; }
        public int TimePeriod { get; set; }
        public string TimeIdentifier { get; set; }
        public SchoolType SchoolType { get; set; }
        public Dictionary<string, string> Indicators { get; set; }
    }
}