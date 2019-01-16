using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Data
    {
        public string Domain { get; set; }
        public Dictionary<string, string> Range { get; set; }
    }
}