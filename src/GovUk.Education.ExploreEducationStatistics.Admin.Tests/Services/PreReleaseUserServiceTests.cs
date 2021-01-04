using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class PreReleaseUserServiceTests
    {
        [Fact]
        public async void GetPreReleaseUsers_OrderedCorrectly()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.2@test.com",
                        }
                    },
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.1@test.com",
                        }
                    }
                );

                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                        Accepted = false
                    },
                    new UserReleaseInvite
                    {
                        Release = release,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                        Accepted = false
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsers(release.Id);

                Assert.True(result.IsRight);

                var users = result.Right;

                Assert.Equal(4, users.Count);
                Assert.Equal("existing.1@test.com", users[0].Email);

                Assert.Equal("existing.2@test.com", users[1].Email);

                Assert.Equal("invited.1@test.com", users[2].Email);

                Assert.Equal("invited.2@test.com", users[3].Email);
            }
        }

        [Fact]
        public async void GetPreReleaseUsers_FiltersInvalidReleaseUsers()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Not a prerelease viewer
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.Lead,
                        User = new User
                        {
                            Email = "existing.1@test.com",
                        }
                    },
                    // Different release user
                    new UserReleaseRole
                    {
                        Release = new Release(),
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "existing.2@test.com",
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsers(release.Id);

                Assert.True(result.IsRight);

                var users = result.Right;

                Assert.Empty(users);
            }
        }

        [Fact]
        public async void GetPreReleaseUsers_FiltersInvalidReleaseInvites()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await context.AddRangeAsync(
                    // Not a prerelease viewer
                    new UserReleaseInvite
                    {
                        Release = release,
                        Email = "invited.1@test.com",
                        Role = ReleaseRole.Contributor,
                        Accepted = false
                    },
                    // Different release
                    new UserReleaseInvite
                    {
                        Release = new Release(),
                        Email = "invited.2@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                        Accepted = false
                    },
                    // Already accepted
                    new UserReleaseInvite
                    {
                        Release = release,
                        Email = "invited.3@test.com",
                        Role = ReleaseRole.PrereleaseViewer,
                        Accepted = true
                    }
                );


                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.GetPreReleaseUsers(release.Id);

                Assert.True(result.IsRight);

                var users = result.Right;

                Assert.Empty(users);
            }
        }

        [Fact]
        public async void AddPreReleaseUser_Fails_InvalidEmail()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.AddPreReleaseUser(release.Id, "not an email");

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);
                Assert.Equal("INVALID_EMAIL_ADDRESS", details.Errors[""].First());
            }
        }

        [Fact]
        public async void AddPreReleaseUser_Fails_NoRelease()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.AddPreReleaseUser(Guid.NewGuid(), "test@test.com");

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async void AddPreReleaseUser_Fails_ExistingReleaseRole()
        {
            var release = new Release
            {
                Publication = new Publication()
            };
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com",
                        }
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.AddPreReleaseUser(release.Id, "test@test.com");

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);
                Assert.Equal("USER_ALREADY_EXISTS", details.Errors[""].First());
            }
        }

        [Fact]
        public async void AddPreReleaseUser_Fails_ExistingReleaseInvite()
        {
            var release = new Release
            {
                Publication = new Publication()
            };
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com",
                    }
                );

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.AddPreReleaseUser(release.Id, "test@test.com");

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);
                Assert.Equal("USER_ALREADY_EXISTS", details.Errors[""].First());
            }
        }

        [Fact]
        public async void AddPreReleaseUser_InvitesExistingUser()
        {
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = DateTime.Parse("2020-09-09T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Publication = new Publication
                {
                    Title = "Test publication",
                }
            };

            var user = new User
            {
                Email = "test@test.com"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(release);
                context.Add(user);

                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var preReleaseService = new Mock<IPreReleaseService>();

                preReleaseService
                    .Setup(s => s.GetPreReleaseWindow(It.IsAny<Release>()))
                    .Returns(
                        new PreReleaseWindow
                        {
                            Start = DateTime.Parse("2020-09-08T08:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                            End = DateTime.Parse("2020-09-08T22:59:59.00Z", styles: DateTimeStyles.AdjustToUniversal),
                        }
                    );

                var emailService = new Mock<IEmailService>();

                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object
                );

                var result = await service.AddPreReleaseUser(
                    release.Id,
                    "test@test.com"
                );

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        "the-template-id",
                        new Dictionary<string, dynamic>
                        {
                            {"newUser", "no"},
                            {"release name", "Calendar Year 2020"},
                            {"publication name", "Test publication"},
                            {
                                "prerelease link",
                                $"http://localhost/publication/{release.PublicationId}/release/{release.Id}/prerelease"
                            },
                            {"prerelease day", "Tuesday 08 September 2020"},
                            {"prerelease time", "09:30"},
                            {"publish day", "Wednesday 09 September 2020"},
                            {"publish time", "09:30"},
                        }
                    )
                );

                Assert.True(result.IsRight);

                Assert.Equal("test@test.com", result.Right.Email);
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var savedUserReleaseRoles = await context.UserReleaseRoles
                    .Where(userReleaseRole => userReleaseRole.ReleaseId == release.Id)
                    .ToListAsync();

                Assert.Single(savedUserReleaseRoles);
                Assert.Equal(release.Id, savedUserReleaseRoles[0].ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRoles[0].Role);
                Assert.Equal(user.Id, savedUserReleaseRoles[0].UserId);
            }
        }

        [Fact]
        public async void AddPreReleaseUser_InvitesNewUser()
        {
            var release = new Release
            {
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.CalendarYear,
                PublishScheduled = DateTime.Parse("2020-09-09T00:00:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                Publication = new Publication
                {
                    Title = "Test publication",
                }
            };

            var identityRole = new IdentityRole
            {
                Name = "Prerelease User"
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                context.Add(release);
                await context.SaveChangesAsync();

                userAndRolesDbContext.Add(identityRole);
                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var preReleaseService = new Mock<IPreReleaseService>();

                preReleaseService
                    .Setup(s => s.GetPreReleaseWindow(It.IsAny<Release>()))
                    .Returns(
                        new PreReleaseWindow
                        {
                            Start = DateTime.Parse("2020-09-08T08:30:00.00Z", styles: DateTimeStyles.AdjustToUniversal),
                            End = DateTime.Parse("2020-09-08T22:59:59.00Z", styles: DateTimeStyles.AdjustToUniversal),
                        }
                    );

                var emailService = new Mock<IEmailService>();

                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext,
                    preReleaseService: preReleaseService.Object,
                    emailService: emailService.Object
                );

                var result = await service.AddPreReleaseUser(
                    release.Id,
                    "test@test.com"
                );

                emailService.Verify(
                    s => s.SendEmail(
                        "test@test.com",
                        "the-template-id",
                        new Dictionary<string, dynamic>
                        {
                            {"newUser", "yes"},
                            {"release name", "Calendar Year 2020"},
                            {"publication name", "Test publication"},
                            {
                                "prerelease link",
                                $"http://localhost/publication/{release.PublicationId}/release/{release.Id}/prerelease"
                            },
                            {"prerelease day", "Tuesday 08 September 2020"},
                            {"prerelease time", "09:30"},
                            {"publish day", "Wednesday 09 September 2020"},
                            {"publish time", "09:30"},
                        }
                    )
                );

                Assert.True(result.IsRight);

                Assert.Equal("test@test.com", result.Right.Email);
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var releaseInvite = await context.UserReleaseInvites
                    .Where(userReleaseInvite => userReleaseInvite.ReleaseId == release.Id)
                    .SingleAsync();

                Assert.Equal("test@test.com", releaseInvite.Email);
                Assert.Equal(ReleaseRole.PrereleaseViewer, releaseInvite.Role);

                var systemInvite = await userAndRolesDbContext.UserInvites
                    .Where(userInvite => userInvite.Email == "test@test.com")
                    .SingleAsync();

                Assert.Equal("test@test.com", systemInvite.Email);
                Assert.Equal(identityRole.Id, systemInvite.RoleId);
            }
        }

        [Fact]
        public async void RemovePreReleaseUser_Fails_InvalidEmail()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext())
            {
                var service = SetupPreReleaseUserService(context, usersAndRolesDbContext: userAndRolesDbContext);
                var result = await service.RemovePreReleaseUser(release.Id, "not an email");

                Assert.True(result.IsLeft);

                var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result.Left);
                var details = Assert.IsType<ValidationProblemDetails>(badRequestObjectResult.Value);
                Assert.Equal("INVALID_EMAIL_ADDRESS", details.Errors[""].First());
            }
        }

        [Fact]
        public async void RemovePreReleaseUser_ReleaseRoleRemoved()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                context.Add(release);
                context.Add(
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    }
                );

                await context.SaveChangesAsync();

                userAndRolesDbContext.Add(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        Role = new IdentityRole
                        {
                            Name = "Prerelease User"
                        },
                        Accepted = false
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    release.Id,
                    "test@test.com"
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var savedUserReleaseRoles = await context.UserReleaseRoles
                    .ToListAsync();

                Assert.Empty(savedUserReleaseRoles);
            }
        }

        [Fact]
        public async void RemovePreReleaseUser_OtherReleaseRolesNotRemoved()
        {
            var release = new Release();
            var otherRelease = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                context.Add(release);
                await context.AddRangeAsync(
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    },
                    // Belongs to another release
                    new UserReleaseRole
                    {
                        Release = otherRelease,
                        Role = ReleaseRole.PrereleaseViewer,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    },
                    // Has a different role on the release
                    new UserReleaseRole
                    {
                        Release = release,
                        Role = ReleaseRole.Lead,
                        User = new User
                        {
                            Email = "test@test.com"
                        }
                    }
                );

                await context.SaveChangesAsync();

                userAndRolesDbContext.Add(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        Role = new IdentityRole
                        {
                            Name = "Prerelease User"
                        },
                        Accepted = false
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    release.Id,
                    "test@test.com"
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var savedUserReleaseRoles = await context.UserReleaseRoles
                    .ToListAsync();

                Assert.Equal(2, savedUserReleaseRoles.Count);

                Assert.Equal(otherRelease.Id, savedUserReleaseRoles[0].ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseRoles[0].Role);

                Assert.Equal(release.Id, savedUserReleaseRoles[1].ReleaseId);
                Assert.Equal(ReleaseRole.Lead, savedUserReleaseRoles[1].Role);
            }
        }

        [Fact]
        public async void RemovePreReleaseUser_AllInvitesRemoved()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                context.Add(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    }
                );

                await context.SaveChangesAsync();

                userAndRolesDbContext.Add(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        Role = new IdentityRole
                        {
                            Name = "Prerelease User"
                        },
                        Accepted = false
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    release.Id,
                    "test@test.com"
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var savedUserReleaseInvites = await context.UserReleaseInvites
                    .ToListAsync();

                Assert.Empty(savedUserReleaseInvites);

                // Removes system invite as there are no release invites left
                var savedUserInvites = await userAndRolesDbContext.UserInvites
                    .ToListAsync();

                Assert.Empty(savedUserInvites);
            }
        }

        [Fact]
        public async void RemovePreReleaseUser_OtherReleaseInvitesNotRemoved()
        {
            var release = new Release();
            var otherRelease = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    },
                    // Invite for another release
                    new UserReleaseInvite
                    {
                        Release = otherRelease,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    },
                    // Invite for a different release role
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.Lead,
                        Email = "test@test.com"
                    }
                );

                await context.SaveChangesAsync();

                userAndRolesDbContext.Add(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        Role = new IdentityRole
                        {
                            Name = "Prerelease User"
                        },
                        Accepted = false
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    release.Id,
                    "test@test.com"
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var savedUserReleaseInvites = await context.UserReleaseInvites
                    .ToListAsync();

                Assert.Equal(2, savedUserReleaseInvites.Count);
                Assert.Equal(otherRelease.Id, savedUserReleaseInvites[0].ReleaseId);
                Assert.Equal(ReleaseRole.PrereleaseViewer, savedUserReleaseInvites[0].Role);
                Assert.Equal("test@test.com", savedUserReleaseInvites[0].Email);

                Assert.Equal(release.Id, savedUserReleaseInvites[1].ReleaseId);
                Assert.Equal(ReleaseRole.Lead, savedUserReleaseInvites[1].Role);
                Assert.Equal("test@test.com", savedUserReleaseInvites[1].Email);

                var savedUserInvites = await userAndRolesDbContext.UserInvites
                    .ToListAsync();

                Assert.Single(savedUserInvites);
                Assert.Equal("test@test.com", savedUserInvites[0].Email);
            }
        }

        [Fact]
        public async void RemovePreReleaseUser_AcceptedSystemInviteNotRemoved()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                await context.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Release = release,
                        Role = ReleaseRole.PrereleaseViewer,
                        Email = "test@test.com"
                    }
                );

                await context.SaveChangesAsync();

                userAndRolesDbContext.Add(
                    new UserInvite
                    {
                        Email = "test@test.com",
                        Role = new IdentityRole
                        {
                            Name = "Prerelease User"
                        },
                        Accepted = true
                    }
                );

                await userAndRolesDbContext.SaveChangesAsync();
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var service = SetupPreReleaseUserService(
                    context,
                    usersAndRolesDbContext: userAndRolesDbContext
                );

                var result = await service.RemovePreReleaseUser(
                    release.Id,
                    "test@test.com"
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var userAndRolesDbContext = DbUtils.InMemoryUserAndRolesDbContext(contextId))
            {
                var savedUserReleaseInvites = await context.UserReleaseInvites
                    .ToListAsync();

                Assert.Empty(savedUserReleaseInvites);

                var savedUserInvites = await userAndRolesDbContext.UserInvites
                    .ToListAsync();

                Assert.Single(savedUserInvites);
                Assert.Equal("test@test.com", savedUserInvites[0].Email);
            }
        }

        private static Mock<IConfiguration> DefaultConfigurationMock()
        {
            var section = new Mock<IConfigurationSection>();
            var configuration = new Mock<IConfiguration>();

            section.Setup(m => m.Value)
                .Returns("the-template-id");

            configuration
                .Setup(m => m.GetSection("NotifyPreReleaseTemplateId"))
                .Returns(section.Object);

            return configuration;
        }

        private static Mock<IHttpContextAccessor> DefaultHttpContextAccessorMock()
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");

            httpContextAccessor
                .SetupGet(m => m.HttpContext)
                .Returns(context);

            return httpContextAccessor;
        }

        private PreReleaseUserService SetupPreReleaseUserService(
            ContentDbContext context,
            UsersAndRolesDbContext usersAndRolesDbContext = null,
            IConfiguration configuration = null,
            IEmailService emailService = null,
            IPreReleaseService preReleaseService = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IUserService userService = null,
            IHttpContextAccessor httpContextAccessor = null)
        {
            return new PreReleaseUserService(
                context,
                usersAndRolesDbContext,
                configuration ?? DefaultConfigurationMock().Object,
                emailService ?? new Mock<IEmailService>().Object,
                preReleaseService ?? new Mock<IPreReleaseService>().Object,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(context),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                httpContextAccessor ?? DefaultHttpContextAccessorMock().Object
            );
        }
    }
}