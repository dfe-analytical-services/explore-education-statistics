using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationLocalAuthorityOptionMetaGeneratorExtensions
{
    public static Generator<LocationLocalAuthorityOptionMeta> DefaultLocationLocalAuthorityOptionMeta(
        this DataFixture fixture
    ) => fixture.Generator<LocationLocalAuthorityOptionMeta>().WithDefaults();

    public static Generator<LocationLocalAuthorityOptionMeta> WithDefaults(
        this Generator<LocationLocalAuthorityOptionMeta> generator
    ) => generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationLocalAuthorityOptionMeta> WithCode(
        this Generator<LocationLocalAuthorityOptionMeta> generator,
        string code
    ) => generator.ForInstance(s => s.SetCode(code));

    public static Generator<LocationLocalAuthorityOptionMeta> WithOldCode(
        this Generator<LocationLocalAuthorityOptionMeta> generator,
        string oldCode
    ) => generator.ForInstance(s => s.SetOldCode(oldCode));

    public static InstanceSetters<LocationLocalAuthorityOptionMeta> SetDefaults(
        this InstanceSetters<LocationLocalAuthorityOptionMeta> setters
    ) => setters.SetBaseDefaults().SetDefaultCode(m => m.Code).SetDefaultCode(m => m.OldCode);

    public static InstanceSetters<LocationLocalAuthorityOptionMeta> SetCode(
        this InstanceSetters<LocationLocalAuthorityOptionMeta> setters,
        string code
    ) => setters.Set(m => m.Code, code);

    public static InstanceSetters<LocationLocalAuthorityOptionMeta> SetOldCode(
        this InstanceSetters<LocationLocalAuthorityOptionMeta> setters,
        string oldCode
    ) => setters.Set(m => m.OldCode, oldCode);
}
