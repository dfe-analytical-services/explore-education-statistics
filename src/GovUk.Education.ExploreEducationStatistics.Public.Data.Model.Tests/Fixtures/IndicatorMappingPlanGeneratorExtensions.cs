using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class IndicatorMappingPlanGeneratorExtensions
{
    /**
     * IndicatorMappingPlan
     */
    public static Generator<IndicatorMappingPlan> DefaultIndicatorMappingPlan(this DataFixture fixture) =>
        fixture.Generator<IndicatorMappingPlan>();

    // public static Generator<IndicatorMappingPlan> IndicatorMappingPlanFromIndicatorMeta(
    //     this DataFixture fixture,
    //     IEnumerable<IndicatorMeta>? sourceIndicators = null,
    //     IEnumerable<IndicatorMeta>? targetIndicators = null
    // )
    // {
    //     var filterMappingPlanGenerator = fixture.Generator<IndicatorMappingPlan>();
    //
    //     sourceIndicators?.ForEach(sourceIndicator =>
    //     {
    //         var autoMappedIndicator = targetIndicators?.SingleOrDefault(f => f.Column == sourceIndicator.Column);
    //
    //         var filterMappingGenerator = fixture
    //             .DefaultIndicatorMapping()
    //             .WithSource(fixture.DefaultMappableIndicator().WithLabel(sourceIndicator.Label))
    //             .WithType(autoMappedIndicator is not null ? MappingType.AutoMapped : MappingType.AutoNone)
    //             .WithCandidateKey(autoMappedIndicator?.Column)
    //             .WithPublicId(sourceIndicator.PublicId);
    //
    //         sourceIndicator.Options.ForEach(option =>
    //         {
    //             var sourceKey = MappingKeyGenerators.IndicatorOptionMeta(option);
    //             var sourceLink = sourceIndicator.OptionLinks.SingleOrDefault(l => l.OptionId == option.Id);
    //
    //             var autoMappedOption = autoMappedIndicator?.Options.SingleOrDefault(o =>
    //                 MappingKeyGenerators.IndicatorOptionMeta(o) == sourceKey
    //             );
    //
    //             filterMappingGenerator.AddOptionMapping(
    //                 sourceKey: sourceKey,
    //                 mapping: fixture
    //                     .DefaultIndicatorOptionMapping()
    //                     .WithSource(fixture.DefaultMappableIndicatorOption().WithLabel(option.Label))
    //                     .WithType(autoMappedOption is not null ? MappingType.AutoMapped : MappingType.AutoNone)
    //                     .WithCandidateKey(
    //                         autoMappedOption is not null
    //                             ? MappingKeyGenerators.IndicatorOptionMeta(autoMappedOption)
    //                             : null
    //                     )
    //                     .WithPublicId(sourceLink?.PublicId ?? $"{sourceIndicator.PublicId} :: {option.Label}")
    //             );
    //         });
    //
    //         filterMappingPlanGenerator.AddIndicatorMapping(sourceIndicator.Column, filterMappingGenerator);
    //     });
    //
    //     targetIndicators?.ForEach(targetIndicator =>
    //     {
    //         var filterCandidateGenerator = fixture.DefaultMappableIndicator().WithLabel(targetIndicator.Label);
    //
    //         targetIndicator.Options.ForEach(option =>
    //         {
    //             filterCandidateGenerator.AddOptionCandidate(
    //                 targetKey: MappingKeyGenerators.IndicatorOptionMeta(option),
    //                 candidate: fixture.DefaultMappableIndicatorOption().WithLabel(option.Label)
    //             );
    //         });
    //
    //         filterMappingPlanGenerator.AddIndicatorCandidate(targetIndicator.Column, filterCandidateGenerator);
    //     });
    //
    //     return filterMappingPlanGenerator;
    // }

    public static Generator<IndicatorMappingPlan> AddIndicatorMapping(
        this Generator<IndicatorMappingPlan> generator,
        string columnName,
        IndicatorMapping mapping
    ) => generator.ForInstance(s => s.AddIndicatorMapping(columnName, mapping));

    public static Generator<IndicatorMappingPlan> AddIndicatorCandidate(
        this Generator<IndicatorMappingPlan> generator,
        string columnName,
        MappableIndicator candidate
    ) => generator.ForInstance(s => s.AddIndicatorCandidate(columnName, candidate));

    public static InstanceSetters<IndicatorMappingPlan> AddIndicatorMapping(
        this InstanceSetters<IndicatorMappingPlan> instanceSetter,
        string columnName,
        IndicatorMapping mapping
    ) => instanceSetter.Set((_, plan) => plan.Mappings.Add(columnName, mapping));

    public static InstanceSetters<IndicatorMappingPlan> AddIndicatorCandidate(
        this InstanceSetters<IndicatorMappingPlan> instanceSetter,
        string columnName,
        MappableIndicator candidate
    ) => instanceSetter.Set((_, plan) => plan.Candidates.Add(columnName, candidate));

    /**
     * IndicatorMapping
     */
    public static Generator<IndicatorMapping> DefaultIndicatorMapping(this DataFixture fixture) =>
        fixture.Generator<IndicatorMapping>().WithDefaults();

    public static Generator<IndicatorMapping> WithDefaults(this Generator<IndicatorMapping> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<IndicatorMapping> WithSource(
        this Generator<IndicatorMapping> generator,
        MappableIndicator source
    ) => generator.ForInstance(s => s.SetSource(source));

    public static Generator<IndicatorMapping> WithPublicId(
        this Generator<IndicatorMapping> generator,
        string publicId
    ) => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<IndicatorMapping> WithType(this Generator<IndicatorMapping> generator, MappingType type) =>
        generator.ForInstance(s => s.SetType(type));

    public static Generator<IndicatorMapping> WithNoMapping(this Generator<IndicatorMapping> generator) =>
        generator.ForInstance(s => s.SetType(MappingType.None).SetCandidateKey(null));

    public static Generator<IndicatorMapping> WithAutoMapped(
        this Generator<IndicatorMapping> generator,
        string candidateKey
    ) => generator.ForInstance(s => s.SetType(MappingType.AutoMapped).SetCandidateKey(candidateKey));

    public static Generator<IndicatorMapping> WithAutoNone(this Generator<IndicatorMapping> generator) =>
        generator.ForInstance(s => s.SetType(MappingType.AutoNone).SetCandidateKey(null));

    public static Generator<IndicatorMapping> WithManualMapped(
        this Generator<IndicatorMapping> generator,
        string candidateKey
    ) => generator.ForInstance(s => s.SetType(MappingType.ManualMapped).SetCandidateKey(candidateKey));

    public static Generator<IndicatorMapping> WithManualNone(this Generator<IndicatorMapping> generator) =>
        generator.ForInstance(s => s.SetType(MappingType.ManualNone).SetCandidateKey(null));

    public static Generator<IndicatorMapping> WithCandidateKey(
        this Generator<IndicatorMapping> generator,
        string? candidateKey
    ) => generator.ForInstance(s => s.SetCandidateKey(candidateKey));

    public static InstanceSetters<IndicatorMapping> SetDefaults(this InstanceSetters<IndicatorMapping> setters) =>
        setters
            .SetDefault(mapping => mapping.PublicId)
            .SetDefault(mapping => mapping.Type)
            .SetDefault(mapping => mapping.CandidateKey);

    public static InstanceSetters<IndicatorMapping> SetSource(
        this InstanceSetters<IndicatorMapping> setters,
        MappableIndicator source
    ) => setters.Set(mapping => mapping.Source, source);

    public static InstanceSetters<IndicatorMapping> SetPublicId(
        this InstanceSetters<IndicatorMapping> setters,
        string publicId
    ) => setters.Set(mapping => mapping.PublicId, publicId);

    public static InstanceSetters<IndicatorMapping> SetType(
        this InstanceSetters<IndicatorMapping> setters,
        MappingType type
    ) => setters.Set(mapping => mapping.Type, type);

    public static InstanceSetters<IndicatorMapping> SetCandidateKey(
        this InstanceSetters<IndicatorMapping> setters,
        string? candidateKey
    ) => setters.Set(mapping => mapping.CandidateKey, candidateKey);

    /**
     * MappableIndicator
     */
    public static Generator<MappableIndicator> DefaultMappableIndicator(this DataFixture fixture) =>
        fixture.Generator<MappableIndicator>().WithDefaults();

    public static Generator<MappableIndicator> WithDefaults(this Generator<MappableIndicator> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<MappableIndicator> WithLabel(this Generator<MappableIndicator> generator, string label) =>
        generator.ForInstance(s => s.SetLabel(label));

    public static InstanceSetters<MappableIndicator> SetDefaults(this InstanceSetters<MappableIndicator> setters) =>
        setters.SetDefault(option => option.Label);

    public static InstanceSetters<MappableIndicator> SetLabel(
        this InstanceSetters<MappableIndicator> instanceSetter,
        string label
    ) => instanceSetter.Set(option => option.Label, label);
}
