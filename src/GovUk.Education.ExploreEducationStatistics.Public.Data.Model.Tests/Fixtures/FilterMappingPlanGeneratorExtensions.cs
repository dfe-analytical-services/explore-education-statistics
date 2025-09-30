using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

public static class FilterMappingPlanGeneratorExtensions
{
    /**
     * FilterMappingPlan
     */
    public static Generator<FilterMappingPlan> DefaultFilterMappingPlan(this DataFixture fixture)
        => fixture.Generator<FilterMappingPlan>();

    public static Generator<FilterMappingPlan> FilterMappingPlanFromFilterMeta(
        this DataFixture fixture,
        IEnumerable<FilterMeta>? sourceFilters = null,
        IEnumerable<FilterMeta>? targetFilters = null)
    {
        var filterMappingPlanGenerator = fixture.Generator<FilterMappingPlan>();

        sourceFilters?.ForEach(sourceFilter =>
        {
            var autoMappedFilter = targetFilters?.SingleOrDefault(f => f.Column == sourceFilter.Column);

            var filterMappingGenerator = fixture.DefaultFilterMapping()
                .WithSource(fixture.DefaultMappableFilter()
                    .WithLabel(sourceFilter.Label))
                .WithType(autoMappedFilter is not null ? MappingType.AutoMapped : MappingType.AutoNone)
                .WithCandidateKey(autoMappedFilter?.Column)
                .WithPublicId(sourceFilter.PublicId);

            sourceFilter.Options.ForEach(option =>
            {
                var sourceKey = MappingKeyGenerators.FilterOptionMeta(option);
                var sourceLink = sourceFilter.OptionLinks.SingleOrDefault(l => l.OptionId == option.Id);

                var autoMappedOption = autoMappedFilter?.Options
                    .SingleOrDefault(o => MappingKeyGenerators.FilterOptionMeta(o) == sourceKey);

                filterMappingGenerator.AddOptionMapping(
                    sourceKey: sourceKey,
                    mapping: fixture.DefaultFilterOptionMapping()
                        .WithSource(fixture.DefaultMappableFilterOption()
                            .WithLabel(option.Label))
                        .WithType(autoMappedOption is not null ? MappingType.AutoMapped : MappingType.AutoNone)
                        .WithCandidateKey(autoMappedOption is not null
                            ? MappingKeyGenerators.FilterOptionMeta(autoMappedOption)
                            : null)
                        .WithPublicId(sourceLink?.PublicId ?? $"{sourceFilter.PublicId} :: {option.Label}")
                );
            });

            filterMappingPlanGenerator.AddFilterMapping(sourceFilter.Column, filterMappingGenerator);
        });

        targetFilters?.ForEach(targetFilter =>
        {
            var filterCandidateGenerator = fixture
                .DefaultFilterMappingCandidate()
                .WithLabel(targetFilter.Label);

            targetFilter.Options.ForEach(option =>
            {
                filterCandidateGenerator.AddOptionCandidate(
                    targetKey: MappingKeyGenerators.FilterOptionMeta(option),
                    candidate: fixture.DefaultMappableFilterOption()
                        .WithLabel(option.Label));
            });

            filterMappingPlanGenerator.AddFilterCandidate(targetFilter.Column, filterCandidateGenerator);
        });

        return filterMappingPlanGenerator;
    }

    public static Generator<FilterMappingPlan> AddFilterMapping(
        this Generator<FilterMappingPlan> generator,
        string columnName,
        FilterMapping mapping)
        => generator.ForInstance(s => s.AddFilterMapping(columnName, mapping));

    public static Generator<FilterMappingPlan> AddFilterCandidate(
        this Generator<FilterMappingPlan> generator,
        string columnName,
        FilterMappingCandidate candidate)
        => generator.ForInstance(s => s.AddFilterCandidate(columnName, candidate));

    public static InstanceSetters<FilterMappingPlan> AddFilterMapping(
        this InstanceSetters<FilterMappingPlan> instanceSetter,
        string columnName,
        FilterMapping mapping)
        => instanceSetter.Set((_, plan) => plan.Mappings.Add(columnName, mapping));

    public static InstanceSetters<FilterMappingPlan> AddFilterCandidate(
        this InstanceSetters<FilterMappingPlan> instanceSetter,
        string columnName,
        FilterMappingCandidate candidate)
        => instanceSetter.Set((_, plan) => plan.Candidates.Add(columnName, candidate));

    /**
     * MappableFilter
     */
    public static Generator<MappableFilter> DefaultMappableFilter(this DataFixture fixture)
        => fixture.Generator<MappableFilter>().WithDefaults();

