using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta
{
    public class TimePeriodMetaViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TimeIdentifier Code { get; set; }

        public string Label { get; set; }
        public int Year { get; set; }
    }
}