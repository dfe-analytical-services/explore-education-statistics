#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class UserManagementServiceTests
    {
        private static readonly Guid _createdById = Guid.NewGuid();
        private const string NotifyContributorTemplateId = "contributor-invite-template-id";

        [Fact]
        public async Task GetUser()
        {
            var userId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                Id = userId.ToString(),
                Email = "test@test.com",
                FirstName = "Test",
                LastName = "Analyst"
            };

            var globalRoles = new List<RoleViewModel>
            {
                new RoleViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role 1",
                    NormalizedName = "ROLE 2"
                },
                new RoleViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Role 2",
                    NormalizedName = "ROLE 2"
                }
            };

            var publicationRoles = new List<UserPublicationRoleViewModel>
            {
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Role = Owner
                },
                new UserPublicationRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Role = Owner
                }
            };

            var releaseRoles = new List<UserReleaseRoleViewModel>
            {
                new UserReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 1",
                    Release = "December 2020",
                    Role = Contributor
                },
                new UserReleaseRoleViewModel
                {
                    Id = Guid.NewGuid(),
                    Publication = "Test Publication 2",
                    Release = "June 2021",
                    Role = Approver
                }
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userRoleService = new Mock<IUserRoleService>(Strict);

            userRoleService.Setup(mock =>
                mock.GetGlobalRoles(user.Id)).ReturnsAsync(globalRoles);

            userRoleService.Setup(mock =>
                mock.GetPublicationRoles(userId)).ReturnsAsync(publicationRoles);

            userRoleService.Setup(mock =>
                mock.GetReleaseRoles(userId)).ReturnsAsync(releaseRoles);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object);

                var result = await service.GetUser(userId);

                Assert.True(result.IsRight);
                var userViewModel = result.Right;

                Assert.Equal(userId, userViewModel.Id);
                Assert.Equal("test@test.com", userViewModel.Email);
                Assert.Equal("Test Analyst", userViewModel.Name);
                // Currently we only allow a user to have a maximum of one global role
                Assert.Equal(globalRoles[0].Id, userViewModel.Role);
                Assert.Equal(publicationRoles, userViewModel.UserPublicationRoles);
                Assert.Equal(releaseRoles, userViewModel.UserReleaseRoles);

                userRoleService.Verify(mock => mock.GetGlobalRoles(user.Id), Times.Once);
                userRoleService.Verify(mock => mock.GetPublicationRoles(userId), Times.Once);
                userRoleService.Verify(mock => mock.GetReleaseRoles(userId), Times.Once);
            }

            VerifyAllMocks(userRoleService);
        }

        [Fact]
        public async Task GetUser_NoUser()
        {
            var userRoleService = new Mock<IUserRoleService>(Strict);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext())
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object);

                var result = await service.GetUser(Guid.NewGuid());

                result.AssertNotFound();
            }

            VerifyAllMocks(userRoleService);
        }

        [Fact]
        public async Task ListPublications()
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
                var service = SetupUserManagementService(contentDbContext: contentDbContext);

                var result = await service.ListPublications();

                Assert.True(result.IsRight);

                var publicationViewModels = result.Right;
                Assert.Equal(2, publicationViewModels.Count);

                Assert.Equal(publication1.Id, publicationViewModels[0].Id);
                Assert.Equal(publication1.Title, publicationViewModels[0].Title);

                Assert.Equal(publication2.Id, publicationViewModels[1].Id);
                Assert.Equal(publication2.Title, publicationViewModels[1].Title);
            }
        }

        [Fact]
        public async Task InviteContributor_CreateInvite()
        {
            var release1 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var release2 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Releases = ListOf(release1, release2)
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic Year 2000/01\n* Academic Year 2001/02");
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
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseIds: ListOf(release1.Id, release2.Id));

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
                Assert.Equal(_createdById.ToString(), userInvites[0].CreatedById);
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
                Assert.Equal(release1.Id, userReleaseInvites[0].ReleaseId);
                Assert.Equal(Contributor, userReleaseInvites[0].Role);
                Assert.Equal(_createdById, userReleaseInvites[0].CreatedById);
                Assert.False(userReleaseInvites[0].Accepted);
                Assert.True(userReleaseInvites[0].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);

                Assert.Equal("test@test.com", userReleaseInvites[1].Email);
                Assert.Equal(release2.Id, userReleaseInvites[1].ReleaseId);
                Assert.Equal(Contributor, userReleaseInvites[1].Role);
                Assert.Equal(_createdById, userReleaseInvites[1].CreatedById);
                Assert.False(userReleaseInvites[1].Accepted);
                Assert.True(userReleaseInvites[1].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[1].Created).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task InviteContributor_ExistingUser()
        {
            var user = new User
            {
                Email = "test@test.com",
            };

            var release1 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var release2 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Releases = ListOf(release1, release2)
            };

            var existingUserReleaseRole = new UserReleaseRole
            {
                User = user,
                Release = release1,
                Role = Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(user, publication, existingUserReleaseRole);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic Year 2001/02");
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
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseIds: ListOf(release1.Id, release2.Id));

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

                Assert.Empty(userInvites); // user already exists, so don't create a user invite
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var userReleaseRoles = await contentDbContext.UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(2, userReleaseRoles.Count); // as user already exists, create missing UserReleaseRoles

                Assert.Equal(existingUserReleaseRole.UserId, userReleaseRoles[0].UserId);
                Assert.Equal(existingUserReleaseRole.ReleaseId, userReleaseRoles[0].ReleaseId);
                Assert.Equal(existingUserReleaseRole.Role, userReleaseRoles[0].Role);
                Assert.Equal(existingUserReleaseRole.Created, userReleaseRoles[0].Created);
                Assert.Equal(existingUserReleaseRole.CreatedById, userReleaseRoles[0].CreatedById);

                Assert.Equal(user.Id, userReleaseRoles[1].UserId);
                Assert.Equal(release2.Id, userReleaseRoles[1].ReleaseId);
                Assert.Equal(Contributor, userReleaseRoles[1].Role);
                Assert.Equal(_createdById, userReleaseRoles[1].CreatedById);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseRoles[1].Created!.Value).Milliseconds, 0, 1500);

                var userReleaseInvites = await contentDbContext.UserReleaseInvites
                    .AsQueryable()
                    .ToListAsync();

                Assert.Single(userReleaseInvites); // only create release invite for missing UserReleaseRole

                Assert.Equal("test@test.com", userReleaseInvites[0].Email);
                Assert.Equal(release2.Id, userReleaseInvites[0].ReleaseId);
                Assert.Equal(Contributor, userReleaseInvites[0].Role);
                Assert.Equal(_createdById, userReleaseInvites[0].CreatedById);
                Assert.True(userReleaseInvites[0].Accepted);
                Assert.True(userReleaseInvites[0].EmailSent);
                Assert.InRange(DateTime.UtcNow.Subtract(userReleaseInvites[0].Created).Milliseconds, 0, 1500);
            }
        }

        [Fact]
        public async Task InviteContributor_UserAlreadyHasReleaseRoleInvites()
        {
            var release1 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var release2 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Releases = ListOf(release1, release2)
            };

            var userRelease1Invite = new UserReleaseInvite()
            {
                Email = "test@test.com",
                Release = release1,
                Role = Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };
            var userRelease2Invite = new UserReleaseInvite()
            {
                Email = "test@test.com",
                Release = release2,
                Role = Contributor,
                Created = new DateTime(2001, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication, userRelease1Invite, userRelease2Invite);
                await contentDbContext.SaveChangesAsync();
            }

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseIds: ListOf(release1.Id, release2.Id));

                var actionResult = result.AssertLeft();
                Assert.IsType<BadRequestObjectResult>(actionResult);
                var badRequestObjectResult = (BadRequestObjectResult)actionResult;
                var validationProblemDetails = (ValidationProblemDetails)badRequestObjectResult.Value;
                Assert.Equal("USER_ALREADY_HAS_RELEASE_ROLE_INVITES", validationProblemDetails.Errors[""].First());
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
            var user = new User
            {
                Email = "test@test.com",
            };

            var release1 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var release2 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2001",
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Releases = ListOf(release1, release2)
            };

            var userRelease1Role = new UserReleaseRole()
            {
                User = user,
                Release = release1,
                Role = Contributor,
                Created = new DateTime(2000, 1, 1),
                CreatedById = Guid.NewGuid(),
            };
            var userRelease2Role = new UserReleaseRole()
            {
                User = user,
                Release = release2,
                Role = Contributor,
                Created = new DateTime(2001, 1, 1),
                CreatedById = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(user, publication,
                    userRelease1Role, userRelease2Role);
                await contentDbContext.SaveChangesAsync();
            }

            var usersAndRolesDbContextId = Guid.NewGuid().ToString();
            await using (var usersAndRolesDbContext = InMemoryUserAndRolesDbContext(usersAndRolesDbContextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseIds: ListOf(release1.Id, release2.Id));

                var actionResult = result.AssertLeft();
                Assert.IsType<BadRequestObjectResult>(actionResult);
                var badRequestObjectResult = (BadRequestObjectResult)actionResult;
                var validationProblemDetails = (ValidationProblemDetails)badRequestObjectResult.Value;
                Assert.Equal("USER_ALREADY_HAS_RELEASE_ROLES", validationProblemDetails.Errors[""].First());
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
            var release1 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Releases = ListOf(release1)
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic Year 2000/01");
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
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseIds: ListOf(release1.Id));

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

            var release1 = new Release()
            {
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
            };
            var publication = new Publication
            {
                Title = "Publication title",
                Releases = ListOf(release1)
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(user, publication);
                await contentDbContext.SaveChangesAsync();
            }

            var emailService = new Mock<IEmailService>(Strict);
            var expectedTemplateValues = GetExpectedContributorInviteTemplateValues(
                publication.Title, "* Academic Year 2000/01");
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
                var service = SetupUserManagementService(
                    contentDbContext: contentDbContext,
                    usersAndRolesDbContext: usersAndRolesDbContext,
                    emailService: emailService.Object);

                var result = await service.InviteContributor(
                    email: "test@test.com",
                    publicationId: publication.Id,
                    releaseIds: ListOf(release1.Id));

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
        public async Task UpdateUser_UserHasExistingRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var role1 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            };

            var role2 = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2"
            };

            var userRole = new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = role1.Id
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddRangeAsync(role1, role2);
                await userAndRolesDbContext.AddRangeAsync(userRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userRoleService = new Mock<IUserRoleService>(Strict);

            userRoleService.Setup(mock =>
                mock.RemoveGlobalRole(user.Id, role1.Id)).ReturnsAsync(Unit.Instance);

            userRoleService.Setup(mock =>
                mock.AddGlobalRole(user.Id, role2.Id)).ReturnsAsync(Unit.Instance);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object);

                var result = await service.UpdateUser(user.Id, role2.Id);

                Assert.True(result.IsRight);

                userRoleService.Verify(mock => mock.RemoveGlobalRole(
                        user.Id, role1.Id),
                    Times.Once);

                userRoleService.Verify(mock => mock.AddGlobalRole(
                        user.Id, role2.Id),
                    Times.Once);
            }

            VerifyAllMocks(userRoleService);
        }

        [Fact]
        public async Task UpdateUser_UserHasNoExistingRole()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString()
            };

            var role = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Role 2",
                NormalizedName = "ROLE 2"
            };

            var userAndRolesDbContextId = Guid.NewGuid().ToString();

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                await userAndRolesDbContext.AddAsync(user);
                await userAndRolesDbContext.AddRangeAsync(role);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            var userRoleService = new Mock<IUserRoleService>(Strict);

            userRoleService.Setup(mock =>
                mock.AddGlobalRole(user.Id, role.Id)).ReturnsAsync(Unit.Instance);

            await using (var userAndRolesDbContext = InMemoryUserAndRolesDbContext(userAndRolesDbContextId))
            {
                var service = SetupUserManagementService(
                    usersAndRolesDbContext: userAndRolesDbContext,
                    userRoleService: userRoleService.Object);

                var result = await service.UpdateUser(user.Id, role.Id);

                Assert.True(result.IsRight);

                userRoleService.Verify(mock => mock.AddGlobalRole(
                        user.Id, role.Id),
                    Times.Once);
            }

            VerifyAllMocks(userRoleService);
        }

        private static Dictionary<string, dynamic> GetExpectedContributorInviteTemplateValues(string publicationTitle,
            string releaseList)
        {
            return new()
            {
                { "url", "http://localhost/" },
                { "publication name", publicationTitle },
                { "release list", releaseList },
            };
        }

        private static Mock<IConfiguration> DefaultConfigurationMock()
        {
            return CreateMockConfiguration(
                TupleOf("NotifyContributorTemplateId", NotifyContributorTemplateId));
        }

        private static Mock<IHttpContextAccessor> DefaultHttpContextAccessorMock()
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>(Strict);
            var context = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "http",
                    Host = new HostString("localhost")
                }
            };

            httpContextAccessor
                .SetupGet(m => m.HttpContext)
                .Returns(context);

            return httpContextAccessor;
        }


        private static UserManagementService SetupUserManagementService(
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersAndRolesDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPersistenceHelper<UsersAndRolesDbContext>? usersAndRolesPersistenceHelper = null,
            IEmailTemplateService? emailTemplateService = null,
            IUserRoleService? userRoleService = null,
            IUserRepository? userRepository = null,
            IUserService? userService = null,
            IUserInviteRepository? userInviteRepository = null,
            IUserReleaseInviteRepository? userReleaseInviteRepository = null,
            IUserReleaseRoleRepository? userReleaseRoleRepository = null,
            IConfiguration? configuration = null,
            IEmailService? emailService = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            contentDbContext ??= InMemoryApplicationDbContext();
            usersAndRolesDbContext ??= InMemoryUserAndRolesDbContext();

            return new UserManagementService(
                usersAndRolesDbContext,
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                usersAndRolesPersistenceHelper ?? new PersistenceHelper<UsersAndRolesDbContext>(usersAndRolesDbContext),
                emailTemplateService ?? new Mock<IEmailTemplateService>(Strict).Object,
                userRoleService ?? new Mock<IUserRoleService>(Strict).Object,
                userRepository ?? new UserRepository(contentDbContext),
                userService ?? AlwaysTrueUserService(_createdById).Object,
                userInviteRepository ?? new UserInviteRepository(usersAndRolesDbContext),
                userReleaseInviteRepository ?? new UserReleaseInviteRepository(contentDbContext),
                userReleaseRoleRepository ?? new UserReleaseRoleRepository(contentDbContext),
                configuration ?? DefaultConfigurationMock().Object,
                emailService ?? new Mock<IEmailService>(Strict).Object,
                httpContextAccessor ?? DefaultHttpContextAccessorMock().Object
            );
        }
    }
}
