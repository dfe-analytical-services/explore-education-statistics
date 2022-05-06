#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Security.AuthorizationHandlers;

public class ViewSubjectDataForPublishedReleasesAuthorizationHandlerTests
{
    [Fact]
    public async Task ReleaseSubjectIsNotPublished()
    {
        var publication = new Publication();

        var previousVersion = new Release
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            Published = DateTime.UtcNow,
            PreviousVersionId = null
        };

        var latestVersion = new Release
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            Published = null,
            PreviousVersionId = previousVersion.Id
        };

        var releaseSubject = new ReleaseSubject
        {
            ReleaseId = latestVersion.Id,
            SubjectId = Guid.NewGuid()
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.Releases.AddRangeAsync(previousVersion, latestVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            var handler = new ViewSubjectDataForPublishedReleasesAuthorizationHandler(contentDbContext);

            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] {Activator.CreateInstance<ViewSubjectDataRequirement>()},
                null,
                releaseSubject);

            await handler.HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
        }
    }

    [Fact]
    public async Task ReleaseSubjectIsLatestPublishedVersion()
    {
        var publication = new Publication();

        var previousVersion = new Release
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            Published = DateTime.UtcNow,
            PreviousVersionId = null
        };

        var latestPublishedVersion = new Release
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            Published = DateTime.UtcNow,
            PreviousVersionId = previousVersion.Id
        };

        var latestDraftVersion = new Release
        {
            Id = Guid.NewGuid(),
            Publication = publication,
            Published = null,
            PreviousVersionId = latestPublishedVersion.Id
        };

        var releaseSubject = new ReleaseSubject
        {
            ReleaseId = latestPublishedVersion.Id,
            SubjectId = Guid.NewGuid()
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            await contentDbContext.Publications.AddAsync(publication);
            await contentDbContext.Releases.AddRangeAsync(previousVersion, latestPublishedVersion, latestDraftVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        {
            var handler = new ViewSubjectDataForPublishedReleasesAuthorizationHandler(contentDbContext);

            var authContext = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] {Activator.CreateInstance<ViewSubjectDataRequirement>()},
                null,
                releaseSubject);

            await handler.HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }
    }
}
