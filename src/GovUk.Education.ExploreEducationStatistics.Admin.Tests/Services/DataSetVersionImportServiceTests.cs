#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetVersionImportServiceTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class IsPublicApiDataSetImportsInProgress(TestApplicationFactory testApp) : DataSetVersionImportServiceTests(testApp)
    {
        /// <summary>
        /// Test that each "terminal" import status (i.e. statuses whereby the importing process has finished
        /// processing for some reason) reports that no imports are in progress. 
        /// </summary>
        [Fact]
        public async Task TerminalStates_ReportNotInProgress()
        {
            await DataSetVersionImportStatusConstants
                .TerminalStates
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(status => 
                    AssertImportOfStatusReturnsExpectedInProgressResult(status, false));
        }
        
        /// <summary>
        /// Test that each "non-terminal" import status (i.e. statuses whereby the importing process in currently
        /// ongoing) reports that imports are currently in progress. 
        /// </summary>
        [Fact]
        public async Task InProgressStates_ReportInProgress()
        {
            await DataSetVersionImportStatusConstants
                .InProgressStates
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(status => 
                    AssertImportOfStatusReturnsExpectedInProgressResult(status, true));
        }
        
        /// <summary>
        /// Test the scenario when no data set import has been initiated, and therefore no data set imports are in
        /// progress.
        /// </summary>
        [Fact]
        public async Task NoDataSetVersionImportCreated_ReportNotInProgress()
        {
            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(DataFixture
                    .DefaultReleaseVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(dataFile));

            var service = testApp.Services.GetRequiredService<IDataSetVersionImportService>();

            Assert.False(await service.IsPublicApiDataSetImportsInProgress(dataFile.ReleaseVersionId));
        }
        
        /// <summary>
        /// Test the scenario when data set imports are in progress, but for an unrelated ReleaseVersion.
        /// </summary>
        [Fact]
        public async Task DataSetVersionImportForDifferentReleaseVersion_ReportNotInProgress()
        {
            var unrelatedReleaseVersion = Guid.NewGuid();
            
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithCsvFileId(dataFile.FileId)
                .WithDataSet(DataFixture.DefaultDataSet());

            DataSetVersionImport import = DataFixture
                .DefaultDataSetVersionImport()
                .WithStatus(DataSetVersionImportStatusConstants.InProgressStates[0])
                .WithDataSetVersion(dataSetVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(dataFile);
            });

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSetVersionImports.Add(import);
            });

            var service = testApp.Services.GetRequiredService<IDataSetVersionImportService>();

            Assert.False(await service.IsPublicApiDataSetImportsInProgress(unrelatedReleaseVersion));
        }

        private async Task AssertImportOfStatusReturnsExpectedInProgressResult(
            DataSetVersionImportStatus status,
            bool expectedResult)
        {
            ReleaseVersion releaseVersion = DataFixture
                .DefaultReleaseVersion();

            ReleaseFile dataFile = DataFixture
                .DefaultReleaseFile()
                .WithFile(DataFixture
                    .DefaultFile()
                    .WithType(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 2)
                .WithCsvFileId(dataFile.FileId)
                .WithDataSet(DataFixture.DefaultDataSet());

            DataSetVersionImport import = DataFixture
                .DefaultDataSetVersionImport()
                .WithStatus(status)
                .WithDataSetVersion(dataSetVersion);

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(dataFile);
            });

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSetVersionImports.Add(import);
            });

            var service = testApp.Services.GetRequiredService<IDataSetVersionImportService>();

            Assert.Equal(expectedResult, await service.IsPublicApiDataSetImportsInProgress(dataFile.ReleaseVersionId));
        }
    }
}
