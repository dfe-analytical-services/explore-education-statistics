using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderObservation : ITableBuilderData
    {
        public LocationViewModel Location { get; set; }

        public Dictionary<long, string> Measures { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TimeIdentifier TimeIdentifier { get; set; }

        public int Year { get; set; }
    }
}