using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class DataBlockServiceTests
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        [Fact]
        public async Task GetDataBlockTableResult()
        {
            var subjectId = Guid.NewGuid();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                    },
                    Charts = new List<IChart>
                    {
                        new LineChart()
                    }
                }
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var path = $"publications/publication-slug/releases/release-slug/data-blocks/{_dataBlockId}.json";

            var tableResult = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.GetDeserializedJson<TableBuilderResultViewModel>(PublicContent, path))
                .ThrowsAsync(new FileNotFoundException());

            blobStorageService
                .Setup(s => s.UploadAsJson(PublicContent, path, tableResult, null));

            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(q => q.SubjectId == subjectId)
                        )
                )
                .ReturnsAsync(tableResult);

            var service = BuildDataBlockService(
                contentDbContext,
                tableBuilderService: tableBuilderService.Object,
                blobStorageService: blobStorageService.Object
            );

            var result = await service.GetDataBlockTableResult(releaseContentBlock);

            Assert.True(result.IsRight);

            Assert.IsType<TableBuilderResultViewModel>(result.Right);
            Assert.Single(result.Right.Results);

            MockUtils.VerifyAllMocks(blobStorageService, tableBuilderService);
        }

        [Fact]
        public async Task GetDataBlockTableResult_MapChartForcesIncludeGeoJson()
        {
            var subjectId = Guid.NewGuid();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                        // This is set to false in the persisted query but
                        // we expect it to be converted to true in the
                        // query that is actually ran by TableBuilderService
                        IncludeGeoJson = false
                    },
                    Charts = new List<IChart>
                    {
                        new MapChart()
                    }
                }
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var path = $"publications/publication-slug/releases/release-slug/data-blocks/{_dataBlockId}.json";

            var tableResult = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.GetDeserializedJson<TableBuilderResultViewModel>(PublicContent, path))
                .ThrowsAsync(new FileNotFoundException());

            blobStorageService
                .Setup(s => s.UploadAsJson(PublicContent, path, tableResult, null));

            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(
                                q =>
                                    q.SubjectId == subjectId && q.IncludeGeoJson == true
                            )
                        )
                )
                .ReturnsAsync(tableResult);

            var service = BuildDataBlockService(
                contentDbContext,
                tableBuilderService: tableBuilderService.Object,
                blobStorageService: blobStorageService.Object
            );

            var result = await service.GetDataBlockTableResult(releaseContentBlock);

            Assert.True(result.IsRight);

            Assert.IsType<TableBuilderResultViewModel>(result.Right);
            Assert.Single(result.Right.Results);

            MockUtils.VerifyAllMocks(blobStorageService, tableBuilderService);
        }

        [Fact]
        public async Task GetDataBlockTableResult_CachedResultExists()
        {
            var subjectId = Guid.NewGuid();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                    },
                    Charts = new List<IChart>
                    {
                        new LineChart()
                    }
                }
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var path = $"publications/publication-slug/releases/release-slug/data-blocks/{_dataBlockId}.json";

            var tableResult = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.GetDeserializedJson<TableBuilderResultViewModel>(PublicContent, path))
                .ReturnsAsync(tableResult);

            var tableBuilderService = new Mock<ITableBuilderService>();

            var service = BuildDataBlockService(
                contentDbContext,
                tableBuilderService: tableBuilderService.Object,
                blobStorageService: blobStorageService.Object
            );

            var result = await service.GetDataBlockTableResult(releaseContentBlock);

            Assert.True(result.IsRight);

            Assert.IsType<TableBuilderResultViewModel>(result.Right);
            Assert.Single(result.Right.Results);

            MockUtils.VerifyAllMocks(blobStorageService, tableBuilderService);
        }

        [Fact]
        public async Task GetDataBlockTableResult_DeletesInvalidCachedResult()
        {
            var subjectId = Guid.NewGuid();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                    },
                    Charts = new List<IChart>
                    {
                        new LineChart()
                    }
                }
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var path = $"publications/publication-slug/releases/release-slug/data-blocks/{_dataBlockId}.json";

            var tableResult = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s =>
                    s.GetDeserializedJson<TableBuilderResultViewModel>(PublicContent, path))
                .ThrowsAsync(new JsonException("Could not deserialize the JSON"));

            blobStorageService
                .Setup(s => s.DeleteBlob(PublicContent, path));

            blobStorageService
                .Setup(s => s.UploadAsJson(PublicContent, path, tableResult, null));

            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(q => q.SubjectId == subjectId)
                        )
                )
                .ReturnsAsync(tableResult);


            var service = BuildDataBlockService(
                contentDbContext,
                tableBuilderService: tableBuilderService.Object,
                blobStorageService: blobStorageService.Object
            );

            var result = await service.GetDataBlockTableResult(releaseContentBlock);

            Assert.True(result.IsRight);

            Assert.IsType<TableBuilderResultViewModel>(result.Right);
            Assert.Single(result.Right.Results);

            MockUtils.VerifyAllMocks(blobStorageService, tableBuilderService);
        }

        [Fact]
        public async Task QueryForDataBlock_NotDataBlockType()
        {
            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new HtmlBlock(),
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var service = BuildDataBlockService(contentDbContext);

            var result = await service.GetDataBlockTableResult(releaseContentBlock);

            Assert.True(result.IsLeft);
            Assert.IsType<NotFoundResult>(result.Left);
        }


        private DataBlockService BuildDataBlockService(
            ContentDbContext contentDbContext,
            IBlobStorageService blobStorageService = null,
            ITableBuilderService tableBuilderService = null,
            IUserService userService = null)
        {
            return new DataBlockService(
                contentDbContext,
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                tableBuilderService ?? new Mock<ITableBuilderService>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                new Mock<ILogger<DataBlockService>>().Object
            );
        }
    }
}
