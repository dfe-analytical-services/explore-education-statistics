using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class HandleProcessingFailureFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public HandleProcessingFailureFunction Function = null!;

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<HandleProcessingFailureFunction>();
    }
}

[CollectionDefinition(nameof(HandleProcessingFailureFunctionTestsFixture))]
public class HandleProcessingFailureFunctionTestsCollection
    : ICollectionFixture<HandleProcessingFailureFunctionTestsFixture>;

[Collection(nameof(HandleProcessingFailureFunctionTestsFixture))]
public abstract class HandleProcessingFailureFunctionTests(HandleProcessingFailureFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    public class HandleProcessingFailureTests(HandleProcessingFailureFunctionTestsFixture fixture)
        : HandleProcessingFailureFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // The stage which the failure occured in - This should not be altered by the handler
            const DataSetVersionImportStage failedStage = DataSetVersionImportStage.CopyingCsvFiles;

            var (_, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                failedStage
            );

            await fixture.Function.HandleProcessingFailure(instanceId, CancellationToken.None);

            var savedImport = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(failedStage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Failed, savedImport.DataSetVersion.Status);
        }
    }
}
