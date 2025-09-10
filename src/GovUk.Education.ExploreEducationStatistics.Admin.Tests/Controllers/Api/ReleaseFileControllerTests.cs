#nullable enable
using System;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api;

public abstract class ReleaseFileControllerTests
{
    public class GetDownloadTokenTests : ReleaseFileControllerTests
    {
        [Fact]
        public async Task Success()
        {
            var releaseVersionId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var downloadToken = new BlobDownloadToken(
                Token: "a-token",
                ContainerName: "a-container",
                Path: "a-path",
                Filename: "a-filename.csv",
                ContentType: MediaTypeNames.Text.Csv);
            
            var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);

            releaseFileService
                .Setup(s => s.GetBlobDownloadToken(
                    releaseVersionId,
                    fileId,
                    default))
                .ReturnsAsync(downloadToken);
            
            var controller = BuildController(releaseFileService: releaseFileService.Object);

            var result = await controller.GetDownloadToken(
                releaseVersionId: releaseVersionId,
                fileId: fileId,
                cancellationToken: default);

            var encodedToken = result.AssertOkResult();
            
            var expectedEncodedToken = JsonSerializer.Serialize(downloadToken).ToBase64String();
            Assert.Equal(expectedEncodedToken, encodedToken);
        }
    }

    private ReleaseFileController BuildController(
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IDataBlockService? dataBlockService = null,
        IReleaseFileService? releaseFileService = null)
    {
        return new ReleaseFileController(
            persistenceHelper: persistenceHelper ?? MockUtils.MockPersistenceHelper<ContentDbContext>().Object,
            dataBlockService ?? Mock.Of<IDataBlockService>(MockBehavior.Strict),
            releaseFileService: releaseFileService ?? Mock.Of<IReleaseFileService>(MockBehavior.Strict)
        );
    }
}
