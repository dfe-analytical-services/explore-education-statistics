using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
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
        Task<Either<ActionResult, PreReleaseSummaryViewModel>> GetPreReleaseSummaryViewModelAsync(Guid releaseId);

        PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime);
    }
}
