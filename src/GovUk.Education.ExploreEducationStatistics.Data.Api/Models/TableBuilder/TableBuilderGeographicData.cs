using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderGeographicData : ITableBuilderData
    {
        public int Year { get; set; }
        public Country Country { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SchoolType SchoolType { get; set; }

        public Dictionary<string, string> Attributes { get; set; }
    }
}