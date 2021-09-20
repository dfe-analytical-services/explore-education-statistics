#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class ManageMethodologyContentViewModel
    {
        public Guid Id { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        public DateTime? Published { get; set; }

        public List<ContentSectionViewModel> Content { get; set; } = new List<ContentSectionViewModel>();

        public List<ContentSectionViewModel> Annexes { get; set; } = new List<ContentSectionViewModel>();

        public List<MethodologyNoteViewModel> Notes { get; set; } = new List<MethodologyNoteViewModel>();
    }
}
