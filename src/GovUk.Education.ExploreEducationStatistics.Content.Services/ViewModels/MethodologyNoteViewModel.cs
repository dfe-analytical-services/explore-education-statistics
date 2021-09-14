#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record MethodologyNoteViewModel
    {
        public Guid Id { get; set; }

        public string Content { get; set; } = null!;

        public DateTime DisplayDate { get; set; }
    }
}
