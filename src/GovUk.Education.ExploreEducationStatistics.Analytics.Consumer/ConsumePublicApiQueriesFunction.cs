using System.Security.Cryptography;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Analytics.Model;
using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;

public class ConsumePublicApiQueriesFunction(
    IPublicApiAnalyticsService analyticsService,
    IAnalyticsPathResolver pathResolver,
    ILogger<ConsumePublicApiQueriesFunction> logger)
{
    [Function(nameof(ConsumePublicApiQueriesFunction))]
    public async Task Run(
        [TimerTrigger("%App:ConsumePublicApiQueriesCronSchedule%")]
        TimerInfo timer)
    {
        logger.LogInformation($"{nameof(ConsumePublicApiQueriesFunction)} triggered");

        var directory = pathResolver.PublicApiQueriesDirectoryPath();

        var queryFiles = Directory.GetFiles(directory).ToList();

        await queryFiles
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(async filename =>
        {
            var filepath = Path.Combine(directory, filename);
            
            logger.LogInformation("Found file {FilePath} - recording query", filename);

            var queryRequest = JsonFileUtils.ReadJsonFile<CaptureDataSetVersionQueryRequest>(filepath);

            if (queryRequest == null)
            {
                File.Delete(filepath);
                logger.LogError("File {FilePath} was unable to be read", filepath);
            }
            else
            {
                await analyticsService.CaptureQuery(queryRequest);
            
                // File.Delete(filepath);
                logger.LogInformation("File {FilePath} processed successfully", filepath);    
            }
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
    }

    public static string GenerateHash(CaptureDataSetVersionQueryRequest request)
    {
        return CreateMd5(request.Query + request.DataSetVersionId);
    }

    public static string CreateMd5(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}
