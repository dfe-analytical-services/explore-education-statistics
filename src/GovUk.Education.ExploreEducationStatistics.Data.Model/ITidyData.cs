using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface ITidyData
    {
        long Id { get; set; }
        Subject Subject { get; set; }
        long SubjectId { get; set; }
        long LevelId { get; set; }
        LevelComposite Level { get; set; }
        int TimePeriod { get; set; }
        string TimeIdentifier { get; set; }
        SchoolType SchoolType { get; set; }
        Dictionary<string, string> Indicators { get; set; }
    }
}