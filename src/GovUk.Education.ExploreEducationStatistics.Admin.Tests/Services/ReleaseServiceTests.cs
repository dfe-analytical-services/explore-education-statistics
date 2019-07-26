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
    public class ReleaseServiceTests
    {
        [Fact]
        public void GetContacts()
        {
            using (var context = InMemoryApplicationDbContext("Get"))
            {
                var contactsToSave = new List<Contact>
                {
                    new Contact()
                    {
                        Id = Guid.NewGuid(),
                        ContactName = "Contact A",
                        TeamEmail = "ContactA@example.com",
                        ContactTelNo = "123456789"
                    },
                    new Contact()
                    {
                        Id = Guid.NewGuid(),
                        ContactName = "Contact B",
                        TeamEmail = "ContactB@example.com",
                        ContactTelNo = "123456789"
                    }
                };


                context.AddRange(contactsToSave);
                context.SaveChanges();

                var service = new ContactService(context);
                // Method under test
                var retrievedContacts = service.GetContacts();
                Assert.True(retrievedContacts.Exists(c => c.ContactName == "Contact A"));
                Assert.True(retrievedContacts.Exists(c => c.ContactName == "Contact B"));
            }
        }


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
                // Service method under test
                var result = new ReleaseService(context).CreateRelease(new CreateReleaseViewModel
                {
                    PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                    ReleaseName = "2018",
                    TimeIdentifier = TimeIdentifier.AcademicYear,
                    PublishScheduled = DateTime.Parse("2050/01/01"),
                    ReleaseTypeId = new Guid("02e664f2-a4bc-43ee-8ff0-c87354adae72")
                });

                Assert.Equal("Academic Year 2018", result.Title);
                Assert.Null(result.Published);
                Assert.False(result.LatestRelease); // Most recent - but not published yet.
                Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriodCoverage);
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
                var result = new ReleaseService(context).CreateRelease(new CreateReleaseViewModel
                {
                    PublicationId = new Guid("403d3c5d-a8cd-4d54-a029-0c74c86c55b2"),
                    TemplateReleaseId = new Guid("26f17bad-fc48-4496-9387-d6e5b2cb0e7f"),
                    ReleaseName = "2018",
                    TimeIdentifier = TimeIdentifier.AcademicYear,
                    PublishScheduled = DateTime.Parse("2050/01/01"),
                    ReleaseTypeId = new Guid("2a0217ca-c514-45da-a8b3-44c68a6737e8")
                });
                
                // Do an in depth check of the saved release
                var release = context.Releases.Single(r => r.Id == result.Id);
                Assert.Equal(2, release.Content.Count);
                Assert.Equal("Template caption index 0", release.Content[0].Caption);
                Assert.Equal("Template heading index 0", release.Content[0].Heading);
                Assert.Equal(0, release.Content[0].Order);
                Assert.Null(release.Content[0].Content); // TODO currently is not copied - should it be?
                
                Assert.Equal("Template caption index 1", release.Content[1].Caption);
                Assert.Equal("Template heading index 1", release.Content[1].Heading);
                Assert.Equal(1, release.Content[1].Order);
                Assert.Null(release.Content[1].Content);// TODO currently is not copied - should it be?
            }
        }
    }
}