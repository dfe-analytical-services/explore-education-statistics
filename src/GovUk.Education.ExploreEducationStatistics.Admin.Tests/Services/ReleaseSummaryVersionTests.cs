using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.EqualityUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseService2Tests
    {
        [Fact]
        public void CreateReleaseNoTemplate2()
        {
            var (releaseTypeId, publicationId) = CreatePublicationAndReleaseType("CreateReleaseNoTemplate2");

            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate2"))
            {
                var result = new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publicationId,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        TypeId = releaseTypeId
                    });

                Assert.Equal("Academic Year 2018/19", result.Result.Right.Title);
                Assert.Null(result.Result.Right.Published);
                Assert.False(result.Result.Right.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.AcademicYear, result.Result.Right.TimePeriodCoverage);
            }
        }


        [Fact]
        public void CreateReleaseWithTemplate2()
        {
            using (var context = InMemoryApplicationDbContext("CreateReleaseWithTemplate2"))
            {
                context.Add(new ReleaseType {Id = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8"), Title = "Ad Hoc",});
                context.Add(new Publication
                {
                    Id = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                    Title = "Publication",
                    Releases = new List<Release>
                    {
                        new Release // Template release
                        {
                            Id = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                            ReleaseName = "2018",
                            Content = new List<ContentSection>
                            {
                                new ContentSection
                                {
                                    Caption = "Template caption index 0", // Should be copied 
                                    Heading = "Template heading index 0", // Should be copied
                                    Order = 0,
                                    Content = new List<IContentBlock>
                                    {
                                        // TODO currently is not copied - should it be?
                                        new HtmlBlock {Body = @"<div></div>"}
                                    }
                                },
                                new ContentSection
                                {
                                    Caption = "Template caption index 1", // Should be copied 
                                    Heading = "Template heading index 1", // Should be copied
                                    Order = 1,
                                    Content = new List<IContentBlock>
                                    {
                                        // TODO currently is not copied - should it be?
                                        new InsetTextBlock {Body = "Text", Heading = "Heading"}
                                    }
                                }
                            }
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("CreateReleaseWithTemplate2"))
            {
                // Service method under test
                var result = new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        TemplateReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        TypeId = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8")
                    });

                // Do an in depth check of the saved release
                var release = context.Releases.Single(r => r.Id == result.Result.Right.Id);
                Assert.Equal(2, release.Content.Count);
                Assert.Equal("Template caption index 0", release.Content[0].Caption);
                Assert.Equal("Template heading index 0", release.Content[0].Heading);
                Assert.Equal(0, release.Content[0].Order);
                Assert.Null(release.Content[0].Content); // TODO currently is not copied - should it be?

                Assert.Equal("Template caption index 1", release.Content[1].Caption);
                Assert.Equal("Template heading index 1", release.Content[1].Heading);
                Assert.Equal(1, release.Content[1].Order);
                Assert.Null(release.Content[1].Content); // TODO currently is not copied - should it be?
            }
        }

        [Fact]
        public async void EditReleaseSummary2()
        {
            var (releaseTypeId, publicationId) = CreatePublicationAndReleaseType("EditReleaseSummary2");

            Guid releaseId;
            using (var context = InMemoryApplicationDbContext("EditReleaseSummary2"))
            {
                // Save the release that will later be edited - use the services. 
                var result = await new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publicationId,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2018/06/01"),
                        TypeId = releaseTypeId,
                        NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2019"}
                    });

                Assert.True(!result.IsLeft);
                var created = result.Right;
                Assert.Null(created.Published);
                Assert.Equal(created.PublishScheduled, DateTime.Parse("2018/06/01"));
                Assert.Equal("2018", created.ReleaseName);    
                Assert.Equal("2019", created.NextReleaseDate.Year);    
                Assert.Equal(TimeIdentifier.AcademicYear, created.TimePeriodCoverage);
                releaseId = created.Id;
            }

            using (var context = InMemoryApplicationDbContext("EditReleaseSummary2"))
            {
                // Call the method under test
                var result = await new ReleaseService2(context).EditReleaseSummaryAsync(
                    new EditReleaseSummaryViewModel
                    {
                        Id = releaseId,
                        ReleaseName = "2019",
                        TimePeriodCoverage = TimeIdentifier.August,
                        PublishScheduled = DateTime.Parse("2019/06/01"),
                        TypeId = releaseTypeId,
                        NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2020"}
                    });
                Assert.True(!result.IsLeft);
                var edited = result.Right;
                Assert.Null(edited.Published);
                Assert.Equal(edited.PublishScheduled, DateTime.Parse("2019/06/01"));
                Assert.Equal("2019", edited.ReleaseName);    
                Assert.Equal("2020", edited.NextReleaseDate.Year);    
                Assert.Equal(TimeIdentifier.August, edited.TimePeriodCoverage);
            }
        }


        [Fact]
        public async void GetReleaseSummaryAsync2()
        {
            var (releaseTypeId, publicationId) = CreatePublicationAndReleaseType("GetReleaseSummaryAsync2");

            Guid releaseId;
            using (var context = InMemoryApplicationDbContext("GetReleaseSummaryAsync2"))
            {
                // Save the release that will retrieved later - use the services.
                var result = await new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publicationId,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2018/06/01"),
                        TypeId = releaseTypeId,
                        NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2019"}
                    });

                Assert.True(!result.IsLeft);
                releaseId = result.Right.Id;
            }
            
            using (var context = InMemoryApplicationDbContext("GetReleaseSummaryAsync2"))
            {
                // Method under test 
                var summary = await new ReleaseService2(context).GetReleaseSummaryAsync(releaseId);
                Assert.Equal(DateTime.Parse("2018/06/01"), summary.PublishScheduled);
                Assert.Equal(new PartialDate {Day = "01", Month = "01", Year = "2019"}, summary.NextReleaseDate, PartialDateEqualityComparer());
                Assert.Equal(releaseTypeId, summary.TypeId);
                Assert.Equal("2018", summary.ReleaseName);
                Assert.Equal(TimeIdentifier.AcademicYear, summary.TimePeriodCoverage);
            }
        }


        [Fact]
        public async void GetReleasesForPublicationAsync()
        {
            var (releaseTypeId, publicationId) = CreatePublicationAndReleaseType("GetReleasesForPublicationAsync2");
            
            Guid firstReleaseId;
            Guid secondReleaseId;

            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync2"))
            {
                // Add first release that will retrieved later - use the services.
                var result = await new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publicationId,
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2018/06/01"),
                        TypeId = releaseTypeId,
                        NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2019"}
                    });
                Assert.True(!result.IsLeft);
                firstReleaseId = result.Right.Id;

            }
            
            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync2"))
            {
                // Add first release that will retrieved later - use the services.
                var result = await new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = publicationId,
                        ReleaseName = "2019",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2019/06/01"),
                        TypeId = releaseTypeId,
                        NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2019"}
                    });
                Assert.True(!result.IsLeft);
                secondReleaseId = result.Right.Id;
            }
            
            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync2"))
            {
                // Method under test 
                var releasesForPublication = await new ReleaseService2(context)
                    .GetReleasesForPublicationAsync(publicationId);
                Assert.Equal(2, releasesForPublication .Count);
                Assert.True(releasesForPublication .Exists(r => r.Id == firstReleaseId && r.TypeId == releaseTypeId));
                Assert.True(releasesForPublication .Exists(r =>
                    r.Id == firstReleaseId && r.TimePeriodCoverage == TimeIdentifier.AcademicYear));
                Assert.True(releasesForPublication .Exists(r =>
                    r.Id == firstReleaseId && r.PublishScheduled == DateTime.Parse("2018/06/01")));
                Assert.True(releasesForPublication .Exists(r => r.Id == firstReleaseId && r.ReleaseName == "2018"));
            }
        }
        
        private static (Guid releaseTypeId, Guid publicationId) CreatePublicationAndReleaseType(string contextName)
        {
            using (var context = InMemoryApplicationDbContext(contextName))
            {
                var releaseTypeId = Guid.NewGuid();
                var publicationId = Guid.NewGuid();
                context.Add(new ReleaseType {Id = releaseTypeId, Title = "Ad Hoc",});
                context.Add(new Publication {Id = publicationId, Title = "Publication"});
                context.SaveChanges();
                return (releaseTypeId, publicationId);
            }
        }
    }
    
    
}