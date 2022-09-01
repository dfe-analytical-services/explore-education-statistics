#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class DataGuidanceServiceTests
    {
        private static readonly List<DataGuidanceSubjectViewModel> DataGuidanceSubjects =
            new()
            {
                new DataGuidanceSubjectViewModel
                {
                    Id = Guid.NewGuid(),
                    Content = "Subject Guidance",
                    Filename = "data.csv",
                    Name = "Subject",
                    GeographicLevels = new List<string>
                    {
                        "National", "Local Authority", "Local Authority District"
                    },
                    TimePeriods = new TimePeriodLabels("2020_AYQ3", "2021_AYQ1"),
                    Variables = new List<LabelValue>
                    {
                        new("Filter label", "test_filter"),
                        new("Indicator label", "test_indicator")
                    }
                }
            };

        [Fact]
        public async Task Get()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            const string publicationSlug = "test-publication";
            const string releaseSlug = "2016-17";

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(Strict);
            var publicationService = new Mock<IPublicationService>(Strict);
            var releaseService = new Mock<IReleaseService>(Strict);

            var service = SetupService(
                dataGuidanceSubjectService: dataGuidanceSubjectService.Object,
                publicationService: publicationService.Object,
                releaseService: releaseService.Object
            );

            publicationService.Setup(mock => mock.GetCachedPublication(publicationSlug))
                .ReturnsAsync(
                    new PublicationViewModel
                    {
                        Id = publicationId,
                        Title = "Test publication",
                        Slug = publicationSlug,
                    }
                );
            releaseService.Setup(mock => mock.GetCachedRelease(publicationSlug, releaseSlug))
                .ReturnsAsync(
                    new CachedReleaseViewModel(releaseId)
                    {
                        Title = "2016-17",
                        Slug = "2016-17",
                        DataGuidance = "Release Guidance",
                        Type = new ReleaseTypeViewModel
                        {
                            Title = "National Statistics"
                        }
                    }
                );

            dataGuidanceSubjectService.Setup(
                    mock => mock.GetSubjects(releaseId, null)
                )
                .ReturnsAsync(DataGuidanceSubjects);

            var result = await service.Get(publicationSlug, releaseSlug);

            publicationService.Verify(
                mock => mock.GetCachedPublication(publicationSlug),
                Times.Once
            );

            releaseService.Verify(
                mock => mock.GetCachedRelease(publicationSlug, releaseSlug),
                Times.Once
            );

            dataGuidanceSubjectService.Verify(
                mock => mock.GetSubjects(releaseId, null),
                Times.Once
            );

            VerifyAllMocks(publicationService, releaseService, dataGuidanceSubjectService);

            Assert.True(result.IsRight);

            Assert.Equal(releaseId, result.Right.Id);
            Assert.Equal("2016-17", result.Right.Title);
            Assert.Equal("2016-17", result.Right.Slug);
            Assert.Equal("Release Guidance", result.Right.DataGuidance);
            Assert.Equal(DataGuidanceSubjects, result.Right.Subjects);

            Assert.Equal(publicationId, result.Right.Publication!.Id);
            Assert.Equal("Test publication", result.Right.Publication.Title);
            Assert.Equal("test-publication", result.Right.Publication.Slug);
        }

        [Fact]
        public async Task Get_NotFoundForPublicationSlug()
        {
            const string publicationSlug = "incorrect-publication-slug";
            const string releaseSlug = "2016-17";

            var publicationService = new Mock<IPublicationService>(Strict);

            var service = SetupService(publicationService: publicationService.Object);

            publicationService.Setup(mock => mock.GetCachedPublication(publicationSlug))
                .ReturnsAsync(new NotFoundResult());

            var result = await service.Get( publicationSlug, releaseSlug);

            publicationService.Verify(
                mock => mock.GetCachedPublication(publicationSlug),
                Times.Once
            );

            VerifyAllMocks(publicationService);

            result.AssertNotFound();
        }

        [Fact]
        public async Task Get_NotFoundForReleaseSlug()
        {
            const string publicationSlug = "test-publication";
            const string releaseSlug = "incorrect-release-slug";

            var publicationService = new Mock<IPublicationService>(Strict);
            var releaseService = new Mock<IReleaseService>(Strict);

            var service = SetupService(
                publicationService: publicationService.Object,
                releaseService: releaseService.Object
            );

            publicationService.Setup(mock => mock.GetCachedPublication(publicationSlug))
                .ReturnsAsync(new PublicationViewModel());

            releaseService.Setup(mock => mock.GetCachedRelease(publicationSlug, releaseSlug))
                .ReturnsAsync(
                    new NotFoundResult()
                );

            var result = await service.Get(publicationSlug, releaseSlug);

            publicationService.Verify(
                mock => mock.GetCachedPublication(publicationSlug),
                Times.Once
            );

            releaseService.Verify(
                mock => mock.GetCachedRelease(publicationSlug, releaseSlug),
                Times.Once
            );

            VerifyAllMocks(publicationService, releaseService);

            result.AssertNotFound();
        }

        private static DataGuidanceService SetupService(
            IDataGuidanceSubjectService? dataGuidanceSubjectService = null,
            IPublicationService? publicationService = null,
            IReleaseService? releaseService = null)
        {
            return new DataGuidanceService(
                dataGuidanceSubjectService ?? Mock.Of<IDataGuidanceSubjectService>(Strict),
                publicationService ?? Mock.Of<IPublicationService>(Strict),
                releaseService ?? Mock.Of<IReleaseService>(Strict)
            );
        }
    }
}
