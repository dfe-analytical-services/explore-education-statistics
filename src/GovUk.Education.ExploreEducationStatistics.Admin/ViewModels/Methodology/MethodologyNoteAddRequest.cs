#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyNoteAddRequest
    {
        public string Content { get; set; } = null!;

        public DateTime DisplayDate { get; set; }
    }
}
