using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderControllerTests
    {
        private readonly ObservationQueryContext _query = new ObservationQueryContext
        {
            SubjectId = Guid.NewGuid(),
        };

        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        [Fact]
        public async Task Query()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService.Setup(s => s.Query(_releaseId, _query)).ReturnsAsync(
                new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>
                    {
                        new ObservationViewModel()
                    }
                }
            );

            var controller = BuildTableBuilderController(tableBuilderService: tableBuilderService.Object);
            var result = await controller.Query(_releaseId, _query);

            Assert.IsType<TableBuilderResultViewModel>(result.Value);
            Assert.Single(result.Value.Results);
        }

        [Fact]
        public async Task QueryForDataBlock()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(
                                q => q.SubjectId == _query.SubjectId
                            )
                        )
                )
                .ReturnsAsync(
                    new TableBuilderResultViewModel
                    {
                        Results = new List<ObservationViewModel>
                        {
                            new ObservationViewModel()
                        }
                    }
                );

            var contentPersistenceHelper =
                MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
                    new ReleaseContentBlock
                    {
                        ReleaseId = _releaseId,
                        Release = new Release
                        {
                            Id = _releaseId,
                        },
                        ContentBlockId = _dataBlockId,
                        ContentBlock = new DataBlock
                        {
                            Id = _dataBlockId,
                            Query = _query,
                            Charts = new List<IChart>()
                        }
                    }
                );

            var controller = BuildTableBuilderController(
                tableBuilderService: tableBuilderService.Object,
                contentPersistenceHelper: contentPersistenceHelper.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);

            Assert.IsType<TableBuilderResultViewModel>(result.Value);
            Assert.Single(result.Value.Results);

            MockUtils.VerifyAllMocks(tableBuilderService, contentPersistenceHelper);
        }

        [Fact]
        public async Task QueryForDataBlock_NoGeoJson()
        {
            var subjectId = Guid.NewGuid();

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
                .ReturnsAsync(
                    new TableBuilderResultViewModel
                    {
                        Results = new List<ObservationViewModel>
                        {
                            new ObservationViewModel()
                        }
                    }
                );

            var contentPersistenceHelper =
                MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
                    new ReleaseContentBlock
                    {
                        ReleaseId = _releaseId,
                        Release = new Release
                        {
                            Id = _releaseId,
                        },
                        ContentBlockId = _dataBlockId,
                        ContentBlock = new DataBlock
                        {
                            Id = _dataBlockId,
                            Query = new ObservationQueryContext
                            {
                                SubjectId = subjectId,
                                IncludeGeoJson = false
                            },
                            Charts = new List<IChart>
                            {
                                new MapChart()
                            }
                        }
                    }
                );

            var controller = BuildTableBuilderController(
                tableBuilderService: tableBuilderService.Object,
                contentPersistenceHelper: contentPersistenceHelper.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);

            Assert.IsType<TableBuilderResultViewModel>(result.Value);
            Assert.Single(result.Value.Results);

            MockUtils.VerifyAllMocks(tableBuilderService, contentPersistenceHelper);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var contentPersistenceHelper =
                MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(null);

            var controller = BuildTableBuilderController(
                contentPersistenceHelper: contentPersistenceHelper.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task QueryForDataBlock_NotDataBlockType()
        {
            var contentPersistenceHelper =
                MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
                    new ReleaseContentBlock
                    {
                        ReleaseId = _releaseId,
                        Release = new Release
                        {
                            Id = _releaseId,
                        },
                        ContentBlockId = _dataBlockId,
                        ContentBlock = new HtmlBlock
                        {
                            Id = _dataBlockId,
                        }
                    }
                );

            var controller = BuildTableBuilderController(
                contentPersistenceHelper: contentPersistenceHelper.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);

            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public async Task QueryForDataBlock_NotModified()
        {
            var contentPersistenceHelper =
                MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
                    new ReleaseContentBlock
                    {
                        ReleaseId = _releaseId,
                        Release = new Release
                        {
                            Id = _releaseId,
                            Published = DateTime.Parse("2019-11-11T12:00:00Z")
                        },
                        ContentBlockId = _dataBlockId,
                        ContentBlock = new DataBlock
                        {
                            Id = _dataBlockId,
                            Query = _query
                        }
                    }
                );

            var controller = BuildTableBuilderController(
                contentPersistenceHelper: contentPersistenceHelper.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Request =
                    {
                        Headers =
                        {
                            {
                                HeaderNames.IfModifiedSince,
                                DateTime.Parse("2019-11-11T12:00:00Z").ToUniversalTime().ToString("R")
                            }
                        }
                    }
                }
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result.Result);
            Assert.Equal(StatusCodes.Status304NotModified, statusCodeResult.StatusCode);

            MockUtils.VerifyAllMocks(contentPersistenceHelper);
        }

        [Fact]
        public async Task QueryForDataBlock_IsModified()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(
                                q => q.SubjectId == _query.SubjectId
                            )
                        )
                )
                .ReturnsAsync(
                    new TableBuilderResultViewModel
                    {
                        Results = new List<ObservationViewModel>
                        {
                            new ObservationViewModel()
                        }
                    }
                );

            var contentPersistenceHelper =
                MockUtils.MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
                    new ReleaseContentBlock
                    {
                        ReleaseId = _releaseId,
                        Release = new Release
                        {
                            Id = _releaseId,
                            Published = DateTime.Parse("2020-11-11T12:00:00Z")
                        },
                        ContentBlockId = _dataBlockId,
                        ContentBlock = new DataBlock
                        {
                            Id = _dataBlockId,
                            Query = _query,
                            Charts = new List<IChart>()
                        }
                    }
                );

            var controller = BuildTableBuilderController(
                tableBuilderService: tableBuilderService.Object,
                contentPersistenceHelper: contentPersistenceHelper.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Request =
                    {
                        Headers =
                        {
                            {
                                HeaderNames.IfModifiedSince,
                                DateTime.Parse("2019-11-11T12:00:00Z").ToUniversalTime().ToString("R")
                            }
                        }
                    }
                }
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);

            Assert.IsType<TableBuilderResultViewModel>(result.Value);
            Assert.Single(result.Value.Results);

            MockUtils.VerifyAllMocks(tableBuilderService, contentPersistenceHelper);
        }

        private TableBuilderController BuildTableBuilderController(
            ITableBuilderService tableBuilderService = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IUserService userService = null)

        {
            return new TableBuilderController(
                tableBuilderService ?? new Mock<ITableBuilderService>().Object,
                contentPersistenceHelper ?? MockUtils.MockPersistenceHelper<ContentDbContext>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}