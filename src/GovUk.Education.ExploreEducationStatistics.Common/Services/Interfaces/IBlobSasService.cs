using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IBlobSasService
{
    /// <summary>
    /// Generate a secure, short-lived SAS token with which a secure download
    /// can be requested.  These tokens should only be obtained after any relevant
    /// permission checks have been carried out.
    /// </summary>
    Task<Either<ActionResult, BlobDownloadToken>> CreateBlobDownloadToken(
        BlobServiceClient blobServiceClient,
        IBlobContainer container,
        string filename,
        string path,
        TimeSpan expiryDuration,
        CancellationToken cancellationToken);

    /// <summary>
    /// Trade in a secure, short-lived SAS token in order to generate a secure
    /// BlobClient that can validate the SAS token and access the requested
    /// resource.
    /// </summary>
    Task<Either<ActionResult, BlobClient>> CreateSecureBlobClient(
        BlobServiceClient blobServiceClient,
        BlobDownloadToken token);
}
