using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationOptionMetaLinkGeneratorExtensions
{
    public static Generator<LocationOptionMetaLink> DefaultLocationOptionMetaLink(this DataFixture fixture) =>
        fixture.Generator<LocationOptionMetaLink>().WithDefaults();

    public static Generator<LocationOptionMetaLink> WithDefaults(this Generator<LocationOptionMetaLink> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationOptionMetaLink> WithPublicId(
        this Generator<LocationOptionMetaLink> generator,
        string publicId
    ) => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<LocationOptionMetaLink> WithMeta(
        this Generator<LocationOptionMetaLink> generator,
        LocationMeta meta
    ) => generator.ForInstance(s => s.SetMeta(meta));

    public static Generator<LocationOptionMetaLink> WithMetaId(
        this Generator<LocationOptionMetaLink> generator,
        int metaId
    ) => generator.ForInstance(s => s.SetMetaId(metaId));

    public static Generator<LocationOptionMetaLink> WithOption(
        this Generator<LocationOptionMetaLink> generator,
        LocationOptionMeta option
    ) => generator.ForInstance(s => s.SetOption(option));

    public static Generator<LocationOptionMetaLink> WithOption(
        this Generator<LocationOptionMetaLink> generator,
        Func<LocationOptionMeta> option
    ) => generator.ForInstance(s => s.SetOption(option));

    public static Generator<LocationOptionMetaLink> WithOptionId(
        this Generator<LocationOptionMetaLink> generator,
        int optionId
    ) => generator.ForInstance(s => s.SetOptionId(optionId));

    public static InstanceSetters<LocationOptionMetaLink> SetDefaults(
        this InstanceSetters<LocationOptionMetaLink> setters
    ) => setters.Set(m => m.PublicId, f => f.Random.AlphaNumeric(10));

    public static InstanceSetters<LocationOptionMetaLink> SetPublicId(
        this InstanceSetters<LocationOptionMetaLink> setters,
        string publicId
    ) => setters.Set(m => m.PublicId, publicId);

    public static InstanceSetters<LocationOptionMetaLink> SetMeta(
        this InstanceSetters<LocationOptionMetaLink> setters,
        LocationMeta meta
    ) => setters.Set(l => l.Meta, meta).Set(l => l.MetaId, meta.Id);

    public static InstanceSetters<LocationOptionMetaLink> SetMetaId(
        this InstanceSetters<LocationOptionMetaLink> setters,
        int metaId
    ) => setters.Set(l => l.MetaId, metaId);

    public static InstanceSetters<LocationOptionMetaLink> SetOption(
        this InstanceSetters<LocationOptionMetaLink> setters,
        LocationOptionMeta option
    ) => setters.SetOption(() => option);

    public static InstanceSetters<LocationOptionMetaLink> SetOption(
        this InstanceSetters<LocationOptionMetaLink> setters,
        Func<LocationOptionMeta> option
    ) =>
        setters.Set(
            (_, l) =>
            {
                l.Option = option();
                l.OptionId = l.Option.Id;
            }
        );

    public static InstanceSetters<LocationOptionMetaLink> SetOptionId(
        this InstanceSetters<LocationOptionMetaLink> setters,
        int optionId
    ) => setters.Set(l => l.OptionId, optionId);
}
