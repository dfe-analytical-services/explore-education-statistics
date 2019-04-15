using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public interface ITableBuilderData
    {
        LocationViewModel Location { get; set; }
        Dictionary<string, string> Measures { get; set; }
        TimePeriod TimePeriod { get; set; }
        int Year { get; set; }
    }
}