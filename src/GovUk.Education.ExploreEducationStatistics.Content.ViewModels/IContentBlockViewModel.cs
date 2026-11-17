using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

[JsonConverter(typeof(JsonKnownTypesConverter<IContentBlockViewModel>))]
[JsonDiscriminator(Name = "type")]
public interface IContentBlockViewModel
{
    Guid Id { get; init; }

    int Order { get; init; }

    public static IContentBlockViewModel FromModel(ContentBlock contentBlock) =>
        contentBlock switch
        {
            HtmlBlock htmlBlock => HtmlBlockViewModel.FromModel(htmlBlock),
            _ => throw new ArgumentOutOfRangeException(nameof(contentBlock)),
        };
}
