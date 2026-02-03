using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ContentSectionGeneratorExtensions
{
    public static Generator<ContentSection> DefaultContentSection(this DataFixture fixture) =>
        fixture.Generator<ContentSection>().WithDefaults();

    public static Generator<ContentSection> WithDefaults(this Generator<ContentSection> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<ContentSection> WithContentBlocks(
        this Generator<ContentSection> generator,
        IEnumerable<ContentBlock> blocks
    ) => generator.ForInstance(s => s.SetContentBlocks(blocks));

    public static Generator<ContentSection> WithHeading(this Generator<ContentSection> generator, string heading) =>
        generator.ForInstance(s => s.SetHeading(heading));

    public static Generator<ContentSection> WithOrder(this Generator<ContentSection> generator, int order) =>
        generator.ForInstance(s => s.SetOrder(order));

    public static Generator<ContentSection> WithType(
        this Generator<ContentSection> generator,
        ContentSectionType type
    ) => generator.ForInstance(s => s.SetType(type));

    public static InstanceSetters<ContentSection> SetDefaults(this InstanceSetters<ContentSection> setters) =>
        setters
            .SetDefault(cs => cs.Id)
            .SetDefault(cs => cs.Heading)
            .SetDefault(cs => cs.Order)
            .Set(cs => cs.Type, ContentSectionType.Generic);

    public static InstanceSetters<ContentSection> SetContentBlocks(
        this InstanceSetters<ContentSection> setters,
        IEnumerable<ContentBlock> blocks
    ) => setters.Set(cs => cs.Content, blocks.ToList());

    public static InstanceSetters<ContentSection> SetHeading(
        this InstanceSetters<ContentSection> setters,
        string heading
    ) => setters.Set(cs => cs.Heading, heading);

    public static InstanceSetters<ContentSection> SetOrder(this InstanceSetters<ContentSection> setters, int order) =>
        setters.Set(cs => cs.Order, order);

    public static InstanceSetters<ContentSection> SetType(
        this InstanceSetters<ContentSection> setters,
        ContentSectionType type
    ) => setters.Set(cs => cs.Type, type);
}
