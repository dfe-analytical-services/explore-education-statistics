using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public class CompleteProcessingFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class CompleteProcessingTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : CompleteProcessingFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var instanceId = Guid.NewGuid();

            DataSet dataSet = DataFixture
                .DefaultDataSet();

            await AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion()
                .WithDataSet(dataSet)
                .WithStatus(DataSetVersionStatus.Processing)
                .WithImports(() => DataFixture
                    .DefaultDataSetVersionImport()
                    .WithInstanceId(instanceId)
                    .WithStage(DataSetVersionImportStage.Pending)
                    .Generate(1))
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var function = GetRequiredService<CompleteProcessingFunction>();
            await function.CompleteProcessing(new ProcessInitialDataSetVersionContext
                {
                    DataSetVersionId = dataSetVersion.Id
                },
                instanceId,
                CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedDataSetVersion = await publicDataDbContext.DataSetVersions
                .Include(dsv => dsv.Imports)
                .FirstAsync(dsv => dsv.Id == dataSetVersion.Id);

            Assert.Equal(DataSetVersionStatus.Draft, savedDataSetVersion.Status);

            var savedImport = savedDataSetVersion.Imports.Single();
            Assert.Equal(DataSetVersionImportStage.Completing, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();
        }
    }
}
