using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class ContentSectionGeneratorExtensions
{
    public static Generator<ContentSection> DefaultContentSection(this DataFixture fixture) =>
        fixture.Generator<ContentSection>().WithDefaults();

    public static Generator<ContentSection> WithDefaults(this Generator<ContentSection> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static Generator<ContentSection> WithContentBlocks(
        this Generator<ContentSection> generator,
        IEnumerable<ContentBlock> blocks
    ) => generator.ForInstance(d => d.SetContentBlocks(blocks));

    public static Generator<ContentSection> WithType(
        this Generator<ContentSection> generator,
        ContentSectionType type
    ) => generator.ForInstance(d => d.SetType(type));

    public static InstanceSetters<ContentSection> SetDefaults(this InstanceSetters<ContentSection> setters) =>
        setters
            .SetDefault(p => p.Id)
            .SetDefault(p => p.Heading)
            .SetDefault(p => p.Order)
            .Set(p => p.Type, ContentSectionType.Generic);

    public static InstanceSetters<ContentSection> SetContentBlocks(
        this InstanceSetters<ContentSection> setters,
        IEnumerable<ContentBlock> blocks
    ) => setters.Set(d => d.Content, blocks.ToList());

    public static InstanceSetters<ContentSection> SetType(
        this InstanceSetters<ContentSection> setters,
        ContentSectionType type
    ) => setters.Set(d => d.Type, type);
}
