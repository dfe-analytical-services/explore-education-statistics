using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class ImportMetadataFunctionMetaInsertBatchSizeTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public ImportMetadataFunction Function = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        var dataFilesBasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        serviceModifications.AddInMemoryCollection([
            new KeyValuePair<string, string?>("DataFiles:BasePath", dataFilesBasePath),
            new KeyValuePair<string, string?>("App:MetaInsertBatchSize", "1"),
        ]);
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<ImportMetadataFunction>();
    }

    public override async Task BeforeEachTest()
    {
        await base.BeforeEachTest();

        // The expected test data in a great deal of these tests is dependent on id sequences
        // beginning at 1, so for simplicity we reset data between each test.
        await GetPublicDataDbContext().ClearTestData();
    }
}

[CollectionDefinition(nameof(ImportMetadataFunctionMetaInsertBatchSizeTestsFixture))]
public class ImportMetadataFunctionMetaInsertBatchSizeTestsCollection
    : ICollectionFixture<ImportMetadataFunctionMetaInsertBatchSizeTestsFixture>;

[Collection(nameof(ImportMetadataFunctionMetaInsertBatchSizeTestsFixture))]
public abstract class ImportMetadataFunctionMetaInsertBatchSizeTests(
    ImportMetadataFunctionMetaInsertBatchSizeTestsFixture fixture
) : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingMetadata;

    public static readonly TheoryData<ProcessorTestData> TestDataFiles = new()
    {
        ProcessorTestData.AbsenceSchool,
        ProcessorTestData.FilterDefaultOptions,
    };

    public static readonly TheoryData<ProcessorTestData, int> TestDataFilesWithMetaInsertBatchSize = new()
    {
        { ProcessorTestData.AbsenceSchool, 1 },
        { ProcessorTestData.AbsenceSchool, 1000 },
        { ProcessorTestData.FilterDefaultOptions, 1000 },
    };

    public class ImportMetadataDbTests(ImportMetadataFunctionMetaInsertBatchSizeTestsFixture fixture)
        : ImportMetadataFunctionMetaInsertBatchSizeTests(fixture)
    {
        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectLocationOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            var actualLocations = await fixture
                .GetPublicDataDbContext()
                .LocationMetas.Include(lm => lm.Options)
                .Include(lm => lm.OptionLinks)
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .OrderBy(lm => lm.Level)
                .ToListAsync();

            // Locations are expected in order of level
            // Location options are expected in order of code(s) and then by label
            Assert.Equal(testData.ExpectedLocations.Count, actualLocations.Count);
            Assert.All(
                testData.ExpectedLocations,
                (expectedLocation, index) =>
                {
                    var actualLocation = actualLocations[index];
                    actualLocation.AssertDeepEqualTo(
                        expectedLocation,
                        ignoreProperties: [l => l.DataSetVersionId, l => l.Options, l => l.OptionLinks, l => l.Created]
                    );

                    Assert.Equal(dataSetVersion.Id, actualLocation.DataSetVersionId);
                    Assert.NotEqual(expectedLocation.Created, actualLocation.Created);

                    Assert.Equal(expectedLocation.Options.Count, actualLocation.Options.Count);
                    Assert.All(
                        expectedLocation.Options,
                        (expectedOption, optionIndex) =>
                        {
                            var actualOption = actualLocation.Options[optionIndex];
                            actualOption.AssertDeepEqualTo(
                                expectedOption,
                                ignoreProperties: [o => o.Metas, o => o.MetaLinks]
                            );
                        }
                    );
                }
            );

            // Public Ids should be Sqids based on the option's id.
            var actualLinks = actualLocations.SelectMany(level => level.OptionLinks).ToList();

            Assert.Equal(testData.ExpectedLocations.Sum(l => l.Options.Count), actualLinks.Count);
            Assert.All(actualLinks, link => Assert.Equal(SqidEncoder.Encode(link.OptionId), link.PublicId));
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task NextVersion_CorrectLocationOptions_WithMappings(ProcessorTestData testData)
        {
            var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    fixture.GetPublicDataDbContext(),
                    nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                    nextVersionStatus: DataSetVersionStatus.Mapping
                );

            // In this test, we will create mappings for all the original location options.
            // 2 of these mappings will have candidates, and the rest will have no candidates
            // mapped.
            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture.LocationMappingPlanFromLocationMeta(sourceLocations: testData.ExpectedLocations)
                );

            var random = new Random();

            mappings.LocationMappingPlan.Levels.ForEach(level =>
                level.Value.Mappings.ForEach(mapping =>
                    mapping.Value.Type = random.Next(1) == 0 ? MappingType.AutoNone : MappingType.ManualNone
                )
            );

            // Amend a couple of arbitrary mappings to identify some candidates.
            var firstLevel = testData.ExpectedLocations.First();
            var lastLevel = testData.ExpectedLocations.Last();
            var mappedOption1Key = MappingKeyGenerators.LocationOptionMeta(firstLevel.Options.First());
            var mappedOption2Key = MappingKeyGenerators.LocationOptionMeta(lastLevel.Options.Last());
            var mappedOption1 = mappings.GetLocationOptionMapping(firstLevel.Level, mappedOption1Key);
            var mappedOption2 = mappings.GetLocationOptionMapping(lastLevel.Level, mappedOption2Key);

            mappings.LocationMappingPlan.Levels[firstLevel.Level].Mappings[mappedOption1Key] = mappedOption1 with
            {
                PublicId = "id-1",
                Type = MappingType.AutoMapped,
                CandidateKey = mappedOption1Key,
            };

            mappings.LocationMappingPlan.Levels[lastLevel.Level].Mappings[mappedOption2Key] = mappedOption2 with
            {
                PublicId = "id-2",
                Type = MappingType.ManualMapped,
                CandidateKey = mappedOption2Key,
            };

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersionMappings.Add(mappings);
                });

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualLocations = await fixture
                .GetPublicDataDbContext()
                .LocationMetas.Include(lm => lm.Options)
                .Include(lm => lm.OptionLinks)
                    .ThenInclude(locationOptionMetaLink => locationOptionMetaLink.Option)
                .Where(lm => lm.DataSetVersionId == targetDataSetVersion.Id)
                .OrderBy(lm => lm.Level)
                .ToListAsync();

            // Locations are expected in order of level
            // Location options are expected in order of code(s) and then by label
            Assert.Equal(testData.ExpectedLocations.Count, actualLocations.Count);
            Assert.All(
                testData.ExpectedLocations,
                (expectedLocation, index) =>
                {
                    var actualLocation = actualLocations[index];
                    actualLocation.AssertDeepEqualTo(
                        expectedLocation,
                        ignoreProperties: [l => l.DataSetVersionId, l => l.Options, l => l.OptionLinks, l => l.Created]
                    );

                    Assert.Equal(targetDataSetVersion.Id, actualLocation.DataSetVersionId);
                    Assert.NotEqual(expectedLocation.Created, actualLocation.Created);

                    Assert.Equal(expectedLocation.Options.Count, actualLocation.Options.Count);
                    Assert.All(
                        expectedLocation.Options,
                        (expectedOption, optionIndex) =>
                        {
                            var actualOption = actualLocation.Options[optionIndex];
                            actualOption.AssertDeepEqualTo(
                                expectedOption,
                                ignoreProperties: [o => o.Metas, o => o.MetaLinks]
                            );
                        }
                    );
                }
            );

            var actualLinks = actualLocations.SelectMany(level => level.OptionLinks).ToList();

            // Public Ids should be SQIDs based on the option's id unless otherwise directed by the
            // mappings.
            var actualMappedOption1Link = actualLinks.Single(link => link.Option.Label == mappedOption1.Source.Label);
            Assert.Equal("id-1", actualMappedOption1Link.PublicId);

            var actualMappedOption2Link = actualLinks.Single(link => link.Option.Label == mappedOption2.Source.Label);
            Assert.Equal("id-2", actualMappedOption2Link.PublicId);

            var otherLinks = actualLocations
                .SelectMany(level => level.OptionLinks)
                .Where(link => link != actualMappedOption1Link && link != actualMappedOption2Link)
                .ToList();

            Assert.Equal(testData.ExpectedLocations.Sum(l => l.Options.Count), actualLinks.Count);
            Assert.All(otherLinks, link => Assert.Equal(SqidEncoder.Encode(link.OptionId), link.PublicId));
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectFiltersAndOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            var actualFilters = await GetDbFilterMetas(dataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    AssertFiltersEqual(expectedFilter, actualFilter);
                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }
    }

    private async Task ImportMetadata(ProcessorTestData testData, DataSetVersion dataSetVersion, Guid instanceId)
    {
        CommonTestDataUtils.SetupCsvDataFilesForDataSetVersion(
            fixture.GetDataSetVersionPathResolver(),
            testData,
            dataSetVersion
        );

        await fixture.Function.ImportMetadata(instanceId, CancellationToken.None);
    }

    private async Task<List<FilterMeta>> GetDbFilterMetas(Guid dataSetVersionId) =>
        await fixture
            .GetPublicDataDbContext()
            .FilterMetas.Include(fm => fm.Options.OrderBy(o => o.Label))
            .Include(fm => fm.OptionLinks.OrderBy(l => l.Option.Label))
                .ThenInclude(fm => fm.Option)
            .Where(fm => fm.DataSetVersionId == dataSetVersionId)
            .OrderBy(fm => fm.Label)
            .ToListAsync();

    private static void AssertAllFilterOptionsEqual(FilterMeta expectedFilter, FilterMeta actualFilter)
    {
        Assert.Equal(expectedFilter.Options.Count, actualFilter.Options.Count);
        Assert.Equal(expectedFilter.OptionLinks.Count, actualFilter.OptionLinks.Count);

        Assert.All(
            expectedFilter.OptionLinks,
            (expectedOptionLink, linkIndex) =>
            {
                var actualOptionLink = actualFilter.OptionLinks[linkIndex];

                Assert.Equal(expectedOptionLink.PublicId, actualOptionLink.PublicId);

                var expectedOption = expectedFilter.Options[linkIndex];

                Assert.Equal(expectedOption.Label, actualOptionLink.Option.Label);
            }
        );
    }

    private static void AssertFiltersEqual(FilterMeta expectedFilter, FilterMeta actualFilter)
    {
        actualFilter.AssertDeepEqualTo(
            expectedFilter,
            ignoreProperties:
            [
                m => m.DefaultOption,
                m => m.DataSetVersionId,
                m => m.Options,
                m => m.OptionLinks,
                m => m.Created,
            ]
        );
    }
}
