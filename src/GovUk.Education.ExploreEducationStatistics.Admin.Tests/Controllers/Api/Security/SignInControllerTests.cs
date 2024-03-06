#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Security;

public class SignInControllerTests : IntegrationTest<TestStartup>
{
    public SignInControllerTests(TestApplicationFactory<TestStartup> testApp)
        : base(testApp)
    {}

    public class RegistrationTests : SignInControllerTests
    {
        public RegistrationTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Theory]
        [InlineData("VALID-USER@education.gov.uk", null, "FirstName", "LastName", null, "Role 2", null, null)]
        [InlineData(null, "VALID-USER@education.gov.uk", "FirstName", "LastName", null, "Role 1", null, null)]
        [InlineData(null, "VALID-USER@education.gov.uk", null, null, "FirstName LastName", "Role 1", null, null)]
        [InlineData(null, "VALID-USER@education.gov.uk", null, null, "FirstName MiddleName LastName", "Role 1", null, null)]
        [InlineData(null, "VALID-USER@education.gov.uk", null, null, "FirstName", "Role 1", null, null)]
        [InlineData("VALID-USER@education.gov.uk", "VALID-USER@education.gov.uk", "FirstName", "LastName", "FirstName LastName", "Role 1", null, null)]
        [InlineData("VALID-USER@education.gov.uk", null, "FirstName", "LastName", null, "Role 1", "Approver", null)]
        [InlineData("VALID-USER@education.gov.uk", null, "FirstName", "LastName", null, "Role 1", null, "Approver")]
        [InlineData("VALID-USER@education.gov.uk", null, "FirstName", "LastName", null, "Role 1", "Contributor", "Approver")]
        public async Task Success(
            string? emailClaimValue,
            string? nameClaimValue,
            string? givenNameClaimValue,
            string? surnameClaimValue,
            string? combinedNameClaimValue,
            string globalRoleNameInInvite,
            string? releaseRoleInInvite,
            string? publicationRoleInInvite)
        {
            const string unrelatedUserEmail = "unrelated-user@education.gov.uk";

            var email = "";
            var firstName = "";
            var lastName = "";

            var claimsPrincipal = ClaimsPrincipalUtils.VerifiedByIdentityProviderUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;

            // One possibility is that the Identity Provider sends the user's email in the "emailaddress" Claim.
            if (emailClaimValue != null)
            {
                email = emailClaimValue;
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, emailClaimValue));
            }
            // Another possibility is that the Identity Provider sends the user's email in the "name" Claim.
            else if (nameClaimValue != null)
            {
                email = nameClaimValue;
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, nameClaimValue));
            }

            if (givenNameClaimValue != null)
            {
                firstName = givenNameClaimValue;
                claimsIdentity.AddClaim(new Claim(ClaimTypes.GivenName, givenNameClaimValue));
            }

            if (surnameClaimValue != null)
            {
                lastName = surnameClaimValue;
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Surname, surnameClaimValue));
            }

            if (combinedNameClaimValue != null)
            {
                var nameParts = combinedNameClaimValue.Split(' ');
                firstName = nameParts[0];
                lastName = nameParts.Length > 1 ? nameParts.Last() : "";
                claimsIdentity.AddClaim(new Claim(EesClaimTypes.Name, combinedNameClaimValue));
            }

            var releaseVersionId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            // Set up scenario and test data.
            var client = TestApp
                .SetUser(claimsPrincipal)
                .ResetUsersAndRolesDbContext()
                .ResetContentDbContext()
                .AddUsersAndRolesDbTestData(context =>
                {
                    var globalRoles = GetGlobalRoles();

                    context.Roles.AddRange(globalRoles);

                    // Add a global role invite for an unrelated user.
                    context.UserInvites.Add(new UserInvite
                    {
                        Email = unrelatedUserEmail,
                        Role = globalRoles.Single(r => r.Name == globalRoleNameInInvite),
                        Created = DateTime.Now.AddDays(-1)
                    });

                    // Add a global role invite for the user.
                    context.UserInvites.Add(new UserInvite
                    {
                        // Change the case of the email address in the invite to test this scenario.
                        Email = email.ToLower(),
                        Role = globalRoles.Single(r => r.Name == globalRoleNameInInvite),
                        Created = DateTime.Now.AddDays(-1)
                    });
                })
                .AddContentDbTestData(context =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Publication = new Publication
                        {
                            Id = publicationId
                        }
                    };

                    context.ReleaseVersions.Add(releaseVersion);

                    // Add a release role invite for an unrelated user.
                    context.UserReleaseInvites.Add(new UserReleaseInvite
                    {
                        ReleaseVersionId = releaseVersionId,
                        Email = unrelatedUserEmail,
                        Role = ReleaseRole.Approver,
                        Created = DateTime.Now.AddDays(-1),
                    });

                    if (releaseRoleInInvite != null)
                    {
                        // Add a release role invite for this user.
                        context.UserReleaseInvites.Add(new UserReleaseInvite
                        {
                            ReleaseVersionId = releaseVersionId,
                            Email = email.ToLower(),
                            Role = Enum.Parse<ReleaseRole>(releaseRoleInInvite),
                            Created = DateTime.Now.AddDays(-1),
                        });
                    }

                    // Add a publication role invite for an unrelated user.
                    context.UserPublicationInvites.Add(new UserPublicationInvite
                    {
                        PublicationId = publicationId,
                        Email = unrelatedUserEmail,
                        Role = PublicationRole.Approver,
                        Created = DateTime.Now.AddDays(-1),
                    });

                    if (publicationRoleInInvite != null)
                    {
                        // Add a publication role invite for this user.
                        context.UserPublicationInvites.Add(new UserPublicationInvite
                        {
                            PublicationId = releaseVersion.Publication.Id,
                            Email = email.ToLower(),
                            Role = Enum.Parse<PublicationRole>(publicationRoleInInvite),
                            Created = DateTime.Now.AddDays(-1),
                        });
                    }
                })
                .CreateClient();

            var response = await client.PostAsync("/api/sign-in", null);

            // Perform the test.
            var (loginResult, userProfile) = response.AssertOk<SignInResponseViewModel>();

            // Assert registration and role assignments went successfully.
            Assert.Equal(LoginResult.RegistrationSuccess , loginResult);
            Assert.NotNull(userProfile);
            Assert.NotEqual(Guid.Empty , userProfile.Id);
            Assert.Equal(firstName , userProfile.FirstName);

            TestApp.VerifyUsersAndRolesDbContext(context =>
            {
                // Verify that the new Identity Framework user is created.
                var newIdentityUser = Assert.Single(context.Users);
                Assert.Equal(firstName, newIdentityUser.FirstName);
                Assert.Equal(lastName, newIdentityUser.LastName);
                Assert.Equal(email, newIdentityUser.Email);
                Assert.Equal(email.ToUpper(), newIdentityUser.NormalizedEmail);
                Assert.Equal(userProfile.Id.ToString(), newIdentityUser.Id);

                // Verify that the new Identity Framework user is connected to the correct global role.
                var globalRoleAssignment = context.UserRoles.Single(role => role.UserId == newIdentityUser.Id);
                var globalRoleAssigned = context.Roles.Single(role => role.Id == globalRoleAssignment.RoleId);
                Assert.Equal(globalRoleNameInInvite, globalRoleAssigned.Name);

                // Verify that the invite for this user was accepted successfully.
                var userInvite = context.UserInvites.Single(i => i.Email.ToLower() == email.ToLower());
                Assert.True(userInvite.Accepted);

                // Verify that the invite for the unrelated user was left alone.
                var unrelatedUserInvite = context.UserInvites.Single(i => i.Email == unrelatedUserEmail);
                Assert.False(unrelatedUserInvite.Accepted);
            });

            TestApp.VerifyContentDbContext(context =>
            {
                // Verify that the new internal user is created.
                var newInternalUser = Assert.Single(context.Users);
                Assert.Equal(firstName, newInternalUser.FirstName);
                Assert.Equal(lastName, newInternalUser.LastName);
                Assert.Equal(email, newInternalUser.Email);
                Assert.Equal($"{firstName} {lastName}", newInternalUser.DisplayName);
                Assert.Equal(userProfile.Id, newInternalUser.Id);

                if (releaseRoleInInvite != null)
                {
                    // Verify that the user received the desired role on the Release they were invited to.
                    var releaseRoleAssignment = context.UserReleaseRoles.Single();
                    Assert.Equal(releaseVersionId, releaseRoleAssignment.ReleaseVersionId);
                    Assert.Equal(releaseRoleInInvite, releaseRoleAssignment.Role.ToString());
                    Assert.Equal(userProfile.Id, releaseRoleAssignment.UserId);
                }

                if (publicationRoleInInvite != null)
                {
                    // Verify that the user received the desired role on the Publication they were invited to.
                    var publicationRoleAssignment = context.UserPublicationRoles.Single();
                    Assert.Equal(publicationId, publicationRoleAssignment.PublicationId);
                    Assert.Equal(publicationRoleInInvite, publicationRoleAssignment.Role.ToString());
                    Assert.Equal(userProfile.Id, publicationRoleAssignment.UserId);
                }
            });
        }

        [Theory]
        [InlineData(13, false)]
        [InlineData(14, true)]
        [InlineData(15, true)]
        public async Task ExpiredInvites(int daysOld, bool expectExpiredInviteResult)
        {
            const string email = "user@education.gov.uk";

            var claimsPrincipal = ClaimsPrincipalUtils.VerifiedByIdentityProviderUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));
            claimsIdentity.AddClaim(new Claim(EesClaimTypes.Name, "FirstName LastName"));

            var releaseVersionId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            // Set up scenario and test data.
            var client = TestApp
                .SetUser(claimsPrincipal)
                .ResetUsersAndRolesDbContext()
                .ResetContentDbContext()
                .AddUsersAndRolesDbTestData(context =>
                {
                    var globalRoles = GetGlobalRoles();
                    context.Roles.AddRange(globalRoles);

                    // Add a global role invite for the user and back-date its created date.
                    context.UserInvites.Add(new UserInvite
                    {
                        // Change the case of the email address in the invite to test this scenario.
                        Email = email.ToLower(),
                        Role = globalRoles.First(),
                        Created = DateTime.Now.AddDays(-daysOld),
                    });
                })
                .AddContentDbTestData(context =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Publication = new Publication
                        {
                            Id = publicationId
                        }
                    };

                    context.ReleaseVersions.Add(releaseVersion);

                    // Add a release role invite for this user.
                    context.UserReleaseInvites.Add(new UserReleaseInvite
                    {
                        ReleaseVersionId = releaseVersionId,
                        Email = email.ToLower(),
                        Role = ReleaseRole.Approver,
                        Created = DateTime.Now.AddDays(-1),
                    });

                    // Add a publication role invite for this user.
                    context.UserPublicationInvites.Add(new UserPublicationInvite
                    {
                        PublicationId = releaseVersion.Publication.Id,
                        Email = email.ToLower(),
                        Role = PublicationRole.Approver,
                        Created = DateTime.Now.AddDays(-1),
                    });
                })
                .CreateClient();

            var response = await client.PostAsync("/api/sign-in", null);

            // Perform the test.
            var (loginResult, userProfile) = response.AssertOk<SignInResponseViewModel>();

            if (expectExpiredInviteResult)
            {
                Assert.Equal(LoginResult.ExpiredInvite, loginResult);
                Assert.Null(userProfile);

                TestApp.VerifyUsersAndRolesDbContext(context =>
                {
                    // Assert that no Identity user got created.
                    Assert.Empty(context.Users);

                    // Assert that the expired global invite got deleted.
                    Assert.Empty(context.UserInvites);
                });

                TestApp.VerifyContentDbContext(context =>
                {
                    // Assert that no internal user got created.
                    Assert.Empty(context.Users);

                    // Assert that the release and publication invites were deleted.
                    Assert.Empty(context.UserReleaseInvites);
                    Assert.Empty(context.UserPublicationInvites);
                });
            }
            else
            {
                Assert.Equal(LoginResult.RegistrationSuccess, loginResult);
                Assert.NotNull(userProfile);
            }
        }

        [Fact]
        public async Task NoInvite()
        {
            const string email = "user@education.gov.uk";

            var claimsPrincipal = ClaimsPrincipalUtils.VerifiedByIdentityProviderUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));
            claimsIdentity.AddClaim(new Claim(EesClaimTypes.Name, "FirstName LastName"));

            var releaseVersionId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            // Set up scenario and test data.
            var client = TestApp
                .SetUser(claimsPrincipal)
                .ResetUsersAndRolesDbContext()
                .ResetContentDbContext()
                .AddUsersAndRolesDbTestData(context =>
                {
                    var globalRoles = GetGlobalRoles();
                    context.Roles.AddRange(globalRoles);

                    // Add a global role invite for an unrelated user.
                    context.UserInvites.Add(new UserInvite
                    {
                        // Change the case of the email address in the invite to test this scenario.
                        Email = "unrelated-user@education.gov.uk",
                        Role = globalRoles.First(),
                        Created = DateTime.Now.AddDays(-1),
                    });
                })
                .AddContentDbTestData(context =>
                {
                    var releaseVersion = new ReleaseVersion
                    {
                        Id = releaseVersionId,
                        Publication = new Publication
                        {
                            Id = publicationId
                        }
                    };

                    context.ReleaseVersions.Add(releaseVersion);

                    // Add a release role invite for this user, but without a global invite, this should have no effect.
                    context.UserReleaseInvites.Add(new UserReleaseInvite
                    {
                        ReleaseVersionId = releaseVersionId,
                        Email = email.ToLower(),
                        Role = ReleaseRole.Approver,
                        Created = DateTime.Now.AddDays(-1),
                    });

                    // Add a publication role invite for this user.
                    context.UserPublicationInvites.Add(new UserPublicationInvite
                    {
                        PublicationId = releaseVersion.Publication.Id,
                        Email = email.ToLower(),
                        Role = PublicationRole.Approver,
                        Created = DateTime.Now.AddDays(-1),
                    });
                })
                .CreateClient();

            var response = await client.PostAsync("/api/sign-in", null);

            // Perform the test.
            var (loginResult, userProfile) = response.AssertOk<SignInResponseViewModel>();

            Assert.Equal(LoginResult.NoInvite, loginResult);
            Assert.Null(userProfile);

            TestApp.VerifyUsersAndRolesDbContext(context =>
            {
                // Assert that no Identity user got created.
                Assert.Empty(context.Users);
            });

            TestApp.VerifyContentDbContext(context =>
            {
                // Assert that no internal user got created.
                Assert.Empty(context.Users);

                // Assert that no release or publication  assignments were made.
                Assert.Empty(context.UserReleaseRoles);
                Assert.Empty(context.UserPublicationRoles);
            });
        }
    }

    public class SignInTests : SignInControllerTests
    {
        public SignInTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task Success()
        {
            const string email = "EXISTING-USER@education.gov.uk";
            const string unrelatedUserEmail = "unrelated-user@education.gov.uk";
            var userId = Guid.NewGuid();

            var claimsPrincipal = ClaimsPrincipalUtils.VerifiedByIdentityProviderUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

            // Set up scenario and test data.
            var client = TestApp
                .SetUser(claimsPrincipal)
                .ResetUsersAndRolesDbContext()
                .ResetContentDbContext()
                .AddUsersAndRolesDbTestData(context =>
                {
                    var globalRoles = GetGlobalRoles();
                    context.Roles.AddRange(globalRoles);

                    // Add an Identity user for an unrelated user.
                    context.Users.Add(new ApplicationUser
                    {
                        Id = userId.ToString(),
                        Email = email,
                        NormalizedEmail = email.ToUpper(),
                        FirstName = "FirstName",
                        LastName = "LastName"
                    });

                    // Add an Identity user for the user.
                    context.Users.Add(new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = unrelatedUserEmail.ToLower(),
                        NormalizedEmail = unrelatedUserEmail.ToUpper(),
                        FirstName = "AnotherFirstName",
                        LastName = "AnotherLastName"
                    });
                })
                .CreateClient();

            var response = await client.PostAsync("/api/sign-in", null);

            // Perform the test.
            var (loginResult, userProfile) = response.AssertOk<SignInResponseViewModel>();

            // Assert registration and role assignments went successfully.
            Assert.Equal(LoginResult.LoginSuccess , loginResult);
            Assert.NotNull(userProfile);
            Assert.Equal(userId , userProfile.Id);
            Assert.Equal("FirstName" , userProfile.FirstName);
        }
    }

    public class PermissionsTests : SignInControllerTests
    {
        public PermissionsTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task NoAccessAdminApiScope()
        {
            // Set up a user with a valid JWT but which holds no scope by which the user can access the Admin API.
            var claimsPrincipal = ClaimsPrincipalUtils.VerifiedButNotAuthorizedByIdentityProviderUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, "email"));

            // Set up scenario and test data.
            var client = TestApp
                .SetUser(claimsPrincipal)
                .CreateClient();

            var response = await client.PostAsync("/api/sign-in", null);

            // Perform the test.
            response.AssertForbidden();
        }
    }

    private static List<IdentityRole> GetGlobalRoles()
    {
        return new List<IdentityRole>
        {
            new()
            {
                Id = "role-1",
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            },
            new()
            {
                Id = "role-2",
                Name = "Role 2",
                NormalizedName = "ROLE 2"
            }
        };
    }
}
