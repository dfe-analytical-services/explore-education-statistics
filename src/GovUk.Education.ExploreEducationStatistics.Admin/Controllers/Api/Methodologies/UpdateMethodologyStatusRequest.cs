using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class UpdateMethodologyStatusRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }
        
        public string InternalReleaseNote { get; set; }
    }
}