#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record MethodologyViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public DateTime? Published { get; set; }

        public List<ContentSectionViewModel> Content { get; set; } = new();

        public List<ContentSectionViewModel> Annexes { get; set; } = new();

        public List<MethodologyNoteViewModel> Notes { get; set; } = new();
    }
}
