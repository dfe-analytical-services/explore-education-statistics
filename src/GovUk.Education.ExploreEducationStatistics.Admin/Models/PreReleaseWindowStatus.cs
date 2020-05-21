using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class PreReleaseWindowStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PreReleaseAccess PreReleaseAccess { get; set; }

        public DateTime PreReleaseWindowStartTime { get; set; }

        public DateTime PreReleaseWindowEndTime { get; set; }
    }

    public enum PreReleaseAccess
    {
        NoneSet,
        Before,
        Within,
        After
    }
}