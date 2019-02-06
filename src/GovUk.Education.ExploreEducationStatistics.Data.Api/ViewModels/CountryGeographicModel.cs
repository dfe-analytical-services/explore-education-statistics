using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    [Obsolete]
    public class CountryGeographicModel
    {
        public Level Level { get; set; }

        public Country Country { get; set; }

        public SchoolType SchoolType { get; set; }

        public List<YearItem> Years { get; set; }
    }

    public class YearItem
    {
        public int Year { get; set; }

        public Dictionary<string, string> Attributes { get; set; }
    }
}