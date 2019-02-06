using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public interface ITableBuilderData
    {
        int Year { get; set; }
        SchoolType SchoolType { get; set; }
        Dictionary<string, string> Attributes { get; set; }
    }
}