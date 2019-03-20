using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderGeographicData : ITableBuilderGeographicData
    {
        public int TimePeriod { get; set; }
        public string TimeIdentifier { get; set; }
        public Country Country { get; set; }
        public Region Region { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
        public School School { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SchoolType SchoolType { get; set; }

        public Dictionary<string, string> Indicators { get; set; }
    }
}