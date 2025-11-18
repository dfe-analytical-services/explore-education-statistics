using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ImportMetadataFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
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

    public class ImportMetadataDbTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ImportMetadataFunctionTests(fixture)
    {
        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);
            var savedDataSetVersion = savedImport.DataSetVersion;

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedDataSetVersion.Status);

            AssertDataSetVersionDirectoryContainsOnlyFiles(
                dataSetVersion,
                DataSetFilenames.CsvDataFile,
                DataSetFilenames.CsvMetadataFile,
                DataSetFilenames.DuckDbDatabaseFile
            );

            Assert.Equal(testData.ExpectedTotalResults, savedDataSetVersion.TotalResults);

            var firstExpectedTimePeriod = testData.ExpectedTimePeriods.First();
            var lastExpectedTimePeriod = testData.ExpectedTimePeriods.Last();

            var expectedMetaSummary = new DataSetVersionMetaSummary
            {
                Filters = testData.ExpectedFilters.Select(fm => fm.Label).ToList(),
                Indicators = testData.ExpectedIndicators.Select(fm => fm.Label).ToList(),
                GeographicLevels = testData.ExpectedGeographicLevels,
                TimePeriodRange = new TimePeriodRange
                {
                    Start = new TimePeriodRangeBound
                    {
                        Code = firstExpectedTimePeriod.Code,
                        Period = firstExpectedTimePeriod.Period,
                    },
                    End = new TimePeriodRangeBound
                    {
                        Code = firstExpectedTimePeriod.Code,
                        Period = lastExpectedTimePeriod.Period,
                    },
                },
            };

            savedDataSetVersion.MetaSummary.AssertDeepEqualTo(expectedMetaSummary);
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectGeographicLevels(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualGeographicLevelMeta = await publicDataDbContext.GeographicLevelMetas.SingleAsync(glm =>
                glm.DataSetVersionId == dataSetVersion.Id
            );

            var actualGeographicLevels = actualGeographicLevelMeta
                .Levels.OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();

            Assert.Equal(testData.ExpectedGeographicLevels.Order(), actualGeographicLevels.Order());
        }

        [Theory]
        [MemberData(nameof(TestDataFilesWithMetaInsertBatchSize))]
        public async Task InitialVersion_CorrectLocationOptions(ProcessorTestData testData, int metaInsertBatchSize)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId, metaInsertBatchSize);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualLocations = await publicDataDbContext
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
        [MemberData(nameof(TestDataFilesWithMetaInsertBatchSize))]
        public async Task NextVersion_CorrectLocationOptions_WithMappings(
            ProcessorTestData testData,
            int metaInsertBatchSize
        )
        {
            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
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

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            await ImportMetadata(testData, targetDataSetVersion, instanceId, metaInsertBatchSize);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualLocations = await publicDataDbContext
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
        public async Task InitialVersion_CorrectTimePeriods_NoExistingTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualTimePeriods = await publicDataDbContext
                .TimePeriodMetas.Where(tpm => tpm.DataSetVersionId == dataSetVersion.Id)
                .OrderBy(tpm => tpm.Period)
                .ToListAsync();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(
                testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    actualTimePeriod.AssertDeepEqualTo(
                        expectedTimePeriod,
                        ignoreProperties: [tpm => tpm.Id, tpm => tpm.DataSetVersionId, tpm => tpm.Created]
                    );

                    Assert.Equal(dataSetVersion.Id, actualTimePeriod.DataSetVersionId);
                    Assert.NotEqual(expectedTimePeriod.Created, actualTimePeriod.Created);
                }
            );
        }

        [Theory]
        [MemberData(nameof(TestDataFilesWithMetaInsertBatchSize))]
        public async Task InitialVersion_CorrectFiltersAndOptions(ProcessorTestData testData, int metaInsertBatchSize)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId, metaInsertBatchSize);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

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

        [Fact]
        public async Task NextVersion_CorrectFilters_AutoMappedWithSamePublicIds()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

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

        [Fact]
        public async Task NextVersion_CorrectFilters_AutoMappedWithOldPublicIds()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                filterMapping.PublicId = $"{filterMapping.PublicId}-old";
                filterMapping.Source.Label = $"{filterMapping.Source.Label} old";
            }

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    // Filter gets old public ID as it was auto mapped
                    expectedFilter.PublicId = $"{expectedFilter.PublicId}-old";

                    AssertFiltersEqual(expectedFilter, actualFilter);
                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectFilters_AutoNone()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                filterMapping.Type = MappingType.AutoNone;
                filterMapping.PublicId = $"{filterMapping.PublicId}-old";
                filterMapping.Source.Label = $"{filterMapping.Source.Label} old";
                filterMapping.CandidateKey = null;
            }

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    // Filter gets new public ID as it had no auto mapping
                    AssertFiltersEqual(expectedFilter, actualFilter);
                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectFilters_ManualMapped()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                filterMapping.Type = MappingType.ManualMapped;
                filterMapping.PublicId = $"{filterMapping.PublicId}-old";
                filterMapping.Source.Label = $"{filterMapping.Source.Label} old";
            }

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    // Filter gets old public ID as it was manually mapped
                    expectedFilter.PublicId = $"{expectedFilter.PublicId}-old";

                    AssertFiltersEqual(expectedFilter, actualFilter);
                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectFilters_MixedMappings()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            var academyTypeMapping = mapping.FilterMappingPlan.Mappings["academy_type"];

            academyTypeMapping.Type = MappingType.AutoNone;
            academyTypeMapping.PublicId = $"{academyTypeMapping.PublicId}-old";
            academyTypeMapping.Source.Label = $"{academyTypeMapping.Source.Label} old";
            academyTypeMapping.CandidateKey = null;

            var ncYearMapping = mapping.FilterMappingPlan.Mappings["ncyear"];

            ncYearMapping.Type = MappingType.ManualMapped;
            ncYearMapping.PublicId = $"{ncYearMapping.PublicId}-old";
            ncYearMapping.Source.Label = $"{ncYearMapping.Source.Label} old";

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    // Filter `ncyear` re-uses old public ID as it was manually mapped.
                    expectedFilter.PublicId =
                        expectedFilter.Column == "ncyear" ? $"{expectedFilter.PublicId}-old" : expectedFilter.PublicId;

                    AssertFiltersEqual(expectedFilter, actualFilter);
                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectFilterOptions_AutoMappedWithSamePublicIds()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

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

        [Fact]
        public async Task NextVersion_CorrectFilterOptions_AutoMappedWithOldPublicIds()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                foreach (var (_, optionMapping) in filterMapping.OptionMappings)
                {
                    optionMapping.PublicId = $"{optionMapping.PublicId}-old";
                    optionMapping.Source.Label = $"{optionMapping.Source.Label} old";
                }
            }

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    AssertFiltersEqual(expectedFilter, actualFilter);

                    // Filter options re-use old public IDs as they were auto mapped
                    foreach (var optionLink in expectedFilter.OptionLinks)
                    {
                        optionLink.PublicId = $"{optionLink.PublicId}-old";
                    }

                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectFilterOptions_AutoNoneOptions()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                foreach (var (_, optionMapping) in filterMapping.OptionMappings)
                {
                    optionMapping.Type = MappingType.AutoNone;
                    optionMapping.PublicId = $"{optionMapping.PublicId}-old";
                    optionMapping.Source.Label = $"{optionMapping.Source.Label} old";
                    optionMapping.CandidateKey = null;
                }
            }

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    AssertFiltersEqual(expectedFilter, actualFilter);

                    // Filter options get new public ID as it they have no auto mapping
                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectFilterOptions_ManualMappedOptions()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                foreach (var (_, optionMapping) in filterMapping.OptionMappings)
                {
                    optionMapping.Type = MappingType.ManualMapped;
                    optionMapping.PublicId = $"{optionMapping.PublicId}-old";
                    optionMapping.Source.Label = $"{optionMapping.Source.Label} old";
                }
            }

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    AssertFiltersEqual(expectedFilter, actualFilter);

                    // Filter options re-use old public IDs as they were manually mapped
                    foreach (var optionLink in expectedFilter.OptionLinks)
                    {
                        optionLink.PublicId = $"{optionLink.PublicId}-old";
                    }

                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectFilterOptions_MixedOptionMappings()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, optionMapping) in mapping.FilterMappingPlan.Mappings["academy_type"].OptionMappings)
            {
                optionMapping.Type = MappingType.AutoNone;
                optionMapping.PublicId = $"{optionMapping.PublicId}-old";
                optionMapping.Source.Label = $"{optionMapping.Source.Label} old";
                optionMapping.CandidateKey = null;
            }

            foreach (var (_, optionMapping) in mapping.FilterMappingPlan.Mappings["ncyear"].OptionMappings)
            {
                optionMapping.Type = MappingType.ManualMapped;
                optionMapping.PublicId = $"{optionMapping.PublicId}-old";
                optionMapping.Source.Label = $"{optionMapping.Source.Label} old";
            }

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    AssertFiltersEqual(expectedFilter, actualFilter);

                    // Only `ncyear` options re-use old public IDs as they were manually mapped.
                    foreach (var optionLink in expectedFilter.OptionLinks)
                    {
                        optionLink.PublicId =
                            expectedFilter.Column == "ncyear" ? $"{optionLink.PublicId}-old" : optionLink.PublicId;
                    }

                    AssertAllFilterOptionsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task InitialVersion_CorrectIndicators()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualIndicators = await GetDbIndicatorMetas(dataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];

                    actualIndicator.AssertDeepEqualTo(
                        expectedIndicator,
                        ignoreProperties: [m => m.DataSetVersionId, m => m.Created]
                    );

                    Assert.Equal(dataSetVersion.Id, actualIndicator.DataSetVersionId);
                    Assert.NotEqual(expectedIndicator.Created, actualIndicator.Created);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectIndicators_OldPublicIds()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var existingIndicators = testData
                .ExpectedIndicators.Select(i =>
                {
                    var indicator = i.ShallowClone();

                    indicator.Id = 0;
                    indicator.PublicId = $"{indicator.PublicId}-old";

                    return indicator;
                })
                .ToList();

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                initialVersionMeta: new DataSetVersionMeta { IndicatorMetas = existingIndicators },
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualIndicators = await GetDbIndicatorMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];

                    actualIndicator.AssertDeepEqualTo(
                        expectedIndicator,
                        ignoreProperties: [m => m.Id, m => m.PublicId, m => m.DataSetVersionId, m => m.Created]
                    );

                    // Indicators re-use old public IDs of existing indicators.
                    Assert.Equal($"{expectedIndicator.PublicId}-old", actualIndicator.PublicId);
                    Assert.Equal(targetDataSetVersion.Id, actualIndicator.DataSetVersionId);
                    Assert.NotEqual(expectedIndicator.Created, actualIndicator.Created);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectIndicators_NewPublicIds()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            // Set up indicators that are nothing like the test data set's indicators.
            var existingIndicators = DataFixture.DefaultIndicatorMeta().GenerateList(3);

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                initialVersionMeta: new DataSetVersionMeta { IndicatorMetas = existingIndicators },
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualIndicators = await GetDbIndicatorMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];

                    actualIndicator.AssertDeepEqualTo(
                        expectedIndicator,
                        ignoreProperties: [m => m.Id, m => m.PublicId, m => m.DataSetVersionId, m => m.Created]
                    );

                    // Indicators get new public IDs offset by number of existing indicators.
                    Assert.Equal(
                        SqidEncoder.Encode(expectedIndicator.Id + existingIndicators.Count),
                        actualIndicator.PublicId
                    );
                    Assert.Equal(targetDataSetVersion.Id, actualIndicator.DataSetVersionId);
                    Assert.NotEqual(expectedIndicator.Created, actualIndicator.Created);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectIndicators_NewPublicIdsWhenColumnsChange()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            // Set up existing indicators that only differ in column names. Currently,
            // indicators can't be mapped so the new indicators must be given new public IDs.
            var existingIndicators = testData
                .ExpectedIndicators.Select(i =>
                {
                    var indicator = i.ShallowClone();

                    indicator.Id = 0;
                    indicator.Column = $"{indicator.Column}_old";

                    return indicator;
                })
                .ToList();

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                initialVersionMeta: new DataSetVersionMeta { IndicatorMetas = existingIndicators },
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualIndicators = await GetDbIndicatorMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];

                    actualIndicator.AssertDeepEqualTo(
                        expectedIndicator,
                        ignoreProperties: [m => m.Id, m => m.PublicId, m => m.DataSetVersionId, m => m.Created]
                    );

                    // Indicators get new public IDs offset by number of existing indicators.
                    Assert.Equal(
                        SqidEncoder.Encode(expectedIndicator.Id + existingIndicators.Count),
                        actualIndicator.PublicId
                    );
                    Assert.Equal(targetDataSetVersion.Id, actualIndicator.DataSetVersionId);
                    Assert.NotEqual(expectedIndicator.Created, actualIndicator.Created);
                }
            );
        }

        [Fact]
        public async Task NextVersion_CorrectIndicators_MixtureHaveCorrectPublicIds()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            List<IndicatorMeta> existingIndicators =
            [
                .. testData.ExpectedIndicators.Select(i =>
                {
                    var indicator = i.ShallowClone();

                    indicator.Id = 0;

                    return indicator;
                }),
                .. DataFixture.DefaultIndicatorMeta().Generate(1),
            ];

            existingIndicators[2].PublicId = $"{existingIndicators[2].PublicId}-old";
            existingIndicators[3].PublicId = $"{existingIndicators[3].PublicId}-old";

            existingIndicators[4].Column = $"{existingIndicators[4].PublicId}_old";

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                initialVersionMeta: new DataSetVersionMeta { IndicatorMetas = existingIndicators },
                nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                nextVersionStatus: DataSetVersionStatus.Mapping
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualIndicators = await GetDbIndicatorMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);

            Assert.Equal(testData.ExpectedIndicators[0].PublicId, actualIndicators[0].PublicId);
            Assert.Equal(testData.ExpectedIndicators[1].PublicId, actualIndicators[1].PublicId);
            Assert.Equal($"{testData.ExpectedIndicators[2].PublicId}-old", actualIndicators[2].PublicId);
            Assert.Equal($"{testData.ExpectedIndicators[3].PublicId}-old", actualIndicators[3].PublicId);
            Assert.Equal(
                SqidEncoder.Encode(testData.ExpectedIndicators[4].Id + existingIndicators.Count),
                actualIndicators[4].PublicId
            );
        }
    }

    public class ImportMetadataDuckDbTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ImportMetadataFunctionTests(fixture)
    {
        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectLocationOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualLocationOptions = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {LocationOptionsTable.TableName:raw}
                        ORDER BY {LocationOptionsTable.Cols.Label:raw}
                        """
                    )
                    .QueryAsync<ParquetLocationOption>()
            ).AsList();

            var actualOptionsByLevel = actualLocationOptions
                .GroupBy(o => o.Level)
                .ToDictionary(g => EnumUtil.GetFromEnumValue<GeographicLevel>(g.Key), g => g.ToList());

            Assert.Equal(testData.ExpectedLocations.Count, actualOptionsByLevel.Count);
            Assert.All(
                testData.ExpectedLocations,
                expectedLocation =>
                {
                    Assert.True(actualOptionsByLevel.TryGetValue(expectedLocation.Level, out var actualOptions));
                    Assert.Equal(expectedLocation.Options.Count, actualOptions.Count);
                    Assert.All(
                        expectedLocation.Options.OrderBy(o => o.Label),
                        (expectedOption, index) => expectedOption.AssertEqual(actualOptions[index])
                    );
                }
            );
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualTimePeriods = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {TimePeriodsTable.TableName:raw}
                        ORDER BY {TimePeriodsTable.Cols.Period:raw}
                        """
                    )
                    .QueryAsync<ParquetTimePeriod>()
            ).AsList();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(
                testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    Assert.Equal(
                        expectedTimePeriod.Code,
                        EnumUtil.GetFromEnumLabel<TimeIdentifier>(actualTimePeriod.Identifier)
                    );
                    Assert.Equal(expectedTimePeriod.Period, TimePeriodFormatter.FormatFromCsv(actualTimePeriod.Period));
                }
            );
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectFilterOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualFilterOptions = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {FilterOptionsTable.TableName:raw}
                        ORDER BY {FilterOptionsTable.Cols.Label:raw}
                        """
                    )
                    .QueryAsync<ParquetFilterOption>()
            ).AsList();

            var actualOptionsByFilterId = actualFilterOptions
                .GroupBy(o => o.FilterId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var globalOptionIndex = 0;

            Assert.Equal(testData.ExpectedFilters.Count, actualOptionsByFilterId.Count);
            Assert.All(
                testData.ExpectedFilters,
                expectedFilter =>
                {
                    Assert.True(actualOptionsByFilterId.TryGetValue(expectedFilter.PublicId, out var actualOptions));
                    Assert.Equal(expectedFilter.Options.Count, actualOptions.Count);
                    Assert.All(
                        expectedFilter.Options,
                        (expectedOption, index) =>
                        {
                            var actualOption = actualOptions[index];
                            Assert.Equal(expectedOption.Label, actualOption.Label);
                            Assert.Equal(expectedFilter.PublicId, actualOption.FilterId);

                            // Expect the PublicId to be that of the option link which in turn is expected to be encoded
                            // based on the sequence of option links inserted across all filters
                            Assert.Equal(SqidEncoder.Encode(++globalOptionIndex), actualOption.PublicId);
                        }
                    );
                }
            );
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectIndicators(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualIndicators = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {IndicatorsTable.TableName:raw}
                        ORDER BY {IndicatorsTable.Cols.Label:raw}
                        """
                    )
                    .QueryAsync<ParquetIndicator>()
            ).AsList();

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];
                    Assert.Equal(expectedIndicator.PublicId, actualIndicator.Id);
                    Assert.Equal(expectedIndicator.Label, actualIndicator.Label);
                    Assert.Equal(expectedIndicator.DecimalPlaces, actualIndicator.DecimalPlaces);

                    if (expectedIndicator.Unit != null)
                    {
                        Assert.Equal(
                            expectedIndicator.Unit,
                            EnumUtil.GetFromEnumValue<IndicatorUnit>(actualIndicator.Unit)
                        );
                    }
                    else
                    {
                        Assert.Empty(actualIndicator.Unit);
                    }
                }
            );
        }
    }

    private async Task ImportMetadata(
        ProcessorTestData testData,
        DataSetVersion dataSetVersion,
        Guid instanceId,
        int? metaInsertBatchSize = null
    )
    {
        SetupCsvDataFilesForDataSetVersion(testData, dataSetVersion);

        // Override default app settings if provided
        if (metaInsertBatchSize.HasValue)
        {
            var appOptions = GetRequiredService<IOptions<AppOptions>>();
            appOptions.Value.MetaInsertBatchSize = metaInsertBatchSize.Value;
        }

        var function = GetRequiredService<ImportMetadataFunction>();
        await function.ImportMetadata(instanceId, CancellationToken.None);
    }

    private async Task<List<FilterMeta>> GetDbFilterMetas(Guid dataSetVersionId) =>
        await GetDbContext<PublicDataDbContext>()
            .FilterMetas.Include(fm => fm.Options.OrderBy(o => o.Label))
            .Include(fm => fm.OptionLinks.OrderBy(l => l.Option.Label))
            .ThenInclude(fm => fm.Option)
            .Where(fm => fm.DataSetVersionId == dataSetVersionId)
            .OrderBy(fm => fm.Label)
            .ToListAsync();

    private Task<List<IndicatorMeta>> GetDbIndicatorMetas(Guid dataSetVersionId) =>
        GetDbContext<PublicDataDbContext>()
            .IndicatorMetas.Where(im => im.DataSetVersionId == dataSetVersionId)
            .OrderBy(im => im.Label)
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
