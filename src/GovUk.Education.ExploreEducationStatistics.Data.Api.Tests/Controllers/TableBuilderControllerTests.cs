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
        private static readonly ObservationQueryContext ObservationQueryContext = new()
        {
            SubjectId = Guid.NewGuid(),
        };

        private static readonly Guid ReleaseId = Guid.NewGuid();
        
        private static readonly Guid DataBlockId = Guid.NewGuid();            
        
        private readonly ReleaseContentBlock _releaseContentBlock = new()
        {
            ReleaseId = ReleaseId,
            Release = new Release
            {
                Id = ReleaseId,
            },
            ContentBlockId = DataBlockId,
            ContentBlock = new DataBlock
            {
                Id = DataBlockId,
                Query = ObservationQueryContext,
                Charts = new List<IChart>()
            }
        };
        
        private readonly TableBuilderResultViewModel _tableBuilderResults = new()
        {
            Results = new List<ObservationViewModel>
            {
                new()
            }
        };

        [Fact]
        public async Task Query()
        {
            var cancellationToken = new CancellationToken();

            var (controller, mocks) = BuildControllerAndDependencies();
            
            mocks.tableBuilderService
                .Setup(s => s.Query(ObservationQueryContext, cancellationToken))
                .ReturnsAsync(_tableBuilderResults);

            var result = await controller.Query(ObservationQueryContext, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }
        
        [Fact]
        public async Task Query_ReleaseId()
        {
            var cancellationToken = new CancellationToken();

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.tableBuilderService
                .Setup(s => s.Query(ReleaseId, ObservationQueryContext, cancellationToken))
                .ReturnsAsync(_tableBuilderResults);

            var result = await controller.Query(ReleaseId, ObservationQueryContext, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }
        
        [Fact]
        public async Task QueryForDataBlock()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(_releaseContentBlock))
                .ReturnsAsync(_tableBuilderResults);

            SetupCall(mocks.persistenceHelper, _releaseContentBlock);
            
            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);
            
            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall<ContentDbContext, ReleaseContentBlock>(mocks.persistenceHelper, null);
            
            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);
            
            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task QueryForDataBlock_NotModified()
        {
            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = ReleaseId,
                Release = new Release
                {
                    Id = ReleaseId,
                    Published = DateTime.Parse("2019-11-11T12:00:00Z")
                },
                ContentBlockId = DataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = DataBlockId,
                    Query = ObservationQueryContext
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

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertNotModified();
        }

        [Fact]
        public async Task QueryForDataBlock_ETagChanged()
        {
            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = ReleaseId,
                Release = new Release
                {
                    Id = ReleaseId,
                    Published = DateTime.Parse("2020-11-11T12:00:00Z")
                },
                ContentBlockId = DataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = DataBlockId,
                    Query = ObservationQueryContext,
                    Charts = new List<IChart>()
                }
            };
            
            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseContentBlock))
                .ReturnsAsync(_tableBuilderResults);
            
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

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_LastModifiedChanged()
        {
            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = ReleaseId,
                Release = new Release
                {
                    Id = ReleaseId,
                    Published = DateTime.Parse("2020-11-11T12:00:00Z")
                },
                ContentBlockId = DataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = DataBlockId,
                    Query = ObservationQueryContext,
                    Charts = new List<IChart>()
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseContentBlock))
                .ReturnsAsync(_tableBuilderResults);
            
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

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
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