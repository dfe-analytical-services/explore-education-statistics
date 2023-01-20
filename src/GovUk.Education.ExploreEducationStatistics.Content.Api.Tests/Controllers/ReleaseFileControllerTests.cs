#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ReleaseFileControllerTests : IClassFixture<TestApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _testApp;

        private readonly Release _release = new()
        {
            Id = Guid.NewGuid(),
            Publication = new Publication()
        };

        public ReleaseFileControllerTests(TestApplicationFactory<TestStartup> testApp)
        {
            _testApp = testApp;
        }

        [Fact]
        public async Task Stream()
        {
            await using var stream = "Test file".ToStream();
            var fileId = Guid.NewGuid();

            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(s => s.StreamFile(_release.Id, fileId))
                .ReturnsAsync(
                    new FileStreamResult(stream, MediaTypeNames.Application.Pdf)
                    {
                        FileDownloadName = "test-file.pdf",
                    }
                );

            var client = SetupApp(releaseFileService: releaseFileService.Object)
                .AddContentDbTestData(context => context.Releases.Add(_release))
                .CreateClient();

            var response = await client
                .GetAsync($"/api/releases/{_release.Id}/files/{fileId}");

            MockUtils.VerifyAllMocks(releaseFileService);

            response.AssertOk("Test file");
        }

        [Fact]
        public async Task StreamFilesToZip()
        {
            var fileId1 = Guid.NewGuid();
            var fileId2 = Guid.NewGuid();

            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(
                    s => s.ZipFilesToStream(
                        _release.Id,
                        It.IsAny<Stream>(),
                        It.Is<IEnumerable<Guid>>(
                            ids => ids.SequenceEqual(ListOf(fileId1, fileId2))),
                        It.IsAny<CancellationToken?>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, Stream, IEnumerable<Guid>, CancellationToken?>(
                    (_, stream, _, _) => { stream.WriteText("Test zip"); }
                );

            var client = SetupApp(releaseFileService: releaseFileService.Object)
                .AddContentDbTestData(context => context.Releases.Add(_release))
                .CreateClient();

            var response = await client
                .GetAsync($"/api/releases/{_release.Id}/files?fileIds={fileId1},{fileId2}");

            MockUtils.VerifyAllMocks(releaseFileService);

            response.AssertOk("Test zip");
        }

        [Fact]
        public async Task StreamFilesToZip_NoFileIds()
        {
            var releaseFileService = new Mock<IReleaseFileService>(Strict);

            releaseFileService
                .Setup(
                    s => s.ZipFilesToStream(
                        _release.Id,
                        It.IsAny<Stream>(),
                        null,
                        It.IsAny<CancellationToken?>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, Stream, IEnumerable<Guid>?, CancellationToken?>(
                    (_, stream, _, _) => { stream.WriteText("Test zip"); }
                );

            var client = SetupApp(releaseFileService: releaseFileService.Object)
                .AddContentDbTestData(context => context.Releases.Add(_release))
                .CreateClient();

            var response = await client
                .GetAsync($"/api/releases/{_release.Id}/files");

            MockUtils.VerifyAllMocks(releaseFileService);

            response.AssertOk("Test zip");
        }

        private WebApplicationFactory<TestStartup> SetupApp(IReleaseFileService? releaseFileService = null)
        {
            return _testApp
                .ResetDbContexts()
                .ConfigureServices(
                    services =>
                        {
                            services.AddTransient(_ => releaseFileService ?? Mock.Of<IReleaseFileService>());
                        }
                    );
        }
    }
}
