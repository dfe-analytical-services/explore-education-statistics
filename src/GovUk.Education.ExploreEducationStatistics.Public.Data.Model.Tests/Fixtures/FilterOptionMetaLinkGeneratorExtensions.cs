using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterOptionMetaLinkGeneratorExtensions
{
    public static Generator<FilterOptionMetaLink> DefaultFilterOptionMetaLink(this DataFixture fixture)
        => fixture.Generator<FilterOptionMetaLink>();
    
    public static Generator<FilterOptionMetaLink> WithMeta(
        this Generator<FilterOptionMetaLink> generator,
        FilterMeta meta)
        => generator.ForInstance(s => s.SetMeta(meta));

    public static Generator<FilterOptionMetaLink> WithMetaId(
        this Generator<FilterOptionMetaLink> generator,
        int metaId)
        => generator.ForInstance(s => s.SetMetaId(metaId));

    public static Generator<FilterOptionMetaLink> WithOption(
        this Generator<FilterOptionMetaLink> generator,
        FilterOptionMeta option)
        => generator.ForInstance(s => s.SetOption(option));

    public static Generator<FilterOptionMetaLink> WithOptionId(
        this Generator<FilterOptionMetaLink> generator,
        int optionId)
        => generator.ForInstance(s => s.SetOptionId(optionId));

    public static Generator<FilterOptionMetaLink> WithPublicId(
        this Generator<FilterOptionMetaLink> generator,
        int publicId)
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static InstanceSetters<FilterOptionMetaLink> SetMeta(
        this InstanceSetters<FilterOptionMetaLink> setters,
        FilterMeta meta)
        => setters
            .Set(l => l.Meta, meta)
            .Set(l => l.MetaId, meta.Id);

    public static InstanceSetters<FilterOptionMetaLink> SetMetaId(
        this InstanceSetters<FilterOptionMetaLink> setters,
        int metaId)
        => setters.Set(l => l.MetaId, metaId);

    public static InstanceSetters<FilterOptionMetaLink> SetOption(
        this InstanceSetters<FilterOptionMetaLink> setters,
        FilterOptionMeta option)
        => setters
            .Set(l => l.Option, option)
            .Set(l => l.OptionId, option.Id);

    public static InstanceSetters<FilterOptionMetaLink> SetOptionId(
        this InstanceSetters<FilterOptionMetaLink> setters,
        int optionId)
        => setters.Set(l => l.OptionId, optionId);

    public static InstanceSetters<FilterOptionMetaLink> SetPublicId(
        this InstanceSetters<FilterOptionMetaLink> setters,
        int publicId)
        => setters.Set(l => l.PublicId, publicId);
}
