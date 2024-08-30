#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
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
using Microsoft.Extensions.Configuration;
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
            var release1Version1 = new ReleaseVersion
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var release2Version1 = new ReleaseVersion
            {
                TimePeriodCoverage = TimeIdentifier.April,
                ReleaseName = "2001",
            };
            var publication1 = new Publication
            {
                Title = "Publication title",
                ReleaseVersions = ListOf(release1Version1, release2Version1)
            };

            var release3Version1 = new ReleaseVersion
            {
                TimePeriodCoverage = TimeIdentifier.January,
                ReleaseName = "2222",
            };
            var publication2 = new Publication
            {
                Title = "Ignored publication title",
                ReleaseVersions = ListOf(release3Version1),
            };

            var invite1 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersion = release1Version1,
                Role = Contributor,
            };
            var invite2 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersion = release2Version1,
                Role = Contributor,
            };
            var notRemoveInvite1 = new UserReleaseInvite
            {
                Email = "notRemoved@test.com", // different email
                ReleaseVersion = release1Version1,
                Role = Contributor,
            };
            var notRemoveInvite2 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersion = release3Version1, // release under different publication
                Role = Contributor,
            };
            var notRemoveInvite3 = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersion = release1Version1,
                Role = Lead, // different role
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication1, publication2, invite1, invite2,
                    notRemoveInvite1, notRemoveInvite2, notRemoveInvite3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext);

                var result = await service.RemoveByPublication(
                    email: "test@test.com",
                    publicationId: publication1.Id,
                    releaseRole: Contributor);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(3, userReleaseInvites.Count);

                Assert.Equal("notRemoved@test.com", userReleaseInvites[0].Email);
                Assert.Equal(release1Version1.Id, userReleaseInvites[0].ReleaseVersionId);
                Assert.Equal(Contributor, userReleaseInvites[0].Role);

                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(release3Version1.Id, userReleaseInvites[1].ReleaseVersionId);
                Assert.Equal(Contributor, userReleaseInvites[1].Role);

                Assert.Equal("test@test.com", userReleaseInvites[2].Email);
                Assert.Equal(release1Version1.Id, userReleaseInvites[2].ReleaseVersionId);
                Assert.Equal(Lead, userReleaseInvites[2].Role);
            }
        }

        [Fact]
        public async Task RemoveByPublication_NoPublication()
        {
            var invite = new UserReleaseInvite
            {
                Email = "test@test.com",
                ReleaseVersion = new ReleaseVersion(),
                Role = Contributor,
            };
            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(invite);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseInviteService(
                    contentDbContext: contentDbContext);

                var result = await service.RemoveByPublication(
                    email: "test@test.com",
                    publicationId: Guid.NewGuid(),
                    releaseRole: Contributor);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Single(userReleaseInvites);

                Assert.Equal(invite.Email, userReleaseInvites[0].Email);
                Assert.Equal(invite.ReleaseVersionId, userReleaseInvites[0].ReleaseVersionId);
                Assert.Equal(invite.Role, userReleaseInvites[0].Role);
            }
        }

        [Fact]
        public async Task RemoveByPublication_NoUserReleaseInvites()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
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
                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(userReleaseInvites);
            }
        }

        private static Dictionary<string, dynamic> GetExpectedContributorInviteTemplateValues(string publicationTitle,
            string releaseList)
        {
            return new()
            {
                { "url", "https://localhost/" },
                { "publication name", publicationTitle },
                { "release list", releaseList },
            };
        }

        private static Mock<IConfiguration> DefaultConfigurationMock()
        {
            return CreateMockConfiguration(
                TupleOf("NotifyContributorTemplateId", NotifyContributorTemplateId),
                TupleOf("AdminUri", "localhost"));
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
            IUserReleaseRoleRepository? userReleaseRoleRepository = null,
            IConfiguration? configuration = null,
            IEmailService? emailService = null)
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
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(contentDbContext),
                configuration ?? DefaultConfigurationMock().Object,
                emailService ?? Mock.Of<IEmailService>(Strict)
            );
        }
    }
}
