using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.Extensions.Logging;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Utils;

public static class CopyDirectoryCallbacks
{
    public static bool TransferBlobIfFileExistsCallback(
        string sourcePath,
        List<File> files,
        IBlobContainer sourceContainerName,
        ILogger logger
    )
    {
        if (files.Exists(file => file.Path() == sourcePath))
        {
            return true;
        }

        logger.LogInformation("Not transferring {0}/{1}", sourceContainerName, sourcePath);
        return false;
    }
}