    public static Generator<MappableFilter> WithDefaults(this Generator<MappableFilter> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<MappableFilter> WithLabel(
        this Generator<MappableFilter> generator,
        string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static InstanceSetters<MappableFilter> SetDefaults(
        this InstanceSetters<MappableFilter> setters)
        => setters
            .SetDefault(option => option.Label);

    public static InstanceSetters<MappableFilter> SetLabel(
        this InstanceSetters<MappableFilter> instanceSetter,
        string label)
        => instanceSetter.Set(option => option.Label, label);

    /**
     * FilterMapping
     */
    public static Generator<FilterMapping> DefaultFilterMapping(this DataFixture fixture)
        => fixture.Generator<FilterMapping>().WithDefaults();

    public static Generator<FilterMapping> WithDefaults(this Generator<FilterMapping> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterMapping> WithSource(
        this Generator<FilterMapping> generator,
        MappableFilter source)
        => generator.ForInstance(s => s.SetSource(source));

    public static Generator<FilterMapping> WithPublicId(
        this Generator<FilterMapping> generator,
        string publicId)
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<FilterMapping> WithType(
        this Generator<FilterMapping> generator,
        MappingType type)
        => generator.ForInstance(s => s.SetType(type));

    public static Generator<FilterMapping> WithNoMapping(
        this Generator<FilterMapping> generator)
        => generator.ForInstance(s => s
            .SetType(MappingType.None)
            .SetCandidateKey(null));

    public static Generator<FilterMapping> WithAutoMapped(
        this Generator<FilterMapping> generator,
        string candidateKey)
        => generator.ForInstance(s => s
            .SetType(MappingType.AutoMapped)
            .SetCandidateKey(candidateKey));

    public static Generator<FilterMapping> WithAutoNone(
        this Generator<FilterMapping> generator)
        => generator.ForInstance(s => s
            .SetType(MappingType.AutoNone)
            .SetCandidateKey(null));

    public static Generator<FilterMapping> WithManualMapped(
        this Generator<FilterMapping> generator,
        string candidateKey)
        => generator.ForInstance(s => s
            .SetType(MappingType.ManualMapped)
            .SetCandidateKey(candidateKey));

    public static Generator<FilterMapping> WithManualNone(
        this Generator<FilterMapping> generator)
        => generator.ForInstance(s => s
            .SetType(MappingType.ManualNone)
            .SetCandidateKey(null));

    public static Generator<FilterMapping> WithCandidateKey(
        this Generator<FilterMapping> generator,
        string? candidateKey)
        => generator.ForInstance(s => s.SetCandidateKey(candidateKey));

    public static Generator<FilterMapping> AddOptionMapping(
        this Generator<FilterMapping> generator,
        string sourceKey,
        FilterOptionMapping mapping)
        => generator.ForInstance(s => s.AddOptionMapping(sourceKey, mapping));

    public static InstanceSetters<FilterMapping> SetDefaults(
        this InstanceSetters<FilterMapping> setters)
        => setters
            .SetDefault(mapping => mapping.PublicId)
            .SetDefault(mapping => mapping.Type)
            .SetDefault(mapping => mapping.CandidateKey);

    public static InstanceSetters<FilterMapping> SetSource(
        this InstanceSetters<FilterMapping> setters,
        MappableFilter source)
        => setters.Set(mapping => mapping.Source, source);

    public static InstanceSetters<FilterMapping> SetPublicId(
        this InstanceSetters<FilterMapping> setters,
        string publicId)
        => setters.Set(mapping => mapping.PublicId, publicId);

    public static InstanceSetters<FilterMapping> SetType(
        this InstanceSetters<FilterMapping> setters,
        MappingType type)
        => setters.Set(mapping => mapping.Type, type);

    public static InstanceSetters<FilterMapping> SetCandidateKey(
        this InstanceSetters<FilterMapping> setters,
        string? candidateKey)
        => setters.Set(mapping => mapping.CandidateKey, candidateKey);

    public static InstanceSetters<FilterMapping> AddOptionMapping(
        this InstanceSetters<FilterMapping> instanceSetter,
        string columnName,
        FilterOptionMapping mapping)
        => instanceSetter.Set((_, plan) => plan.OptionMappings.Add(columnName, mapping));

    /**
     * FilterMappingCandidate
     */
    public static Generator<FilterMappingCandidate> DefaultFilterMappingCandidate(this DataFixture fixture)
        => fixture.Generator<FilterMappingCandidate>().WithDefaults();

    public static Generator<FilterMappingCandidate> WithDefaults(this Generator<FilterMappingCandidate> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterMappingCandidate> WithLabel(
        this Generator<FilterMappingCandidate> generator,
        string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static Generator<FilterMappingCandidate> AddOptionCandidate(
        this Generator<FilterMappingCandidate> generator,
        string targetKey,
        MappableFilterOption candidate)
        => generator.ForInstance(s => s.AddOptionCandidate(targetKey, candidate));

    public static InstanceSetters<FilterMappingCandidate> SetDefaults(
        this InstanceSetters<FilterMappingCandidate> setters)
        => setters
            .SetDefault(option => option.Label);

    public static InstanceSetters<FilterMappingCandidate> SetLabel(
        this InstanceSetters<FilterMappingCandidate> instanceSetter,
        string label)
        => instanceSetter.Set(option => option.Label, label);

    public static InstanceSetters<FilterMappingCandidate> AddOptionCandidate(
        this InstanceSetters<FilterMappingCandidate> instanceSetter,
        string targetKey,
        MappableFilterOption candidate)
        => instanceSetter.Set((_, plan) => plan.Options.Add(targetKey, candidate));

    /**
     * MappableFilterOption
     */
    public static Generator<MappableFilterOption> DefaultMappableFilterOption(this DataFixture fixture)
        => fixture.Generator<MappableFilterOption>().WithDefaults();

    public static Generator<MappableFilterOption> WithDefaults(this Generator<MappableFilterOption> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<MappableFilterOption> WithLabel(
        this Generator<MappableFilterOption> generator,
        string label)
        => generator.ForInstance(s => s.SetLabel(label));

    public static InstanceSetters<MappableFilterOption> SetDefaults(
        this InstanceSetters<MappableFilterOption> setters)
        => setters
            .SetDefault(option => option.Label);

    public static InstanceSetters<MappableFilterOption> SetLabel(
        this InstanceSetters<MappableFilterOption> instanceSetter,
        string label)
        => instanceSetter.Set(option => option.Label, label);

    /**
     * FilterOptionMapping
     */
    public static Generator<FilterOptionMapping> DefaultFilterOptionMapping(this DataFixture fixture)
        => fixture.Generator<FilterOptionMapping>().WithDefaults();

    public static Generator<FilterOptionMapping> WithDefaults(this Generator<FilterOptionMapping> generator)
        => generator.ForInstance(s => s.SetDefaults());

    public static Generator<FilterOptionMapping> WithSource(
        this Generator<FilterOptionMapping> generator,
        MappableFilterOption source)
        => generator.ForInstance(s => s.SetSource(source));

    public static Generator<FilterOptionMapping> WithPublicId(
        this Generator<FilterOptionMapping> generator,
        string publicId)
        => generator.ForInstance(s => s.SetPublicId(publicId));

    public static Generator<FilterOptionMapping> WithType(
        this Generator<FilterOptionMapping> generator,
        MappingType type)
        => generator.ForInstance(s => s.SetType(type));

    public static Generator<FilterOptionMapping> WithNoMapping(
        this Generator<FilterOptionMapping> generator)
        => generator.ForInstance(s => s
            .SetType(MappingType.None)
            .SetCandidateKey(null));

    public static Generator<FilterOptionMapping> WithAutoMapped(
        this Generator<FilterOptionMapping> generator,
        string candidateKey)
        => generator.ForInstance(s => s
            .SetType(MappingType.AutoMapped)
            .SetCandidateKey(candidateKey));

    public static Generator<FilterOptionMapping> WithAutoNone(
        this Generator<FilterOptionMapping> generator)
        => generator.ForInstance(s => s
            .SetType(MappingType.AutoNone)
            .SetCandidateKey(null));

    public static Generator<FilterOptionMapping> WithManualMapped(
        this Generator<FilterOptionMapping> generator,
        string candidateKey)
        => generator.ForInstance(s => s
            .SetType(MappingType.ManualMapped)
            .SetCandidateKey(candidateKey));

    public static Generator<FilterOptionMapping> WithManualNone(
        this Generator<FilterOptionMapping> generator)
        => generator.ForInstance(s => s
            .SetType(MappingType.ManualNone)
            .SetCandidateKey(null));

    public static Generator<FilterOptionMapping> WithCandidateKey(
        this Generator<FilterOptionMapping> generator,
        string? candidateKey)
        => generator.ForInstance(s => s.SetCandidateKey(candidateKey));

    public static InstanceSetters<FilterOptionMapping> SetDefaults(
        this InstanceSetters<FilterOptionMapping> setters)
        => setters
            .SetDefault(mapping => mapping.PublicId)
            .SetDefault(mapping => mapping.Type)
            .SetDefault(mapping => mapping.CandidateKey);

    public static InstanceSetters<FilterOptionMapping> SetSource(
        this InstanceSetters<FilterOptionMapping> setters,
        MappableFilterOption source)
        => setters.Set(mapping => mapping.Source, source);

    public static InstanceSetters<FilterOptionMapping> SetPublicId(
        this InstanceSetters<FilterOptionMapping> setters,
        string publicId)
        => setters.Set(mapping => mapping.PublicId, publicId);

    public static InstanceSetters<FilterOptionMapping> SetType(
        this InstanceSetters<FilterOptionMapping> setters,
        MappingType type)
        => setters.Set(mapping => mapping.Type, type);

    public static InstanceSetters<FilterOptionMapping> SetCandidateKey(
        this InstanceSetters<FilterOptionMapping> setters,
        string? candidateKey)
        => setters.Set(mapping => mapping.CandidateKey, candidateKey);
}
