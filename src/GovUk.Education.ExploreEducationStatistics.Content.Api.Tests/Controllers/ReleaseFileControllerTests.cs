#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class ReleaseFileControllerTests : IClassFixture<TestApplicationFactory<TestStartup>>, IDisposable
    {
        private readonly WebApplicationFactory<TestStartup> _testApp;
        private readonly Mock<IReleaseFileService> _releaseFileService = new(MockBehavior.Strict);

        private readonly Release _release = new()
        {
            Publication = new Publication()
        };

        public ReleaseFileControllerTests(TestApplicationFactory<TestStartup> testApp)
        {
            _testApp = testApp.WithWebHostBuilder(
                builder =>
                {
                    builder.AddTestData<ContentDbContext>(
                        context =>
                        {
                            context.Releases.Add(_release);
                            context.SaveChanges();
                        }
                    );

                    builder.ConfigureServices(
                        services => { services.AddTransient(_ => _releaseFileService.Object); }
                    );
                }
            );
        }

        public void Dispose()
        {
            _releaseFileService.Reset();
        }

        [Fact]
        public async Task Stream()
        {
            var client = _testApp.CreateClient();

            await using var stream = "Test file".ToStream();
            var fileId = Guid.NewGuid();

            _releaseFileService
                .Setup(s => s.StreamFile(_release.Id, fileId))
                .ReturnsAsync(
                    new FileStreamResult(stream, MediaTypeNames.Application.Pdf)
                    {
                        FileDownloadName = "test-file.pdf",
                    }
                );

            var response = await client
                .GetAsync($"/api/releases/{_release.Id}/files/{fileId}");

            MockUtils.VerifyAllMocks(_releaseFileService);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Test file", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task StreamFilesToZip()
        {
            var client = _testApp.CreateClient();

            var fileId1 = Guid.NewGuid();
            var fileId2 = Guid.NewGuid();

            _releaseFileService
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

            var response = await client
                .GetAsync($"/api/releases/{_release.Id}/files?fileIds={fileId1},{fileId2}");

            MockUtils.VerifyAllMocks(_releaseFileService);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Test zip", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task StreamFilesToZip_NoFileIds()
        {
            var client = _testApp.CreateClient();

            _releaseFileService
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

            var response = await client
                .GetAsync($"/api/releases/{_release.Id}/files");

            MockUtils.VerifyAllMocks(_releaseFileService);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Test zip", await response.Content.ReadAsStringAsync());
        }
    }
}
