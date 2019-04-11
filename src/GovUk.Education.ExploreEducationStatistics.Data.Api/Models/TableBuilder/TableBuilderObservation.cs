using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderObservation : ITableBuilderData
    {
        public int Year { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TimePeriod TimePeriod { get; set; }
    }
}