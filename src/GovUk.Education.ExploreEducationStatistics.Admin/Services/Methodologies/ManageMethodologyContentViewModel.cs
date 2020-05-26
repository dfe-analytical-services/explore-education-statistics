using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class ManageMethodologyContentViewModel
    {
        public Guid Id { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        public DateTime? Published { get; set; }

        public DateTime PublishScheduled { get; set; }

        public List<ContentSectionViewModel> Content { get; set; }

        public List<ContentSectionViewModel> Annexes { get; set; }
    }
}
