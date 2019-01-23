using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public interface ITidyData
    {
        Guid PublicationId { get; set; }
        Guid ReleaseId { get; set; }
        DateTime ReleaseDate { get; set; }
        string Term { get; set; }
        int Year { get; set; }
        string Level { get; set; }
        Country Country { get; set; }
        string SchoolType { get; set; }
        Dictionary<string, string> Attributes { get; set; }
    }
}