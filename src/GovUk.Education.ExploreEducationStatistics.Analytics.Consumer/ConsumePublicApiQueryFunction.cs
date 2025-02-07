using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Analytics.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;

public class ConsumePublicApiQueryFunction(ILogger<ConsumePublicApiQueryFunction> logger)
{
    [Function(nameof(ConsumePublicApiQueryFunction))]
    public Task Run(
        [BlobTrigger("analytics/public-api/queries/{filename}")]
        CaptureDataSetVersionQueryRequest request,
        string filename)
    {
        var hash = GenerateHash(request);

        logger.LogInformation("""
                              Processed Public API data set query
                              Name: {Filename}
                              Data: {RequestJson}"
                              Hash: {Hash}
                              """,
            filename,
            JsonSerializer.Serialize(request),
            hash);

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
