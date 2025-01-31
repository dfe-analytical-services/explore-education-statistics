#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record UserReleaseInviteViewModel(
    string Email,
    [property:JsonConverter(typeof(StringEnumConverter))] ReleaseRole Role);
