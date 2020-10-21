using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

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

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var service = SetupMetaGuidanceService(
                fileStorageService: fileStorageService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath))
                .ReturnsAsync(new CachedReleaseViewModel
                {
                    Id = releaseId,
                    MetaGuidance = "Release Meta guidance"
                });

            metaGuidanceSubjectService.Setup(
                    mock => mock.GetSubjects(releaseId, null))
                .ReturnsAsync(SubjectMetaGuidance);

            var result = await service.Get(releasePath);

            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath), Times.Once);

            metaGuidanceSubjectService.Verify(
                mock => mock.GetSubjects(releaseId, null), Times.Once);

            Assert.True(result.IsRight);

            Assert.Equal(releaseId, result.Right.Id);
            Assert.Equal("Release Meta guidance", result.Right.Content);
            Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
        }

        [Fact]
        public async Task Get_FileNotFoundExceptionForReleasePath()
        {
            const string releasePath = "incorrect/release-path";

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var service = SetupMetaGuidanceService(
                fileStorageService: fileStorageService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath))
                .ReturnsAsync(new NotFoundResult());

            var result = await service.Get(releasePath);

            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath), Times.Once);

            Assert.True(result.IsLeft);
            Assert.IsType<NotFoundResult>(result.Left);
        }

        private static MetaGuidanceService SetupMetaGuidanceService(
            IFileStorageService fileStorageService = null,
            IMetaGuidanceSubjectService metaGuidanceSubjectService = null)
        {
            return new MetaGuidanceService(
                fileStorageService ?? new Mock<IFileStorageService>().Object,
                metaGuidanceSubjectService ?? new Mock<IMetaGuidanceSubjectService>().Object
            );
        }
    }
}