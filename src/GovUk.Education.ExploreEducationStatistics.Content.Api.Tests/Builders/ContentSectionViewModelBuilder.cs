using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class ContentSectionViewModelBuilder
{
    private string _heading = "content section heading";
    private readonly List<IContentBlockViewModel> _contentBlocks = new();
    public ContentSectionViewModel Build() => new()
    {
        Heading = _heading,
        Content = _contentBlocks    
    };

    public ContentSectionViewModelBuilder WithHeading(string heading)
    {
        _heading = heading;
        return this;
    }
    
    public ContentSectionViewModelBuilder AddHtmlContent(string htmlContent)
    {
        _contentBlocks.Add(new HtmlBlockViewModel { Body = htmlContent });
        return this;
    }
    
    public static implicit operator ContentSectionViewModel(ContentSectionViewModelBuilder builder) => builder.Build();
}
