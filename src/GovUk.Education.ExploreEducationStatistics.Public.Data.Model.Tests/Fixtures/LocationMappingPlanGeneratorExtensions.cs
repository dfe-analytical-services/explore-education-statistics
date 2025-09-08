using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class LocationMappingPlanGeneratorExtensions
{
    /**
     * LocationMappingPlan
     */
    public static Generator<LocationMappingPlan> DefaultLocationMappingPlan(this DataFixture fixture) =>
        fixture.Generator<LocationMappingPlan>();

    public static Generator<LocationMappingPlan> LocationMappingPlanFromLocationMeta(
        this DataFixture fixture,
        List<LocationMeta>? sourceLocations = null,
        List<LocationMeta>? targetLocations = null
    )
    {
        var locationMappingPlanGenerator = fixture.Generator<LocationMappingPlan>();

        var levels = (sourceLocations ?? [])
            .Concat(targetLocations ?? [])
            .Select(meta => meta.Level)
            .Distinct()
            .ToList();

        levels.ForEach(level =>
        {
            var levelGenerator = fixture.DefaultLocationLevelMappings();

            var sourceLocationsForLevel = sourceLocations?.SingleOrDefault(meta => meta.Level == level);

            sourceLocationsForLevel?.Options.ForEach(option =>
            {
                levelGenerator.AddMapping(
                    sourceKey: MappingKeyGenerators.LocationOptionMeta(option),
                    mapping: fixture
                        .DefaultLocationOptionMapping()
                        .WithSource(
                            fixture.DefaultMappableLocationOption().WithLabel(option.Label).WithCodes(option.ToRow())
                        )
                );
            });

            var targetLocationsForLevel = targetLocations?.SingleOrDefault(meta => meta.Level == level);

            targetLocationsForLevel?.Options.ForEach(option =>
            {
                levelGenerator.AddCandidate(
                    targetKey: MappingKeyGenerators.LocationOptionMeta(option),
                    candidate: fixture.DefaultMappableLocationOption().WithLabel(option.Label).WithCodes(option.ToRow())
                );
            });

            locationMappingPlanGenerator.AddLevel(level, levelGenerator);
        });

        return locationMappingPlanGenerator;
    }

    public static Generator<LocationMappingPlan> AddLevel(
        this Generator<LocationMappingPlan> generator,
        GeographicLevel level,
        LocationLevelMappings mappings
    ) => generator.ForInstance(s => s.AddLevel(level, mappings));

    public static InstanceSetters<LocationMappingPlan> AddLevel(
        this InstanceSetters<LocationMappingPlan> instanceSetter,
        GeographicLevel level,
        LocationLevelMappings mappings
    ) => instanceSetter.Set((_, plan) => plan.Levels.Add(level, mappings));

    /**
     * LocationLevelMappings
     */
    public static Generator<LocationLevelMappings> DefaultLocationLevelMappings(this DataFixture fixture) =>
        fixture.Generator<LocationLevelMappings>();

    public static Generator<LocationLevelMappings> AddCandidate(
        this Generator<LocationLevelMappings> generator,
        string targetKey,
        MappableLocationOption candidate
    ) => generator.ForInstance(s => s.AddCandidate(targetKey, candidate));

    public static Generator<LocationLevelMappings> AddMapping(
        this Generator<LocationLevelMappings> generator,
        string sourceKey,
        LocationOptionMapping mapping
    ) => generator.ForInstance(s => s.AddMapping(sourceKey, mapping));

    public static InstanceSetters<LocationLevelMappings> AddCandidate(
        this InstanceSetters<LocationLevelMappings> instanceSetter,
        string targetKey,
        MappableLocationOption candidate
    ) => instanceSetter.Set((_, mappings) => mappings.Candidates.Add(targetKey, candidate));

    public static InstanceSetters<LocationLevelMappings> AddMapping(
        this InstanceSetters<LocationLevelMappings> instanceSetter,
        string sourceKey,
        LocationOptionMapping mapping
    ) => instanceSetter.Set((_, mappings) => mappings.Mappings.Add(sourceKey, mapping));

    /**
     * MappableLocationOption
     */
    public static Generator<MappableLocationOption> DefaultMappableLocationOption(this DataFixture fixture) =>
        fixture.Generator<MappableLocationOption>().WithDefaults();

    public static Generator<MappableLocationOption> WithDefaults(this Generator<MappableLocationOption> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<MappableLocationOption> WithLabel(
        this Generator<MappableLocationOption> generator,
        string label
    ) => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<MappableLocationOption> WithCodes(
        this Generator<MappableLocationOption> generator,
        string? code,
        string? oldCode,
        string? urn,
        string? laEstab,
        string? ukprn
    ) => generator.ForInstance(s => s.SetCodes(code: code, oldCode: oldCode, urn: urn, laEstab: laEstab, ukprn: ukprn));

    public static Generator<MappableLocationOption> WithCodes(
        this Generator<MappableLocationOption> generator,
        LocationOptionMetaRow metaRow
    ) =>
        generator.ForInstance(s =>
            s.SetCodes(
                code: metaRow.Code,
                oldCode: metaRow.OldCode,
                urn: metaRow.Urn,
                laEstab: metaRow.LaEstab,
                ukprn: metaRow.Ukprn
            )
        );

    public static InstanceSetters<MappableLocationOption> SetDefaults(
        this InstanceSetters<MappableLocationOption> setters
    ) => setters.SetDefault(option => option.Label).SetDefault(option => option.Code);

    public static InstanceSetters<MappableLocationOption> SetLabel(
        this InstanceSetters<MappableLocationOption> instanceSetter,
        string label
    ) => instanceSetter.Set(option => option.Label, label);

    public static InstanceSetters<MappableLocationOption> SetCodes(
        this InstanceSetters<MappableLocationOption> instanceSetter,
        string? code,
        string? oldCode,
        string? urn,
        string? laEstab,
        string? ukprn
    ) =>
        instanceSetter
            .Set(option => option.Code, (_, option) => code ?? option.Code)
            .Set(option => option.OldCode, (_, option) => oldCode ?? option.OldCode)
            .Set(option => option.Urn, (_, option) => urn ?? option.Urn)
            .Set(option => option.LaEstab, (_, option) => laEstab ?? option.LaEstab)
            .Set(option => option.Ukprn, (_, option) => ukprn ?? option.Ukprn);

    /**
     * LocationOptionMapping
     */
    public static Generator<LocationOptionMapping> DefaultLocationOptionMapping(this DataFixture fixture) =>
        fixture.Generator<LocationOptionMapping>().WithDefaults();

    public static Generator<LocationOptionMapping> WithDefaults(this Generator<LocationOptionMapping> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<LocationOptionMapping> WithSource(
        this Generator<LocationOptionMapping> generator,
        MappableLocationOption source
    ) => generator.ForInstance(s => s.SetSource(source));

    public static Generator<LocationOptionMapping> WithPublicId(
        this Generator<LocationOptionMapping> generator,
        string publicId
    ) => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<LocationOptionMapping> WithType(
        this Generator<LocationOptionMapping> generator,
        MappingType type
    ) => generator.ForInstance(s => s.SetType(type));

    public static Generator<LocationOptionMapping> WithNoMapping(this Generator<LocationOptionMapping> generator) =>
        generator.ForInstance(s => s.SetType(MappingType.None).SetCandidateKey(null));

    public static Generator<LocationOptionMapping> WithAutoMapped(
        this Generator<LocationOptionMapping> generator,
        string candidateKey
    ) => generator.ForInstance(s => s.SetType(MappingType.AutoMapped).SetCandidateKey(candidateKey));

    public static Generator<LocationOptionMapping> WithAutoNone(this Generator<LocationOptionMapping> generator) =>
        generator.ForInstance(s => s.SetType(MappingType.AutoNone).SetCandidateKey(null));

    public static Generator<LocationOptionMapping> WithManualMapped(
        this Generator<LocationOptionMapping> generator,
        string candidateKey
    ) => generator.ForInstance(s => s.SetType(MappingType.ManualMapped).SetCandidateKey(candidateKey));

    public static Generator<LocationOptionMapping> WithManualNone(this Generator<LocationOptionMapping> generator) =>
        generator.ForInstance(s => s.SetType(MappingType.ManualNone).SetCandidateKey(null));

    public static Generator<LocationOptionMapping> WithCandidateKey(
        this Generator<LocationOptionMapping> generator,
        string? candidateKey
    ) => generator.ForInstance(s => s.SetCandidateKey(candidateKey));

    public static InstanceSetters<LocationOptionMapping> SetDefaults(
        this InstanceSetters<LocationOptionMapping> setters
    ) =>
        setters
            .SetDefault(mapping => mapping.PublicId)
            .SetDefault(mapping => mapping.Type)
            .SetDefault(mapping => mapping.CandidateKey);

    public static InstanceSetters<LocationOptionMapping> SetSource(
        this InstanceSetters<LocationOptionMapping> setters,
        MappableLocationOption source
    ) => setters.Set(mapping => mapping.Source, source);

    public static InstanceSetters<LocationOptionMapping> SetPublicId(
        this InstanceSetters<LocationOptionMapping> setters,
        string publicId
    ) => setters.Set(mapping => mapping.PublicId, publicId);

    public static InstanceSetters<LocationOptionMapping> SetType(
        this InstanceSetters<LocationOptionMapping> setters,
        MappingType type
    ) => setters.Set(mapping => mapping.Type, type);

    public static InstanceSetters<LocationOptionMapping> SetCandidateKey(
        this InstanceSetters<LocationOptionMapping> setters,
        string? candidateKey
    ) => setters.Set(mapping => mapping.CandidateKey, candidateKey);
}
