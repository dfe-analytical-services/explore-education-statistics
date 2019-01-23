using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class CountryGeographicModel
    {
        public string Level { get; set; }

        public Country Country { get; set; }

        public string SchoolType { get; set; }
        
        public List<YearItem> Years { get; set; }
    }

    public class YearItem
    {
        public int Year { get; set; }
        
        public Dictionary<string, string> Attributes { get; set; }

    }
}