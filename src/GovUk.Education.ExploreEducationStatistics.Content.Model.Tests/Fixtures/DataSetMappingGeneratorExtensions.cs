using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataSetMappingGeneratorExtensions
{
    public static Generator<DataSetMapping> DefaultDataSetMapping(this DataFixture fixture) =>
        fixture.Generator<DataSetMapping>().WithDefaults();

    public static Generator<DataSetMapping> WithDefaults(this Generator<DataSetMapping> generator) =>
        generator.ForInstance(s => s.SetDefaults());

    public static Generator<DataSetMapping> WithOriginalDataFileId(
        this Generator<DataSetMapping> generator,
        Guid originalDataFileId
    ) => generator.ForInstance(m => m.SetOriginalDataFileId(originalDataFileId));

    public static Generator<DataSetMapping> WithOriginalDataFile(
        this Generator<DataSetMapping> generator,
        File originalDataFile
    ) => generator.ForInstance(m => m.SetOriginalDataFile(originalDataFile));

    public static Generator<DataSetMapping> WithReplacementDataFileId(
        this Generator<DataSetMapping> generator,
        Guid replacementDataFileId
    ) => generator.ForInstance(m => m.SetReplacementDataFileId(replacementDataFileId));

    public static Generator<DataSetMapping> WithReplacementDataFile(
        this Generator<DataSetMapping> generator,
        File replacementDataFile
    ) => generator.ForInstance(m => m.SetReplacementDataFile(replacementDataFile));

    public static Generator<DataSetMapping> WithIndicatorMappings(
        this Generator<DataSetMapping> generator,
        Dictionary<Guid, IndicatorMapping> indicatorMappings
    ) => generator.ForInstance(m => m.SetIndicatorMappings(indicatorMappings));

    public static Generator<DataSetMapping> WithUnmappedReplacementIndicators(
        this Generator<DataSetMapping> generator,
        List<UnmappedIndicator> unmappedIndicators
    ) => generator.ForInstance(m => m.SetUnmappedReplacementIndicators(unmappedIndicators));

    public static Generator<DataSetMapping> WithLocationMappings(
        this Generator<DataSetMapping> generator,
        Dictionary<Guid, LocationMapping> locationMappings
    ) => generator.ForInstance(m => m.SetLocationMappings(locationMappings));

    public static Generator<DataSetMapping> WithUnmappedReplacementLocations(
        this Generator<DataSetMapping> generator,
        List<UnmappedLocation> unmappedLocations
    ) => generator.ForInstance(m => m.SetUnmappedReplacementLocations(unmappedLocations));

    public static InstanceSetters<DataSetMapping> SetDefaults(this InstanceSetters<DataSetMapping> setters) =>
        setters
            .Set(m => m.IndicatorMappings, new Dictionary<Guid, IndicatorMapping>())
            .Set(m => m.LocationMappings, new Dictionary<Guid, LocationMapping>());

    public static InstanceSetters<DataSetMapping> SetOriginalDataFileId(
        this InstanceSetters<DataSetMapping> setters,
        Guid originalDataFileId
    ) => setters.Set(m => m.OriginalDataFileId, originalDataFileId);

    public static InstanceSetters<DataSetMapping> SetOriginalDataFile(
        this InstanceSetters<DataSetMapping> setters,
        Content.Model.File originalDataFile
    ) => setters.Set(m => m.OriginalDataFile, originalDataFile).SetOriginalDataFileId(originalDataFile.Id);

    public static InstanceSetters<DataSetMapping> SetReplacementDataFileId(
        this InstanceSetters<DataSetMapping> setters,
        Guid replacementDataFileId
    ) => setters.Set(m => m.ReplacementDataFileId, replacementDataFileId);

    public static InstanceSetters<DataSetMapping> SetReplacementDataFile(
        this InstanceSetters<DataSetMapping> setters,
        Content.Model.File replacementDataFile
    ) => setters.Set(m => m.ReplacementDataFile, replacementDataFile).SetReplacementDataFileId(replacementDataFile.Id);

    public static InstanceSetters<DataSetMapping> SetIndicatorMappings(
        this InstanceSetters<DataSetMapping> setters,
        Dictionary<Guid, IndicatorMapping> indicatorMappings
    ) => setters.Set(m => m.IndicatorMappings, indicatorMappings);

    public static InstanceSetters<DataSetMapping> SetUnmappedReplacementIndicators(
        this InstanceSetters<DataSetMapping> setters,
        List<UnmappedIndicator> unmappedIndicators
    ) => setters.Set(m => m.UnmappedReplacementIndicators, unmappedIndicators);

    public static InstanceSetters<DataSetMapping> SetLocationMappings(
        this InstanceSetters<DataSetMapping> setters,
        Dictionary<Guid, LocationMapping> locationMappings
    ) => setters.Set(m => m.LocationMappings, locationMappings);

    public static InstanceSetters<DataSetMapping> SetUnmappedReplacementLocations(
        this InstanceSetters<DataSetMapping> setters,
        List<UnmappedLocation> unmappedLocations
    ) => setters.Set(m => m.UnmappedReplacementLocations, unmappedLocations);
}
