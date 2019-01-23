using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public interface ITableBuilderData
    {
        string Year { get; set; }
        Dictionary<string, string> Range { get; set; }
    }
}