using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseService2Tests
    {
        [Fact]
        public void CreateReleaseNoTemplate2()
        {
            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate2"))
            {
                context.Add(new ReleaseType {Id = new Guid("484e6b5c-4a0f-47fd-914e-ac4dac5bdd1c"), Title = "Ad Hoc",});
                context.Add(new Publication {Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Title = "Publication"});
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("CreateReleaseNoTemplate2"))
            {
                var result = new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        TypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72")
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
        public async void LatestReleaseCorrectlyReported2()
        {
            var latestReleaseId = new Guid("274d4621-7d21-431b-80de-77b62a4374d2");
            var notLatestReleaseId = new Guid("49b73c2f-141a-4dc7-a2d4-69316aef8bbc");
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported2"))
            {
                context.Add(new Publication
                {
                    Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                    Title = "Publication",
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            Id = notLatestReleaseId,
                            Order = 0,
                            Published = DateTime.Now.AddDays(-2) // Is published but not the latest by order
                        },
                        new Release
                        {
                            Id = latestReleaseId,
                            Order = 1,
                            Published = DateTime.Now.AddDays(-1) // Is published and the the latest by order
                        }
                    }
                });

                context.SaveChanges();
            }

            // Note that we use different contexts for each method call - this is to avoid misleadingly optimistic
            // loading of the entity graph as we go.
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported2"))
            {
                // Method under test
                var notLatest =
                    await new ReleaseService2(context).GetReleaseForIdAsync(
                        notLatestReleaseId);
                Assert.False(notLatest.LatestRelease);
            }

            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                // Method under test
                var notLatest =
                    await new ReleaseService2(context).GetReleaseForIdAsync(
                        notLatestReleaseId);
                Assert.False(notLatest.LatestRelease);
            }
        }


        [Fact]
        public async void EditReleaseSummary2()
        {
            using (var context = InMemoryApplicationDbContext("EditReleaseSummary2"))
            {
                context.Add(new ReleaseType {Id = new Guid("484e6b5c-4a0f-47fd-914e-ac4dac5bdd1c"), Title = "Ad Hoc",});
                context.Add(new Publication {Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Title = "Publication"});
                context.SaveChanges();
            }

            Guid releaseId;
            using (var context = InMemoryApplicationDbContext("EditReleaseSummary2"))
            {
                // Save the release that will later be edited
                var created = await new ReleaseService2(context).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        TypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72"),
                        NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2014"}
                    });

                Assert.Equal("Academic Year 2018/19", created.Right.Title);
                Assert.Null(created.Right.Published);
                Assert.False(created.Right.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.AcademicYear, created.Right.TimePeriodCoverage);
                releaseId = created.Right.Id;
            }

            using (var context = InMemoryApplicationDbContext("EditReleaseSummary2"))
            {
                var edited = await new ReleaseService2(context).EditReleaseSummaryAsync(
                    new EditReleaseSummaryViewModel
                    {
                        Id = releaseId,
                        ReleaseName = "2019",
                        TimePeriodCoverage = TimeIdentifier.August,
                        PublishScheduled = DateTime.Parse("2051/01/01"),
                        TypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72"),
                        NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2015"}
                    });
                Assert.Equal("August 2019", edited.Right.Title);
                Assert.Null(edited.Right.Published);
                Assert.False(edited.Right.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.August, edited.Right.TimePeriodCoverage);
            }
        }


        [Fact]
        public async void GetEditReleaseSummaryAsync2()
        {
            var releaseId = new Guid("5cf345d4-7f7b-425c-8267-de785cfc040b");
            var addHocReleaseTypeId = new Guid("19b024dc-339c-4e2c-b2ca-b55e5c509ad2");
            var publishScheduled = DateTime.Now.AddDays(1);
            var nextReleaseDate = new PartialDate {Day = "1", Month = "1", Year = "2040"};
            const string releaseName = "2035";
            const TimeIdentifier timePeriodCoverage = TimeIdentifier.January;
            using (var context = InMemoryApplicationDbContext("GetEditReleaseSummaryAsync2"))
            {
                context.AddRange(new List<ReleaseType>
                {
                    new ReleaseType
                    {
                        Id = addHocReleaseTypeId,
                        Title = "Ad Hoc"
                    },
                });
                context.Add(
                    new Release
                    {
                        Id = releaseId,
                        TypeId = addHocReleaseTypeId,
                        TimePeriodCoverage = TimeIdentifier.January,
                        PublishScheduled = publishScheduled,
                        NextReleaseDate = nextReleaseDate,
                        ReleaseName = releaseName
                    });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetEditReleaseSummaryAsync2"))
            {
                // Method under test 
                var summary = await new ReleaseService2(context)
                    .GetReleaseSummaryAsync(releaseId);


                Assert.Equal(publishScheduled, summary.PublishScheduled);
                Assert.Equal(nextReleaseDate, summary.NextReleaseDate);
                Assert.Equal(addHocReleaseTypeId, summary.TypeId);
                Assert.Equal(releaseName, summary.ReleaseName);
                Assert.Equal(timePeriodCoverage, summary.TimePeriodCoverage);
            }
        }


        [Fact]
        public async void GetReleasesForPublicationAsync()
        {
            var addHocReleaseTypeId = new Guid("19b024dc-339c-4e2c-b2ca-b55e5c509ad2");
            var publicationId = new Guid("94af186f-5dbe-4f46-8a8e-f5480ed9f4fc");
            var publishScheduledForFirstRelease = DateTime.Now.AddDays(1);
            var publishScheduledForSecondRelease = DateTime.Now.AddDays(2);
            var firstReleaseId = new Guid("8781ebf8-790c-484b-8a34-19ea501c9c74");
            var secondReleaseId = new Guid("cace48cf-9f08-4471-815c-b3703aade096");

            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync"))
            {
                context.AddRange(new List<ReleaseType>
                {
                    new ReleaseType
                    {
                        Id = addHocReleaseTypeId,
                        Title = "Ad Hoc"
                    },
                });
                context.Add(new Publication
                {
                    Id = publicationId,
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            Id = firstReleaseId,
                            TypeId = addHocReleaseTypeId,
                            TimePeriodCoverage = TimeIdentifier.February,
                            PublishScheduled = publishScheduledForFirstRelease,
                            NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2040"},
                            ReleaseName = "2035",
                        },
                        new Release
                        {
                            Id = secondReleaseId,
                            TypeId = addHocReleaseTypeId,
                            TimePeriodCoverage = TimeIdentifier.January,
                            PublishScheduled = publishScheduledForSecondRelease,
                            NextReleaseDate = new PartialDate {Day = "01", Month = "01", Year = "2041"},
                            ReleaseName = "2036"
                        }
                    }
                });
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("GetReleasesForPublicationAsync2"))
            {
                // Method under test 
                var summary = await new ReleaseService2(context)
                    .GetReleasesForPublicationAsync(publicationId);
                Assert.Equal(2, summary.Count);
                Assert.True(summary.Exists(r => r.Id == firstReleaseId && r.TypeId == addHocReleaseTypeId));
                Assert.True(summary.Exists(r =>
                    r.Id == firstReleaseId && r.TimePeriodCoverage == TimeIdentifier.February));
                Assert.True(summary.Exists(r =>
                    r.Id == firstReleaseId && r.PublishScheduled == publishScheduledForFirstRelease));
                Assert.True(summary.Exists(r => r.Id == firstReleaseId && r.ReleaseName == "2035"));
            }
        }
    }
}