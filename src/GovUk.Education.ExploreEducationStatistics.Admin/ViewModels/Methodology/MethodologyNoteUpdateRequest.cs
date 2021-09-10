#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyNoteUpdateRequest
    {
        public string Content { get; set; } = null!;

        public DateTime DisplayDate { get; set; }
    }
}
