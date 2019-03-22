using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface ITidyData
    {
        long Id { get; set; }
        Guid PublicationId { get; set; }
        Release Release { get; set; }
        long ReleaseId { get; set; }
        int TimePeriod { get; set; }
        string TimeIdentifier { get; set; }
        Level Level { get; set; }
        Country Country { get; set; }
        SchoolType SchoolType { get; set; }
        Dictionary<string, string> Indicators { get; set; }
    }
}