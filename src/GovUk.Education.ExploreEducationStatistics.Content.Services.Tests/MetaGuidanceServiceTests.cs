using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
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
                    TimePeriods = new TimePeriodLabels("2020_AYQ3", "2021_AYQ1"),
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
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            const string publicationPath = "test-publication";
            const string releasePath = "2016-17";

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var service = SetupMetaGuidanceService(
                fileStorageService: fileStorageService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedPublicationViewModel>(publicationPath)
                )
                .ReturnsAsync(
                    new CachedPublicationViewModel
                    {
                        Id = publicationId,
                        Title = "Test publication",
                        Slug = "test-publication",
                        LatestReleaseId = releaseId
                    }
                );

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath)
                )
                .ReturnsAsync(
                    new CachedReleaseViewModel
                    {
                        Id = releaseId,
                        Title = "2016-17",
                        Slug = "2016-17",
                        MetaGuidance = "Release Meta guidance"
                    }
                );

            metaGuidanceSubjectService.Setup(
                    mock => mock.GetSubjects(releaseId, null)
                )
                .ReturnsAsync(SubjectMetaGuidance);

            var result = await service.Get(publicationPath, releasePath);

            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedPublicationViewModel>(publicationPath),
                Times.Once
            );
            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath),
                Times.Once
            );

            metaGuidanceSubjectService.Verify(
                mock => mock.GetSubjects(releaseId, null),
                Times.Once
            );

            Assert.True(result.IsRight);

            Assert.Equal(releaseId, result.Right.Id);
            Assert.Equal("2016-17", result.Right.Title);
            Assert.Equal("2016-17", result.Right.Slug);
            Assert.Equal("Release Meta guidance", result.Right.MetaGuidance);
            Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);

            Assert.Equal(publicationId, result.Right.Publication.Id);
            Assert.Equal("Test publication", result.Right.Publication.Title);
            Assert.Equal("test-publication", result.Right.Publication.Slug);
        }

        [Fact]
        public async Task Get_NotFoundForPublicationPath()
        {
            const string publicationPath = "incorrect-publication-path";
            const string releasePath = "2016-17";

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var service = SetupMetaGuidanceService(
                fileStorageService: fileStorageService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedPublicationViewModel>(publicationPath)
                )
                .ReturnsAsync(new NotFoundResult());

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath)
                )
                .ReturnsAsync(new CachedReleaseViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "2016-17",
                    Slug = "2016-17",
                    MetaGuidance = "Release Meta guidance"
                });

            var result = await service.Get(publicationPath, releasePath);

            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedPublicationViewModel>(publicationPath),
                Times.Once
            );
            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath),
                Times.Once
            );

            result.AssertNotFound();
        }

        [Fact]
        public async Task Get_NotFoundForReleasePath()
        {
            const string publicationPath = "test-publication";
            const string releasePath = "incorrect-release-path";

            var fileStorageService = new Mock<IFileStorageService>(MockBehavior.Strict);
            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var service = SetupMetaGuidanceService(
                fileStorageService: fileStorageService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedPublicationViewModel>(publicationPath)
                )
                .ReturnsAsync(
                    new CachedPublicationViewModel
                    {
                        Id = Guid.NewGuid(),
                        Title = "Test publication",
                        Slug = "test-publication",
                        LatestReleaseId = Guid.NewGuid()
                    }
                );

            fileStorageService.Setup(
                    mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath)
                )
                .ReturnsAsync(new NotFoundResult());

            var result = await service.Get(publicationPath, releasePath);

            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedPublicationViewModel>(publicationPath),
                Times.Once
            );
            fileStorageService.Verify(
                mock => mock.GetDeserialized<CachedReleaseViewModel>(releasePath),
                Times.Once
            );

            result.AssertNotFound();
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
