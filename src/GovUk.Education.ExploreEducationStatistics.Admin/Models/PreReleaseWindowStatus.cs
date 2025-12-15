using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public class PreReleaseWindowStatus
{
    [JsonConverter(typeof(StringEnumConverter))]
    public PreReleaseAccess Access { get; set; }

    public DateTimeOffset Start { get; set; }

    public DateTimeOffset ScheduledPublishDate { get; set; }
}

public enum PreReleaseAccess
{
    NoneSet,
    Before,
    Within,
    After,
}
