using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class ReleasePublicationStatusViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatus Status;

        public bool Amendment;

        public bool Live;
    }
}