using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class TimeIdentifierModel
    {
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier Identifier { get; set; }
    }
}