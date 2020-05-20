using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class UpdateMethodologyRequest
    {
        public string InternalReleaseNote { get; set; }

        [Required] public string Title { get; set; }

        public DateTime? PublishScheduled { get; set; }

        [JsonConverter(typeof(StringEnumConverter<,,>))]
        public MethodologyStatus? Status { get; set; }
    }
}