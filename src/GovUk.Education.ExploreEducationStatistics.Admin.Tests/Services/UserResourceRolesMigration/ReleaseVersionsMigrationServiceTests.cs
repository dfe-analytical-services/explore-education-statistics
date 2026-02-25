#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.UserResourceRolesMigration;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.UserResourceRolesMigration;

/// <summary>
/// TODO EES-6957 Remove after the User Resource Roles migration is complete.
/// </summary>
public abstract class UserResourceRolesMigrationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class SingleUserPublicationPairTests : UserResourceRolesMigrationServiceTests
    {
        [Fact]
        public async Task NoOldRoles()
        {
            // Should do nothing

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Users.Add(user);
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Had none to begin with, and should still have none after.
                Assert.Empty(remainingUserPublicationRoles);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NoOldRoles_HasNewRole(PublicationRole expectedNewPublicationRoleToRemove)
        {
            // Should remove the NEW role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(expectedNewPublicationRoleToRemove);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have removed the NEW role.
                Assert.Empty(remainingUserPublicationRoles);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Owner, PublicationRole.Drafter)]
        [InlineData(PublicationRole.Allower, PublicationRole.Approver)]
        public async Task NoReleaseRoles_SingleOldPublicationRole(
            PublicationRole oldPublicationRole,
            PublicationRole expectedNewPublicationRoleToCreate
        )
        {
            // Should create the NEW role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(oldPublicationRole);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have the OLD role, as well as the NEW role that should have been created.
                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == oldPublicationRole);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == expectedNewPublicationRoleToCreate);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(publication.Id, upr.PublicationId);
                        Assert.Equal(user.Id, upr.UserId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(PublicationRole.Owner, PublicationRole.Drafter, null, null)]
        [InlineData(
            PublicationRole.Allower,
            PublicationRole.Drafter,
            PublicationRole.Drafter,
            PublicationRole.Approver
        )]
        [InlineData(PublicationRole.Owner, PublicationRole.Approver, PublicationRole.Approver, PublicationRole.Drafter)]
        [InlineData(PublicationRole.Allower, PublicationRole.Approver, null, null)]
        public async Task NoReleaseRoles_SingleOldPublicationRole_HasNewRole(
            PublicationRole oldPublicationRole,
            PublicationRole existingNewPublicationRole,
            PublicationRole? expectedNewPublicationRoleToRemove,
            PublicationRole? expectedNewPublicationRoleToCreate
        )
        {
            // Should remove/create the expected NEW roles

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(oldPublicationRole))
                .ForIndex(1, s => s.SetRole(existingNewPublicationRole))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have the OLD role, and the expected NEW roles should have been removed/created
                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == oldPublicationRole);

                if (expectedNewPublicationRoleToRemove.HasValue)
                {
                    Assert.DoesNotContain(
                        remainingUserPublicationRoles,
                        upr => upr.Role == expectedNewPublicationRoleToRemove.Value
                    );
                }

                if (expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == expectedNewPublicationRoleToCreate
                    );
                }

                if (!expectedNewPublicationRoleToRemove.HasValue && !expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == existingNewPublicationRole);
                }

                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(publication.Id, upr.PublicationId);
                        Assert.Equal(user.Id, upr.UserId);
                    }
                );
            }
        }

        [Fact]
        public async Task NoReleaseRoles_MultipleOldPublicationRoles()
        {
            // Has publication Owner & Allower
            // Should create the NEW role of Approver

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have both the OLD roles, and the expected NEW role of Approver should have been created
                Assert.Equal(3, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Owner);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Allower);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(publication.Id, upr.PublicationId);
                        Assert.Equal(user.Id, upr.UserId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NoReleaseRoles_MultipleOldPublicationRoles_HasNewRole(
            PublicationRole existingNewPublicationRole
        )
        {
            // Has publication Owner & Allower
            // Should remove/create the expected NEW roles, but always end up in the state of having the NEW Approver role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .ForIndex(2, s => s.SetRole(existingNewPublicationRole))
                .GenerateList(3);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have both the OLD roles, and the expected NEW role of Approver should have been created.
                // If they originally had the Drafter role, it should have been removed.
                Assert.Equal(3, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Owner);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Allower);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(publication.Id, upr.PublicationId);
                        Assert.Equal(user.Id, upr.UserId);
                    }
                );
            }
        }

        [Fact]
        public async Task NoOldPublicationRoles_OldPrereleaseRole()
        {
            // Should do nothing, as prerelease roles should not have any impact on the NEW publication roles to create

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.PrereleaseViewer);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the Prerelease role, but no NEW ones
                var prereleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Empty(remainingUserPublicationRoles);
                Assert.Equal(ReleaseRole.PrereleaseViewer, prereleaseRole.Role);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NoOldPublicationRoles_OldPrereleaseRole_HasNewRole(PublicationRole existingNewPublicationRole)
        {
            // Should remove the NEW role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(existingNewPublicationRole);
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(ReleaseRole.PrereleaseViewer);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the Prerelease role, but should have removed the NEW role
                var prereleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Empty(remainingUserPublicationRoles);
                Assert.Equal(ReleaseRole.PrereleaseViewer, prereleaseRole.Role);
            }
        }

        [Theory]
        [InlineData(ReleaseRole.Contributor, PublicationRole.Drafter)]
        [InlineData(ReleaseRole.Approver, PublicationRole.Approver)]
        public async Task NoOldPublicationRoles_SingleReleaseRole_SingleReleaseVersion(
            ReleaseRole releaseRole,
            PublicationRole expectedNewPublicationRoleToCreate
        )
        {
            // Should create the expected NEW role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRole);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the release role, and should have created the expected NEW role
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(releaseRole, remainingUserReleaseRole.Role);

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(expectedNewPublicationRoleToCreate, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Theory]
        [InlineData(ReleaseRole.Contributor, PublicationRole.Drafter, null, null)]
        [InlineData(
            ReleaseRole.Contributor,
            PublicationRole.Approver,
            PublicationRole.Approver,
            PublicationRole.Drafter
        )]
        [InlineData(ReleaseRole.Approver, PublicationRole.Drafter, PublicationRole.Drafter, PublicationRole.Approver)]
        [InlineData(ReleaseRole.Approver, PublicationRole.Approver, null, null)]
        public async Task NoOldPublicationRoles_SingleReleaseRole_SingleReleaseVersion_HasNewRole(
            ReleaseRole releaseRole,
            PublicationRole existingNewPublicationRole,
            PublicationRole? expectedNewPublicationRoleToRemove,
            PublicationRole? expectedNewPublicationRoleToCreate
        )
        {
            // Should remove/create the expected NEW roles

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(existingNewPublicationRole);
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRole);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the release role, and the expected NEW roles should have been removed/created
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(releaseRole, remainingUserReleaseRole.Role);

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);

                if (expectedNewPublicationRoleToRemove.HasValue)
                {
                    Assert.NotEqual(expectedNewPublicationRoleToRemove.Value, remainingUserPublicationRole.Role);
                }

                if (expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Equal(expectedNewPublicationRoleToCreate, remainingUserPublicationRole.Role);
                }

                if (!expectedNewPublicationRoleToRemove.HasValue && !expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Equal(existingNewPublicationRole, remainingUserPublicationRole.Role);
                }

                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Theory]
        [InlineData(ReleaseRole.Contributor, PublicationRole.Drafter)]
        [InlineData(ReleaseRole.Approver, PublicationRole.Approver)]
        public async Task NoOldPublicationRoles_SingleReleaseRole_AcrossMultipleReleaseVersions(
            ReleaseRole releaseRole,
            PublicationRole expectedNewPublicationRoleToCreate
        )
        {
            // Should create the expected NEW role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithRole(releaseRole)
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and should have created the expected NEW role
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.All(remainingUserReleaseRoles, urr => Assert.Equal(releaseRole, urr.Role));

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(expectedNewPublicationRoleToCreate, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Theory]
        [InlineData(ReleaseRole.Contributor, PublicationRole.Drafter, null, null)]
        [InlineData(
            ReleaseRole.Contributor,
            PublicationRole.Approver,
            PublicationRole.Approver,
            PublicationRole.Drafter
        )]
        [InlineData(ReleaseRole.Approver, PublicationRole.Drafter, PublicationRole.Drafter, PublicationRole.Approver)]
        [InlineData(ReleaseRole.Approver, PublicationRole.Approver, null, null)]
        public async Task NoOldPublicationRoles_SingleReleaseRole_AcrossMultipleReleaseVersions_HasNewRole(
            ReleaseRole releaseRole,
            PublicationRole existingNewPublicationRole,
            PublicationRole? expectedNewPublicationRoleToRemove,
            PublicationRole? expectedNewPublicationRoleToCreate
        )
        {
            // Should remove/create the expected NEW roles

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(existingNewPublicationRole);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithRole(releaseRole)
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and the expected NEW roles should have been removed/created
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.All(remainingUserReleaseRoles, urr => Assert.Equal(releaseRole, urr.Role));

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);

                if (expectedNewPublicationRoleToRemove.HasValue)
                {
                    Assert.NotEqual(expectedNewPublicationRoleToRemove.Value, remainingUserPublicationRole.Role);
                }

                if (expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Equal(expectedNewPublicationRoleToCreate, remainingUserPublicationRole.Role);
                }

                if (!expectedNewPublicationRoleToRemove.HasValue && !expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Equal(existingNewPublicationRole, remainingUserPublicationRole.Role);
                }

                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Fact]
        public async Task NoOldPublicationRoles_MultipleReleaseRoles_SingleReleaseVersion()
        {
            // Has release Contributor & Approver
            // Should create the NEW role of Approver

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and should have created the NEW role of Approver
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Contributor);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Approver);

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(PublicationRole.Approver, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NoOldPublicationRoles_MultipleReleaseRoles_SingleReleaseVersion_HasNewRole(
            PublicationRole existingNewPublicationRole
        )
        {
            // Has release Contributor & Approver
            // Should remove/create the expected NEW roles, but always end up in the state of having the NEW Approver role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(existingNewPublicationRole);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRole);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and should have ended up with the NEW Approver role.
                // If they originally had a Drafter role, it should have been removed.
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Contributor);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Approver);

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(PublicationRole.Approver, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Fact]
        public async Task NoOldPublicationRoles_MultipleReleaseRoles_AcrossMultipleReleaseVersions()
        {
            // Has release Contributor & Approver
            // Should create the NEW role of Approver

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Approver))
                .ForIndex(2, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
                .ForIndex(3, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                .GenerateList(4);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 4 release roles, and should have ended up with the NEW Approver role.
                Assert.Equal(4, remainingUserReleaseRoles.Count);
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion1.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion1.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion2.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion2.Id
                );

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(PublicationRole.Approver, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NoOldPublicationRoles_MultipleReleaseRoles_AcrossMultipleReleaseVersions_HasNewRole(
            PublicationRole existingNewPublicationRole
        )
        {
            // Has release Contributor & Approver
            // Should remove/create the expected NEW roles, but always end up in the state of having the NEW Approver role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(existingNewPublicationRole);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Approver))
                .ForIndex(2, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Contributor))
                .ForIndex(3, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                .GenerateList(4);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 4 release roles, and should have ended up with the NEW Approver role.
                // If they originally had a Drafter role, it should have been removed.
                Assert.Equal(4, remainingUserReleaseRoles.Count);
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion1.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion1.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion2.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion2.Id
                );

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(PublicationRole.Approver, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Fact]
        public async Task NoOldPublicationRoles_DifferentReleaseRolesAcrossDifferentReleaseVersions()
        {
            // Has release Contributor for one release version, and release Approver for another release version
            // Should create the NEW role of Approver

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and should have ended up with the NEW Approver role.
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion1.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion2.Id
                );

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(PublicationRole.Approver, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task NoOldPublicationRoles_DifferentReleaseRolesAcrossDifferentReleaseVersions_HasNewRole(
            PublicationRole existingNewPublicationRole
        )
        {
            // Has release Contributor for one release version, and release Approver for another release version
            // Should remove/create the expected NEW roles, but always end up in the state of having the NEW Approver role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(existingNewPublicationRole);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and should have ended up with the NEW Approver role.
                // If they originally had a Drafter role, it should have been removed.
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion1.Id
                );
                Assert.Contains(
                    remainingUserReleaseRoles,
                    urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion2.Id
                );

                var remainingUserPublicationRole = Assert.Single(remainingUserPublicationRoles);
                Assert.Equal(PublicationRole.Approver, remainingUserPublicationRole.Role);
                Assert.Equal(user.Id, remainingUserPublicationRole.UserId);
                Assert.Equal(publication.Id, remainingUserPublicationRole.PublicationId);
            }
        }

        [Theory]
        [InlineData(PublicationRole.Owner, ReleaseRole.Contributor, PublicationRole.Drafter)]
        [InlineData(PublicationRole.Owner, ReleaseRole.Approver, PublicationRole.Approver)]
        [InlineData(PublicationRole.Allower, ReleaseRole.Contributor, PublicationRole.Approver)]
        [InlineData(PublicationRole.Allower, ReleaseRole.Approver, PublicationRole.Approver)]
        public async Task SingleOldPublicationRole_SingleReleaseRole(
            PublicationRole oldPublicationRole,
            ReleaseRole releaseRole,
            PublicationRole expectedNewPublicationRoleToCreate
        )
        {
            // Should create the expected NEW role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(oldPublicationRole);
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRole);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the release role, and the existing OLD publication role.
                // But should have also ended up with the expected NEW publication role.
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(releaseRole, remainingUserReleaseRole.Role);

                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == oldPublicationRole);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == expectedNewPublicationRoleToCreate);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(PublicationRole.Owner, ReleaseRole.Contributor, PublicationRole.Drafter, null, null)]
        [InlineData(
            PublicationRole.Owner,
            ReleaseRole.Contributor,
            PublicationRole.Approver,
            PublicationRole.Approver,
            PublicationRole.Drafter
        )]
        [InlineData(
            PublicationRole.Owner,
            ReleaseRole.Approver,
            PublicationRole.Drafter,
            PublicationRole.Drafter,
            PublicationRole.Approver
        )]
        [InlineData(PublicationRole.Owner, ReleaseRole.Approver, PublicationRole.Approver, null, null)]
        [InlineData(
            PublicationRole.Allower,
            ReleaseRole.Contributor,
            PublicationRole.Drafter,
            PublicationRole.Drafter,
            PublicationRole.Approver
        )]
        [InlineData(PublicationRole.Allower, ReleaseRole.Contributor, PublicationRole.Approver, null, null)]
        [InlineData(
            PublicationRole.Allower,
            ReleaseRole.Approver,
            PublicationRole.Drafter,
            PublicationRole.Drafter,
            PublicationRole.Approver
        )]
        [InlineData(PublicationRole.Allower, ReleaseRole.Approver, PublicationRole.Approver, null, null)]
        public async Task SingleOldPublicationRole_SingleReleaseRole_HasNewRole(
            PublicationRole oldPublicationRole,
            ReleaseRole releaseRole,
            PublicationRole existingNewPublicationRole,
            PublicationRole? expectedNewPublicationRoleToRemove,
            PublicationRole? expectedNewPublicationRoleToCreate
        )
        {
            // Should remove/create the expected NEW roles

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(oldPublicationRole))
                .ForIndex(1, s => s.SetRole(existingNewPublicationRole))
                .GenerateList(2);
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRole);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the release role, and the existing OLD publication role.
                // But should have also ended up with the expected NEW publication role, and any expected NEW role removals
                // should have taken place.
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(releaseRole, remainingUserReleaseRole.Role);

                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == oldPublicationRole);

                if (expectedNewPublicationRoleToRemove.HasValue)
                {
                    Assert.DoesNotContain(
                        remainingUserPublicationRoles,
                        upr => upr.Role == expectedNewPublicationRoleToRemove
                    );
                }

                if (expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == expectedNewPublicationRoleToCreate
                    );
                }

                if (!expectedNewPublicationRoleToRemove.HasValue && !expectedNewPublicationRoleToCreate.HasValue)
                {
                    Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == existingNewPublicationRole);
                }

                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(PublicationRole.Owner)]
        [InlineData(PublicationRole.Allower)]
        public async Task SingleOldPublicationRole_MultipleReleaseRoles(PublicationRole oldPublicationRole)
        {
            // Has release Contributor & Approver
            // Should create the expected NEW role of Approver

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(oldPublicationRole);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and the existing OLD publication role.
                // But should have also ended up with the NEW publication Approver role
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Contributor);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Approver);

                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == oldPublicationRole);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(PublicationRole.Owner, PublicationRole.Drafter)]
        [InlineData(PublicationRole.Owner, PublicationRole.Approver)]
        [InlineData(PublicationRole.Allower, PublicationRole.Drafter)]
        [InlineData(PublicationRole.Allower, PublicationRole.Approver)]
        public async Task SingleOldPublicationRole_MultipleReleaseRoles_HasNewRole(
            PublicationRole oldPublicationRole,
            PublicationRole existingNewPublicationRole
        )
        {
            // Has release Contributor & Approver
            // Should remove/create the expected NEW roles, but always end up in the state of having the NEW Approver role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(oldPublicationRole))
                .ForIndex(1, s => s.SetRole(existingNewPublicationRole))
                .GenerateList(2);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and the existing OLD publication role.
                // But should have also ended up with the NEW publication Approver role.
                // If they originally had the Drafter role, it should have been removed
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Contributor);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Approver);

                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == oldPublicationRole);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(ReleaseRole.Contributor)]
        [InlineData(ReleaseRole.Approver)]
        public async Task MultipleOldPublicationRoles_SingleReleaseRole(ReleaseRole releaseRole)
        {
            // Has publication Owner & Allower
            // Should create the expected NEW role of Approver

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .GenerateList(2);
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRole);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the release role, and the existing 2 OLD publication roles.
                // But should have also ended up with the NEW publication Approver role.
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(releaseRole, remainingUserReleaseRole.Role);

                Assert.Equal(3, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Owner);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Allower);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(ReleaseRole.Contributor, PublicationRole.Drafter)]
        [InlineData(ReleaseRole.Contributor, PublicationRole.Approver)]
        [InlineData(ReleaseRole.Approver, PublicationRole.Drafter)]
        [InlineData(ReleaseRole.Approver, PublicationRole.Approver)]
        public async Task MultipleOldPublicationRoles_SingleReleaseRole_HasNewRole(
            ReleaseRole releaseRole,
            PublicationRole existingOldPublicationRole
        )
        {
            // Has publication Owner & Allower
            // Should remove/create the expected NEW roles, but always end up in the state of having the NEW Approver role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .ForIndex(2, s => s.SetRole(existingOldPublicationRole))
                .GenerateList(3);
            UserReleaseRole userReleaseRole = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .WithRole(releaseRole);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                context.UserReleaseRoles.Add(userReleaseRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the release role, and the existing 2 OLD publication roles.
                // But should have also ended up with the NEW publication Approver role.
                // If they originally had the Drafter role, it should have been removed.
                var remainingUserReleaseRole = Assert.Single(remainingUserReleaseRoles);
                Assert.Equal(releaseRole, remainingUserReleaseRole.Role);

                Assert.Equal(3, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Owner);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Allower);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }

        [Fact]
        public async Task MultipleOldPublicationRoles_MultipleReleaseRoles()
        {
            // Has release Contributor & Approver
            // Has publication Owner & Allower
            // Should create the expected NEW role of Approver

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .GenerateList(2);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and the existing 2 OLD publication roles.
                // But should have also ended up with the NEW publication Approver role.
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Contributor);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Approver);

                Assert.Equal(3, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Owner);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Allower);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }

        [Theory]
        [InlineData(PublicationRole.Drafter)]
        [InlineData(PublicationRole.Approver)]
        public async Task MultipleOldPublicationRoles_MultipleReleaseRoles_HasNewRole(
            PublicationRole existingOldPublicationRole
        )
        {
            // Has release Contributor & Approver
            // Has publication Owner & Allower
            // Should remove/create the expected NEW roles, but always end up in the state of having the NEW Approver role

            User user = _dataFixture.DefaultUser();
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            var userPublicationRoles = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .ForIndex(0, s => s.SetRole(PublicationRole.Owner))
                .ForIndex(1, s => s.SetRole(PublicationRole.Allower))
                .ForIndex(2, s => s.SetRole(existingOldPublicationRole))
                .GenerateList(3);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, s => s.SetRole(ReleaseRole.Contributor))
                .ForIndex(1, s => s.SetRole(ReleaseRole.Approver))
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.AddRange(userPublicationRoles);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                // Should still have the 2 release roles, and the existing 2 OLD publication roles.
                // But should have also ended up with the NEW publication Approver role.
                // If they originally had the Drafter role, it should have been removed.
                Assert.Equal(2, remainingUserReleaseRoles.Count);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Contributor);
                Assert.Contains(remainingUserReleaseRoles, urr => urr.Role == ReleaseRole.Approver);

                Assert.Equal(3, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Owner);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Allower);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Approver);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }
    }

    public class MultipleUserPublicationPairsTests : UserResourceRolesMigrationServiceTests
    {
        public class SameUserDifferentPublicationsTests : UserResourceRolesMigrationServiceTests
        {
            [Fact]
            public async Task OnePairHasNoRoles()
            {
                // Only the pair with roles should have NEW roles created

                User user = _dataFixture.DefaultUser();
                Publication publication1 = _dataFixture.DefaultPublication();
                Publication publication2 = _dataFixture.DefaultPublication();
                UserPublicationRole userPublicationRole = _dataFixture
                    .DefaultUserPublicationRole()
                    .WithUser(user)
                    .WithPublication(publication1)
                    .WithRole(PublicationRole.Owner);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Publications.AddRange(publication1, publication2);
                    context.UserPublicationRoles.Add(userPublicationRole);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    // Should still have the OLD publication role.
                    // But should have also ended up with the NEW publication Drafter role for Publication 1.
                    // Publication 2 should still have no roles.
                    Assert.Equal(2, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Owner && upr.PublicationId == publication1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.PublicationId == publication1.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(user.Id, upr.UserId);
                        }
                    );
                }
            }

            [Fact]
            public async Task NoReleaseRoles_DifferentPublicationRoles()
            {
                // Creates the expected NEW roles for both pairs, based on the different publication roles each pair has

                User user = _dataFixture.DefaultUser();
                Publication publication1 = _dataFixture.DefaultPublication();
                Publication publication2 = _dataFixture.DefaultPublication();
                var userPublicationRoles = _dataFixture
                    .DefaultUserPublicationRole()
                    .WithUser(user)
                    .ForIndex(0, s => s.SetPublication(publication1).SetRole(PublicationRole.Owner))
                    .ForIndex(1, s => s.SetPublication(publication2).SetRole(PublicationRole.Allower))
                    .GenerateList(2);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.UserPublicationRoles.AddRange(userPublicationRoles);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    // Should still have the OLD publication role for each publication.
                    // But Publication 1 should have had the NEW Drafter role added
                    // And Publication 2 should have had the NEW Approver role added
                    Assert.Equal(4, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Owner && upr.PublicationId == publication1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.PublicationId == publication1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Allower && upr.PublicationId == publication2.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Approver && upr.PublicationId == publication2.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(user.Id, upr.UserId);
                        }
                    );
                }
            }

            [Fact]
            public async Task NoPublicationRoles_DifferentReleaseRoles()
            {
                // Creates the expected NEW roles for both pairs, based on the different release roles each pair has

                User user = _dataFixture.DefaultUser();
                Publication publication1 = _dataFixture.DefaultPublication();
                Publication publication2 = _dataFixture.DefaultPublication();
                ReleaseVersion releaseVersion1 = _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication1));
                ReleaseVersion releaseVersion2 = _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication2));
                var userReleaseRoles = _dataFixture
                    .DefaultUserReleaseRole()
                    .WithUser(user)
                    .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                    .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                    .GenerateList(2);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.UserReleaseRoles.AddRange(userReleaseRoles);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                    // Should still have the existing release role for each publication.
                    // But Publication 1 should have had the NEW Drafter role added
                    // And Publication 2 should have had the NEW Approver role added
                    Assert.Equal(2, remainingUserReleaseRoles.Count);
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion1.Id
                    );
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion2.Id
                    );

                    Assert.Equal(2, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.PublicationId == publication1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Approver && upr.PublicationId == publication2.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(user.Id, upr.UserId);
                        }
                    );
                }
            }

            [Fact]
            public async Task DifferentPublicationRoles_DifferentReleaseRoles()
            {
                // Creates the expected NEW roles for both pairs, based on the different publication/release roles each pair has

                User user = _dataFixture.DefaultUser();
                Publication publication1 = _dataFixture.DefaultPublication();
                Publication publication2 = _dataFixture.DefaultPublication();
                ReleaseVersion releaseVersion1 = _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication1));
                ReleaseVersion releaseVersion2 = _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication2));
                var userPublicationRoles = _dataFixture
                    .DefaultUserPublicationRole()
                    .WithUser(user)
                    .ForIndex(0, s => s.SetPublication(publication1).SetRole(PublicationRole.Owner))
                    .ForIndex(1, s => s.SetPublication(publication2).SetRole(PublicationRole.Allower))
                    .GenerateList(2);
                var userReleaseRoles = _dataFixture
                    .DefaultUserReleaseRole()
                    .WithUser(user)
                    .ForIndex(0, s => s.SetReleaseVersion(releaseVersion1).SetRole(ReleaseRole.Contributor))
                    .ForIndex(1, s => s.SetReleaseVersion(releaseVersion2).SetRole(ReleaseRole.Approver))
                    .GenerateList(2);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.UserPublicationRoles.AddRange(userPublicationRoles);
                    context.UserReleaseRoles.AddRange(userReleaseRoles);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                    // Should still have the existing release role, and OLD publication role for each publication.
                    // But Publication 1 should have had the NEW Drafter role added
                    // And Publication 2 should have had the NEW Approver role added
                    Assert.Equal(2, remainingUserReleaseRoles.Count);
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Contributor && urr.ReleaseVersionId == releaseVersion1.Id
                    );
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Approver && urr.ReleaseVersionId == releaseVersion2.Id
                    );

                    Assert.Equal(4, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Owner && upr.PublicationId == publication1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.PublicationId == publication1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Allower && upr.PublicationId == publication2.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Approver && upr.PublicationId == publication2.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(user.Id, upr.UserId);
                        }
                    );
                }
            }

            // No point testing combinations where they have the same roles, because here we're trying to test that the two pairs are treated independently; so we need them to be different to test that.
        }

        public class DifferentUsersSamePublicationTests : UserResourceRolesMigrationServiceTests
        {
            [Fact]
            public async Task OnePairHasNoRoles()
            {
                // Only the pair with roles should have NEW roles created

                User user1 = _dataFixture.DefaultUser();
                User user2 = _dataFixture.DefaultUser();
                Publication publication = _dataFixture.DefaultPublication();
                UserPublicationRole userPublicationRole = _dataFixture
                    .DefaultUserPublicationRole()
                    .WithUser(user1)
                    .WithPublication(publication)
                    .WithRole(PublicationRole.Owner);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.Users.AddRange(user1, user2);
                    context.UserPublicationRoles.Add(userPublicationRole);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    // Should still have the OLD publication role.
                    // But should have also ended up with the NEW publication Drafter role for User 1.
                    // User 2 should still have no roles.
                    Assert.Equal(2, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Owner && upr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.UserId == user1.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(publication.Id, upr.PublicationId);
                        }
                    );
                }
            }

            [Fact]
            public async Task NoReleaseRoles_DifferentPublicationRoles()
            {
                // Creates the expected NEW roles for both pairs, based on the different publication roles each pair has

                User user1 = _dataFixture.DefaultUser();
                User user2 = _dataFixture.DefaultUser();
                Publication publication = _dataFixture.DefaultPublication();
                var userPublicationRoles = _dataFixture
                    .DefaultUserPublicationRole()
                    .WithPublication(publication)
                    .ForIndex(0, s => s.SetUser(user1).SetRole(PublicationRole.Owner))
                    .ForIndex(1, s => s.SetUser(user2).SetRole(PublicationRole.Allower))
                    .GenerateList(2);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.UserPublicationRoles.AddRange(userPublicationRoles);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    // Should still have the OLD publication role for each User.
                    // But User 1 should have had the NEW Drafter role added
                    // And User 2 should have had the NEW Approver role added
                    Assert.Equal(4, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Owner && upr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Allower && upr.UserId == user2.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Approver && upr.UserId == user2.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(publication.Id, upr.PublicationId);
                        }
                    );
                }
            }

            [Fact]
            public async Task NoPublicationRoles_DifferentReleaseRoles()
            {
                // Creates the expected NEW roles for both pairs, based on the different release roles each pair has

                User user1 = _dataFixture.DefaultUser();
                User user2 = _dataFixture.DefaultUser();
                Publication publication = _dataFixture.DefaultPublication();
                ReleaseVersion releaseVersion = _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
                var userReleaseRoles = _dataFixture
                    .DefaultUserReleaseRole()
                    .WithReleaseVersion(releaseVersion)
                    .ForIndex(0, s => s.SetUser(user1).SetRole(ReleaseRole.Contributor))
                    .ForIndex(1, s => s.SetUser(user2).SetRole(ReleaseRole.Approver))
                    .GenerateList(2);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.UserReleaseRoles.AddRange(userReleaseRoles);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                    // Should still have the existing release role for each User.
                    // But User 1 should have had the NEW Drafter role added
                    // And User 2 should have had the NEW Approver role added
                    Assert.Equal(2, remainingUserReleaseRoles.Count);
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Contributor && urr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Approver && urr.UserId == user2.Id
                    );

                    Assert.Equal(2, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Approver && upr.UserId == user2.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(publication.Id, upr.PublicationId);
                        }
                    );
                }
            }

            [Fact]
            public async Task DifferentPublicationRoles_DifferentReleaseRoles()
            {
                // Creates the expected NEW roles for both pairs, based on the different publication/release roles each pair has

                User user1 = _dataFixture.DefaultUser();
                User user2 = _dataFixture.DefaultUser();
                Publication publication = _dataFixture.DefaultPublication();
                ReleaseVersion releaseVersion = _dataFixture
                    .DefaultReleaseVersion()
                    .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
                var userPublicationRoles = _dataFixture
                    .DefaultUserPublicationRole()
                    .WithPublication(publication)
                    .ForIndex(0, s => s.SetUser(user1).SetRole(PublicationRole.Owner))
                    .ForIndex(1, s => s.SetUser(user2).SetRole(PublicationRole.Allower))
                    .GenerateList(2);
                var userReleaseRoles = _dataFixture
                    .DefaultUserReleaseRole()
                    .WithReleaseVersion(releaseVersion)
                    .ForIndex(0, s => s.SetUser(user1).SetRole(ReleaseRole.Contributor))
                    .ForIndex(1, s => s.SetUser(user2).SetRole(ReleaseRole.Approver))
                    .GenerateList(2);

                var contextId = Guid.NewGuid().ToString();
                await using (var context = InMemoryContentDbContext(contextId))
                {
                    context.UserPublicationRoles.AddRange(userPublicationRoles);
                    context.UserReleaseRoles.AddRange(userReleaseRoles);
                    await context.SaveChangesAsync();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var service = BuildService(context);

                    var result = await service.MigrateUserResourceRoles(dryRun: false);

                    result.AssertRight();
                }

                await using (var context = InMemoryContentDbContext(contextId))
                {
                    var remainingUserPublicationRoles = await context
                        .UserPublicationRoles.IgnoreQueryFilters()
                        .ToListAsync();

                    var remainingUserReleaseRoles = await context.UserReleaseRoles.ToListAsync();

                    // Should still have the existing release role, and OLD publication role for each User.
                    // But User 1 should have had the NEW Drafter role added
                    // And User 2 should have had the NEW Approver role added
                    Assert.Equal(2, remainingUserReleaseRoles.Count);
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Contributor && urr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserReleaseRoles,
                        urr => urr.Role == ReleaseRole.Approver && urr.UserId == user2.Id
                    );

                    Assert.Equal(4, remainingUserPublicationRoles.Count);
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Owner && upr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Drafter && upr.UserId == user1.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Allower && upr.UserId == user2.Id
                    );
                    Assert.Contains(
                        remainingUserPublicationRoles,
                        upr => upr.Role == PublicationRole.Approver && upr.UserId == user2.Id
                    );
                    Assert.All(
                        remainingUserPublicationRoles,
                        upr =>
                        {
                            Assert.Equal(publication.Id, upr.PublicationId);
                        }
                    );
                }
            }

            // No point testing combinations where they have the same roles, because here we're trying to test that the two pairs are treated independently; so we need them to be different to test that.
        }
    }

    public class MiscellaneousTests : UserResourceRolesMigrationServiceTests
    {
        public static readonly TheoryData<DateTime, DateTime, DateTime> RoleCreationDates = new()
        {
            // { old publication role creation date, release role 1 creation date, release role 2 creation date }
            { DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2) },
            { DateTime.UtcNow, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1) },
            { DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2), DateTime.UtcNow },
            { DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), DateTime.UtcNow },
            { DateTime.UtcNow.AddDays(-2), DateTime.UtcNow, DateTime.UtcNow.AddDays(-1) },
            { DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, DateTime.UtcNow.AddDays(-2) },
        };

        public static readonly TheoryData<Func<DataFixture, User>> AllTypesOfUser =
        [
            // Active User
            fixture => fixture.DefaultUser(),
            // User with Pending Invite
            fixture => fixture.DefaultUserWithPendingInvite(),
            // User with Expired Invite
            fixture => fixture.DefaultUserWithExpiredInvite(),
            // Soft Deleted User (These ones shouldn't ever have associated roles, as they
            // get removed when the user is soft deleted. But is added here for completeness)
            fixture => fixture.DefaultSoftDeletedUser(),
        ];

        [Theory]
        [MemberData(nameof(RoleCreationDates))]
        public async Task CreatesNewPublicationRoleWithPropertiesMatchingEarliestOldRoleWhichMapsToIt(
            DateTime oldPublicationRoleCreatedDate,
            DateTime releaseRole1CreatedDate,
            DateTime releaseRole2CreatedDate
        )
        {
            // Should set the Created date, CreatedById, and the EmailSent date to be that of the earliest created old role which maps to the NEW publication role being created

            User user = _dataFixture.DefaultUser();
            User createdByUser1 = _dataFixture.DefaultUser();
            User createdByUser2 = _dataFixture.DefaultUser();
            User createdByUser3 = _dataFixture.DefaultUser();
            DateTimeOffset emailSentDate1 = DateTimeOffset.UtcNow.AddHours(-1);
            DateTimeOffset emailSentDate2 = DateTimeOffset.UtcNow.AddDays(-1).AddHours(-1);
            DateTimeOffset emailSentDate3 = DateTimeOffset.UtcNow.AddDays(-2).AddHours(-1);
            Publication publication = _dataFixture.DefaultPublication();
            ReleaseVersion releaseVersion1 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));
            ReleaseVersion releaseVersion2 = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));

            // Set different Created dates, CreatedBy users, and EmailSent dates for the publication role and release roles,
            // so that we can test that the NEW publication role gets the properties of the earliest created old role
            // (which will vary from test to test based on the different date combinations being passed in)
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Owner)
                .WithCreated(oldPublicationRoleCreatedDate)
                .WithCreatedBy(createdByUser1)
                .WithEmailSent(emailSentDate1);
            var userReleaseRoles = _dataFixture
                .DefaultUserReleaseRole()
                .WithUser(user)
                .WithRole(ReleaseRole.Contributor)
                .ForIndex(
                    0,
                    s =>
                        s.SetReleaseVersion(releaseVersion1)
                            .SetCreated(releaseRole1CreatedDate)
                            .SetCreatedBy(createdByUser2)
                            .SetEmailSent(emailSentDate2)
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetReleaseVersion(releaseVersion2)
                            .SetCreated(releaseRole2CreatedDate)
                            .SetCreatedBy(createdByUser3)
                            .SetEmailSent(emailSentDate3)
                )
                .GenerateList(2);

            var (ExpectedCreated, ExpectedCreatedById, ExpectedEmailSent) = new[]
            {
                (userPublicationRole.Created, userPublicationRole.CreatedById, userPublicationRole.EmailSent),
                (userReleaseRoles[0].Created, userReleaseRoles[0].CreatedById, userReleaseRoles[0].EmailSent),
                (userReleaseRoles[1].Created, userReleaseRoles[1].CreatedById, userReleaseRoles[1].EmailSent),
            }.MinBy(o => o.Created);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                context.UserReleaseRoles.AddRange(userReleaseRoles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should have had the NEW Drafter role added
                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(
                    remainingUserPublicationRoles,
                    upr =>
                        upr.Role == PublicationRole.Drafter
                        && upr.UserId == user.Id
                        && upr.PublicationId == publication.Id
                        && upr.Created == ExpectedCreated
                        && upr.CreatedById == ExpectedCreatedById
                        && upr.EmailSent == ExpectedEmailSent
                );
            }
        }

        [Theory]
        [MemberData(nameof(AllTypesOfUser))]
        public async Task ShouldProcessAllTypesOfUser(Func<DataFixture, User> userFactory)
        {
            // Should process all types of user (active, pending invite, expired invite, soft deleted)

            var user = userFactory(_dataFixture);
            Publication publication = _dataFixture.DefaultPublication();
            UserPublicationRole userPublicationRole = _dataFixture
                .DefaultUserPublicationRole()
                .WithUser(user)
                .WithPublication(publication)
                .WithRole(PublicationRole.Owner);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.UserPublicationRoles.Add(userPublicationRole);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var service = BuildService(context);

                var result = await service.MigrateUserResourceRoles(dryRun: false);

                result.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var remainingUserPublicationRoles = await context
                    .UserPublicationRoles.IgnoreQueryFilters()
                    .ToListAsync();

                // Should still have the existing OLD publication role.
                // But should have had the NEW Drafter role added
                Assert.Equal(2, remainingUserPublicationRoles.Count);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Owner);
                Assert.Contains(remainingUserPublicationRoles, upr => upr.Role == PublicationRole.Drafter);
                Assert.All(
                    remainingUserPublicationRoles,
                    upr =>
                    {
                        Assert.Equal(user.Id, upr.UserId);
                        Assert.Equal(publication.Id, upr.PublicationId);
                    }
                );
            }
        }
    }

    private static UserResourceRolesMigrationService BuildService(ContentDbContext? contentDbContext = null)
    {
        contentDbContext ??= InMemoryContentDbContext();

        var newPermissionsSystemHelper = new NewPermissionsSystemHelper();

        var userReleaseRoleQueryRepository = new UserReleaseRoleQueryRepository(contentDbContext);

        var userRepository = new UserRepository(contentDbContext);

        var userService = MockUtils.AlwaysTrueUserService().Object;

        var userPublicationRoleRepository = new UserPublicationRoleRepository(
            contentDbContext: contentDbContext,
            newPermissionsSystemHelper: newPermissionsSystemHelper,
            userReleaseRoleQueryRepository: userReleaseRoleQueryRepository,
            userRepository: userRepository
        );

        var userReleaseRoleRepository = new UserReleaseRoleRepository(
            contentDbContext: contentDbContext,
            newPermissionsSystemHelper: newPermissionsSystemHelper,
            userPublicationRoleRepository: userPublicationRoleRepository,
            userReleaseRoleQueryRepository: userReleaseRoleQueryRepository,
            userRepository: userRepository
        );

        return new(
            contentDbContext: contentDbContext,
            userPublicationRoleRepository: userPublicationRoleRepository,
            userReleaseRoleRepository: userReleaseRoleRepository,
            userService: userService
        );
    }
}
