using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent
{
    public class CreateOrUpdateReleaseNoteRequest
    {
        public DateTime? On { get; set; }
        public string Reason { get; set; }
    }
}