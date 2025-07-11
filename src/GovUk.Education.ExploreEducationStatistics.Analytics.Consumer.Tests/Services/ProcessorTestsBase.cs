using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services;

public abstract class ProcessorTestsBase : IDisposable
{
    protected readonly TestAnalyticsPathResolver PathResolver = new();
    protected readonly IProcessRequestFilesWorkflow Workflow;
    private readonly IFileAccessor _filesystemFileAccessor = new FilesystemFileAccessor();
    private readonly DateTimeProvider _dateTimeProvider = new();

    protected ProcessorTestsBase()
    {
        Workflow = new ProcessRequestFilesWorkflow(
            logger: Mock.Of<ILogger<ProcessRequestFilesWorkflow>>(),
            fileAccessor: _filesystemFileAccessor,
            dateTimeProvider: _dateTimeProvider,
            temporaryProcessingFolderNameGenerator: () => "temp-processing-folder");
    }

    public virtual void Dispose()
    {
        PathResolver.Dispose();
    }
}
