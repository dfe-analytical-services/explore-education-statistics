using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    public class UpdateReleaseStatusRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatus ReleaseStatus { get; set; }

        public string InternalReleaseNote { get; set; }
    }
}