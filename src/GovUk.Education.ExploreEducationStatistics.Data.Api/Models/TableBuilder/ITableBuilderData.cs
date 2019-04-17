using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public interface ITableBuilderData
    {
        LocationViewModel Location { get; set; }
        Dictionary<long, string> Measures { get; set; }
        TimeIdentifier TimeIdentifier { get; set; }
        int Year { get; set; }
    }
}