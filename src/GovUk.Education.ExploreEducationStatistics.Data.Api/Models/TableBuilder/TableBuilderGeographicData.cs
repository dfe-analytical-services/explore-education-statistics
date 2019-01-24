using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderGeographicData : ITableBuilderData
    {
        public int Year { get; set; }
        public string SchoolType { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}