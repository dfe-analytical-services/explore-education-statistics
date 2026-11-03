using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

[JsonKnownThisType("HtmlBlock")]
public record HtmlBlockViewModel : IContentBlockViewModel
{
    public required Guid Id { get; init; }

    public required int Order { get; init; }

    public required string? Body { get; init; }

    public static HtmlBlockViewModel FromModel(HtmlBlock htmlBlock) =>
        new()
        {
            Id = htmlBlock.Id,
            Order = htmlBlock.Order,
            Body = htmlBlock.Body,
        };
}
