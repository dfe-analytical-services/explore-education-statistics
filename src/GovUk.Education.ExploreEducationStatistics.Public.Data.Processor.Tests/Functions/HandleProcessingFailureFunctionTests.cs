using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class HandleProcessingFailureFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class HandleProcessingFailureTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : HandleProcessingFailureFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // The stage which the failure occured in - This should not be altered by the handler
            const DataSetVersionImportStage failedStage = DataSetVersionImportStage.CopyingCsvFiles;

            var (_, instanceId) = await CreateDataSetInitialVersion(failedStage);

            var function = GetRequiredService<HandleProcessingFailureFunction>();
            await function.HandleProcessingFailure(instanceId, CancellationToken.None);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(failedStage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Failed, savedImport.DataSetVersion.Status);
        }
    }
}
