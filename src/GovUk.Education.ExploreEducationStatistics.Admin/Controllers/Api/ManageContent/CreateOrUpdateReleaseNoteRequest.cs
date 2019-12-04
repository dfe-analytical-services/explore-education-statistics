using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    public class CreateOrUpdateReleaseNoteRequest
    {
        public DateTime? On { get; set; }
        public string ReleaseNote { get; set; }
    }
}