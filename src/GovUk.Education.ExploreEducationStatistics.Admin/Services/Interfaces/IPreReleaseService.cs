using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
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
    
    public interface IPreReleaseService
    {
        PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime);
    }
}
