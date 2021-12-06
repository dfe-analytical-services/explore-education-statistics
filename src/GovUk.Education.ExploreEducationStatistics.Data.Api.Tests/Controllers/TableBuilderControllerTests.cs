using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderControllerTests
    {
        private readonly ObservationQueryContext _query = new()
        {
            SubjectId = Guid.NewGuid(),
        };

        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        [Fact]
        public async Task Query()
        {
            var cancellationToken = new CancellationToken();

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };
            
            var (controller, mocks) = BuildControllerAndDependencies();
            
            mocks.tableBuilderService
                .Setup(s => s.Query(_query, cancellationToken))
                .ReturnsAsync(tableBuilderResults);

            var result = await controller.Query(_query, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(tableBuilderResults);
        }
        
        [Fact]
        public async Task Query_ReleaseId()
        {
            var cancellationToken = new CancellationToken();

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };
            
            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.tableBuilderService
                .Setup(s => s.Query(_releaseId, _query, cancellationToken))
                .ReturnsAsync(tableBuilderResults);

            var result = await controller.Query(_releaseId, _query, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(tableBuilderResults);
        }
        
        [Fact]
        public async Task QueryForDataBlock()
        {
            var releaseContentBlock = new ReleaseContentBlock
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
            };

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseContentBlock))
                .ReturnsAsync(tableBuilderResults);

            SetupCall(mocks.persistenceHelper, releaseContentBlock);
            
            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks);
            
            result.AssertOkResult(tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall<ContentDbContext, ReleaseContentBlock>(mocks.persistenceHelper, null);
            
            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks);
            
            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task QueryForDataBlock_NotModified()
        {
            var releaseContentBlock = new ReleaseContentBlock
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
            };
            
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, releaseContentBlock);
                
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
                        {
                            {
                                HeaderNames.IfModifiedSince,
                                DateTime.Parse("2019-11-11T12:00:00Z").ToUniversalTime().ToString("R")
                            },
                            {
                                HeaderNames.IfNoneMatch,
                                $"W/\"{TableBuilderController.ApiVersion}\""
                            }
                        }
                    }
                }
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks);

            result.AssertNotModified();
        }

        [Fact]
        public async Task QueryForDataBlock_ETagChanged()
        {
            var releaseContentBlock = new ReleaseContentBlock
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
            };
            
            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseContentBlock))
                .ReturnsAsync(
                    tableBuilderResults
                );
            
            SetupCall(mocks.persistenceHelper, releaseContentBlock);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
                        {
                            {
                                HeaderNames.IfModifiedSince,
                                DateTime.Parse("2019-11-11T12:00:00Z").ToUniversalTime().ToString("R")
                            },
                            {
                                HeaderNames.IfNoneMatch,
                                "\"not the same etag\""
                            }
                        }
                    }
                }
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_LastModifiedChanged()
        {
            var releaseContentBlock = new ReleaseContentBlock
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
            };

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseContentBlock))
                .ReturnsAsync(tableBuilderResults);
            
            SetupCall(mocks.persistenceHelper, releaseContentBlock);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
                        {
                            {
                                HeaderNames.IfModifiedSince,
                                DateTime.Parse("2019-11-11T12:00:00Z").ToUniversalTime().ToString("R")
                            },
                            {
                                HeaderNames.IfNoneMatch,
                                $"W/\"{TableBuilderController.ApiVersion}\""
                            }
                        }
                    }
                }
            };

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(tableBuilderResults);
        }
        
        private (
            TableBuilderController controller, 
            (
                Mock<ITableBuilderService> tableBuilderService,
                Mock<IDataBlockService> dataBlockService,
                Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper) mocks) BuildControllerAndDependencies()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);
            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var persistenceHelper = MockPersistenceHelper<ContentDbContext>();

            var controller = new TableBuilderController(
                tableBuilderService.Object,
                dataBlockService.Object,
                persistenceHelper.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return (controller, (tableBuilderService, dataBlockService, persistenceHelper));
        }
    }
}
