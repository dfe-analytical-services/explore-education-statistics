using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

[JsonKnownThisType("HtmlBlock")]
public record HtmlBlockViewModel : IContentBlockViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Body { get; set; } = string.Empty;
}
