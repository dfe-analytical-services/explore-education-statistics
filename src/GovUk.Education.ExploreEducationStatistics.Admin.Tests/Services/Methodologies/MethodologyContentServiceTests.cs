#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyContentServiceTests
    {
        [Fact]
        public async Task GetContent()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title",
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        Published = DateTime.UtcNow,
                        AlternativeTitle = "Alternative title"
                    }
                }
            };
            var methodologyVersion = methodology.Versions[0];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext);

                var result = (await service.GetContent(methodologyVersion.Id)).AssertRight();

                Assert.Equal(methodologyVersion.Id, result.Id);
                Assert.Equal(methodology.Slug, result.Slug);
                Assert.Equal(methodologyVersion.Title, result.Title);
                Assert.Equal(methodologyVersion.Status, result.Status);
                Assert.Equal(methodologyVersion.Published, result.Published);
                Assert.Empty(result.Annexes);
                Assert.Empty(result.Content);
                Assert.Empty(result.Notes);
            }
        }

        [Fact]
        public async Task GetContent_TestContentSections()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title",
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        MethodologyContent = new MethodologyVersionContent
                        {
                            Annexes = new List<ContentSection>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Order = 2,
                                    Heading = "Annex 3 heading",
                                    Caption = "Annex 3 caption"
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Order = 0,
                                    Heading = "Annex 1 heading",
                                    Caption = "Annex 1 caption"
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Order = 1,
                                    Heading = "Annex 2 heading",
                                    Caption = "Annex 2 caption"
                                }
                            },
                            Content = new List<ContentSection>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Order = 2,
                                    Heading = "Section 3 heading",
                                    Caption = "Section 3 caption"
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Order = 0,
                                    Heading = "Section 1 heading",
                                    Caption = "Section 1 caption"
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Order = 1,
                                    Heading = "Section 2 heading",
                                    Caption = "Section 2 caption"
                                }
                            },
                        },
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Alternative title"
                    }
                }
            };
            var methodologyVersion = methodology.Versions[0];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext);

                var result = (await service.GetContent(methodologyVersion.Id)).AssertRight();

                Assert.Equal(3, result.Annexes.Count);
                Assert.Equal(3, result.Content.Count);

                var expectedAnnex1 = methodologyVersion.MethodologyContent.Annexes.Single(section => section.Order == 0);
                var expectedAnnex2 = methodologyVersion.MethodologyContent.Annexes.Single(section => section.Order == 1);
                var expectedAnnex3 = methodologyVersion.MethodologyContent.Annexes.Single(section => section.Order == 2);

                AssertContentSectionAndViewModelEqual(expectedAnnex1, result.Annexes[0]);
                AssertContentSectionAndViewModelEqual(expectedAnnex2, result.Annexes[1]);
                AssertContentSectionAndViewModelEqual(expectedAnnex3, result.Annexes[2]);

                var expectedContent1 = methodologyVersion.MethodologyContent.Content.Single(section => section.Order == 0);
                var expectedContent2 = methodologyVersion.MethodologyContent.Content.Single(section => section.Order == 1);
                var expectedContent3 = methodologyVersion.MethodologyContent.Content.Single(section => section.Order == 2);

                AssertContentSectionAndViewModelEqual(expectedContent1, result.Content[0]);
                AssertContentSectionAndViewModelEqual(expectedContent2, result.Content[1]);
                AssertContentSectionAndViewModelEqual(expectedContent3, result.Content[2]);
            }
        }

        private static void AssertContentSectionAndViewModelEqual(
            ContentSection expected,
            ContentSectionViewModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Caption, actual.Caption);
            Assert.Equal(expected.Heading, actual.Heading);
            Assert.Equal(expected.Order, actual.Order);
        }

        [Fact]
        public async Task GetContent_TestNotes()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title",
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Alternative title"
                    }
                }
            };
            var methodologyVersion = methodology.Versions[0];

            var otherNote = new MethodologyNote
            {
                DisplayDate = DateTime.Today.AddDays(-2).ToUniversalTime(),
                Content = "Other note",
                MethodologyVersion = methodologyVersion
            };

            var earliestNote = new MethodologyNote
            {
                DisplayDate = DateTime.Today.AddDays(-3).ToUniversalTime(),
                Content = "Earliest note",
                MethodologyVersion = methodologyVersion
            };

            var latestNote = new MethodologyNote
            {
                DisplayDate = DateTime.Today.AddDays(-1).ToUniversalTime(),
                Content = "Latest note",
                MethodologyVersion = methodologyVersion
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyNotes.AddRangeAsync(otherNote, earliestNote, latestNote);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext);

                var result = (await service.GetContent(methodologyVersion.Id)).AssertRight();

                Assert.Equal(3, result.Notes.Count);

                AssertMethodologyNoteAndViewModelEqual(latestNote, result.Notes[0]);
                AssertMethodologyNoteAndViewModelEqual(otherNote, result.Notes[1]);
                AssertMethodologyNoteAndViewModelEqual(earliestNote, result.Notes[2]);
            }
        }

        private static void AssertMethodologyNoteAndViewModelEqual(
            MethodologyNote expected,
            MethodologyNoteViewModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Content, actual.Content);
            Assert.Equal(expected.DisplayDate, actual.DisplayDate);
        }

        [Fact]
        public async Task GetContent_MethodologyVersionNotFound()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

            var result = await service.GetContent(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetContentBlocks_NoContentSections()
        {
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodologyVersion.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks_NoContentBlocks()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "New section",
                            Order = 1
                        }
                    },
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "New section",
                            Order = 2
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodologyVersion.Id);

                Assert.True(result.IsRight);

                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task GetContentBlocks()
        {
            var annexHtmlBlock1 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var annexHtmlBlock2 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var contentHtmlBlock1 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var contentHtmlBlock2 = new HtmlBlock
            {
                Id = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "New section",
                            Order = 1,
                            Content = new List<ContentBlock>
                            {
                                annexHtmlBlock1,
                                new DataBlock
                                {
                                    Id = Guid.NewGuid()
                                }
                            }
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "New section",
                            Order = 2,
                            Content = new List<ContentBlock>
                            {
                                annexHtmlBlock2,
                                new DataBlock
                                {
                                    Id = Guid.NewGuid()
                                }
                            }
                        }
                    },
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "New section",
                            Order = 1,
                            Content = new List<ContentBlock>
                            {
                                contentHtmlBlock1,
                                new DataBlock
                                {
                                    Id = Guid.NewGuid()
                                }
                            }
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "New section",
                            Order = 2,
                            Content = new List<ContentBlock>
                            {
                                contentHtmlBlock2,
                                new DataBlock
                                {
                                    Id = Guid.NewGuid()
                                }
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyContentService(contentDbContext: contentDbContext);

                var result = await service.GetContentBlocks<HtmlBlock>(methodologyVersion.Id);

                Assert.True(result.IsRight);

                var contentBlocks = result.Right;

                Assert.Equal(4, contentBlocks.Count);
                Assert.Equal(annexHtmlBlock1.Id, contentBlocks[0].Id);
                Assert.Equal(annexHtmlBlock2.Id, contentBlocks[1].Id);
                Assert.Equal(contentHtmlBlock1.Id, contentBlocks[2].Id);
                Assert.Equal(contentHtmlBlock2.Id, contentBlocks[3].Id);
            }
        }

        [Fact]
        public async Task AddContentSection_Draft()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Draft,
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);
                var result = await methodologyContentService.AddContentSection(
                    methodologyVersion.Id,
                    new ContentSectionAddRequest(),
                    MethodologyContentService.ContentListType.Content);

                Assert.True(result.IsRight);
                Assert.Empty(result.Right.Content);
                Assert.Equal(0, result.Right.Order);
            }
        }

        [Fact]
        public async Task AddContentSection_Approved()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Status = Approved,
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);
                var result = await methodologyContentService.AddContentSection(
                    methodologyVersion.Id,
                    new ContentSectionAddRequest(),
                    MethodologyContentService.ContentListType.Content);

                result.AssertBadRequest(MethodologyMustBeDraft);
            }
        }

        [Fact]
        public async Task ReorderContentSections_UpdateContent()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 0
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 1
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 2
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var contentSections = methodologyVersion.MethodologyContent.Content;

                var result = await methodologyContentService.ReorderContentSections(
                    methodologyVersion.Id,
                    new Dictionary<Guid, int>
                    {
                        {
                            contentSections[2].Id,
                            0
                        },
                        {
                            contentSections[0].Id,
                            1
                        },
                        {
                            contentSections[1].Id,
                            2
                        }
                    }
                );

                var updatedSections = result.AssertRight();

                Assert.Equal(3, updatedSections.Count);
                Assert.Equal(contentSections[2].Id, updatedSections[0].Id);
                Assert.Equal(contentSections[0].Id, updatedSections[1].Id);
                Assert.Equal(contentSections[1].Id, updatedSections[2].Id);
            }
        }

        [Fact]
        public async Task ReorderContentSections_UpdateAnnexes()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 0
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 1
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 2
                        }
                    },
                    Content = new List<ContentSection>()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var annexeSections = methodologyVersion.MethodologyContent.Annexes;

                var result = await methodologyContentService.ReorderContentSections(
                    methodologyVersion.Id,
                    new Dictionary<Guid, int>
                    {
                        {
                            annexeSections[2].Id,
                            0
                        },
                        {
                            annexeSections[0].Id,
                            1
                        },
                        {
                            annexeSections[1].Id,
                            2
                        }
                    }
                );

                var updatedSections = result.AssertRight();

                Assert.Equal(3, updatedSections.Count);
                Assert.Equal(annexeSections[2].Id, updatedSections[0].Id);
                Assert.Equal(annexeSections[0].Id, updatedSections[1].Id);
                Assert.Equal(annexeSections[1].Id, updatedSections[2].Id);
            }
        }

        [Fact]
        public async Task ReorderContentSections_MethodologyVersionIdNotFound()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 0
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var contentSections = methodologyVersion.MethodologyContent.Content;

                var result = await methodologyContentService.ReorderContentSections(
                    Guid.NewGuid(),
                    new Dictionary<Guid, int>
                    {
                        {
                            contentSections[0].Id,
                            0
                        }
                    }
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ReorderContentSections_ContentSectionMissingFromOrder()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 0
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 1
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var contentSections = methodologyVersion.MethodologyContent.Content;

                var result = await methodologyContentService.ReorderContentSections(
                    methodologyVersion.Id,
                    new Dictionary<Guid, int>
                    {
                        {
                            contentSections[1].Id,
                            0
                        }
                    }
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ReorderContentSections_AnnexeSectionMissingFromOrder()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 0
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 1
                        }
                    },
                    Content = new List<ContentSection>()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var annexeSections = methodologyVersion.MethodologyContent.Annexes;

                var result = await methodologyContentService.ReorderContentSections(
                    methodologyVersion.Id,
                    new Dictionary<Guid, int>
                    {
                        {
                            annexeSections[1].Id,
                            0
                        }
                    }
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ReorderContentSections_SectionIdNotFoundInContent()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Order = 0
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var contentSections = methodologyVersion.MethodologyContent.Content;

                var result = await methodologyContentService.ReorderContentSections(
                    methodologyVersion.Id,
                    new Dictionary<Guid, int>
                    {
                        {
                            contentSections[0].Id,
                            0
                        },
                        {
                            Guid.NewGuid(),
                            1
                        }
                    }
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UpdateContentSectionHeading_ContentSection()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "Section 1 heading"
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var contentSections = methodologyVersion.MethodologyContent.Content;

                var result = await methodologyContentService.UpdateContentSectionHeading(
                    methodologyVersion.Id,
                    contentSections[0].Id,
                    "Section 1 heading updated"
                );

                var viewModel = result.AssertRight();

                Assert.Equal(contentSections[0].Id, viewModel.Id);
                Assert.Equal("Section 1 heading updated", viewModel.Heading);
            }
        }

        [Fact]
        public async Task UpdateContentSectionHeading_AnnexeSection()
        {
            var methodologyVersion = new MethodologyVersion
            {
                MethodologyContent = new MethodologyVersionContent
                {
                    Annexes = new List<ContentSection>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Heading = "Section 1 heading"
                        }
                    },
                    Content = new List<ContentSection>()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var methodologyContentService = SetupMethodologyContentService(contentDbContext);

                var contentSections = methodologyVersion.MethodologyContent.Annexes;

                var result = await methodologyContentService.UpdateContentSectionHeading(
                    methodologyVersion.Id,
                    contentSections[0].Id,
                    "Section 1 heading updated"
                );

                var viewModel = result.AssertRight();

                Assert.Equal(contentSections[0].Id, viewModel.Id);
                Assert.Equal("Section 1 heading updated", viewModel.Heading);
            }
        }

        private static MethodologyContentService SetupMethodologyContentService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IUserService? userService = null)
        {
            return new(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                AdminMapper()
            );
        }
    }
}
