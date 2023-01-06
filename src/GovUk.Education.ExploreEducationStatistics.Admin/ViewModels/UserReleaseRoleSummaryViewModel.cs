#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
 
public record UserReleaseRoleSummaryViewModel(
    Guid UserId, 
    string UserDisplayName, 
    string UserEmail,
    [property:JsonConverter(typeof(StringEnumConverter))] ReleaseRole Role);