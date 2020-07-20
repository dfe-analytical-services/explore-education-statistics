using System;
using System.Collections.Generic;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    [JsonConverter(typeof(JsonKnownTypesConverter<IContentBlockViewModel>))]
    [JsonDiscriminator(Name = "type")]
    public interface IContentBlockViewModel
    {
        Guid Id { get; set; }

        List<CommentViewModel> Comments { get; set; }

        int Order { get; set; }
    }
}