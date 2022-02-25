using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Azure.Cosmos.Table;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class FastTrackServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var publicationId = Guid.NewGuid();
            
            var releaseId = Guid.NewGuid();
            const string releaseSlug = "release-slug";
            var release = new Release
            {
                Id = releaseId,
                PublicationId = publicationId,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.April,
                Slug = releaseSlug
            };

            var subjectId = Guid.NewGuid();
            const string subjectName = "subject name";

            var fastTrackId = Guid.NewGuid();
            var fastTrackCreated = DateTime.Now.AddDays(-10);
            
            var fastTrackAsBlob = new FastTrack(fastTrackId, new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    Rows = new List<TableHeader>
                    {
                        new("table header 1", TableHeaderType.Filter)
                    }
                }
            }, new ObservationQueryContext
            {
                SubjectId = subjectId
            }, releaseId)
            {
                Created = fastTrackCreated
            };
            var tableBuilderResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new ResultSubjectMetaViewModel
                {
                    SubjectName = subjectName
                }
            };

            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var tableStorageService = new Mock<ITableStorageService>(Strict);
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var tableBuilderService = new Mock<ITableBuilderService>();
            
            var service = BuildService(
                tableStorageService.Object,
                blobStorageService.Object,
                tableBuilderService.Object,
                releaseRepository.Object
                );

            tableStorageService
                .Setup(s => s.ExecuteQueryAsync(
                    PublicReleaseFastTrackTableName, It.IsAny<TableQuery<ReleaseFastTrack>>()))
                .ReturnsAsync(new List<ReleaseFastTrack>
                {
                    new()
                    {
                        PartitionKey = releaseId.ToString()
                    }
                });

            tableBuilderService
                .Setup(s => s
                    .Query(
                        releaseId, 
                        It.Is<ObservationQueryContext>(query => query.SubjectId == fastTrackAsBlob.Query.SubjectId),
                        default))
                .ReturnsAsync(tableBuilderResult);

            blobStorageService
                .Setup(s => s.DownloadBlobText(PublicContent, It.IsAny<string>()))
                .ReturnsAsync(JsonConvert.SerializeObject(fastTrackAsBlob));
            
            releaseRepository
                .Setup(s => s.FindOrNotFoundAsync(releaseId))
                .ReturnsAsync(release);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(publicationId))
                .Returns(release);
            
            var result = await service.GetFastTrackAndResults(fastTrackId);
            Assert.True(result.IsRight);

            var viewModel = result.Right;
            Assert.Equal(fastTrackId, viewModel.Id);
            Assert.Equal(releaseId, viewModel.ReleaseId);
            Assert.Equal(releaseSlug, viewModel.ReleaseSlug);
            Assert.Equal(fastTrackCreated, viewModel.Created);
            var tableHeader = Assert.Single(viewModel.Configuration.TableHeaders.Rows);
            Assert.NotNull(tableHeader);
            Assert.Equal("table header 1", tableHeader.Value);
            Assert.Equal(subjectName, viewModel.FullTable.SubjectMeta.SubjectName);
            Assert.Equal(subjectId, viewModel.Query.SubjectId);
            Assert.True(viewModel.LatestData);
            Assert.Equal("April 2021", viewModel.LatestReleaseTitle);
        }
        
        [Fact]
        public async Task Get_NotLatestRelease()
        {
            var publicationId = Guid.NewGuid();
            
            var releaseId = Guid.NewGuid();
            const string releaseSlug = "release-slug";
            var release = new Release
            {
                Id = releaseId,
                PublicationId = publicationId,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.January,
                Slug = releaseSlug
            };
            
            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publicationId,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.April,
                Slug = "latest-release-slug"
            };

            var fastTrackId = Guid.NewGuid();
            var fastTrackAsBlob = new FastTrack(fastTrackId, new TableBuilderConfiguration(), new ObservationQueryContext(), releaseId);
            var tableBuilderResult = new TableBuilderResultViewModel();

            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var tableStorageService = new Mock<ITableStorageService>(Strict);
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var tableBuilderService = new Mock<ITableBuilderService>();
            
            var service = BuildService(
                tableStorageService.Object,
                blobStorageService.Object,
                tableBuilderService.Object,
                releaseRepository.Object
                );

            tableStorageService
                .Setup(s => s.ExecuteQueryAsync(
                    PublicReleaseFastTrackTableName, It.IsAny<TableQuery<ReleaseFastTrack>>()))
                .ReturnsAsync(new List<ReleaseFastTrack>
                {
                    new ReleaseFastTrack
                    {
                        PartitionKey = releaseId.ToString()
                    }
                });

            tableBuilderService
                .Setup(s => s
                    .Query(
                        releaseId, 
                        It.Is<ObservationQueryContext>(query => query.SubjectId == fastTrackAsBlob.Query.SubjectId),
                        default))
                .ReturnsAsync(tableBuilderResult);

            blobStorageService
                .Setup(s => s.DownloadBlobText(PublicContent, It.IsAny<string>()))
                .ReturnsAsync(JsonConvert.SerializeObject(fastTrackAsBlob));
            
            releaseRepository
                .Setup(s => s.FindOrNotFoundAsync(releaseId))
                .ReturnsAsync(release);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(publicationId))
                .Returns(latestRelease);
            
            var result = await service.GetFastTrackAndResults(fastTrackId);
            Assert.True(result.IsRight);

            var viewModel = result.Right;
            Assert.Equal(releaseSlug, viewModel.ReleaseSlug);
            Assert.False(viewModel.LatestData);
            Assert.Equal("April 2021", viewModel.LatestReleaseTitle);
        }

        private static FastTrackService BuildService(
            ITableStorageService tableStorageService = null,
            IBlobStorageService blobStorageService = null,
            ITableBuilderService tableBuilderService = null,
            IReleaseRepository releaseRepository = null)
        {
            return new FastTrackService(
                tableBuilderService ?? new Mock<ITableBuilderService>().Object,
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                tableStorageService ?? new Mock<ITableStorageService>().Object,
                releaseRepository ?? new Mock<IReleaseRepository>().Object,
                MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
