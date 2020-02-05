using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public enum PreReleaseWindowStatus
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
