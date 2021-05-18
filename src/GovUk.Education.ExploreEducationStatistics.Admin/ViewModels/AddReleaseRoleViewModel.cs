using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class AddReleaseRoleViewModel
    {
        public Guid ReleaseId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseRole ReleaseRole { get; set; }
    }
}
