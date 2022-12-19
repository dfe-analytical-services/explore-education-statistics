#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services
{
    public class DataImportServiceTests
    {
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
                RowsPerBatch = 1,
                ImportedRows = 1,
                NumBatches = 1
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(import);
                await contentDbContext.SaveChangesAsync();
            }


            var service = BuildDataImportService(contentDbContextId: contentDbContextId);

            await service.Update(import.Id,
                rowsPerBatch: 1000,
                importedRows: 5000,
                totalRows: 10000,
                numBatches: 10,
                geographicLevels: new HashSet<GeographicLevel>
                {
                    GeographicLevel.Country,
                    GeographicLevel.Region
                });

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var updated = await contentDbContext.DataImports.SingleAsync(i => i.Id == import.Id);

                Assert.Equal(1000, updated.RowsPerBatch);
                Assert.Equal(5000, updated.ImportedRows);
                Assert.Equal(10000, updated.TotalRows);
                Assert.Equal(10, updated.NumBatches);

                Assert.Equal(2, updated.GeographicLevels.Count);
                Assert.Contains(GeographicLevel.Country, updated.GeographicLevels);
                Assert.Contains(GeographicLevel.Region, updated.GeographicLevels);
            }
        }

        private static DataImportService BuildDataImportService(
            string? contentDbContextId = null)
        {
            var dbContextSupplier = new InMemoryDbContextSupplier(contentDbContextId ?? Guid.NewGuid().ToString());
            
            return new DataImportService(
                dbContextSupplier,
                Mock.Of<ILogger<DataImportService>>(Strict),
                new InMemoryDatabaseHelper(dbContextSupplier)
            );
        }
    }
}
