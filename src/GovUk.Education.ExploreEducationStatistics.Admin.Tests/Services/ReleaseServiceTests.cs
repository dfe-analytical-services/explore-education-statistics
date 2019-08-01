using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseServiceTests
    {
        [Fact]
        public void CreateReleaseNoTemplate()
        {
            using (var context = InMemoryApplicationDbContext("Create"))
            {
                context.Add(new ReleaseType {Id = new Guid("484e6b5c-4a0f-47fd-914e-ac4dac5bdd1c"), Title = "Ad Hoc",});
                context.Add(new Publication
                    {Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Title = "Publication"});
                context.SaveChanges();
            }

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                var result = new ReleaseService(context, MapperForProfile<MappingProfiles>()).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        ReleaseTypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72")
                    });

                Assert.Equal("Academic Year 2018/19", result.Result.Title);
                Assert.Null(result.Result.Published);
                Assert.False(result.Result.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.AcademicYear, result.Result.TimePeriodCoverage);
            }
        }


        [Fact]
        public void CreateReleaseWithTemplate()
        {
            using (var context = InMemoryApplicationDbContext("Create"))
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

            using (var context = InMemoryApplicationDbContext("Create"))
            {
                // Service method under test
                var result = new ReleaseService(context, MapperForProfile<MappingProfiles>()).CreateReleaseAsync(
                    new CreateReleaseViewModel
                    {
                        PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                        TemplateReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                        ReleaseName = "2018",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublishScheduled = DateTime.Parse("2050/01/01"),
                        ReleaseTypeId = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8")
                    });

                // Do an in depth check of the saved release
                var release = context.Releases.Single(r => r.Id == result.Result.Id);
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
        public async void LatestReleaseCorrectlyReported()
        {
            var latestReleaseId = new Guid("274d4621-7d21-431b-80de-77b62a4374d2");
            var notLatestReleaseId = new Guid("49b73c2f-141a-4dc7-a2d4-69316aef8bbc");
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
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

            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                // Method under test
                var latest =
                    await new ReleaseService(context, MapperForProfile<MappingProfiles>())
                        .GetViewModel(latestReleaseId);
                Assert.True(latest.LatestRelease);

                // Method under test
                var notLatest =
                    await new ReleaseService(context, MapperForProfile<MappingProfiles>()).GetViewModel(
                        notLatestReleaseId);
                Assert.False(notLatest.LatestRelease);
            }
        }


        [Fact]
        public async void EditReleaseSummary()
        {
            var releaseId = new Guid("02c73027-3e06-4495-82a4-62b778c005a9");
            var addHocReleaseTypeId = new Guid("f3800c32-1e1c-4d42-8165-d1bcb3c8b47c");
            var officialStatisticsReleaseTypeId = new Guid("fdc4dd4c-85f7-49dd-87a4-e04446bc606f");
            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                context.AddRange(new List<ReleaseType>
                {
                    new ReleaseType
                    {
                        Id = addHocReleaseTypeId,
                        Title = "Ad Hoc"
                    },
                    new ReleaseType
                    {
                        Id = officialStatisticsReleaseTypeId,
                        Title = "Official Statistics"
                    }
                });
                context.Add(new Publication
                {
                    Id = new Guid("f7da23e2-304a-4b47-a8f5-dba28a554de9"),
                    Releases = new List<Release>
                    {
                        new Release
                        {
                            Id = releaseId,
                            TypeId = addHocReleaseTypeId
                        }
                    }
                });
                context.SaveChanges();
            }


            var publishScheduledEdited = DateTime.Now.AddDays(1);
            var nextReleaseDateEdited = new PartialDate {Day = "1", Month = "1", Year = "2040"};
            var typeEditedId = officialStatisticsReleaseTypeId;
            const string releaseNameEdited = "2035";
            const TimeIdentifier timePeriodCoverageEdited = TimeIdentifier.March;


            using (var context = InMemoryApplicationDbContext("LatestReleaseCorrectlyReported"))
            {
                // Method under test 
                var edited = await new ReleaseService(context, MapperForProfile<MappingProfiles>())
                    .EditReleaseSummaryAsync(
                        new EditReleaseSummaryViewModel
                        {
                            Id = releaseId,
                            PublishScheduled = publishScheduledEdited,
                            NextReleaseDate = nextReleaseDateEdited,
                            TypeId = typeEditedId,
                            ReleaseName = releaseNameEdited,
                            TimePeriodCoverage = timePeriodCoverageEdited
                        });

                Assert.Equal(publishScheduledEdited, edited.PublishScheduled);
                Assert.Equal(nextReleaseDateEdited, edited.NextReleaseDate);
                Assert.Equal(typeEditedId, edited.TypeId);
                Assert.Equal(releaseNameEdited, edited.ReleaseName);
                Assert.Equal(timePeriodCoverageEdited, edited.TimePeriodCoverage);
            }
        }
        
        
        
        [Fact]
        public async void GetEditReleaseSummary()
        {
            var releaseId = new Guid("5cf345d4-7f7b-425c-8267-de785cfc040b");
            var addHocReleaseTypeId = new Guid("19b024dc-339c-4e2c-b2ca-b55e5c509ad2");
            var publishScheduled = DateTime.Now.AddDays(1);
            var nextReleaseDate = new PartialDate { Day = "1", Month = "1", Year = "2040"};
            const string releaseName = "2035";
            const TimeIdentifier timePeriodCoverage = TimeIdentifier.January;
            using (var context = InMemoryApplicationDbContext("GetEditReleaseSummary"))
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
            
            using (var context = InMemoryApplicationDbContext("GetEditReleaseSummary"))
            {
                // Method under test 
                var summary = await new ReleaseService(context, MapperForProfile<MappingProfiles>())
                    .GetReleaseSummaryAsync(releaseId);
                        

                Assert.Equal(publishScheduled, summary.PublishScheduled);
                Assert.Equal(nextReleaseDate, summary.NextReleaseDate);
                Assert.Equal(addHocReleaseTypeId, summary.TypeId);
                Assert.Equal(releaseName, summary.ReleaseName);
                Assert.Equal(timePeriodCoverage, summary.TimePeriodCoverage);
            }
        }

    }
}