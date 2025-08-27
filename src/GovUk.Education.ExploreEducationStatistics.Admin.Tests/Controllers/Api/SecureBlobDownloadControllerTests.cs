using System.Net.Mime;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public abstract class SecureBlobDownloadControllerTests
{
    public class StreamWithTokenTests : SecureBlobDownloadControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var originalToken = new BlobDownloadToken(
                Token: "a-token",
                ContainerName: "a-container",
                Path: "a-path",
                Filename: "a-filename.csv",
                ContentType: MediaTypeNames.Text.Csv);

            var encodedToken = JsonSerializer
                .Serialize(originalToken)
                .ToBase64String();

            var fileStreamResult = new FileStreamResult(
                fileStream: "test text".ToStream(),
                originalToken.ContentType) { FileDownloadName = originalToken.Filename };

            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);

            privateBlobStorageService
                .Setup(s => s.StreamWithToken(originalToken, default))
                .ReturnsAsync(fileStreamResult);

            var controller = new SecureBlobDownloadController(blobService: privateBlobStorageService.Object);

            var result = await controller
                .StreamWithToken(token: encodedToken, cancellationToken: default);

            result.AssertFileStreamResult(expectedResult: fileStreamResult);
        }
    }
}
