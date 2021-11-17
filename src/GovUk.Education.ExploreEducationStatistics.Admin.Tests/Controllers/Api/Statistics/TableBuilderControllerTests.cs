using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class TableBuilderControllerTests
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        private readonly ObservationQueryContext _query = new()
        {
            SubjectId = Guid.NewGuid()
        };

        [Fact]
        public async Task Query()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService
                .Setup(s => s.Query(_releaseId, _query, default))
                .ReturnsAsync(new TableBuilderResultViewModel
                    {
                        Results = new List<ObservationViewModel>
                        {
                            new()
                        }
                    });

            var controller = BuildTableBuilderController(tableBuilderService: tableBuilderService.Object);
            var result = await controller.Query(_releaseId, _query);

            Assert.IsAssignableFrom<TableBuilderResultViewModel>(result.Value);
            Assert.Single(result.Value.Results);
        }

        [Fact]
        public async Task Query_CancellationToken()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();

            var cancellationToken = new CancellationToken();

            // Assert that the passed in CancellationToken is passed down to the child calls.
            tableBuilderService
                .Setup(s => s.Query(_releaseId, _query, cancellationToken))
                .ReturnsAsync(new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>
                    {
                        new()
                    }
                });

            var controller = BuildTableBuilderController(tableBuilderService: tableBuilderService.Object);
            var result = await controller.Query(_releaseId, _query, cancellationToken);
            result.AssertOkResult();
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
                            ),
                            default
                        )
                )
                .ReturnsAsync(
                    new TableBuilderResultViewModel
                    {
                        Results = new List<ObservationViewModel>
                        {
                            new()
                        }
                    }
                );

            var contentPersistenceHelper =
                MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
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

            VerifyAllMocks(tableBuilderService, contentPersistenceHelper);
        }

        [Fact]
        public async Task QueryForDataBlock_CancellationToken()
        {
            var cancellationToken = new CancellationToken();
            
            var tableBuilderService = new Mock<ITableBuilderService>();

            // Assert that the passed in CancellationToken is passed down to the child calls.
            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(
                                q => q.SubjectId == _query.SubjectId
                            ),
                            cancellationToken
                        )
                )
                .ReturnsAsync(
                    new TableBuilderResultViewModel
                    {
                        Results = new List<ObservationViewModel>
                        {
                            new()
                        }
                    }
                );

            var contentPersistenceHelper =
                MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
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

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId, cancellationToken);
            result.AssertOkResult();
            VerifyAllMocks(tableBuilderService, contentPersistenceHelper);
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
                            ),
                            default
                        )
                )
                .ReturnsAsync(
                    new TableBuilderResultViewModel
                    {
                        Results = new List<ObservationViewModel>
                        {
                            new()
                        }
                    }
                );

            var contentPersistenceHelper =
                MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
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

            VerifyAllMocks(tableBuilderService, contentPersistenceHelper);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var contentPersistenceHelper =
                MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(null);

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
                MockPersistenceHelper<ContentDbContext, ReleaseContentBlock>(
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

        private TableBuilderController BuildTableBuilderController(
            ITableBuilderService tableBuilderService = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IUserService userService = null,
            ILogger<TableBuilderController> logger = null)
        {
            return new TableBuilderController(
                tableBuilderService ?? new Mock<ITableBuilderService>().Object,
                contentPersistenceHelper ?? MockPersistenceHelper<ContentDbContext>().Object,
                userService ?? AlwaysTrueUserService().Object,
                logger ?? new Mock<ILogger<TableBuilderController>>().Object
            );
        }
    }
}
