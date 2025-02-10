using System.Security.Cryptography;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Analytics.Model;
using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;

public class ConsumePublicApiQueryFunction(
    IAnalyticsPathResolver pathResolver,
    ILogger<ConsumePublicApiQueryFunction> logger)
{
    [Function(nameof(ConsumePublicApiQueryFunction))]
    public Task Run(
        [TimerTrigger("%App:ConsumePublicApiQueriesCronSchedule%")]
        TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumePublicApiQueryFunction)} triggered");

        var folder = pathResolver.GetPublicApiQueriesFolderPath();

        var queryFiles = Directory.GetFiles(folder).ToList();

        queryFiles.ForEach(filename =>
        {
            logger.LogInformation("Found file {Filename} - deleting", filename);

            File.Delete($"{folder}{Path.PathSeparator}{filename}");

            logger.LogInformation("File {Filename} deleted", filename);
        });

        // var hash = GenerateHash(request);
        //
        // logger.LogInformation("""
        //                       Processed Public API data set query
        //                       Name: {Filename}
        //                       Data: {RequestJson}"
        //                       Hash: {Hash}
        //                       """,
        //     filename,
        //     JsonSerializer.Serialize(request),
        //     hash);

        return Task.CompletedTask;
    }

    public static string GenerateHash(CaptureDataSetVersionQueryRequest request)
    {
        return CreateMd5(request.query + request.dataSetVersionId);
    }

    public static string CreateMd5(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}
