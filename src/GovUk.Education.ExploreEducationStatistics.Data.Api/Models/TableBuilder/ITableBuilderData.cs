using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public interface ITableBuilderData
    {
        int Year { get; set; }
        string SchoolType { get; set; }
        Dictionary<string, string> Attributes { get; set; }
    }
}