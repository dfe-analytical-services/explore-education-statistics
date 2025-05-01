namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteStrategyBase(
    ILogger<AnalyticsWriteStrategyBase> logger)
{
    protected async Task WriteFileToShare(
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
