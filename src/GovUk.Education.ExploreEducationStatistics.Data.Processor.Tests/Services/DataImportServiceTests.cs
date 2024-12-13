#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services
{
    public class DataImportServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task GetImportStatus()
        {
            var import = new DataImport
            {
                Status = STAGE_1
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var service = BuildDataImportService(contentDbContextId: contentDbContextId);
            var result = await service.GetImportStatus(import.Id);

            Assert.Equal(STAGE_1, result);
        }

        [Fact]
        public async Task GetImportStatus_NotFound()
        {
            var service = BuildDataImportService();
            var result = await service.GetImportStatus(Guid.NewGuid());

            Assert.Equal(NOT_FOUND, result);
        }

        [Fact]
        public async Task GetImport()
        {
            var import = new DataImport
            {
                File = new File(),
                MetaFile = new File(),
                ZipFile = new File(),
                Status = STAGE_1
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var service = BuildDataImportService(contentDbContextId: contentDbContextId);
            var result = await service.GetImport(import.Id);

            Assert.Equal(import.Id, result.Id);

            Assert.Empty(result.Errors);

            Assert.NotNull(import.File);
            Assert.Equal(import.File.Id, result.File.Id);

            Assert.NotNull(import.MetaFile);
            Assert.Equal(import.MetaFile.Id, result.MetaFile.Id);

            Assert.NotNull(import.ZipFile);
            Assert.Equal(import.ZipFile.Id, result.ZipFile!.Id);
        }

        [Fact]
        public async Task GetImport_ImportHasErrors()
        {
            var import = new DataImport
            {
                Errors = new List<DataImportError>
                {
                    new("error 1"),
                    new("error 2")
                },
                File = new File(),
                MetaFile = new File(),
                ZipFile = new File(),
                Status = STAGE_1
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var service = BuildDataImportService(contentDbContextId: contentDbContextId);
            var result = await service.GetImport(import.Id);

            Assert.Equal(import.Id, result.Id);

            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("error 1", result.Errors[0].Message);
            Assert.Equal("error 2", result.Errors[1].Message);
        }

        [Fact]
        public async Task Update()
        {
            var import = new DataImport
            {
                ExpectedImportedRows = 1,
                ImportedRows = 1,
                TotalRows = 1
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }


            var service = BuildDataImportService(contentDbContextId);

            await service.Update(
                import.Id,
                expectedImportedRows: 3000,
                importedRows: 5000,
                totalRows: 10000,
                geographicLevels: new HashSet<GeographicLevel>
                {
                    GeographicLevel.Country,
                    GeographicLevel.Region
                });

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updated = await contentDbContext.DataImports.SingleAsync(i => i.Id == import.Id);

                Assert.Equal(3000, updated.ExpectedImportedRows);
                Assert.Equal(5000, updated.ImportedRows);
                Assert.Equal(10000, updated.TotalRows);

                Assert.Equal(2, updated.GeographicLevels.Count);
                Assert.Contains(GeographicLevel.Country, updated.GeographicLevels);
                Assert.Contains(GeographicLevel.Region, updated.GeographicLevels);
            }
        }
        
        [Fact]
        public async Task Update_Partial()
        {
            var import = new DataImport
            {
                ExpectedImportedRows = 1,
                ImportedRows = 1,
                TotalRows = 1
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            var service = BuildDataImportService(contentDbContextId);

            await service.Update(
                import.Id,
                importedRows: 5000,
                geographicLevels: new HashSet<GeographicLevel>
                {
                    GeographicLevel.Country,
                    GeographicLevel.Region
                });

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updated = await contentDbContext.DataImports.SingleAsync(i => i.Id == import.Id);

                Assert.Equal(1, updated.ExpectedImportedRows);
                Assert.Equal(5000, updated.ImportedRows);
                Assert.Equal(1, updated.TotalRows);

                Assert.Equal(2, updated.GeographicLevels.Count);
                Assert.Contains(GeographicLevel.Country, updated.GeographicLevels);
                Assert.Contains(GeographicLevel.Region, updated.GeographicLevels);
            }
        }

        [Fact]
        public async Task WriteDataSetMetaFile_Success()
        {
            var subject = _fixture.DefaultSubject()
                .Generate();

            var file = _fixture.DefaultFile(FileType.Data)
                .WithDataSetFileMeta(null)
                .WithSubjectId(subject.Id)
                .Generate();

            var observation1 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithLocation(new Location { GeographicLevel = GeographicLevel.Country })
                .WithTimePeriod(2000, TimeIdentifier.April)
                .Generate();

            var observation2 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithLocation(new Location { GeographicLevel = GeographicLevel.LocalAuthority, })
                .WithTimePeriod(2001, TimeIdentifier.May)
                .Generate();

            var observation3 = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithLocation(new Location { GeographicLevel = GeographicLevel.Region, })
                .WithTimePeriod(2002, TimeIdentifier.June)
                .Generate();

            var filter = new Filter
            {
                SubjectId = subject.Id,
                Id = Guid.NewGuid(),
                Label = "Filter label",
                Hint = "Filter hint",
                Name = "filter_column_name",
            };

            var indicatorGroup = new IndicatorGroup
            {
                SubjectId = subject.Id,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator label",
                        Name = "indicator_column_name",
                    },
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Files.Add(file);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Observation.AddRange(observation1, observation2, observation3);
                statisticsDbContext.Filter.Add(filter);
                statisticsDbContext.IndicatorGroup.Add(indicatorGroup);
                await statisticsDbContext.SaveChangesAsync();
            }

            var service = BuildDataImportService(
                contentDbContextId,
                statisticsDbContextId);

            await service.WriteDataSetFileMeta(file.Id, subject.Id);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updatedFile = contentDbContext.Files.Single(f => f.SubjectId == subject.Id);

                Assert.NotNull(updatedFile.DataSetFileMeta);

                var meta = updatedFile.DataSetFileMeta;
                Assert.Equal(3, meta.GeographicLevels.Count);
                Assert.Contains(GeographicLevel.Country, meta.GeographicLevels);
                Assert.Contains(GeographicLevel.LocalAuthority, meta.GeographicLevels);
                Assert.Contains(GeographicLevel.Region, meta.GeographicLevels);

                Assert.Equal("2000", meta.TimePeriodRange.Start.Period);
                Assert.Equal(TimeIdentifier.April, meta.TimePeriodRange.Start.TimeIdentifier);
                Assert.Equal("2002", meta.TimePeriodRange.End.Period);
                Assert.Equal(TimeIdentifier.June, meta.TimePeriodRange.End.TimeIdentifier);

                var dbFilter = Assert.Single(meta.Filters);
                Assert.Equal(filter.Id, dbFilter.Id);
                Assert.Equal(filter.Label, dbFilter.Label);
                Assert.Equal(filter.Name, dbFilter.ColumnName);

                var dbIndicator = Assert.Single(meta.Indicators);
                Assert.Equal(indicatorGroup.Indicators[0].Id, dbIndicator.Id);
                Assert.Equal(indicatorGroup.Indicators[0].Label, dbIndicator.Label);
                Assert.Equal(indicatorGroup.Indicators[0].Name, dbIndicator.ColumnName);
            }
        }

        private static DataImportService BuildDataImportService(
            string? contentDbContextId = null,
            string? statisticsDbContextId = null)
        {
            var dbContextSupplier = new InMemoryDbContextSupplier(
                contentDbContextId ?? Guid.NewGuid().ToString(),
                statisticsDbContextId ?? Guid.NewGuid().ToString());

            return new DataImportService(
                dbContextSupplier,
                Mock.Of<ILogger<DataImportService>>(Strict));
        }
    }
}
