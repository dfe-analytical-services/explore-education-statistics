using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public interface ITidyData
    {
        Guid PublicationId { get; set; }
        Guid ReleaseId { get; set; }
        DateTime ReleaseDate { get; set; }
        string Term { get; set; }
        int Year { get; set; }
        Level Level { get; set; }
        Country Country { get; set; }
        SchoolType SchoolType { get; set; }
        Dictionary<string, string> Attributes { get; set; }
    }
}