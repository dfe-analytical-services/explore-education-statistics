#nullable enable
using System;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    [JsonConverter(typeof(JsonKnownTypesConverter<IContentBlockViewModel>))]
    [JsonDiscriminator(Name = "type")]
    public interface IContentBlockViewModel
    {
        Guid Id { get; set; }

        int Order { get; set; }
    }
}
