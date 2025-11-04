using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class HtmlBlockGeneratorExtensions
{
    public static Generator<HtmlBlock> DefaultHtmlBlock(this DataFixture fixture) =>
        fixture.Generator<HtmlBlock>().WithDefaults();

    public static Generator<HtmlBlock> WithDefaults(this Generator<HtmlBlock> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static Generator<HtmlBlock> WithBody(this Generator<HtmlBlock> generator, string body) =>
        generator.ForInstance(d => d.SetBody(body));

    public static Generator<HtmlBlock> WithOrder(this Generator<HtmlBlock> generator, int order) =>
        generator.ForInstance(s => s.SetOrder(order));

    public static Generator<HtmlBlock> WithComments(
        this Generator<HtmlBlock> generator,
        IEnumerable<Comment> comments
    ) => generator.ForInstance(s => s.SetComments(comments));

    public static InstanceSetters<HtmlBlock> SetDefaults(this InstanceSetters<HtmlBlock> setters) =>
        setters
            .SetDefault(block => block.Id)
            .SetDefault(block => block.Created)
            .SetDefault(block => block.Updated)
            .SetDefault(block => block.Body)
            .SetDefault(block => block.Order, offset: 1)
            .Set(
                d => d.Comments,
                (_, htmlBlock) =>
                    Enumerable
                        .Range(1, 2)
                        .Select(num => new Comment { Id = Guid.NewGuid(), Content = $"{htmlBlock.Body} comment {num}" })
                        .ToList()
            );

    public static InstanceSetters<HtmlBlock> SetBody(this InstanceSetters<HtmlBlock> setters, string body) =>
        setters.Set(d => d.Body, body);

    public static InstanceSetters<HtmlBlock> SetOrder(this InstanceSetters<HtmlBlock> setters, int order) =>
        setters.Set(d => d.Order, order);

    public static InstanceSetters<HtmlBlock> SetComments(
        this InstanceSetters<HtmlBlock> setters,
        IEnumerable<Comment> comments
    ) => setters.Set(d => d.Comments, comments.ToList());
}
