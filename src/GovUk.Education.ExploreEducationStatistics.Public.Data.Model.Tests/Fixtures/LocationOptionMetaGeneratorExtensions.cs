using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationOptionMetaGeneratorExtensions
{
    public static Generator<LocationOptionMeta> DefaultLocationOptionMeta(this DataFixture fixture)
        => fixture.Generator<LocationOptionMeta>().WithDefaults();

    public static Generator<LocationOptionMeta> WithDefaults(this Generator<LocationOptionMeta> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationOptionMeta> WithPublicId(
        this Generator<LocationOptionMeta> generator,
        string publicId)
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<LocationOptionMeta> WithPrivateId(
        this Generator<LocationOptionMeta> generator,
        int privateId)
        => generator.ForInstance(s => s.SetPrivateId(privateId));

    public static Generator<LocationOptionMeta> WithLabel(this Generator<LocationOptionMeta> generator, string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<LocationOptionMeta> WithCode(this Generator<LocationOptionMeta> generator, string code)
        => generator.ForInstance(s => s.SetLabel(code));

    public static InstanceSetters<LocationOptionMeta> SetDefaults(this InstanceSetters<LocationOptionMeta> setters)
        => setters
            .SetDefault(m => m.PublicId)
            .SetDefault(m => m.PrivateId)
            .SetDefault(m => m.Label)
            .SetDefault(m => m.Code);

    public static InstanceSetters<LocationOptionMeta> SetPublicId(
        this InstanceSetters<LocationOptionMeta> setters,
        string publicId)
        => setters.Set(m => m.PublicId, publicId);

    public static InstanceSetters<LocationOptionMeta> SetPrivateId(
        this InstanceSetters<LocationOptionMeta> setters,
        int privateId)
        => setters.Set(m => m.PrivateId, privateId);

    public static InstanceSetters<LocationOptionMeta> SetLabel(
        this InstanceSetters<LocationOptionMeta> setters,
        string label)
        => setters.Set(m => m.Label, label);

    public static InstanceSetters<LocationOptionMeta> SetCode(
        this InstanceSetters<LocationOptionMeta> setters,
        string code)
        => setters.Set(m => m.Code, code);
}
