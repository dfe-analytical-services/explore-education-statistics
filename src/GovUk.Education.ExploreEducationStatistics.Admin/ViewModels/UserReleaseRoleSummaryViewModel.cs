#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserReleaseRoleSummaryViewModel
    {
        public Guid UserId { get; set; }

        public string UserDisplayName { get; set; } = "";

        public string UserEmail { get; set; } = "";
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseRole Role { get; set; }
    }
}
