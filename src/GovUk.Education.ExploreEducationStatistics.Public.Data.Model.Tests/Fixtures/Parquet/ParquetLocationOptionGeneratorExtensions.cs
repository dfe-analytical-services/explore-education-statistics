using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures.Parquet;

public static class ParquetLocationOptionGeneratorExtensions
{
    public static Generator<ParquetLocationOption> DefaultParquetLocationOption(
        this DataFixture fixture,
        GeographicLevel geographicLevel = GeographicLevel.Country
    ) => fixture.Generator<ParquetLocationOption>().WithDefaults(geographicLevel);

    public static Generator<ParquetLocationOption> WithDefaults(
        this Generator<ParquetLocationOption> generator,
        GeographicLevel level = GeographicLevel.Country
    ) => generator.ForInstance(s => s.SetDefaults(level));

    public static Generator<ParquetLocationOption> WithId(
        this Generator<ParquetLocationOption> generator,
        int id
    ) => generator.ForInstance(s => s.SetId(id));

    public static Generator<ParquetLocationOption> WithPublicId(
        this Generator<ParquetLocationOption> generator,
        string publicId
    ) => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<ParquetLocationOption> WithLabel(
        this Generator<ParquetLocationOption> generator,
        string label
    ) => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<ParquetLocationOption> WithLevel(
        this Generator<ParquetLocationOption> generator,
        GeographicLevel level
    ) => generator.ForInstance(s => s.SetLevel(level));

    public static Generator<ParquetLocationOption> WithCode(
        this Generator<ParquetLocationOption> generator,
        string code
    ) => generator.ForInstance(s => s.SetCode(code));

    public static Generator<ParquetLocationOption> WithOldCode(
        this Generator<ParquetLocationOption> generator,
        string oldCode
    ) => generator.ForInstance(s => s.SetOldCode(oldCode));

    public static Generator<ParquetLocationOption> WithUkprn(
        this Generator<ParquetLocationOption> generator,
        string ukprn
    ) => generator.ForInstance(s => s.SetUkprn(ukprn));

    public static Generator<ParquetLocationOption> WithUrn(
        this Generator<ParquetLocationOption> generator,
        string urn
    ) => generator.ForInstance(s => s.SetUrn(urn));

    public static Generator<ParquetLocationOption> WithLaEstab(
        this Generator<ParquetLocationOption> generator,
        string laEstab
    ) => generator.ForInstance(s => s.SetLaEstab(laEstab));

    public static InstanceSetters<ParquetLocationOption> SetDefaults(
        this InstanceSetters<ParquetLocationOption> setters,
        GeographicLevel level = GeographicLevel.Country
    )
    {
        setters
            .SetDefault(o => o.Id)
            .SetDefault(o => o.Label)
            .SetDefault(o => o.PublicId)
            .SetLevel(level);

        return level switch
        {
            GeographicLevel.LocalAuthority => setters
                .SetDefault(o => o.Code)
                .SetDefault(o => o.OldCode),
            GeographicLevel.Provider => setters.SetDefault(o => o.Ukprn),
            GeographicLevel.RscRegion => setters,
            GeographicLevel.School => setters.SetDefault(o => o.Urn).SetDefault(o => o.LaEstab),
            _ => setters.SetDefault(o => o.Code),
        };
    }

    public static InstanceSetters<ParquetLocationOption> SetId(
        this InstanceSetters<ParquetLocationOption> setters,
        int id
    ) => setters.Set(o => o.Id, id);

    public static InstanceSetters<ParquetLocationOption> SetPublicId(
        this InstanceSetters<ParquetLocationOption> setters,
        string publicId
    ) => setters.Set(o => o.PublicId, publicId);

    public static InstanceSetters<ParquetLocationOption> SetLabel(
        this InstanceSetters<ParquetLocationOption> setters,
        string label
    ) => setters.Set(o => o.Label, label);

    public static InstanceSetters<ParquetLocationOption> SetLevel(
        this InstanceSetters<ParquetLocationOption> setters,
        GeographicLevel level
    ) => setters.Set(o => o.Level, level.GetEnumValue());

    public static InstanceSetters<ParquetLocationOption> SetCode(
        this InstanceSetters<ParquetLocationOption> setters,
        string code
    ) => setters.Set(o => o.Code, code);

    public static InstanceSetters<ParquetLocationOption> SetOldCode(
        this InstanceSetters<ParquetLocationOption> setters,
        string oldCode
    ) => setters.Set(o => o.OldCode, oldCode);

    public static InstanceSetters<ParquetLocationOption> SetUkprn(
        this InstanceSetters<ParquetLocationOption> setters,
        string ukprn
    ) => setters.Set(o => o.Ukprn, ukprn);

    public static InstanceSetters<ParquetLocationOption> SetUrn(
        this InstanceSetters<ParquetLocationOption> setters,
        string urn
    ) => setters.Set(o => o.Urn, urn);

    public static InstanceSetters<ParquetLocationOption> SetLaEstab(
        this InstanceSetters<ParquetLocationOption> setters,
        string laEstab
    ) => setters.Set(o => o.LaEstab, laEstab);
}
