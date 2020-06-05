using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public interface IContentBlockViewModel
    {
        Guid Id { get; set; }

        List<CommentViewModel> Comments { get; set; }

        int Order { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        ContentBlockType Type { get; }
    }
}