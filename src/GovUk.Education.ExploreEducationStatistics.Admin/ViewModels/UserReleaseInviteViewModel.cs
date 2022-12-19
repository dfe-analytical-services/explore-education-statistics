#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserReleaseInviteViewModel
    {
        [EmailAddress] public string Email { get; set; } = string.Empty;
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseRole Role { get; set; }
    }
}
