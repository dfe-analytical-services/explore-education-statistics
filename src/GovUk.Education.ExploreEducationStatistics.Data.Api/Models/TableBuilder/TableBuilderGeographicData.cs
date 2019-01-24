using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderGeographicData : ITableBuilderData
    {
        public string Year { get; set; }
        public Dictionary<string, string> Range { get; set; }
    }
}