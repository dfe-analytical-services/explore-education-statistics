using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.SampleContentJson;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class MetaGuidanceServiceTests
    {
        private static readonly List<MetaGuidanceSubjectViewModel> SubjectMetaGuidance =
            new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Id = Guid.NewGuid(),
                    Content = "Subject Meta Guidance",
                    Filename = "data.csv",
                    Name = "Subject",
                    GeographicLevels = new List<string>
                    {
                        "National", "Local Authority", "Local Authority District"
                    },
                    TimePeriods = new MetaGuidanceSubjectTimePeriodsViewModel("2020_AYQ3", "2021_AYQ1"),
                    Variables = new List<LabelValue>
                    {
                        new LabelValue("Filter label", "test_filter"),
                        new LabelValue("Indicator label", "test_indicator")
                    }
                }
            };

        [Fact]
        public async Task Get()
        {
            var releaseId = Guid.Parse("2ca4bbbc-e52d-4cb7-8dd2-541623973d68");
            const string releasePath = "publication/2016-17";

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var service = SetupMetaGuidanceService(blobStorageService: blobStorageService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

            blobStorageService.Setup(
                    mock => mock.DownloadBlobText(PublicContentContainerName, releasePath))
                .ReturnsAsync(ReleaseJson);

            metaGuidanceSubjectService.Setup(
                    mock => mock.GetSubjects(releaseId))
                .ReturnsAsync(SubjectMetaGuidance);

            var result = await service.Get(releasePath);

            blobStorageService.Verify(
                mock => mock.DownloadBlobText(PublicContentContainerName, releasePath), Times.Once);

            metaGuidanceSubjectService.Verify(
                mock => mock.GetSubjects(releaseId), Times.Once);

            Assert.True(result.IsRight);

            Assert.Equal(releaseId, result.Right.Id);
            Assert.Equal("Release Meta Guidance", result.Right.Content);
            Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
        }

        private static MetaGuidanceService SetupMetaGuidanceService(
            IBlobStorageService blobStorageService = null,
            IMetaGuidanceSubjectService metaGuidanceSubjectService = null)
        {
            return new MetaGuidanceService(
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                metaGuidanceSubjectService ?? new Mock<IMetaGuidanceSubjectService>().Object
            );
        }
    }
}