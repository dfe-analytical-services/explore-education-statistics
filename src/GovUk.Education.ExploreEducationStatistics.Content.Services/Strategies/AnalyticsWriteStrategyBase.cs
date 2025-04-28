#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

public abstract class AnalyticsWriteStrategyBase(
    ILogger<AnalyticsWriteStrategyBase> logger
    )
{
    protected async Task WriteToFileShare(
        string requestTypeName,
        string directory,
        string filename,
        string serialisedRequest)
    {
        try
        {
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, filename);

            await File.WriteAllTextAsync(
                filePath,
                contents: serialisedRequest);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error whilst writing {RequestTypeName} to disk",
                requestTypeName);
        }
    }
}
