using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class UpdateMethodologyRequest
    {
        public string InternalReleaseNote { get; set; }

        [Required] public string Title { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }
    }
}