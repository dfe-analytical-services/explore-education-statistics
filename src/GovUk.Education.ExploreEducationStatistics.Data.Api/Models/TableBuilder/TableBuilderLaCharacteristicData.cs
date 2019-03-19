using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderLaCharacteristicData : ITableBuilderGeographicData
    {
        public int TimePeriod { get; set; }
        public string TimeIdentifier { get; set; }
        public Country Country { get; set; }
        public Region Region { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public SchoolType SchoolType { get; set; }

        public CharacteristicViewModel Characteristic { get; set; }
        public Dictionary<string, string> Indicators { get; set; }
    }
}