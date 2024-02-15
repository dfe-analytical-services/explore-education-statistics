using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationOptionMetaGeneratorExtensions
{
    public static Generator<LocationOptionMeta> DefaultLocationOptionMeta(this DataFixture fixture)
        => fixture.Generator<LocationOptionMeta>().WithDefaults();

    public static Generator<T> WithDefaults<T>(this Generator<T> generator) where T : LocationOptionMeta
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<T> WithPublicId<T>(
        this Generator<T> generator,
        string publicId) where T : LocationOptionMeta
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<T> WithPrivateId<T>(
        this Generator<T> generator,
        int privateId) where T : LocationOptionMeta
        => generator.ForInstance(s => s.SetPrivateId(privateId));

    public static Generator<T> WithLabel<T>(this Generator<T> generator, string label) where T : LocationOptionMeta
        => generator.ForInstance(s => s.SetLabel(label));

    public static InstanceSetters<T> SetDefaults<T>(this InstanceSetters<T> setters) where T : LocationOptionMeta
        => setters
            .SetDefault(m => m.PublicId)
            .SetDefault(m => m.PrivateId)
            .SetDefault(m => m.Label);

    public static InstanceSetters<T> SetPublicId<T>(
        this InstanceSetters<T> setters,
        string publicId) where T : LocationOptionMeta
        => setters.Set(m => m.PublicId, publicId);

    public static InstanceSetters<T> SetPrivateId<T>(
        this InstanceSetters<T> setters,
        int privateId) where T : LocationOptionMeta
        => setters.Set(m => m.PrivateId, privateId);

    public static InstanceSetters<T> SetLabel<T>(
        this InstanceSetters<T> setters,
        string label) where T : LocationOptionMeta
        => setters.Set(m => m.Label, label);
}
