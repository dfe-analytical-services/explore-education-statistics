#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces.IReleaseVersionRepository;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.ReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseInviteServiceTests
    {
        private static readonly Guid CreatedById = Guid.NewGuid();
        private const string NotifyContributorTemplateId = "contributor-invite-template-id";

        private readonly DataFixture _dataFixture = new();

        [Fact]
        public async Task InviteContributor_CreateInvite()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(2));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2000/01\n* Academic year 2001/02");
            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id,
                        publication.ReleaseVersions[1].Id));

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        NotifyContributorTemplateId,
                        expectedTemplateValues
                    ), Times.Once
                );

                VerifyAllMocks(emailService);

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = await usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Single(userInvites);

                Assert.Equal("test@test.com", userInvites[0].Email);
                Assert.Equal(Role.Analyst.GetEnumValue(), userInvites[0].RoleId);
                Assert.Equal(CreatedById.ToString(), userInvites[0].CreatedById);
                Assert.False(userInvites[0].Accepted);
                Assert.InRange(DateTime.UtcNow.Subtract(userInvites[0].Created).Milliseconds, 0, 1500);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Equal(2, userReleaseInvites.Count);

                Assert.Equal("test@test.com", userReleaseInvites[0].Email);
                Assert.Equal(publication.ReleaseVersions[0].Id, userReleaseInvites[0].ReleaseVersionId);
                Assert.Equal(Contributor, userReleaseInvites[0].Role);
                Assert.Equal(CreatedById, userReleaseInvites[0].CreatedById);
                Assert.True(userReleaseInvites[0].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);

                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(publication.ReleaseVersions[1].Id, userReleaseInvites[1].ReleaseVersionId);
                Assert.Equal(Contributor, userReleaseInvites[1].Role);
                Assert.Equal(CreatedById, userReleaseInvites[1].CreatedById);
                Assert.True(userReleaseInvites[1].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[1].Created).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task InviteContributor_ExistingUser()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(2));

            var user = new User
            {
                Email = "test@test.com",
            };

            var existingUserReleaseRole = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.ReleaseVersions[0],
                Role = Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.Users.Add(user);
                contentDbContext.UserReleaseRoles.Add(existingUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2001/02");
            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(Unit.Instance);

            var userRoleService = new Mock<IUserRoleService>(Strict);
            userRoleService
                .Setup(mock =>
                    mock.GetAssociatedGlobalRoleNameForReleaseRole(Contributor))
                .Returns(RoleNames.Analyst);
            userRoleService
                .Setup(mock =>
                    mock.UpgradeToGlobalRoleIfRequired(RoleNames.Analyst, user.Id))
                .ReturnsAsync(Unit.Instance);

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    userRoleService: userRoleService.Object,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id,
                        publication.ReleaseVersions[1].Id));

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        NotifyContributorTemplateId,
                        expectedTemplateValues
                    ), Times.Once
                );

                VerifyAllMocks(userRoleService, emailService);

                result.AssertRight();
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = await usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(userInvites); // user already exists, so don't create a user invite
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(2, userReleaseRoles.Count); // as user already exists, create missing UserReleaseRoles

                Assert.Equal(existingUserReleaseRole.UserId, userReleaseRoles[0].UserId);
                Assert.Equal(existingUserReleaseRole.ReleaseVersionId, userReleaseRoles[0].ReleaseVersionId);
                Assert.Equal(existingUserReleaseRole.Role, userReleaseRoles[0].Role);
                Assert.Equal(existingUserReleaseRole.Created, userReleaseRoles[0].Created);
                Assert.Equal(existingUserReleaseRole.CreatedById, userReleaseRoles[0].CreatedById);

                Assert.Equal(user.Id, userReleaseRoles[1].UserId);
                Assert.Equal(publication.ReleaseVersions[1].Id, userReleaseRoles[1].ReleaseVersionId);
                Assert.Equal(Contributor, userReleaseRoles[1].Role);
                Assert.Equal(CreatedById, userReleaseRoles[1].CreatedById);
                userReleaseRoles[1].Created.AssertUtcNow();

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(userReleaseInvites); // no release invite created as user already exists
            }
        }

        [Fact]
        public async Task InviteContributor_UserAlreadyHasReleaseRoleInvites()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(2));

            var userRelease1Invite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersion = publication.ReleaseVersions[0],
                Role = Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var userRelease2Invite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersion = publication.ReleaseVersions[1],
                Role = Contributor,
                Created = new DateTime(2001, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.UserReleaseInvites.AddRange(userRelease1Invite, userRelease2Invite);
                await contentDbContext.SaveChangesAsync();
            }

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id,
                        publication.ReleaseVersions[1].Id));

                result.AssertBadRequest(UserAlreadyHasReleaseRoleInvites);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = await usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(userInvites);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(2, userReleaseInvites.Count);
                Assert.Equal(userRelease1Invite.Id, userReleaseInvites[0].Id);
                Assert.Equal(userRelease2Invite.Id, userReleaseInvites[1].Id);
            }
        }

        [Fact]
        public async Task InviteContributor_UserAlreadyHasReleaseRoles()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(2));

            var user = new User
            {
                Email = "test@test.com",
            };

            var userRelease1Role = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.ReleaseVersions[0],
                Role = Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var userRelease2Role = new UserReleaseRole
            {
                User = user,
                ReleaseVersion = publication.ReleaseVersions[1],
                Role = Contributor,
                Created = new DateTime(2001, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.Users.Add(user);
                contentDbContext.UserReleaseRoles.AddRange(userRelease1Role, userRelease2Role);
                await contentDbContext.SaveChangesAsync();
            }

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id,
                        publication.ReleaseVersions[1].Id));

                result.AssertBadRequest(UserAlreadyHasReleaseRoles);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = await usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(userInvites); // user already exists, so don't create a user invite
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(2, userReleaseRoles.Count);
                Assert.Equal(userRelease1Role.Id, userReleaseRoles[0].Id);
                Assert.Equal(userRelease2Role.Id, userReleaseRoles[1].Id);

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(userReleaseInvites); // only create release invite for missing UserReleaseRole
            }
        }

        [Fact]
        public async Task InviteContributor_NewUser_FailsSendingEmail()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2000/01");
            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id));

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        NotifyContributorTemplateId,
                        expectedTemplateValues
                    ), Times.Once
                );

                VerifyAllMocks(emailService);

                var actionResult = result.AssertLeft();
                Assert.IsType<BadRequestResult>(actionResult);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = await usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userInvites);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseInvites);
            }
        }

        [Fact]
        public async Task InviteContributor_ExistingUser_FailsSendingEmail()
        {
            var user = new User
            {
                Email = "test@test.com",
            };

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic year 2000/01");
            emailService.Setup(mock => mock.SendEmail(
                    "test@test.com",
                    NotifyContributorTemplateId,
                    expectedTemplateValues
                ))
                .Returns(new BadRequestResult());

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseVersionIds: ListOf(publication.ReleaseVersions[0].Id));

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        NotifyContributorTemplateId,
                        expectedTemplateValues
                    ), Times.Once
                );

                VerifyAllMocks(emailService);

                var actionResult = result.AssertLeft();
                Assert.IsType<BadRequestResult>(actionResult);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = await usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(userInvites); // user already exists, so don't create a user invite
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseInvites);
            }
        }

        [Fact]
        public async Task InviteContributor_NotAllReleasesBelongToPublication()
        {
            var (publication1, publication2) = _dataFixture
                .DefaultPublication()
                .WithReleases(_dataFixture
                    .DefaultRelease(publishedVersions: 0, draftVersion: true)
                    .Generate(1))
                .GenerateTuple2();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication1.Id,
                    releaseVersionIds: ListOf(publication1.ReleaseVersions[0].Id,
                        publication2.ReleaseVersions[0].Id));

                result.AssertBadRequest(NotAllReleasesBelongToPublication);
            }

            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            {
                var userInvites = await usersAndRolesDbContext.UserInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Empty(userInvites);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseRoles);

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseInvites);
            }
        }

        [Fact]
        public async Task RemoveByPublication()
        {
            Publication otherPublication = _dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ => [
                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true),
                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                ]);

            var userReleaseInvites = _dataFixture.DefaultUserReleaseInvite()
                .WithEmail("test@test.com")
                .WithRole(Contributor)
                .ForIndex(0, s => s.SetReleaseVersion(publication.Releases[0].Versions[0]))
                .ForRange(1..4, s => s.SetReleaseVersion(publication.Releases[1].Versions[0]))
                .ForIndex(2, s => s.SetEmail("notRemoved@test.com"))
                .ForIndex(3, s => s.SetRole(PrereleaseViewer))
                .ForIndex(4, s => s.SetReleaseVersion(otherPublication.Releases[0].Versions[0]))
                .GenerateList(5);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication, otherPublication);
                contentDbContext.UserReleaseInvites.AddRange(userReleaseInvites);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext);

                var result = await service.RemoveByPublication(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseRole: Contributor);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var actualUserReleaseInvites = await contentDbContext.UserReleaseInvites
                    .ToListAsync();

                Assert.Equal(3, actualUserReleaseInvites.Count);

                Assert.Equal(userReleaseInvites[2].Id, actualUserReleaseInvites[0].Id); // Different email
                Assert.Equal(userReleaseInvites[3].Id, actualUserReleaseInvites[1].Id); // Different role
                Assert.Equal(userReleaseInvites[4].Id, actualUserReleaseInvites[2].Id); // Different publication
            }
        }

        [Fact]
        public async Task RemoveByPublication_NoPublication()
        {
            UserReleaseInvite userReleaseInvite = _dataFixture.DefaultUserReleaseInvite()
                .WithReleaseVersion(_dataFixture.DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease()
                        .WithPublication(_dataFixture.DefaultPublication())))
                .WithRole(Contributor);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.UserReleaseInvites.Add(userReleaseInvite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext);

                var result = await service.RemoveByPublication(
                    email: userReleaseInvite.Email,
                    publicationId: Guid.NewGuid(),
                    releaseRole: userReleaseInvite.Role);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var actualUserReleaseInvites = await contentDbContext.UserReleaseInvites
                    .ToListAsync();
                Assert.Single(actualUserReleaseInvites);

                Assert.Equal(userReleaseInvite.Id, actualUserReleaseInvites[0].Id);
            }
        }

        [Fact]
        public async Task RemoveByPublication_NoUserReleaseInvites()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext);

                var result = await service.RemoveByPublication(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseRole: Contributor);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var actualUserReleaseInvites = await contentDbContext.UserReleaseInvites
                    .ToListAsync();
                Assert.Empty(actualUserReleaseInvites);
            }
        }

        private static Dictionary<string, dynamic> GetExpectedContributorInviteTemplateValues(string publicationTitle,
            string releaseList)
        {
            return new()
            {
                { "url", "https://localhost" },
                { "publication name", publicationTitle },
                { "release list", releaseList },
            };
        }

        private static IOptions<AppOptions> DefaultAppOptions()
        {
            return new AppOptions { Url = "https://localhost" }.ToOptionsWrapper();
        }

        private static IOptions<NotifyOptions> DefaultNotifyOptions()
        {
            return new NotifyOptions { ContributorTemplateId = NotifyContributorTemplateId }.ToOptionsWrapper();
        }

        private static ReleaseInviteService SetupReleaseInviteService(
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            IUserRepository? userRepository = null,
            IUserService? userService = null,
            IUserRoleService? userRoleService = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            IUserReleaseRoleAndInviteManager? userReleaseRoleRepository = null,
            IEmailService? emailService = null,
            IOptions<AppOptions>? appOptions = null,
            IOptions<NotifyOptions>? notifyOptions = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new ReleaseInviteService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
                userRepository ?? new UserRepository(contentDbContext),
                userService ?? AlwaysTrueUserService(CreatedById).Object,
                userRoleService ?? Mock.Of<IUserRoleService>(Strict),
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleManager(contentDbContext),
                emailService ?? Mock.Of<IEmailService>(Strict),
                appOptions ?? DefaultAppOptions(),
                notifyOptions ?? DefaultNotifyOptions()
            );
        }
    }
}
