using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public record BlobDownloadToken(
    string Token,
    string ContainerName,
    string Path,
    string FileName,
    string ContentType)
{
    public string ToBase64JsonString()
    {
        return JsonSerializer.Serialize(this).ToBase64String();
    }

    public static BlobDownloadToken FromBase64JsonString(string token)
    {
        return JsonSerializer.Deserialize<BlobDownloadToken>(token.FromBase64String());
    }
}
