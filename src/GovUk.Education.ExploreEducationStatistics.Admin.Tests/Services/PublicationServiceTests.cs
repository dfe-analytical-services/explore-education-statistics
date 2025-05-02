#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class PublicationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task ListPublications_CanViewAllPublications_Theme()
    {
        var theme = new Theme
        {
            Title = "Theme title",
        };

        var publication1 = new Publication
        {
            Title = "Test Publication",
            Summary = "Test summary",
            Slug = "test-slug",
            Theme = theme,
            Contact = new Contact
            {
                ContactName = "contact name",
                ContactTelNo = "1234",
                TeamName = "team name",
                TeamEmail = "team@email",
            },
            SupersededBy = null,
        };

        var publication2 = new Publication
        {
            Theme = new(),
        };

        var publication3 = new Publication
        {
            Theme = new(),
        };

        var userService = new Mock<IUserService>(Strict);

        userService.Setup(s => s.GetUserId()).Returns(new Guid());
        userService.Setup(s => s.MatchesPolicy(RegisteredUser)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(true);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddRangeAsync(publication1, publication2, publication3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(
                context: contentDbContext,
                userService: userService.Object);

            var result = await publicationService
                .ListPublications(publication1.Theme.Id);

            var publicationViewModelList = result.AssertRight();

            var publicationViewModel = Assert.Single(publicationViewModelList);

            Assert.Equal(publication1.Id, publicationViewModel.Id);
            Assert.Equal(publication1.Title, publicationViewModel.Title);
            Assert.Equal(publication1.Summary, publicationViewModel.Summary);
            Assert.Equal(publication1.Slug, publicationViewModel.Slug);
            Assert.Equal(publication1.Theme.Id, publicationViewModel.Theme.Id);
            Assert.Equal(publication1.Theme.Title, publicationViewModel.Theme.Title);

            Assert.Null(publicationViewModel.SupersededById);
            Assert.False(publicationViewModel.IsSuperseded);

            Assert.Null(publicationViewModel.Permissions);
        }
    }

    [Fact]
    public async Task ListPublications_CanViewAllPublications_Order()
    {
        var theme = new Theme();

        var publication1 = new Publication
        {
            Title = "A",
            Theme = theme,
        };

        var publication2 = new Publication
        {
            Title = "B",
            Theme = theme,
        };

        var publication3 = new Publication
        {
            Title = "C",
            Theme = theme,
        };

        var userService = new Mock<IUserService>(Strict);

        userService.Setup(s => s.GetUserId()).Returns(Guid.NewGuid());
        userService.Setup(s => s.MatchesPolicy(RegisteredUser)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(true);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(publication2, publication3, publication1);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(
                context: contentDbContext,
                userService: userService.Object);

            var result = await publicationService
                .ListPublications(theme.Id);

            var publicationViewModelList = result.AssertRight();

            Assert.Equal(3, publicationViewModelList.Count);
            Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
            Assert.Equal(publication1.Title, publicationViewModelList[0].Title);

            Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
            Assert.Equal(publication2.Title, publicationViewModelList[1].Title);

            Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
            Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
        }
    }

    [Fact]
    public async Task ListPublications_CanViewAllPublications_NoTheme()
    {
        var publication1 = new Publication
        {
            Title = "publication1",
            Theme = new(),
        };

        var publication2 = new Publication
        {
            Title = "publication2",
            Theme = new(),
        };

        var publication3 = new Publication
        {
            Title = "publication3",
            Theme = new(),
        };

        var userService = new Mock<IUserService>(Strict);

        userService.Setup(s => s.GetUserId()).Returns(Guid.NewGuid());
        userService.Setup(s => s.MatchesPolicy(RegisteredUser)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(true);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.AddRangeAsync(publication1, publication2, publication3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(
                context: contentDbContext,
                userService: userService.Object);

            var result = await publicationService
                .ListPublications();

            var publicationViewModelList = result.AssertRight();

            Assert.Equal(3, publicationViewModelList.Count);
            Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
            Assert.Equal(publication1.Title, publicationViewModelList[0].Title);
            Assert.Equal(publication1.ThemeId, publicationViewModelList[0].Theme.Id);

            Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
            Assert.Equal(publication2.Title, publicationViewModelList[1].Title);
            Assert.Equal(publication2.ThemeId, publicationViewModelList[1].Theme.Id);

            Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
            Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
            Assert.Equal(publication3.ThemeId, publicationViewModelList[2].Theme.Id);
        }
    }

    [Fact]
    public async Task ListPublications_CannotViewAllPublications()
    {
        var user = new User { Id = Guid.NewGuid(), };

        var theme = new Theme
        {
            Title = "Theme title",
        };

        var publication1 = new Publication
        {
            Title = "Test Publication",
            Summary = "Test summary",
            Slug = "test-slug",
            Theme = theme,
            SupersededBy = null,
        };

        var publication2 = new Publication
        {
            Theme = new(),
        };

        var publication3 = new Publication
        {
            Theme = new(),
        };

        var userService = new Mock<IUserService>(Strict);

        userService.Setup(s => s.GetUserId()).Returns(user.Id);
        userService.Setup(s => s.MatchesPolicy(RegisteredUser)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(false);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddRangeAsync(publication1, publication2, publication3);
            await contentDbContext.UserPublicationRoles.AddRangeAsync(
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication1,
                    Role = PublicationRole.Owner,
                },
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication2,
                    Role = PublicationRole.Owner,
                },
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication3,
                    Role = PublicationRole.Owner,
                });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(
                context: contentDbContext,
                userService: userService.Object);

            var result = await publicationService
                .ListPublications(theme.Id);

            var publicationViewModelList = result.AssertRight();

            var publicationViewModel = Assert.Single(publicationViewModelList);

            Assert.Equal(publication1.Id, publicationViewModel.Id);
            Assert.Equal(publication1.Title, publicationViewModel.Title);
            Assert.Equal(publication1.Summary, publicationViewModel.Summary);
            Assert.Equal(publication1.Slug, publicationViewModel.Slug);
            Assert.Equal(publication1.Theme.Id, publicationViewModel.Theme.Id);
            Assert.Equal(publication1.Theme.Title, publicationViewModel.Theme.Title);

            Assert.Null(publicationViewModel.SupersededById);
            Assert.False(publicationViewModel.IsSuperseded);

            Assert.Null(publicationViewModel.Permissions);
        }
    }

    [Fact]
    public async Task ListPublications_CannotViewAllPublications_Order()
    {
        var user = new User { Id = Guid.NewGuid(), };

        var theme = new Theme();

        var publication1 = new Publication
        {
            Title = "A",
            Theme = theme,
        };

        var publication2 = new Publication
        {
            Title = "B",
            Theme = theme,
        };

        var publication3 = new Publication
        {
            Title = "C",
            Theme = theme,
        };

        var userService = new Mock<IUserService>(Strict);

        userService.Setup(s => s.GetUserId()).Returns(user.Id);
        userService.Setup(s => s.MatchesPolicy(RegisteredUser)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(false);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddRangeAsync(publication2, publication3, publication1);
            await contentDbContext.UserPublicationRoles.AddRangeAsync(
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication1,
                    Role = PublicationRole.Owner,
                },
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication2,
                    Role = PublicationRole.Owner,
                },
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication3,
                    Role = PublicationRole.Owner,
                });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(
                context: contentDbContext,
                userService: userService.Object);

            var result = await publicationService
                .ListPublications(theme.Id);

            var publicationViewModelList = result.AssertRight();

            Assert.Equal(3, publicationViewModelList.Count);

            Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
            Assert.Equal(publication1.Title, publicationViewModelList[0].Title);

            Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
            Assert.Equal(publication2.Title, publicationViewModelList[1].Title);

            Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
            Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
        }
    }

    [Fact]
    public async Task ListPublications_CannotViewAllPublications_NoTheme()
    {
        var user = new User { Id = Guid.NewGuid(), };

        var publication1 = new Publication
        {
            Title = "publication1",
            Theme = new(),
        };

        var publication2 = new Publication
        {
            Title = "publication2",
            Theme = new(),
        };

        var publication3 = new Publication
        {
            Title = "publication3",
            Theme = new(),
        };

        var publication4 = new Publication
        {
            Title = "publication4",
            Theme = new(),
        };

        var userService = new Mock<IUserService>(Strict);

        userService.Setup(s => s.GetUserId()).Returns(user.Id);
        userService.Setup(s => s.MatchesPolicy(RegisteredUser)).ReturnsAsync(true);
        userService.Setup(s => s.MatchesPolicy(CanViewAllPublications)).ReturnsAsync(false);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddRangeAsync(
                publication1,
                publication2,
                publication3,
                publication4);
            await contentDbContext.UserPublicationRoles.AddRangeAsync(
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication1,
                    Role = PublicationRole.Owner,
                },
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication2,
                    Role = PublicationRole.Owner,
                },
                new UserPublicationRole
                {
                    User = user,
                    Publication = publication3,
                    Role = PublicationRole.Owner,
                });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(
                context: contentDbContext,
                userService: userService.Object);

            var result = await publicationService
                .ListPublications();

            var publicationViewModelList = result.AssertRight();

            Assert.Equal(3, publicationViewModelList.Count);

            Assert.Equal(publication1.Id, publicationViewModelList[0].Id);
            Assert.Equal(publication1.Title, publicationViewModelList[0].Title);
            Assert.Equal(publication1.ThemeId, publicationViewModelList[0].Theme.Id);

            Assert.Equal(publication2.Id, publicationViewModelList[1].Id);
            Assert.Equal(publication2.Title, publicationViewModelList[1].Title);
            Assert.Equal(publication2.ThemeId, publicationViewModelList[1].Theme.Id);

            Assert.Equal(publication3.Id, publicationViewModelList[2].Id);
            Assert.Equal(publication3.Title, publicationViewModelList[2].Title);
            Assert.Equal(publication3.ThemeId, publicationViewModelList[2].Theme.Id);
        }
    }

    [Fact]
    public async Task GetPublication()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Summary = "Test summary",
            Slug = "test-publication",
            Theme = new Theme
            {
                Title = "Test theme"
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = (await publicationService.GetPublication(publication.Id)).AssertRight();

            Assert.Equal(publication.Id, result.Id);
            Assert.Equal(publication.Title, result.Title);
            Assert.Equal(publication.Summary, result.Summary);
            Assert.Equal(publication.Slug, result.Slug);

            Assert.Equal(publication.Theme.Id, result.Theme.Id);
            Assert.Equal(publication.Theme.Title, result.Theme.Title);

            Assert.Null(result.SupersededById);
            Assert.False(result.IsSuperseded);

            Assert.Null(result.Permissions);
        }
    }

    [Fact]
    public async Task GetPublication_Permissions()
    {
        var publication = new Publication
        {
            Theme = new Theme(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var userService = new Mock<IUserService>(Strict);

            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanViewSpecificPublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanUpdateSpecificPublicationSummary))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanUpdatePublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanUpdateContact))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanCreateReleaseForSpecificPublication))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanAdoptMethodologyForSpecificPublication))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanCreateMethodologyForSpecificPublication))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanManageExternalMethodologyForSpecificPublication))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanManagePublicationReleaseSeries))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanUpdateContact))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Tuple<Publication, ReleaseRole>>(tuple =>
                        tuple.Item1.Id == publication.Id && tuple.Item2 == ReleaseRole.Contributor),
                    CanUpdateSpecificReleaseRole))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(
                    It.Is<Publication>(p => p.Id == publication.Id),
                    CanViewReleaseTeamAccess))
                .ReturnsAsync(false);

            var publicationService = BuildPublicationService(context,
                userService: userService.Object);

            var result = (await publicationService.GetPublication(publication.Id, includePermissions: true))
                .AssertRight();

            Assert.Equal(publication.Id, result.Id);

            Assert.NotNull(result.Permissions);
            Assert.True(result.Permissions!.CanUpdatePublication);
            Assert.True(result.Permissions.CanUpdatePublicationSummary);
            Assert.False(result.Permissions.CanCreateReleases);
            Assert.False(result.Permissions.CanAdoptMethodologies);
            Assert.False(result.Permissions.CanCreateMethodologies);
            Assert.False(result.Permissions.CanManageExternalMethodology);
            Assert.True(result.Permissions.CanManageReleaseSeries);
            Assert.True(result.Permissions.CanUpdateContact);
            Assert.False(result.Permissions.CanUpdateContributorReleaseRole);
        }
    }

    [Fact]
    public async Task GetPublication_IsSuperseded()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Theme = new Theme(),
            SupersededBy = new Publication
            {
                LatestPublishedReleaseVersionId = Guid.NewGuid()
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = (await publicationService.GetPublication(publication.Id, includePermissions: true))
                .AssertRight();

            Assert.Equal(publication.Id, result.Id);

            Assert.Equal(publication.SupersededById, result.SupersededById);
            Assert.True(result.IsSuperseded);
        }
    }

    [Fact]
    public async Task GetPublication_IsSuperseded_False()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Theme = new Theme(),
            SupersededBy = new Publication
            {
                // Superseding publication doesn't have a published release
                LatestPublishedReleaseVersionId = null
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = (await publicationService.GetPublication(publication.Id, includePermissions: true))
                .AssertRight();

            Assert.Equal(publication.Id, result.Id);

            Assert.Equal(publication.SupersededById, result.SupersededById);
            Assert.False(result.IsSuperseded);
        }
    }

    [Fact]
    public async Task GetPublication_NotFound()
    {
        await using var context = InMemoryApplicationDbContext();

        var publicationService = BuildPublicationService(context);

        var result = await publicationService.GetPublication(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task ListPublicationSummaries()
    {
        var publication1 = new Publication
        {
            Title = "Test Publication 1"
        };

        var publication2 = new Publication
        {
            Title = "Test Publication 2"
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddRangeAsync(publication1, publication2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildPublicationService(contentDbContext);

            var result = await service.ListPublicationSummaries();

            var publicationViewModels = result.AssertRight();
            Assert.Equal(2, publicationViewModels.Count);

            Assert.Equal(publication1.Id, publicationViewModels[0].Id);
            Assert.Equal(publication1.Title, publicationViewModels[0].Title);

            Assert.Equal(publication2.Id, publicationViewModels[1].Id);
            Assert.Equal(publication2.Title, publicationViewModels[1].Title);
        }
    }

    [Fact]
    public async Task ListPublicationSummaries_NoPublications()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
        var service = BuildPublicationService(contentDbContext);

        var result = await service.ListPublicationSummaries();

        var publicationViewModels = result.AssertRight();
        Assert.Empty(publicationViewModels);
    }

    [Fact]
    public async Task CreatePublication()
    {
        var theme = new Theme
        {
            Title = "Test theme",
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            // Service method under test
            var result = await publicationService.CreatePublication(
                new PublicationCreateRequest
                {
                    Title = "Test publication",
                    Summary = "Test summary",
                    Contact = new ContactSaveRequest
                    {
                        ContactName = "John Smith",
                        ContactTelNo = "0123456789",
                        TeamName = "Test team",
                        TeamEmail = "john.smith@test.com",
                    },
                    ThemeId = theme.Id
                }
            );

            var publicationViewModel = result.AssertRight();
            Assert.Equal("Test publication", publicationViewModel.Title);
            Assert.Equal("Test summary", publicationViewModel.Summary);

            Assert.Equal("John Smith", publicationViewModel.Contact.ContactName);
            Assert.Equal("0123456789", publicationViewModel.Contact.ContactTelNo);
            Assert.Equal("Test team", publicationViewModel.Contact.TeamName);
            Assert.Equal("john.smith@test.com", publicationViewModel.Contact.TeamEmail);

            Assert.Equal(theme.Id, publicationViewModel.Theme.Id);
            Assert.Equal(theme.Title, publicationViewModel.Theme.Title);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var createdPublication = await context.Publications
                .Include(p => p.Contact)
                .Include(p => p.Theme)
                .FirstAsync(p => p.Title == "Test publication");

            Assert.NotNull(createdPublication);
            Assert.False(createdPublication.Live);
            Assert.Equal("test-publication", createdPublication.Slug);
            Assert.False(createdPublication.Updated.HasValue);
            Assert.Equal("Test publication", createdPublication.Title);
            Assert.Equal("Test summary", createdPublication.Summary);

            Assert.Equal("John Smith", createdPublication.Contact.ContactName);
            Assert.Equal("0123456789", createdPublication.Contact.ContactTelNo);
            Assert.Equal("Test team", createdPublication.Contact.TeamName);
            Assert.Equal("john.smith@test.com", createdPublication.Contact.TeamEmail);

            Assert.Equal(theme.Id, createdPublication.ThemeId);
            Assert.Equal("Test theme", createdPublication.Theme.Title);
        }
    }

    [Fact]
    public async Task CreatePublication_NoContactTelNo()
    {
        var theme = new Theme
        {
            Title = "Test theme",
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            // Service method under test
            var result = await publicationService.CreatePublication(
                new PublicationCreateRequest
                {
                    Title = "Test publication",
                    Summary = "Test summary",
                    Contact = new ContactSaveRequest
                    {
                        ContactName = "John Smith",
                        ContactTelNo = "",
                        TeamName = "Test team",
                        TeamEmail = "john.smith@test.com",
                    },
                    ThemeId = theme.Id
                }
            );

            var publicationViewModel = result.AssertRight();
            Assert.Equal("Test publication", publicationViewModel.Title);

            Assert.Equal("John Smith", publicationViewModel.Contact.ContactName);
            Assert.Null(publicationViewModel.Contact.ContactTelNo);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var createdPublication = await context.Publications
                .Include(p => p.Contact)
                .FirstAsync(p =>
                    p.Title == "Test publication");
            Assert.Equal("John Smith", createdPublication.Contact.ContactName);
            Assert.Null(createdPublication.Contact.ContactTelNo);
        }
    }

    [Fact]
    public async Task CreatePublication_FailsWithNonExistingTheme()
    {
        await using var context = InMemoryApplicationDbContext();

        var publicationService = BuildPublicationService(context);

        // Service method under test
        var result = await publicationService.CreatePublication(
            new PublicationCreateRequest
            {
                Title = "Test publication",
                ThemeId = Guid.NewGuid()
            });

        result.AssertBadRequest(ThemeDoesNotExist);
    }

    [Fact]
    public async Task CreatePublication_FailsWithNonUniqueSlug()
    {
        var theme = new Theme
        {
            Title = "Test theme"
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            context.Add(
                new Publication
                {
                    Title = "Test publication",
                    Slug = "test-publication"
                }
            );

            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            // Service method under test
            var result = await publicationService.CreatePublication(
                new PublicationCreateRequest
                {
                    Title = "Test publication",
                    ThemeId = theme.Id
                }
            );

            result.AssertBadRequest(PublicationSlugNotUnique);
        }
    }

    [Fact]
    public async Task UpdatePublication_NotPublished()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithTheme(_dataFixture.DefaultTheme());

        Theme otherTheme = _dataFixture.DefaultTheme();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            context.Themes.Add(otherTheme);
            await context.SaveChangesAsync();
        }

        var methodologyService = new Mock<IMethodologyService>(Strict);
        methodologyService
            .Setup(s => s.PublicationTitleOrSlugChanged(
                publication.Id,
                publication.Slug,
                "New title",
                "new-title"))
            .Returns(Task.CompletedTask);

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context,
                methodologyService: methodologyService.Object);

            // Service method under test
            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "New title",
                    Slug = "new-title",
                    Summary = "New summary",
                    ThemeId = otherTheme.Id,
                    SupersededById = null
                }
            );

            VerifyAllMocks(methodologyService);

            var viewModel = result.AssertRight();

            Assert.Equal("New title", viewModel.Title);
            Assert.Equal("New summary", viewModel.Summary);

            Assert.Equal(otherTheme.Id, viewModel.Theme.Id);
            Assert.Equal(otherTheme.Title, viewModel.Theme.Title);

            Assert.Null(viewModel.SupersededById);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var updatedPublication = await context.Publications
                .Include(p => p.Contact)
                .Include(p => p.Theme)
                .SingleAsync(p => p.Id == publication.Id);

            Assert.False(updatedPublication.Live);
            updatedPublication.Updated.AssertUtcNow();
            Assert.Equal("new-title", updatedPublication.Slug);
            Assert.Equal("New title", updatedPublication.Title);
            Assert.Null(updatedPublication.SupersededById);

            Assert.Equal(publication.Contact.ContactName, updatedPublication.Contact.ContactName);
            Assert.Equal(publication.Contact.ContactTelNo, updatedPublication.Contact.ContactTelNo);
            Assert.Equal(publication.Contact.TeamName, updatedPublication.Contact.TeamName);
            Assert.Equal(publication.Contact.TeamEmail, updatedPublication.Contact.TeamEmail);

            Assert.Equal(otherTheme.Id, updatedPublication.ThemeId);
            Assert.Equal(otherTheme.Title, updatedPublication.Theme.Title);

            var publicationRedirects = await context.PublicationRedirects.ToListAsync();
            Assert.Empty(publicationRedirects);
        }

        _publicationCacheServiceMockBuilder.Assert.CacheNotInvalidatedForPublicationTree();
        _publicationCacheServiceMockBuilder.Assert.CacheNotInvalidatedForPublicationEntry("new-title");
        _publicationCacheServiceMockBuilder.Assert.CacheNotInvalidatedForPublicationAndReleases(publication.Slug);
        AssertOnPublicationChangedEventsNotRaised();
    }

    [Fact]
    public async Task UpdatePublication_AlreadyPublished()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)])
            .WithTheme(_dataFixture.DefaultTheme());

        Theme otherTheme = _dataFixture.DefaultTheme();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            context.Themes.Add(otherTheme);
            await context.SaveChangesAsync();
        }

        var methodologyService = new Mock<IMethodologyService>(Strict);
        methodologyService
            .Setup(s => s.PublicationTitleOrSlugChanged(
                publication.Id,
                publication.Slug,
                "New title",
                "new-title"))
            .Returns(Task.CompletedTask);

        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>([]));

        var redirectsCacheService = new Mock<IRedirectsCacheService>(Strict);
        redirectsCacheService.Setup(mock => mock.UpdateRedirects())
            .ReturnsAsync(new RedirectsViewModel(
                PublicationRedirects: [],
                MethodologyRedirects: [],
                ReleaseRedirectsByPublicationSlug: []));

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context,
                methodologyService: methodologyService.Object,
                methodologyCacheService: methodologyCacheService.Object,
                redirectsCacheService: redirectsCacheService.Object);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "New title",
                    Slug = "new-title",
                    Summary = "New summary",
                    ThemeId = otherTheme.Id,
                    SupersededById = null
                }
            );

            VerifyAllMocks(
                methodologyService,
                methodologyCacheService,
                redirectsCacheService);

            var viewModel = result.AssertRight();

            Assert.Equal("New title", viewModel.Title);
            Assert.Equal("New summary", viewModel.Summary);

            Assert.Equal(otherTheme.Id, viewModel.Theme.Id);
            Assert.Equal(otherTheme.Title, viewModel.Theme.Title);

            Assert.Null(viewModel.SupersededById);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var updatedPublication = await context.Publications
                .Include(p => p.Contact)
                .Include(p => p.Theme)
                .SingleAsync(p => p.Id == publication.Id);

            Assert.True(updatedPublication.Live);
            updatedPublication.Updated.AssertUtcNow();
            Assert.Equal("new-title", updatedPublication.Slug);
            Assert.Equal("New title", updatedPublication.Title);
            Assert.Null(updatedPublication.SupersededById);

            Assert.Equal(publication.Contact.ContactName, updatedPublication.Contact.ContactName);
            Assert.Equal(publication.Contact.ContactTelNo, updatedPublication.Contact.ContactTelNo);
            Assert.Equal(publication.Contact.TeamName, updatedPublication.Contact.TeamName);
            Assert.Equal(publication.Contact.TeamEmail, updatedPublication.Contact.TeamEmail);

            Assert.Equal(otherTheme.Id, updatedPublication.ThemeId);
            Assert.Equal(otherTheme.Title, updatedPublication.Theme.Title);

            var publicationRedirects = await context.PublicationRedirects
                .ToListAsync();

            var publicationRedirect = Assert.Single(publicationRedirects);

            Assert.Equal(publication.Slug, publicationRedirect.Slug);
            Assert.Equal(publication.Id, publicationRedirect.PublicationId);

            AssertOnPublicationChangedEventRaised(updatedPublication);
        }

        _publicationCacheServiceMockBuilder.Assert.CacheInvalidatedForPublicationTree();
        _publicationCacheServiceMockBuilder.Assert.CacheInvalidatedForPublicationEntry("new-title");
        _publicationCacheServiceMockBuilder.Assert.CacheInvalidatedForPublicationAndReleases(publication.Slug);
    }

    [Theory]
    [MemberData(nameof(PublicationServiceTestsTheoryData.PublicationArchivedEventTestData),
        MemberType = typeof(PublicationServiceTestsTheoryData))]
    public async Task UpdatePublication_RaisesEventsDependentOnLiveAndArchivedStatus(
        Func<Publication> initialPublicationGenerator,
        Func<Publication?> initialPublicationSupersededByGenerator,
        Func<Publication?> updatedPublicationSupersededByGenerator,
        bool expectPublicationArchivedEventRaised)
    {
        // Generate the initial publication being updated (live or not live)
        var publication = initialPublicationGenerator();

        // Generate the initial publication's `SupersededBy` publication (null, live, or not live)
        publication.SupersededBy = initialPublicationSupersededByGenerator();

        // Generate the new `SupersededBy` publication being set in the update (null, live, or not live)
        var newSupersededBy = updatedPublicationSupersededByGenerator();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            if (newSupersededBy != null)
            {
                context.Publications.Add(newSupersededBy);
            }

            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context,
                methodologyCacheService: new Mock<IMethodologyCacheService>(Default).Object,
                redirectsCacheService: new Mock<IRedirectsCacheService>(Default).Object);

            await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = publication.Title,
                    Slug = publication.Slug,
                    Summary = publication.Summary,
                    ThemeId = publication.ThemeId,
                    SupersededById = newSupersededBy?.Id
                }
            );
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var updatedPublication = await context.Publications
                .SingleAsync(p => p.Id == publication.Id);

            if (expectPublicationArchivedEventRaised)
            {
                AssertOnPublicationArchivedEventRaised(updatedPublication);
            }
            else
            {
                AssertOnPublicationChangedEventsNotRaised();
            }
        }
    }

    [Fact]
    public async Task UpdatePublication_CacheInvalidatedForSupersededPublications()
    {
        Theme theme = _dataFixture.DefaultTheme();

        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)])
            .WithTheme(theme);

        // These publications are superseded by the publication being updated
        var (supersededPublication1, supersededPublication2) = _dataFixture
            .DefaultPublication()
            .WithSupersededBy(publication)
            .WithTheme(theme)
            .GenerateTuple2();

        // This is another publication not superseded by the publication being updated
        Publication notSupersededPublication = _dataFixture
            .DefaultPublication();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Themes.Add(theme);
            context.Publications.AddRange(publication,
                supersededPublication1,
                supersededPublication2,
                notSupersededPublication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context,
                methodologyCacheService: new Mock<IMethodologyCacheService>(Default).Object,
                redirectsCacheService: new Mock<IRedirectsCacheService>(Default).Object);

            await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = publication.Title,
                    Slug = publication.Slug,
                    Summary = publication.Summary,
                    ThemeId = publication.ThemeId,
                    SupersededById = null
                }
            );
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var updatedPublication = await context.Publications
                .SingleAsync(p => p.Id == publication.Id);

            // Ensure the cache was only invalidated for the updated and superseded publications
            Publication[] expectedInvalidatedPublications =
                [updatedPublication, supersededPublication1, supersededPublication2];
            foreach (var expected in expectedInvalidatedPublications)
            {
                _publicationCacheServiceMockBuilder.Assert.CacheInvalidatedForPublicationEntry(expected.Slug);
            }

            _publicationCacheServiceMockBuilder.Assert.CacheNotInvalidatedForPublicationEntry(
                notSupersededPublication.Slug);
        }
    }

    [Fact]
    public async Task UpdatePublication_TitleChangesPublicationAndMethodologySlug()
    {
        var publication = new Publication
        {
            Slug = "old-title",
            Title = "Old title",
            Theme = new Theme(),
            Contact = new Contact
            {
                ContactName = "Old name",
                ContactTelNo = "0987654321",
                TeamName = "Old team",
                TeamEmail = "old.smith@test.com",
            },
            LatestPublishedReleaseVersion = new ReleaseVersion(),
        };

        var methodologyVersionId = Guid.NewGuid();
        var publicationMethodology = new PublicationMethodology
        {
            Publication = publication,
            Owner = true,
            Methodology = new Methodology
            {
                LatestPublishedVersionId = methodologyVersionId,
                Versions = ListOf(new MethodologyVersion
                {
                    Id = methodologyVersionId,
                }),
                OwningPublicationTitle = "Old title",
                OwningPublicationSlug = "old-title",
            }
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.PublicationMethodologies.AddRange(publicationMethodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            var redirectsCacheService = new Mock<IRedirectsCacheService>(Strict);

            methodologyService.Setup(mock =>
                    mock.ValidateMethodologySlug("new-title", null, null))
                .ReturnsAsync(Unit.Instance);

            methodologyService
                .Setup(s => s.PublicationTitleOrSlugChanged(
                    publication.Id,
                    publication.Slug,
                    "New title",
                    "new-title"))
                .Returns(Task.CompletedTask);

            methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                    new List<AllMethodologiesThemeViewModel>()));

            publicationCacheService.Setup(mock => mock.UpdatePublication("new-title"))
                .ReturnsAsync(new PublicationCacheViewModel());

            publicationCacheService.Setup(mock => mock.UpdatePublicationTree())
                .ReturnsAsync(new List<PublicationTreeThemeViewModel>());

            publicationCacheService.Setup(mock => mock.RemovePublication("old-title"))
                .ReturnsAsync(Unit.Instance);

            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var publicationService = BuildPublicationService(context,
                methodologyService: methodologyService.Object,
                publicationCacheService: publicationCacheService.Object,
                methodologyCacheService: methodologyCacheService.Object,
                redirectsCacheService: redirectsCacheService.Object);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    ThemeId = publication.ThemeId,
                    Title = "New title",
                    Summary = "New summary",
                }
            );

            VerifyAllMocks(methodologyService,
                methodologyCacheService,
                publicationCacheService);

            var viewModel = result.AssertRight();

            Assert.Equal("New title", viewModel.Title);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var updatedPublication = await context.Publications
                .Include(p => p.Contact)
                .Include(p => p.Theme)
                .SingleAsync(p => p.Title == "New title");

            Assert.True(updatedPublication.Live);
            Assert.True(updatedPublication.Updated.HasValue);
            Assert.InRange(DateTime.UtcNow.Subtract(updatedPublication.Updated!.Value).Milliseconds, 0, 1500);

            Assert.Equal("new-title", updatedPublication.Slug);
            Assert.Equal("New title", updatedPublication.Title);

            // We don't check whether methodology titles/slugs have changed, because this is done by
            // PublicationTitleOrSlugChanged, which has been mocked.

            var publicationRedirects = context.PublicationRedirects.ToList();
            var redirect = Assert.Single(publicationRedirects);
            Assert.Equal(publication.Id, redirect.PublicationId);
            Assert.Equal("old-title", redirect.Slug);
            AssertOnPublicationChangedEventRaised(updatedPublication);
        }
    }

    [Fact]
    public async Task UpdatePublication_NoTitleOrSupersededByChange()
    {
        var theme = new Theme
        {
            Title = "theme",
        };

        var publication = new Publication
        {
            Title = "Old title",
            Summary = "Old summary",
            Slug = "old-title",
            Theme = new Theme
            {
                Title = "Old theme"
            },
            Contact = new Contact
            {
                ContactName = "Old name",
                ContactTelNo = "0987654321",
                TeamName = "Old team",
                TeamEmail = "old.smith@test.com",
            },
            SupersededById = Guid.NewGuid(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            context.Add(publication);

            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            // Expect no calls to be made on this Mock as the Publication's Title and Slug haven't changed.
            var methodologyService = new Mock<IMethodologyService>(Strict);

            var publicationService = BuildPublicationService(context,
                methodologyService: methodologyService.Object);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "Old title",
                    Summary = "New summary",
                    ThemeId = theme.Id,
                    SupersededById = publication.SupersededById,
                }
            );

            VerifyAllMocks(methodologyService);

            var viewModel = result.AssertRight();

            Assert.Equal("Old title", viewModel.Title);
            Assert.Equal("New summary", viewModel.Summary);
            Assert.Equal(publication.SupersededById, viewModel.SupersededById);
        }

        AssertOnPublicationChangedEventsNotRaised();
    }

    [Fact]
    public async Task UpdatePublication_RemovesSupersededPublicationCacheBlobs()
    {
        var publication = new Publication
        {
            Title = "Test title",
            Slug = "test-slug",
            Theme = new Theme
            {
                Title = "Test theme",
            },
            LatestPublishedReleaseVersion = new ReleaseVersion()
        };

        var supersededPublication1 = new Publication
        {
            Title = "Superseded title 1",
            Slug = "superseded-slug-1",
            SupersededBy = publication,
        };

        var supersededPublication2 = new Publication
        {
            Title = "Superseded title 2",
            Slug = "superseded-slug-2",
            SupersededBy = publication,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.AddRange(
                publication,
                supersededPublication1,
                supersededPublication2);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication(supersededPublication1.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication(supersededPublication2.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            publicationCacheService.Setup(mock => mock.UpdatePublicationTree())
                .ReturnsAsync(new List<PublicationTreeThemeViewModel>());

            methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                .ReturnsAsync(
                    new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                        new List<AllMethodologiesThemeViewModel>()));

            var publicationService = BuildPublicationService(context,
                publicationCacheService: publicationCacheService.Object,
                methodologyCacheService: methodologyCacheService.Object);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "Test title",
                    Slug = "test-slug",
                    ThemeId = publication.ThemeId,
                }
            );

            VerifyAllMocks(methodologyCacheService,
                publicationCacheService,
                publicationCacheService);

            var viewModel = result.AssertRight();
            Assert.Equal(publication.Id, viewModel.Id);
            Assert.Equal("Test title", viewModel.Title);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var updatedPublication = await context.Publications.FirstAsync(p => p.Title == "Test title");
            Assert.NotNull(updatedPublication);
        }
    }

    [Fact]
    public async Task UpdatePublication_FailsWithNonExistingTheme()
    {
        var publication = new Publication
        {
            Title = "Test publication",
            Theme = new Theme
            {
                Title = "Test theme"
            },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            // Service method under test
            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "Test publication",
                    ThemeId = Guid.NewGuid(),
                }
            );

            result.AssertBadRequest(ThemeDoesNotExist);
        }

        AssertOnPublicationChangedEventsNotRaised();
    }

    [Fact]
    public async Task UpdatePublication_CreateRedirectIfLiveSlugChanged()
    {
        var theme = new Theme
        {
            Title = "Theme title",
        };
        var publication = new Publication
        {
            Title = "Current title",
            Slug = "current-title",
            Theme = theme,
            LatestPublishedReleaseVersionId = Guid.NewGuid(),
        };
        var olderRedirect = new PublicationRedirect
        {
            Publication = publication,
            Slug = "old-title",
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.PublicationRedirects.AddAsync(olderRedirect);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            methodologyService.Setup(mock =>
                    mock.ValidateMethodologySlug("new-title", null, null))
                .ReturnsAsync(Unit.Instance);
            methodologyService.Setup(mock =>
                    mock.PublicationTitleOrSlugChanged(
                        publication.Id, "current-title", "New title", "new-title"))
                .Returns(Task.CompletedTask);

            var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
            methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                .ReturnsAsync(
                    new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                        new List<AllMethodologiesThemeViewModel>()));

            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock => mock.UpdatePublicationTree())
                .ReturnsAsync(new List<PublicationTreeThemeViewModel>());
            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication("new-title"))
                .ReturnsAsync(new PublicationCacheViewModel());
            publicationCacheService.Setup(mock => mock.RemovePublication("current-title"))
                .ReturnsAsync(Unit.Instance);

            var redirectsCacheService = new Mock<IRedirectsCacheService>(Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var publicationService = BuildPublicationService(context,
                methodologyService: methodologyService.Object,
                methodologyCacheService: methodologyCacheService.Object,
                publicationCacheService: publicationCacheService.Object,
                redirectsCacheService: redirectsCacheService.Object);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "New title",
                    ThemeId = theme.Id,
                }
            );

            var viewModel = result.AssertRight();
            Assert.Equal("New title", viewModel.Title);
            Assert.Equal("new-title", viewModel.Slug);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var dbPublication = context.Publications
                .Single(p => p.Id == publication.Id);
            Assert.Equal("New title", dbPublication.Title);
            Assert.Equal("new-title", dbPublication.Slug);

            var publicationRedirects = context.PublicationRedirects.ToList();
            Assert.Equal(2, publicationRedirects.Count);

            Assert.Equal(publication.Id, publicationRedirects[0].PublicationId);
            Assert.Equal("old-title", publicationRedirects[0].Slug);

            Assert.Equal(publication.Id, publicationRedirects[1].PublicationId);
            Assert.Equal("current-title", publicationRedirects[1].Slug);
        }
    }

    [Fact]
    public async Task UpdatePublication_ChangeBackToPreviousLiveSlug()
    {
        var theme = new Theme
        {
            Title = "Theme title",
        };
        var publication = new Publication
        {
            Title = "Title",
            Slug = "title",
            Theme = theme,
            LatestPublishedReleaseVersionId = Guid.NewGuid(),
        };
        var olderRedirect = new PublicationRedirect
        {
            Publication = publication,
            Slug = "older-title",
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.PublicationRedirects.AddAsync(olderRedirect);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            methodologyService.Setup(mock =>
                    mock.ValidateMethodologySlug("older-title", null, null))
                .ReturnsAsync(Unit.Instance);
            methodologyService.Setup(mock =>
                    mock.PublicationTitleOrSlugChanged(
                        publication.Id, "title", "Older title", "older-title"))
                .Returns(Task.CompletedTask);

            var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
            methodologyCacheService.Setup(mock => mock.UpdateSummariesTree())
                .ReturnsAsync(
                    new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                        new List<AllMethodologiesThemeViewModel>()));

            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock => mock.UpdatePublicationTree())
                .ReturnsAsync(new List<PublicationTreeThemeViewModel>());
            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication("older-title"))
                .ReturnsAsync(new PublicationCacheViewModel());
            publicationCacheService.Setup(mock => mock.RemovePublication("title"))
                .ReturnsAsync(Unit.Instance);

            var redirectsCacheService = new Mock<IRedirectsCacheService>(Strict);
            redirectsCacheService.Setup(mock => mock.UpdateRedirects())
                .ReturnsAsync(new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []));

            var publicationService = BuildPublicationService(context,
                methodologyService: methodologyService.Object,
                methodologyCacheService: methodologyCacheService.Object,
                publicationCacheService: publicationCacheService.Object,
                redirectsCacheService: redirectsCacheService.Object);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "Older title",
                    ThemeId = theme.Id,
                }
            );

            var viewModel = result.AssertRight();
            Assert.Equal("Older title", viewModel.Title);
            Assert.Equal("older-title", viewModel.Slug);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var dbPublication = context.Publications
                .Single(p => p.Id == publication.Id);
            Assert.Equal("Older title", dbPublication.Title);
            Assert.Equal("older-title", dbPublication.Slug);

            var publicationRedirects = context.PublicationRedirects.ToList();
            var redirect = Assert.Single(publicationRedirects);

            // As the current slug is now "older-title", the "older-title" redirect is redundant
            // and so is removed

            Assert.Equal(publication.Id, redirect.PublicationId);
            Assert.Equal("title", redirect.Slug);
        }
    }

    [Fact]
    public async Task UpdatePublication_OtherPublicationHasSlug()
    {
        var theme = new Theme
        {
            Title = "Theme title"
        };
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
            Theme = theme,
        };
        var otherPublication = new Publication
        {
            Title = "Duplicated title",
            Slug = "duplicated-title",
            Theme = theme,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Add(publication);
            context.Add(otherPublication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            // Service method under test
            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "Duplicated title",
                    ThemeId = theme.Id,
                }
            );

            result.AssertBadRequest(PublicationSlugNotUnique);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            // check there are no changes
            var dbPublication = context.Publications
                .Single(p => p.Id == publication.Id);
            Assert.Equal("Test publication", dbPublication.Title);
            Assert.Equal("test-publication", dbPublication.Slug);

            var publicationRedirects = context.PublicationRedirects.ToList();
            Assert.Empty(publicationRedirects);
        }
    }

    [Fact]
    public async Task UpdatePublication_SlugUsedByRedirect()
    {
        var theme = new Theme
        {
            Title = "Theme title"
        };
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
            Theme = theme,
        };
        var publicationRedirect = new PublicationRedirect
        {
            Slug = "duplicated-title",
            Publication = new Publication(),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.Publications.AddAsync(publication);
            await context.PublicationRedirects.AddAsync(publicationRedirect);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "Duplicated title",
                    ThemeId = theme.Id,
                }
            );

            result.AssertBadRequest(PublicationSlugUsedByRedirect);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            // check there are no changes
            var dbPublication = context.Publications
                .Single(p => p.Id == publication.Id);
            Assert.Equal("Test publication", dbPublication.Title);
            Assert.Equal("test-publication", dbPublication.Slug);

            var publicationRedirects = context.PublicationRedirects.ToList();
            var redirect = Assert.Single(publicationRedirects);
            Assert.Equal(publicationRedirect.PublicationId, redirect.PublicationId);
            Assert.Equal(publicationRedirect.Slug, redirect.Slug);
        }
    }

    [Fact]
    public async Task UpdatePublication_MethodologyInheritedNewSlugAlreadyUsed()
    {
        var theme = new Theme
        {
            Title = "Theme title"
        };
        var publication = new Publication
        {
            Title = "Test publication",
            Slug = "test-publication",
            Theme = theme,
        };
        var methodology = new Methodology
        {
            OwningPublicationSlug = "test-publication",
            Versions = new List<MethodologyVersion>
            {
                new()
                {
                    Version = 0,
                }
            }
        };
        var publicationMethodology = new PublicationMethodology
        {
            Publication = publication,
            Methodology = methodology,
            Owner = true,
        };

        var otherMethodology = new Methodology
        {
            OwningPublicationSlug = "already-used-methodology"
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.PublicationMethodologies.Add(publicationMethodology);
            context.Methodologies.Add(otherMethodology);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService.Setup(mock =>
                    mock.ValidateMethodologySlug("already-used-by-methodology", null, null))
                .ReturnsAsync(ValidationUtils.ValidationActionResult(MethodologySlugNotUnique));

            var publicationService = BuildPublicationService(context,
                methodologyService: methodologyService.Object);

            var result = await publicationService.UpdatePublication(
                publication.Id,
                new PublicationSaveRequest
                {
                    Title = "Already used by methodology",
                    ThemeId = theme.Id,
                }
            );

            result.AssertBadRequest(MethodologySlugNotUnique);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            // check there are no changes
            var dbPublication = context.Publications
                .Single(p => p.Id == publication.Id);
            Assert.Equal("Test publication", dbPublication.Title);
            Assert.Equal("test-publication", dbPublication.Slug);

            var publicationRedirects = context.PublicationRedirects.ToList();
            Assert.Empty(publicationRedirects);

            var dbMethodology = context.Methodologies
                .Include(m => m.Versions)
                .Single(m => m.Id == methodology.Id);
            Assert.Equal("test-publication", dbMethodology.Versions[0].Slug);
        }
    }

    [Fact]
    public async Task GetExternalMethodology()
    {
        var publication = new Publication
        {
            ExternalMethodology = new ExternalMethodology
            {
                Title = "Test external methodology",
                Url = "http://test.external.methodology",
            }
        };

        var contentDbContext = InMemoryApplicationDbContext();
        await contentDbContext.Publications.AddAsync(publication);
        await contentDbContext.SaveChangesAsync();

        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.GetExternalMethodology(publication.Id);
        var externalMethodology = result.AssertRight();

        Assert.Equal(publication.ExternalMethodology.Title, externalMethodology.Title);
        Assert.Equal(publication.ExternalMethodology.Url, externalMethodology.Url);
    }

    [Fact]
    public async Task GetExternalMethodology_NoPublication()
    {
        var contentDbContext = InMemoryApplicationDbContext();

        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.GetExternalMethodology(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task GetExternalMethodology_NoExternalMethodology()
    {
        var publication = new Publication
        {
            ExternalMethodology = null,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = BuildPublicationService(context: contentDbContext);

            var result = await service.GetExternalMethodology(publication.Id);
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateExternalMethodology()
    {
        var publication = new Publication
        {
            ExternalMethodology = new ExternalMethodology
            {
                Title = "Original external methodology",
                Url = "http://test.external.methodology/original",
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(s => s.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var service = BuildPublicationService(context: contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var result = await service.UpdateExternalMethodology(
                publication.Id,
                new ExternalMethodologySaveRequest
                {
                    Title = "New external methodology",
                    Url = "http://test.external.methodology/new",
                });

            VerifyAllMocks(publicationCacheService);

            var externalMethodology = result.AssertRight();

            Assert.Equal("New external methodology", externalMethodology.Title);
            Assert.Equal("http://test.external.methodology/new", externalMethodology.Url);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbPublication = contentDbContext.Publications
                .Single(p => p.Id == publication.Id);

            Assert.NotNull(dbPublication.ExternalMethodology);
            Assert.Equal("New external methodology", dbPublication.ExternalMethodology!.Title);
            Assert.Equal("http://test.external.methodology/new", dbPublication.ExternalMethodology.Url);
        }
    }

    [Fact]
    public async Task UpdateExternalMethodology_NoPublication()
    {
        var contentDbContext = InMemoryApplicationDbContext();
        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.UpdateExternalMethodology(
            publicationId: Guid.NewGuid(),
            new ExternalMethodologySaveRequest
            {
                Title = "New external methodology",
                Url = "http://test.external.methodology/new",
            });

        result.AssertNotFound();
    }

    [Fact]
    public async Task RemoveExternalMethodology()
    {
        var publication = new Publication
        {
            ExternalMethodology = new ExternalMethodology
            {
                Title = "Original external methodology",
                Url = "http://test.external.methodology/original",
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(s => s.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());
            var service = BuildPublicationService(context: contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var result = await service.RemoveExternalMethodology(publication.Id);

            VerifyAllMocks(publicationCacheService);

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbPublication = contentDbContext.Publications
                .Single(p => p.Id == publication.Id);

            Assert.Null(dbPublication.ExternalMethodology);
        }
    }

    [Fact]
    public async Task RemoveExternalMethodology_NoPublication()
    {
        var contentDbContext = InMemoryApplicationDbContext();
        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.RemoveExternalMethodology(publicationId: Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetContact()
    {
        var publication = new Publication
        {
            Contact = new Contact
            {
                ContactName = "contact name",
                ContactTelNo = "12345",
                TeamName = "team name",
                TeamEmail = "team@email.com",
            },
        };

        var contentDbContext = InMemoryApplicationDbContext();
        await contentDbContext.Publications.AddAsync(publication);
        await contentDbContext.SaveChangesAsync();

        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.GetContact(publication.Id);
        var contact = result.AssertRight();

        Assert.Equal(publication.Contact.ContactName, contact.ContactName);
        Assert.Equal(publication.Contact.ContactTelNo, contact.ContactTelNo);
        Assert.Equal(publication.Contact.TeamName, contact.TeamName);
        Assert.Equal(publication.Contact.TeamEmail, contact.TeamEmail);
    }

    [Fact]
    public async Task GetContact_NoContact()
    {
        var publication = new Publication();

        var contentDbContext = InMemoryApplicationDbContext();
        await contentDbContext.Publications.AddAsync(publication);
        await contentDbContext.SaveChangesAsync();

        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.GetContact(publication.Id);
        result.AssertNotFound();
    }

    [Fact]
    public async Task GetContact_NoPublication()
    {
        var contentDbContext = InMemoryApplicationDbContext();

        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.GetContact(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateContact()
    {
        var publication = new Publication
        {
            Contact = new Contact
            {
                ContactName = "contact name",
                ContactTelNo = "12345",
                TeamName = "team name",
                TeamEmail = "team@email.com",
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(s => s.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var service = BuildPublicationService(context: contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var updatedContact = new ContactSaveRequest
            {
                ContactName = "new contact name",
                ContactTelNo = "12345 6789",
                TeamName = "new team name",
                TeamEmail = "new_team@email.com",
            };

            var result = await service.UpdateContact(publication.Id, updatedContact);
            var contact = result.AssertRight();

            Assert.Equal(updatedContact.ContactName, contact.ContactName);
            Assert.Equal(updatedContact.ContactTelNo, contact.ContactTelNo);
            Assert.Equal(updatedContact.TeamName, contact.TeamName);
            Assert.Equal(updatedContact.TeamEmail, contact.TeamEmail);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbPublication = await contentDbContext.Publications
                .Include(p => p.Contact)
                .SingleAsync(p => p.Id == publication.Id);

            Assert.Equal("new contact name", dbPublication.Contact.ContactName);
            Assert.Equal("12345 6789", dbPublication.Contact.ContactTelNo);
            Assert.Equal("new team name", dbPublication.Contact.TeamName);
            Assert.Equal("new_team@email.com", dbPublication.Contact.TeamEmail);
        }
    }

    [Fact]
    public async Task UpdateContact_NoContactTelNo()
    {
        var publication = new Publication
        {
            Contact = new Contact
            {
                ContactName = "contact name",
                ContactTelNo = "12345",
                TeamName = "team name",
                TeamEmail = "team@email.com",
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(s => s.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var service = BuildPublicationService(context: contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var updatedContact = new ContactSaveRequest
            {
                ContactName = "new contact name",
                ContactTelNo = "",
                TeamName = "new team name",
                TeamEmail = "new_team@email.com",
            };

            var result = await service.UpdateContact(publication.Id, updatedContact);
            var contact = result.AssertRight();

            Assert.Equal(updatedContact.ContactName, contact.ContactName);
            Assert.Null(contact.ContactTelNo);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbPublication = await contentDbContext.Publications
                .Include(p => p.Contact)
                .SingleAsync(p => p.Id == publication.Id);

            Assert.Equal("new contact name", dbPublication.Contact.ContactName);
            Assert.Null(dbPublication.Contact.ContactTelNo);
        }
    }

    [Fact]
    public async Task UpdateContact_SharedContact()
    {
        var sharedContact = new Contact
        {
            ContactName = "contact name",
            ContactTelNo = "12345",
            TeamName = "team name",
            TeamEmail = "team@email.com",
        };

        var publication1 = new Publication
        {
            Contact = sharedContact,
        };

        var publication2 = new Publication
        {
            Contact = sharedContact,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddRangeAsync(publication1, publication2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(s => s.UpdatePublication(publication1.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var service = BuildPublicationService(context: contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var updatedContact = new ContactSaveRequest
            {
                ContactName = "new contact name",
                ContactTelNo = "12345 6789",
                TeamName = "new team name",
                TeamEmail = "new_team@email.com",
            };

            var result = await service.UpdateContact(publication1.Id, updatedContact);
            var contact = result.AssertRight();

            Assert.Equal(updatedContact.ContactName, contact.ContactName);
            Assert.Equal(updatedContact.ContactTelNo, contact.ContactTelNo);
            Assert.Equal(updatedContact.TeamName, contact.TeamName);
            Assert.Equal(updatedContact.TeamEmail, contact.TeamEmail);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var dbPublication1 = contentDbContext.Publications
                .Include(p => p.Contact)
                .Single(p => p.Id == publication1.Id);

            Assert.Equal("new contact name", dbPublication1.Contact.ContactName);
            Assert.Equal("12345 6789", dbPublication1.Contact.ContactTelNo);
            Assert.Equal("new team name", dbPublication1.Contact.TeamName);
            Assert.Equal("new_team@email.com", dbPublication1.Contact.TeamEmail);

            var dbPublication2 = contentDbContext.Publications
                .Include(p => p.Contact)
                .Single(p => p.Id == publication2.Id);

            Assert.Equal(sharedContact.ContactName, dbPublication2.Contact.ContactName);
            Assert.Equal(sharedContact.ContactTelNo, dbPublication2.Contact.ContactTelNo);
            Assert.Equal(sharedContact.TeamName, dbPublication2.Contact.TeamName);
            Assert.Equal(sharedContact.TeamEmail, dbPublication2.Contact.TeamEmail);
        }
    }

    [Fact]
    public async Task UpdateContact_NoPublication()
    {
        var contentDbContext = InMemoryApplicationDbContext();

        var service = BuildPublicationService(context: contentDbContext);

        var result = await service.UpdateContact(
            Guid.NewGuid(), new ContactSaveRequest());
        result.AssertNotFound();
    }

    [Fact]
    public async Task ListReleaseVersions_Latest()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(ListOf<Release>(
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020),
                _dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                _dataFixture
                    .DefaultRelease(publishedVersions: 2, year: 2022)));

        var latestReleaseVersion2020 = publication.Releases
            .Single(r => r.Year == 2020)
            .Versions
            .Single(rv => rv.Version == 1);
        var latestReleaseVersion2021 = publication.Releases
            .Single(r => r.Year == 2021)
            .Versions
            .Single(rv => rv.Version == 0);
        var latestReleaseVersion2022 = publication.Releases
            .Single(r => r.Year == 2022)
            .Versions
            .Single(rv => rv.Version == 1);

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.AddRange(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.ListReleaseVersions(
                publication.Id,
                ReleaseVersionsType.Latest);

            var releaseVersions = result.AssertRight();

            Assert.Equal(
                [
                    latestReleaseVersion2022.Id,
                    latestReleaseVersion2021.Id,
                    latestReleaseVersion2020.Id
                ], 
                releaseVersions.Select(r => r.Id));
        }
    }

    [Fact]
    public async Task ListReleaseVersions_Latest_SingleRelease()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 1)
                .WithLabel("initial")
                .Generate(1));

        var releaseVersion = publication.Releases.Single().Versions.Single();

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.ListReleaseVersions(
                publication.Id,
                ReleaseVersionsType.Latest);

            var releases = result.AssertRight();

            var summaryViewModel = Assert.Single(releases);

            Assert.Equal(releaseVersion.Id, summaryViewModel.Id);
            Assert.Equal(releaseVersion.Release.Title, summaryViewModel.Title);
            Assert.Equal(releaseVersion.Release.Slug, summaryViewModel.Slug);
            Assert.Equal(releaseVersion.Type, summaryViewModel.Type);
            Assert.Equal(releaseVersion.Release.Id, summaryViewModel.ReleaseId);
            Assert.Equal(releaseVersion.Release.Year, summaryViewModel.Year);
            Assert.Equal(releaseVersion.Release.YearTitle, summaryViewModel.YearTitle);
            Assert.Equal(releaseVersion.Release.TimePeriodCoverage, summaryViewModel.TimePeriodCoverage);
            Assert.Equal(releaseVersion.Release.Label, summaryViewModel.Label);
            Assert.Equal(releaseVersion.Published, summaryViewModel.Published);
            Assert.Equal(releaseVersion.Live, summaryViewModel.Live);
            Assert.Equal(releaseVersion.PublishScheduled?.ConvertUtcToUkTimeZone(), summaryViewModel.PublishScheduled);
            Assert.Equal(releaseVersion.NextReleaseDate, summaryViewModel.NextReleaseDate);
            Assert.Equal(releaseVersion.ApprovalStatus, summaryViewModel.ApprovalStatus);
            Assert.Equal(releaseVersion.Amendment, summaryViewModel.Amendment);
            Assert.Equal(releaseVersion.PreviousVersionId, summaryViewModel.PreviousVersionId);
            Assert.Null(summaryViewModel.Permissions);
        }
    }

    [Fact]
    public async Task ListReleaseVersions_LatestPublished()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(ListOf<Release>(
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020),
                _dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2022)));

        var latestPublishedReleaseVersion2020 = publication.Releases
            .Single(r => r.Year == 2020)
            .Versions
            .Single(rv => rv.Version == 0);
        var latestPublishedReleaseVersion2022 = publication.Releases
            .Single(r => r.Year == 2022)
            .Versions
            .Single(rv => rv.Version == 0);

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.ListReleaseVersions(
                publication.Id,
                ReleaseVersionsType.LatestPublished);

            var releaseVersions = result.AssertRight();

            Assert.Equal(
                [
                    latestPublishedReleaseVersion2022.Id,
                    latestPublishedReleaseVersion2020.Id
                ], 
                releaseVersions.Select(r => r.Id));
        }
    }

    [Fact]
    public async Task ListReleaseVersions_OnlyDraft()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(ListOf<Release>(
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2020),
                _dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2022)));

        var latestDraftReleaseVersion2020 = publication.Releases
            .Single(r => r.Year == 2020)
            .Versions
            .Single(rv => rv.Version == 1);
        var latestDraftReleaseVersion2021 = publication.Releases
            .Single(r => r.Year == 2021)
            .Versions
            .Single(rv => rv.Version == 0);

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.ListReleaseVersions(
                publication.Id,
                ReleaseVersionsType.NotPublished);

            var releaseVersions = result.AssertRight();

            Assert.Equal(
                [
                    latestDraftReleaseVersion2021.Id,
                    latestDraftReleaseVersion2020.Id
                ], 
                releaseVersions.Select(r => r.Id));
        }
    }

    [Fact]
    public async Task ListReleaseVersions_Latest_IncludePermissionsTrue()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(1));

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.ListReleaseVersions(
                publicationId: publication.Id, 
                versionsType: ReleaseVersionsType.Latest,
                includePermissions: true);

            var releaseVersions = result.AssertRight();

            var releaseVersion = Assert.Single(releaseVersions);

            Assert.NotNull(releaseVersion.Permissions);
            Assert.True(releaseVersion.Permissions!.CanDeleteReleaseVersion);
            Assert.True(releaseVersion.Permissions!.CanUpdateRelease);
            Assert.True(releaseVersion.Permissions!.CanUpdateReleaseVersion);
            Assert.True(releaseVersion.Permissions!.CanAddPrereleaseUsers);
            Assert.True(releaseVersion.Permissions!.CanMakeAmendmentOfReleaseVersion);
        }
    }

    [Fact]
    public async Task ListReleaseVersions_Latest_IncludePermissionsFalse()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 0, draftVersion: true)
                .Generate(1));

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            var result = await publicationService.ListReleaseVersions(
                publicationId: publication.Id,
                versionsType: ReleaseVersionsType.Latest,
                includePermissions: false);

            var releaseVersions = result.AssertRight();

            var releaseVersion = Assert.Single(releaseVersions);

            Assert.Null(releaseVersion.Permissions);
        }
    }

    [Fact]
    public async Task ListReleaseVersionsPaginated_Latest()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_dataFixture
                .DefaultRelease(publishedVersions: 2)
                .Generate(4));

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            await context.SaveChangesAsync();
        }

        var expectedPagesAndYears = new Dictionary<int, int[]>
        {
            { 1, [2003, 2002] },
            { 2, [2001, 2000] }
        };
        var expectedTotalPages = expectedPagesAndYears.Count;
        var expectedTotalResults = expectedPagesAndYears.SelectMany(pair => pair.Value).Count();
        const int pageSize = 2;

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var publicationService = BuildPublicationService(context);

            foreach (var (page, years) in expectedPagesAndYears)
            {
                var result = await publicationService.ListReleaseVersionsPaginated(
                    publication.Id,
                    versionsType: ReleaseVersionsType.Latest,
                    page: page,
                    pageSize: pageSize
                );

                var pagedResult = result.AssertRight();

                Assert.Equal(page, pagedResult.Paging.Page);
                Assert.Equal(pageSize, pagedResult.Paging.PageSize);
                Assert.Equal(expectedTotalPages, pagedResult.Paging.TotalPages);
                Assert.Equal(expectedTotalResults, pagedResult.Paging.TotalResults);

                var expectedLatestReleaseVersionIds = years.Select(year =>
                        publication.Releases.Single(r => r.Year == year)
                            .Versions.Single(rv => rv.Version == 1).Id)
                    .ToArray();

                Assert.Equal(expectedLatestReleaseVersionIds, pagedResult.Results.Select(r => r.Id).ToArray());
            }
        }
    }

    [Fact]
    public async Task GetReleaseSeries()
    {
        ReleaseSeriesItem legacyLink = _dataFixture.DefaultLegacyReleaseSeriesItem();

        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2020),
                _dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                _dataFixture
                    .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2022)
            ])
            .WithLegacyLinks([legacyLink])
            .WithTheme(_dataFixture.DefaultTheme());

        var release2020 = publication.Releases.Single(r => r.Year == 2020);
        var release2021 = publication.Releases.Single(r => r.Year == 2021);
        var release2022 = publication.Releases.Single(r => r.Year == 2022);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(contentDbContext);

            var result = await publicationService.GetReleaseSeries(publication.Id);
            var viewModels = result.AssertRight();

            Assert.Equal(4, viewModels.Count);

            Assert.False(viewModels[0].IsLegacyLink);
            Assert.Equal(release2022.Title, viewModels[0].Description);
            Assert.Equal(release2022.Id, viewModels[0].ReleaseId);
            Assert.Equal(release2022.Slug, viewModels[0].ReleaseSlug);
            Assert.True(viewModels[0].IsLatest);
            Assert.True(viewModels[0].IsPublished);
            Assert.Null(viewModels[0].LegacyLinkUrl);

            Assert.False(viewModels[1].IsLegacyLink);
            Assert.Equal(release2021.Title, viewModels[1].Description);
            Assert.Equal(release2021.Id, viewModels[1].ReleaseId);
            Assert.Equal(release2021.Slug, viewModels[1].ReleaseSlug);
            Assert.False(viewModels[1].IsLatest);
            Assert.False(viewModels[1].IsPublished);
            Assert.Null(viewModels[1].LegacyLinkUrl);

            Assert.False(viewModels[2].IsLegacyLink);
            Assert.Equal(release2020.Title, viewModels[2].Description);
            Assert.Equal(release2020.Id, viewModels[2].ReleaseId);
            Assert.Equal(release2020.Slug, viewModels[2].ReleaseSlug);
            Assert.False(viewModels[2].IsLatest);
            Assert.True(viewModels[2].IsPublished);
            Assert.Null(viewModels[2].LegacyLinkUrl);

            Assert.True(viewModels[3].IsLegacyLink);
            Assert.Equal(legacyLink.LegacyLinkDescription, viewModels[3].Description);
            Assert.Null(viewModels[3].ReleaseId);
            Assert.Null(viewModels[3].ReleaseSlug);
            Assert.Null(viewModels[3].IsLatest);
            Assert.Null(viewModels[3].IsPublished);
            Assert.Equal(legacyLink.LegacyLinkUrl, viewModels[3].LegacyLinkUrl);
        }
    }

    [Fact]
    public async Task GetReleaseSeries_NoReleases()
    {
        var legacyLinks = _dataFixture.DefaultLegacyReleaseSeriesItem()
            .GenerateList(2);

        Publication publication = _dataFixture
            .DefaultPublication()
            .WithLegacyLinks(legacyLinks)
            .WithTheme(_dataFixture.DefaultTheme());

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(contentDbContext);

            var result = await publicationService.GetReleaseSeries(publication.Id);
            var viewModels = result.AssertRight();

            Assert.Equal(2, viewModels.Count);

            Assert.True(viewModels[0].IsLegacyLink);
            Assert.Equal(legacyLinks[0].LegacyLinkDescription, viewModels[0].Description);
            Assert.Null(viewModels[0].ReleaseId);
            Assert.Null(viewModels[0].ReleaseSlug);
            Assert.Null(viewModels[0].IsLatest);
            Assert.Null(viewModels[0].IsPublished);
            Assert.Equal(legacyLinks[0].LegacyLinkUrl, viewModels[0].LegacyLinkUrl);

            Assert.True(viewModels[1].IsLegacyLink);
            Assert.Equal(legacyLinks[1].LegacyLinkDescription, viewModels[1].Description);
            Assert.Null(viewModels[1].ReleaseId);
            Assert.Null(viewModels[1].ReleaseSlug);
            Assert.Null(viewModels[1].IsLatest);
            Assert.Null(viewModels[1].IsPublished);
            Assert.Equal(legacyLinks[1].LegacyLinkUrl, viewModels[1].LegacyLinkUrl);
        }
    }

    [Fact]
    public async Task GetReleaseSeries_Empty()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithTheme(_dataFixture.DefaultTheme());

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(contentDbContext);

            var result = await publicationService.GetReleaseSeries(publication.Id);
            var viewModels = result.AssertRight();

            Assert.Empty(viewModels);
        }
    }

    [Fact]
    public async Task AddReleaseSeriesLegacyLink()
    {
        ReleaseSeriesItem legacyLink = _dataFixture.DefaultLegacyReleaseSeriesItem();

        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
            .WithLegacyLinks([legacyLink])
            .WithTheme(_dataFixture.DefaultTheme());

        var release = publication.Releases.Single();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock => mock
                    .UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var publicationService = BuildPublicationService(
                contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var result = await publicationService.AddReleaseSeriesLegacyLink(
                publication.Id,
                new ReleaseSeriesLegacyLinkAddRequest
                {
                    Description = "New legacy link",
                    Url = "https://test.com/new"
                });

            var viewModels = result.AssertRight();
            VerifyAllMocks(publicationCacheService);

            Assert.Equal(3, viewModels.Count);

            Assert.False(viewModels[0].IsLegacyLink);
            Assert.Equal(release.Title, viewModels[0].Description);
            Assert.Equal(release.Id, viewModels[0].ReleaseId);
            Assert.Equal(release.Slug, viewModels[0].ReleaseSlug);
            Assert.True(viewModels[0].IsLatest);
            Assert.True(viewModels[0].IsPublished);
            Assert.Null(viewModels[0].LegacyLinkUrl);

            Assert.True(viewModels[1].IsLegacyLink);
            Assert.Equal(legacyLink.LegacyLinkDescription, viewModels[1].Description);
            Assert.Null(viewModels[1].ReleaseId);
            Assert.Null(viewModels[1].ReleaseSlug);
            Assert.Null(viewModels[1].IsLatest);
            Assert.Null(viewModels[1].IsPublished);
            Assert.Equal(legacyLink.LegacyLinkUrl, viewModels[1].LegacyLinkUrl);

            Assert.True(viewModels[2].IsLegacyLink);
            Assert.Equal("New legacy link", viewModels[2].Description);
            Assert.Null(viewModels[2].ReleaseId);
            Assert.Null(viewModels[2].ReleaseSlug);
            Assert.Null(viewModels[2].IsLatest);
            Assert.Null(viewModels[2].IsPublished);
            Assert.Equal("https://test.com/new", viewModels[2].LegacyLinkUrl);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualPublication = await contentDbContext.Publications
                .SingleAsync(p => p.Id == publication.Id);

            var actualReleaseSeries = actualPublication.ReleaseSeries;
            Assert.Equal(3, actualReleaseSeries.Count);

            Assert.Equal(release.Id, actualReleaseSeries[0].ReleaseId);

            Assert.Equal(legacyLink.LegacyLinkDescription, actualReleaseSeries[1].LegacyLinkDescription);
            Assert.Equal(legacyLink.LegacyLinkUrl, actualReleaseSeries[1].LegacyLinkUrl);

            Assert.Equal("New legacy link", actualReleaseSeries[2].LegacyLinkDescription);
            Assert.Equal("https://test.com/new", actualReleaseSeries[2].LegacyLinkUrl);
        }
    }

    [Fact]
    public async Task AddReleaseSeriesLegacyLink_AddToEmptySeries()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithTheme(_dataFixture.DefaultTheme());

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock => mock
                    .UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var publicationService = BuildPublicationService(
                contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var result = await publicationService.AddReleaseSeriesLegacyLink(
                publication.Id,
                new ReleaseSeriesLegacyLinkAddRequest
                {
                    Description = "New legacy link",
                    Url = "https://test.com/new"
                });

            VerifyAllMocks(publicationCacheService);

            var viewModels = result.AssertRight();

            var newSeriesItem = Assert.Single(viewModels);
            Assert.True(newSeriesItem.IsLegacyLink);
            Assert.Equal("New legacy link", newSeriesItem.Description);
            Assert.Null(newSeriesItem.ReleaseId);
            Assert.Null(newSeriesItem.ReleaseSlug);
            Assert.Null(newSeriesItem.IsLatest);
            Assert.Null(newSeriesItem.IsPublished);
            Assert.Equal("https://test.com/new", newSeriesItem.LegacyLinkUrl);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualPublication = await contentDbContext.Publications
                .SingleAsync(p => p.Id == publication.Id);

            var actualReleaseSeriesItem = Assert.Single(actualPublication.ReleaseSeries);

            Assert.Equal("New legacy link", actualReleaseSeriesItem.LegacyLinkDescription);
            Assert.Equal("https://test.com/new", actualReleaseSeriesItem.LegacyLinkUrl);
        }
    }

    [Fact]
    public async Task UpdateReleaseSeries()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2020),
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2021),
                _dataFixture
                    .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2022)
            ])
            .WithLegacyLinks([_dataFixture.DefaultLegacyReleaseSeriesItem()])
            .WithTheme(_dataFixture.DefaultTheme());

        var release2020 = publication.Releases.Single(r => r.Year == 2020);
        var release2021 = publication.Releases.Single(r => r.Year == 2021);
        var release2022 = publication.Releases.Single(r => r.Year == 2022);

        // Check the publication's latest published release version in the generated test data setup
        Assert.Equal(release2022.Versions[1].Id, publication.LatestPublishedReleaseVersionId);

        // Check the expected order of the release series items in the generated test data setup
        Assert.Equal(4, publication.ReleaseSeries.Count);
        Assert.Equal(release2022.Id, publication.ReleaseSeries[0].ReleaseId);
        Assert.Equal(release2021.Id, publication.ReleaseSeries[1].ReleaseId);
        Assert.Equal(release2020.Id, publication.ReleaseSeries[2].ReleaseId);
        Assert.True(publication.ReleaseSeries[3].IsLegacyLink);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var publicationService = BuildPublicationService(
                contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            // Let's swap the order of the oldest two releases to 2022, 2020, 2021 and insert a legacy link.
            var result = await publicationService.UpdateReleaseSeries(
                publication.Id,
                updatedReleaseSeriesItems:
                [
                    new ReleaseSeriesItemUpdateRequest
                    {
                        LegacyLinkDescription = "Legacy link new",
                        LegacyLinkUrl = "https://test.com/new"
                    },
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2022.Id },
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2020.Id },
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2021.Id }
                ]);

            VerifyAllMocks(publicationCacheService);

            var viewModels = result.AssertRight();
            Assert.Equal(4, viewModels.Count);

            Assert.True(viewModels[0].IsLegacyLink);
            Assert.Equal("Legacy link new", viewModels[0].Description);
            Assert.Null(viewModels[0].ReleaseId);
            Assert.Null(viewModels[0].ReleaseSlug);
            Assert.Null(viewModels[0].IsLatest);
            Assert.Null(viewModels[0].IsPublished);
            Assert.Equal("https://test.com/new", viewModels[0].LegacyLinkUrl);

            Assert.False(viewModels[1].IsLegacyLink);
            Assert.Equal(release2022.Title, viewModels[1].Description);
            Assert.Equal(release2022.Id, viewModels[1].ReleaseId);
            Assert.Equal(release2022.Slug, viewModels[1].ReleaseSlug);
            Assert.True(viewModels[1].IsLatest);
            Assert.True(viewModels[1].IsPublished);
            Assert.Null(viewModels[1].LegacyLinkUrl);

            Assert.False(viewModels[2].IsLegacyLink);
            Assert.Equal(release2020.Title, viewModels[2].Description);
            Assert.Equal(release2020.Id, viewModels[2].ReleaseId);
            Assert.Equal(release2020.Slug, viewModels[2].ReleaseSlug);
            Assert.False(viewModels[2].IsLatest);
            Assert.True(viewModels[2].IsPublished);
            Assert.Null(viewModels[2].LegacyLinkUrl);

            Assert.False(viewModels[3].IsLegacyLink);
            Assert.Equal(release2021.Title, viewModels[3].Description);
            Assert.Equal(release2021.Id, viewModels[3].ReleaseId);
            Assert.Equal(release2021.Slug, viewModels[3].ReleaseSlug);
            Assert.False(viewModels[3].IsLatest);
            Assert.True(viewModels[3].IsPublished);
            Assert.Null(viewModels[3].LegacyLinkUrl);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualPublication = await contentDbContext.Publications
                .SingleAsync(p => p.Id == publication.Id);

            var actualReleaseSeries = actualPublication.ReleaseSeries;
            Assert.Equal(4, actualReleaseSeries.Count);

            Assert.Equal("Legacy link new", actualReleaseSeries[0].LegacyLinkDescription);
            Assert.Equal("https://test.com/new", actualReleaseSeries[0].LegacyLinkUrl);

            Assert.Equal(release2022.Id, actualReleaseSeries[1].ReleaseId);
            Assert.Equal(release2020.Id, actualReleaseSeries[2].ReleaseId);
            Assert.Equal(release2021.Id, actualReleaseSeries[3].ReleaseId);

            // The publication's latest published release version should be unchanged as 2022 was positioned
            // as the first release after the legacy link
            Assert.Equal(release2022.Versions[1].Id, actualPublication.LatestPublishedReleaseVersionId);
        }
        
        // The latest release is unchanged so no events should have been raised
        AssertOnPublicationChangedEventsNotRaised();
    }

    [Fact]
    public async Task UpdateReleaseSeries_UpdatesLatestPublishedReleaseVersion()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2020),
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2021),
                _dataFixture
                    .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2022)
            ])
            .WithTheme(_dataFixture.DefaultTheme());

        var release2020 = publication.Releases.Single(r => r.Year == 2020);
        var release2021 = publication.Releases.Single(r => r.Year == 2021);
        var release2022 = publication.Releases.Single(r => r.Year == 2022);

        var originalLatestPublishedReleaseVersionId = release2022.Versions[1].Id;
        var expectedLatestPublishedReleaseVersionId = release2021.Versions[0].Id;

        // Check the publication's latest published release version in the generated test data setup
        Assert.Equal(originalLatestPublishedReleaseVersionId, publication.LatestPublishedReleaseVersionId);

        // Check the expected order of the release series items in the generated test data setup
        Assert.Equal(3, publication.ReleaseSeries.Count);
        Assert.Equal(release2022.Id, publication.ReleaseSeries[0].ReleaseId);
        Assert.Equal(release2021.Id, publication.ReleaseSeries[1].ReleaseId);
        Assert.Equal(release2020.Id, publication.ReleaseSeries[2].ReleaseId);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var releaseCacheService = new Mock<IReleaseCacheService>(Strict);
            releaseCacheService.Setup(mock => mock.UpdateRelease(
                    expectedLatestPublishedReleaseVersionId,
                    publication.Slug,
                    null))
                .ReturnsAsync(new ReleaseCacheViewModel(expectedLatestPublishedReleaseVersionId));

            var publicationService = BuildPublicationService(
                contentDbContext,
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);

            var result = await publicationService.UpdateReleaseSeries(
                publication.Id,
                updatedReleaseSeriesItems:
                [
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2021.Id },
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2020.Id },
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2022.Id }
                ]);

            VerifyAllMocks(publicationCacheService, releaseCacheService);

            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);

            Assert.Equal(release2021.Id, viewModels[0].ReleaseId);
            Assert.True(viewModels[0].IsLatest);
            Assert.True(viewModels[0].IsPublished);

            Assert.Equal(release2020.Id, viewModels[1].ReleaseId);
            Assert.False(viewModels[1].IsLatest);
            Assert.True(viewModels[1].IsPublished);

            Assert.Equal(release2022.Id, viewModels[2].ReleaseId);
            Assert.False(viewModels[2].IsLatest);
            Assert.True(viewModels[2].IsPublished);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualPublication = await contentDbContext.Publications
                .SingleAsync(p => p.Id == publication.Id);

            var actualReleaseSeries = actualPublication.ReleaseSeries;
            Assert.Equal(3, actualReleaseSeries.Count);

            Assert.Equal(release2021.Id, actualReleaseSeries[0].ReleaseId);
            Assert.Equal(release2020.Id, actualReleaseSeries[1].ReleaseId);
            Assert.Equal(release2022.Id, actualReleaseSeries[2].ReleaseId);

            // The latest published version of 2021 should now be the publication's latest published release
            // version since it was positioned as the first release
            Assert.Equal(expectedLatestPublishedReleaseVersionId,
                actualPublication.LatestPublishedReleaseVersionId);
            
            AssertOnPublicationLatestPublishedReleaseReorderedWasRaised(
                actualPublication,
                originalLatestPublishedReleaseVersionId);
        }
    }

    [Fact]
    public async Task UpdateReleaseSeries_UpdatesLatestPublishedReleaseVersion_SkipsUnpublishedReleases()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([
                _dataFixture
                    .DefaultRelease(publishedVersions: 1, year: 2020),
                _dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                _dataFixture
                    .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2022)
            ])
            .WithTheme(_dataFixture.DefaultTheme());

        var release2020 = publication.Releases.Single(r => r.Year == 2020);
        var release2021 = publication.Releases.Single(r => r.Year == 2021);
        var release2022 = publication.Releases.Single(r => r.Year == 2022);

        var originalLatestPublishedReleaseVersionId = release2022.Versions[1].Id;
        var expectedLatestPublishedReleaseVersionId = release2020.Versions[0].Id;

        // Check the publication's latest published release version in the generated test data setup
        Assert.Equal(originalLatestPublishedReleaseVersionId, publication.LatestPublishedReleaseVersionId);

        // Check the expected order of the release series items in the generated test data setup
        Assert.Equal(3, publication.ReleaseSeries.Count);
        Assert.Equal(release2022.Id, publication.ReleaseSeries[0].ReleaseId);
        Assert.Equal(release2021.Id, publication.ReleaseSeries[1].ReleaseId);
        Assert.Equal(release2020.Id, publication.ReleaseSeries[2].ReleaseId);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var releaseCacheService = new Mock<IReleaseCacheService>(Strict);
            releaseCacheService.Setup(mock => mock.UpdateRelease(
                    expectedLatestPublishedReleaseVersionId,
                    publication.Slug,
                    null))
                .ReturnsAsync(new ReleaseCacheViewModel(expectedLatestPublishedReleaseVersionId));

            var publicationService = BuildPublicationService(
                contentDbContext,
                publicationCacheService: publicationCacheService.Object,
                releaseCacheService: releaseCacheService.Object);

            var result = await publicationService.UpdateReleaseSeries(
                publication.Id,
                updatedReleaseSeriesItems:
                [
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2021.Id }, // Unpublished
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2020.Id },
                    new ReleaseSeriesItemUpdateRequest { ReleaseId = release2022.Id }
                ]);

            VerifyAllMocks(publicationCacheService, releaseCacheService);

            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);

            Assert.Equal(release2021.Id, viewModels[0].ReleaseId);
            Assert.False(viewModels[0].IsLatest);
            Assert.False(viewModels[0].IsPublished);

            Assert.Equal(release2020.Id, viewModels[1].ReleaseId);
            Assert.True(viewModels[1].IsLatest);
            Assert.True(viewModels[1].IsPublished);

            Assert.Equal(release2022.Id, viewModels[2].ReleaseId);
            Assert.False(viewModels[2].IsLatest);
            Assert.True(viewModels[2].IsPublished);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualPublication = await contentDbContext.Publications
                .SingleAsync(p => p.Id == publication.Id);

            var actualReleaseSeries = actualPublication.ReleaseSeries;
            Assert.Equal(3, actualReleaseSeries.Count);

            Assert.Equal(release2021.Id, actualReleaseSeries[0].ReleaseId);
            Assert.Equal(release2020.Id, actualReleaseSeries[1].ReleaseId);
            Assert.Equal(release2022.Id, actualReleaseSeries[2].ReleaseId);

            // The latest published version of 2020 should now be the publication's latest published release
            // version since it was positioned as the next release after 2021 which is unpublished
            Assert.Equal(expectedLatestPublishedReleaseVersionId,
                actualPublication.LatestPublishedReleaseVersionId);

            AssertOnPublicationLatestPublishedReleaseReorderedWasRaised(
                actualPublication,
                originalLatestPublishedReleaseVersionId);
        }
    }

    [Fact]
    public async Task UpdateReleaseSeries_SetEmpty()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithLegacyLinks([_dataFixture.DefaultLegacyReleaseSeriesItem()])
            .WithTheme(_dataFixture.DefaultTheme());

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationCacheService = new Mock<IPublicationCacheService>(Strict);
            publicationCacheService.Setup(mock =>
                    mock.UpdatePublication(publication.Slug))
                .ReturnsAsync(new PublicationCacheViewModel());

            var publicationService = BuildPublicationService(
                contentDbContext,
                publicationCacheService: publicationCacheService.Object);

            var result = await publicationService.UpdateReleaseSeries(
                publication.Id,
                updatedReleaseSeriesItems: []);

            VerifyAllMocks(publicationCacheService);

            var viewModels = result.AssertRight();

            Assert.Empty(viewModels);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualPublication = await contentDbContext.Publications
                .SingleAsync(p => p.Id == publication.Id);

            Assert.Empty(actualPublication.ReleaseSeries);
        }

        AssertOnPublicationChangedEventsNotRaised();
    }

    [Fact]
    public async Task UpdateReleaseSeries_UnsetRelease()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
            .WithTheme(_dataFixture.DefaultTheme());

        var release = publication.Releases.Single();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                publicationService.UpdateReleaseSeries(
                    publication.Id,
                    updatedReleaseSeriesItems: []));

            Assert.Equal($"Missing or duplicate release in new release series. Expected ReleaseIds: {release.Id}",
                exception.Message);
        }
    }

    [Fact]
    public async Task UpdateReleaseSeries_SetDuplicateRelease()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
            .WithTheme(_dataFixture.DefaultTheme());

        var release = publication.Releases.Single();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                publicationService.UpdateReleaseSeries(
                    publication.Id,
                    updatedReleaseSeriesItems:
                    [
                        new ReleaseSeriesItemUpdateRequest { ReleaseId = release.Id },
                        new ReleaseSeriesItemUpdateRequest { ReleaseId = release.Id }
                    ]));

            Assert.Equal($"Missing or duplicate release in new release series. Expected ReleaseIds: {release.Id}",
                exception.Message);
        }
    }

    [Fact]
    public async Task UpdateReleaseSeries_InvalidSeriesItem1()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
            .WithTheme(_dataFixture.DefaultTheme());

        var release = publication.Releases.Single();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                publicationService.UpdateReleaseSeries(
                    publication.Id,
                    [
                        new ReleaseSeriesItemUpdateRequest
                        {
                            ReleaseId = release.Id,
                            LegacyLinkDescription = "this should be null",
                            LegacyLinkUrl = "https://should.be/null",
                        }
                    ]));

            Assert.Equal("LegacyLink details shouldn't be set if ReleaseId is set.", exception.Message);
        }
    }

    [Fact]
    public async Task UpdateReleaseSeries_InvalidSeriesItem2()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithTheme(_dataFixture.DefaultTheme());

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Publications.Add(publication);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var publicationService = BuildPublicationService(contentDbContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                publicationService.UpdateReleaseSeries(
                    publication.Id,
                    [
                        new ReleaseSeriesItemUpdateRequest
                        {
                            ReleaseId = null,
                            LegacyLinkDescription = null,
                            LegacyLinkUrl = null,
                        }
                    ]));

            Assert.Equal("LegacyLink details should be set if ReleaseId is null.", exception.Message);
        }
    }

    private PublicationService BuildPublicationService(
        ContentDbContext context,
        IUserService? userService = null,
        IPublicationRepository? publicationRepository = null,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IMethodologyService? methodologyService = null,
        IPublicationCacheService? publicationCacheService = null,
        IReleaseCacheService? releaseCacheService = null,
        IMethodologyCacheService? methodologyCacheService = null,
        IRedirectsCacheService? redirectsCacheService = null)
    {
        return new(
            context,
            AdminMapper(),
            new PersistenceHelper<ContentDbContext>(context),
            userService ?? AlwaysTrueUserService().Object,
            publicationRepository ?? new PublicationRepository(context),
            releaseVersionRepository ?? new ReleaseVersionRepository(context),
            methodologyService ?? Mock.Of<IMethodologyService>(Strict),
            publicationCacheService ?? _publicationCacheServiceMockBuilder.Build(),
            releaseCacheService ?? Mock.Of<IReleaseCacheService>(Strict),
            methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
            redirectsCacheService ?? Mock.Of<IRedirectsCacheService>(Strict),
            _adminEventRaiserMockBuilder.Build());
    }

    private readonly AdminEventRaiserMockBuilder _adminEventRaiserMockBuilder = new();
    private readonly PublicationCacheServiceMockBuilder _publicationCacheServiceMockBuilder = new();

    private void AssertOnPublicationArchivedEventRaised(Publication publication) =>
        _adminEventRaiserMockBuilder.Assert.OnPublicationArchivedWasRaised(
            publication.Id,
            publication.Slug,
            publication.SupersededById);

    private void AssertOnPublicationChangedEventRaised(Publication publication) =>
        _adminEventRaiserMockBuilder.Assert.OnPublicationChangedWasRaised(publication);
    
    private void AssertOnPublicationLatestPublishedReleaseReorderedWasRaised(
        Publication publication,
        Guid previousReleaseVersionId) =>
        _adminEventRaiserMockBuilder.Assert.OnPublicationLatestPublishedReleaseReorderedWasRaised(
            publication,
            previousReleaseVersionId);

    private void AssertOnPublicationChangedEventsNotRaised()
    {
        _adminEventRaiserMockBuilder.Assert.AssertOnPublicationChangedEventsNotRaised();
    }
}
