using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels
{
    public class ReleaseNoteViewModel
    {
        public Guid Id { get; set; }

        public string Reason { get; set; }

        public DateTime On { get; set; }
    }
}