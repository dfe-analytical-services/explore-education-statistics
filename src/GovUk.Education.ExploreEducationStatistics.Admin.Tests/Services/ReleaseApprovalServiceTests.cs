#nullable enable
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseApprovalServiceTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateReleaseStatus()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        var nextReleaseDateEdited = new PartialDate
        {
            Day = "1",
            Month = "1",
            Year = "2040",
        };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context, contentService: contentService.Object);

            await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                    NextReleaseDate = nextReleaseDateEdited,
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    InternalReleaseNote = "Test note",
                }
            );

            VerifyAllMocks(contentService);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            Assert.Equal(ReleaseApprovalStatus.Draft, saved.ApprovalStatus);
            Assert.Equal(new DateTime(2051, 6, 29, 23, 0, 0, DateTimeKind.Utc), saved.PublishScheduled);
            Assert.Equal(nextReleaseDateEdited, saved.NextReleaseDate);
            // NotifySubscribers should default to true for first release versions
            Assert.True(saved.NotifySubscribers);
            Assert.False(saved.UpdatePublishedDate);

            var savedStatus = Assert.Single(saved.ReleaseStatuses);
            Assert.Equal(releaseVersion.Id, savedStatus.ReleaseVersionId);
            Assert.Equal(ReleaseApprovalStatus.Draft, savedStatus.ApprovalStatus);
            Assert.Equal(_userId, savedStatus.CreatedById);
            Assert.Equal("Test note", savedStatus.InternalReleaseNote);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_FailsOnChecklistErrors()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        releaseChecklistService
            .Setup(s => s.GetErrors(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
            .ReturnsAsync(
                new List<ReleaseChecklistIssue>
                {
                    new(DataFileImportsMustBeCompleted),
                    new(DataFileReplacementsMustBeCompleted),
                }
            );

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                context,
                releaseChecklistService: releaseChecklistService.Object,
                contentService: contentService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            VerifyAllMocks(releaseChecklistService, contentService);

            result.AssertBadRequest(DataFileImportsMustBeCompleted, DataFileReplacementsMustBeCompleted);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_FailsNullPublishDate()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context);

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = null,
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            result.AssertBadRequest(PublishDateCannotBeEmpty);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_FailsEmptyPublishDate()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context);

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "",
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            result.AssertBadRequest(PublishDateCannotBeEmpty);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsPublishDateCannotBeScheduled_PreviousDay()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        // Set up the current time in UTC
        var timeProvider = new FakeTimeProvider(
            DateTime.Parse("2023-01-01T00:00:00Z", styles: DateTimeStyles.RoundtripKind)
        );

        // Set up the cron schedules for publishing
        var options = new ReleaseApprovalOptions
        {
            StageScheduledReleasesFunctionCronSchedule = "0 0 0 * * *", // Next occurrence 2023-01-01T00:00:00Z
            PublishScheduledReleasesFunctionCronSchedule = "0 30 9 * * *", // Next occurrence 2023-01-01T09:30:00Z
        };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context, timeProvider: timeProvider, options: options.ToOptionsWrapper());

            // Request a publish day which is earlier than today
            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2022-12-31",
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            // Expect this to fail because there's no occurrence of the first function until 2023-01-01T00:00:00Z
            // A publish date of 2022-12-31 needs to be approved before 2022-12-31T00:00:00Z
            result.AssertBadRequest(PublishDateCannotBeScheduled);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsPublishDateCannotBeScheduled_PreviousDay_DaylightSavingTime()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        // Set up a current time in UTC which crosses a day boundary in British Summer Time.
        // Date is 6th June UTC but 7th June in British Summer Time which is the timezone we expect the user to be
        // located in and the Publisher functions to be running in.
        var timeProvider = new FakeTimeProvider(
            DateTime.Parse("2023-06-06T23:00:00Z", styles: DateTimeStyles.RoundtripKind)
        );

        // Set up the cron schedules for publishing
        var options = new ReleaseApprovalOptions
        {
            StageScheduledReleasesFunctionCronSchedule = "0 0 0 * * *", // Next occurrence 2023-06-06T23:00:00Z
            PublishScheduledReleasesFunctionCronSchedule = "0 30 9 * * *", // Next occurrence 2023-06-07T08:30:00Z
        };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context, timeProvider: timeProvider, options: options.ToOptionsWrapper());

            // Request a publish day which is the same as the UTC day but earlier than the BST day
            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2023-06-06",
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            // Expect this to fail because there's no occurrence of the first function until 2023-06-06T23:00:00Z
            // A publish date of 2022-06-06 needs to be approved before 2022-06-05T23:00:00Z
            result.AssertBadRequest(PublishDateCannotBeScheduled);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsPublishDateCannotBeScheduled_SameDay()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        // Set up a current time in UTC after the first publishing function has run
        var timeProvider = new FakeTimeProvider(
            DateTime.Parse("2023-01-01T01:00:00Z", styles: DateTimeStyles.RoundtripKind)
        );

        // Set up the cron schedules for publishing
        var options = new ReleaseApprovalOptions
        {
            StageScheduledReleasesFunctionCronSchedule = "0 0 0 * * *", // Next occurrence 2023-01-02T00:00:00Z
            PublishScheduledReleasesFunctionCronSchedule = "0 30 9 * * *", // Next occurrence 2023-01-01T09:30:00Z
        };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context, timeProvider: timeProvider, options: options.ToOptionsWrapper());

            // Request a publish day which is today
            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2023-01-01",
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            // Expect this to fail because there's no occurrence of the first function until 2023-01-02T00:00:00Z
            // A publish date of 2023-01-01 needs to be approved before 2023-01-01T00:00:00Z
            result.AssertBadRequest(PublishDateCannotBeScheduled);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsPublishDateCannotBeScheduled_SameDay_DaylightSavingTime()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        // Set up a current time in UTC after the first publishing function has run.
        // Based on a time now of 2023-06-06T23:30:00Z the cron schedule in the UK timezone will have had an
        // occurrence at 2023-06-06T23:30:00Z and the next occurrence will not be until 2023-06-07T23:00:00Z.
        var timeProvider = new FakeTimeProvider(
            DateTime.Parse("2023-06-06T23:30:00Z", styles: DateTimeStyles.RoundtripKind)
        );

        // Set up the cron schedules for publishing
        var options = new ReleaseApprovalOptions
        {
            StageScheduledReleasesFunctionCronSchedule = "0 0 0 * * *", // Next occurrence 2023-06-07T23:00:00Z
            PublishScheduledReleasesFunctionCronSchedule = "0 30 9 * * *", // Next occurrence 2023-06-07T08:30:00Z
        };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context, timeProvider: timeProvider, options: options.ToOptionsWrapper());

            // Request a publish day which is today
            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2023-06-07",
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            // Expect this to fail because there's no occurrence of the first function until 2023-06-07T23:00:00Z
            // A publish date of 2023-06-07 needs to be approved before 2023-06-06T23:00:00Z
            result.AssertBadRequest(PublishDateCannotBeScheduled);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsPublishDateCannotBeScheduled_FutureDay()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        // Set up the current time in UTC
        var timeProvider = new FakeTimeProvider(
            DateTime.Parse("2023-01-01T12:00:00Z", styles: DateTimeStyles.RoundtripKind)
        );

        // Set up the cron schedules for publishing
        var options = new ReleaseApprovalOptions
        {
            StageScheduledReleasesFunctionCronSchedule = "0 0 0 * * 1-5", // Only occurs on weekdays Monday - Friday
            PublishScheduledReleasesFunctionCronSchedule = "0 30 9 * * 1-5", // Only occurs on weekdays Monday - Friday
        };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context, timeProvider: timeProvider, options: options.ToOptionsWrapper());

            // Request a publish day in the future which has no scheduled occurrence
            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2023-01-08", // Sunday
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            // Expect this to fail because there's no occurrences of the functions on a Sunday
            // A publish date of 2023-01-08 can't be requested
            result.AssertBadRequest(PublishDateCannotBeScheduled);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsPublishDateCannotBeScheduled_SecondFunctionHasNoOccurrence()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        // Set up the current time in UTC
        var timeProvider = new FakeTimeProvider(
            DateTime.Parse("2023-01-01T23:20:00Z", styles: DateTimeStyles.RoundtripKind)
        );

        // Set up cron schedules for publishing hourly on specific minutes in a way that the first function
        // will have an occurrence on the day, but not the second
        var options = new ReleaseApprovalOptions
        {
            StageScheduledReleasesFunctionCronSchedule = "0 30 * * * *", // Occurs hourly at minute 30
            PublishScheduledReleasesFunctionCronSchedule = "0 15 * * * *", // Occurs hourly at minute 15
        };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(context, timeProvider: timeProvider, options: options.ToOptionsWrapper());

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2023-01-01",
                    NextReleaseDate = new PartialDate { Month = "12", Year = "2000" },
                }
            );

            // Expect this to fail because there's no occurrences of the second function remaining on 2023-01-01.
            // A publish date of 2023-01-01 needs to be approved before 2023-01-01T22:30:00Z
            // in order to be published by the second function at 2023-01-01T23:15:00Z
            result.AssertBadRequest(PublishDateCannotBeScheduled);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsChangingToDraft()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()))
            .WithApprovalStatus(ReleaseApprovalStatus.Approved)
            .WithPublished(DateTime.UtcNow);

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(contentDbContext: context, contentService: contentService.Object);

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    PublishMethod = PublishMethod.Scheduled,
                }
            );

            VerifyAllMocks(contentService);

            result.AssertBadRequest(PublishedReleaseCannotBeUnapproved);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        User existingUser1 = _fixture.DefaultUser().WithEmail("test@test.com");

        var existingUser1Invite = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.PrereleaseViewer,
            Email = existingUser1.Email,
            EmailSent = false,
        };

        User existingUser2 = _fixture.DefaultUser().WithEmail("test2@test.com");

        var existingUser2SentInvite = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.PrereleaseViewer,
            Email = existingUser2.Email,
            EmailSent = true,
        };

        User existingUser3 = _fixture.DefaultUser().WithEmail("test3@test.com");

        var existingUser3NonPreReleaseInvite = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.Contributor,
            Email = existingUser3.Email,
            EmailSent = false,
        };

        var nonExistingUserPreReleaseInvite = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.PrereleaseViewer,
            Email = "nonexistent1@test.com",
            EmailSent = false,
        };

        var nonExistingUserNonPreReleaseInvite = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.Contributor,
            Email = "nonexistent2@test.com",
            EmailSent = false,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.Users.AddRange(existingUser1, existingUser2, existingUser3);
            context.UserReleaseInvites.AddRange(
                existingUser1Invite,
                existingUser2SentInvite,
                existingUser3NonPreReleaseInvite,
                nonExistingUserPreReleaseInvite,
                nonExistingUserNonPreReleaseInvite
            );
            await context.SaveChangesAsync();
        }

        var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
        var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var preReleaseUserService = new Mock<IPreReleaseUserService>(MockBehavior.Strict);
        var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(MockBehavior.Strict);

        releaseChecklistService
            .Setup(s => s.GetErrors(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
            .ReturnsAsync(new List<ReleaseChecklistIssue>());

        publishingService
            .Setup(s =>
                s.ReleaseChanged(
                    It.Is<ReleasePublishingKey>(key => key.ReleaseVersionId == releaseVersion.Id),
                    false,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(Unit.Instance);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        userResourceRoleNotificationService
            .Setup(mock =>
                mock.NotifyUserOfNewPreReleaseRole(
                    existingUser1Invite.Email,
                    releaseVersion.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        userResourceRoleNotificationService
            .Setup(mock =>
                mock.NotifyUserOfNewPreReleaseRole(
                    nonExistingUserPreReleaseInvite.Email,
                    releaseVersion.Id,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        preReleaseUserService
            .Setup(mock =>
                mock.MarkInviteEmailAsSent(It.Is<UserReleaseInvite>(i => i.Email == existingUser1Invite.Email))
            )
            .Returns(Task.CompletedTask);

        preReleaseUserService
            .Setup(mock =>
                mock.MarkInviteEmailAsSent(
                    It.Is<UserReleaseInvite>(i => i.Email == nonExistingUserPreReleaseInvite.Email)
                )
            )
            .Returns(Task.CompletedTask);

        var nextReleaseDateEdited = new PartialDate { Month = "12", Year = "2000" };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                contentDbContext: context,
                releaseChecklistService: releaseChecklistService.Object,
                publishingService: publishingService.Object,
                contentService: contentService.Object,
                preReleaseUserService: preReleaseUserService.Object,
                userResourceRoleNotificationService: userResourceRoleNotificationService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                    NextReleaseDate = nextReleaseDateEdited,
                }
            );

            VerifyAllMocks(
                contentService,
                preReleaseUserService,
                publishingService,
                releaseChecklistService,
                userResourceRoleNotificationService
            );

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            Assert.Equal(ReleaseApprovalStatus.Approved, saved.ApprovalStatus);

            // PublishScheduled should have been set to the scheduled date specified in the request.
            Assert.Equal(new DateTime(2051, 6, 29, 23, 0, 0, DateTimeKind.Utc), saved.PublishScheduled);
            nextReleaseDateEdited.AssertDeepEqualTo(saved.NextReleaseDate);

            // NotifySubscribers should default to true for original releases
            Assert.True(saved.NotifySubscribers);
            Assert.False(saved.UpdatePublishedDate);

            var savedStatus = Assert.Single(saved.ReleaseStatuses);
            Assert.Equal(releaseVersion.Id, savedStatus.ReleaseVersionId);
            Assert.Equal(ReleaseApprovalStatus.Approved, savedStatus.ApprovalStatus);
            Assert.Equal(_userId, savedStatus.CreatedById);
            Assert.Equal("Test note", savedStatus.InternalReleaseNote);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Immediately()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
        var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
        var contentService = new Mock<IContentService>(MockBehavior.Strict);

        releaseChecklistService
            .Setup(s => s.GetErrors(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
            .ReturnsAsync(new List<ReleaseChecklistIssue>());

        publishingService
            .Setup(s =>
                s.ReleaseChanged(
                    It.Is<ReleasePublishingKey>(key => key.ReleaseVersionId == releaseVersion.Id),
                    true,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(Unit.Instance);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        var nextReleaseDateEdited = new PartialDate { Month = "12", Year = "2000" };

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                contentDbContext: context,
                releaseChecklistService: releaseChecklistService.Object,
                publishingService: publishingService.Object,
                contentService: contentService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Immediate,
                    NextReleaseDate = nextReleaseDateEdited,
                }
            );

            // We don't expect PrereleaseUserService.SendPreReleaseUserInviteEmails to be called if the
            // release version is being published immediately.
            VerifyAllMocks(contentService, publishingService, releaseChecklistService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            Assert.Equal(ReleaseApprovalStatus.Approved, saved.ApprovalStatus);

            // PublishScheduled should have been set to "now".
            saved.PublishScheduled.AssertUtcNow();

            nextReleaseDateEdited.AssertDeepEqualTo(saved.NextReleaseDate);
            // NotifySubscribers should default to true for first release versions
            Assert.True(saved.NotifySubscribers);
            Assert.False(saved.UpdatePublishedDate);

            var savedStatus = Assert.Single(saved.ReleaseStatuses);
            Assert.Equal(releaseVersion.Id, savedStatus.ReleaseVersionId);
            Assert.Equal(ReleaseApprovalStatus.Approved, savedStatus.ApprovalStatus);
            Assert.Equal(_userId, savedStatus.CreatedById);
            Assert.Equal("Test note", savedStatus.InternalReleaseNote);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Amendment()
    {
        Publication publication = _fixture
            .DefaultPublication()
            .WithReleases(_ => [_fixture.DefaultRelease(publishedVersions: 1, draftVersion: true)]);

        var amendedReleaseVersion = publication.Releases[0].Versions.Single(rv => rv is { Published: null });

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.Publications.Add(publication);
            await context.SaveChangesAsync();
        }

        var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
        var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
        var contentService = new Mock<IContentService>(MockBehavior.Strict);

        releaseChecklistService
            .Setup(s => s.GetErrors(It.Is<ReleaseVersion>(rv => rv.Id == amendedReleaseVersion.Id)))
            .ReturnsAsync(new List<ReleaseChecklistIssue>());

        publishingService
            .Setup(s =>
                s.ReleaseChanged(
                    It.Is<ReleasePublishingKey>(key => key.ReleaseVersionId == amendedReleaseVersion.Id),
                    false,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(Unit.Instance);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(amendedReleaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                context,
                releaseChecklistService: releaseChecklistService.Object,
                publishingService: publishingService.Object,
                contentService: contentService.Object
            );

            // Alter the approval status to Approved,
            // toggling the values of NotifySubscribers and UpdatePublishedDate from their initial values
            var result = await releaseService.CreateReleaseStatus(
                amendedReleaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    NotifySubscribers = false,
                    UpdatePublishedDate = true,
                }
            );

            VerifyAllMocks(contentService, publishingService, releaseChecklistService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context.ReleaseVersions.SingleAsync(rv => rv.Id == amendedReleaseVersion.Id);

            // Expect NotifySubscribers and UpdatePublishedDate to match the approval request values
            Assert.False(saved.NotifySubscribers);
            Assert.True(saved.UpdatePublishedDate);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Draft_DoNotSendPreReleaseEmails()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            contentService
                .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            var releaseService = BuildService(context, contentService: contentService.Object);

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Draft,
                    InternalReleaseNote = "Test note",
                }
            );

            VerifyAllMocks(contentService);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_ReleaseHasImages()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var imageFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image1.png",
                Type = FileType.Image,
            },
        };

        var imageFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image2.png",
                Type = FileType.Image,
            },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleaseFiles.AddRange(imageFile1, imageFile2);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(MockBehavior.Strict);

        userReleaseRoleService
            .Setup(mock =>
                mock.ListUserReleaseRolesByPublication(ReleaseRole.Approver, releaseVersion.Release.PublicationId)
            )
            .ReturnsAsync(ListOf<UserReleaseRole>());

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(
                new List<HtmlBlock>
                {
                    new()
                    {
                        Body =
                            $@"
    <img src=""/api/releases/{{releaseId}}/images/{imageFile1.File.Id}""/>
    <img src=""/api/releases/{{releaseId}}/images/{imageFile2.File.Id}""/>",
                    },
                }
            );

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                context,
                contentService: contentService.Object,
                userReleaseRoleService: userReleaseRoleService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    PublishScheduled = "2051-06-30",
                    ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                    InternalReleaseNote = "Internal note",
                }
            );

            VerifyAllMocks(contentService, userReleaseRoleService);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_ReleaseHasUnusedImages()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var imageFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image1.png",
                Type = FileType.Image,
            },
        };

        var imageFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image2.png",
                Type = FileType.Image,
            },
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.ReleaseFiles.AddRange(imageFile1, imageFile2);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(MockBehavior.Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        releaseFileService
            .Setup(mock =>
                mock.Delete(releaseVersion.Id, new List<Guid> { imageFile1.File.Id, imageFile2.File.Id }, true)
            )
            .ReturnsAsync(Unit.Instance);

        userReleaseRoleService
            .Setup(mock =>
                mock.ListUserReleaseRolesByPublication(ReleaseRole.Approver, releaseVersion.Release.PublicationId)
            )
            .ReturnsAsync(ListOf<UserReleaseRole>());

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                contentDbContext: context,
                contentService: contentService.Object,
                releaseFileService: releaseFileService.Object,
                userReleaseRoleService: userReleaseRoleService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    PublishScheduled = "2051-06-30",
                    ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                    InternalReleaseNote = "Test internal note",
                }
            );

            releaseFileService.Verify(
                mock => mock.Delete(releaseVersion.Id, new List<Guid> { imageFile1.File.Id, imageFile2.File.Id }, true),
                Times.Once
            );

            VerifyAllMocks(contentService, releaseFileService, userReleaseRoleService);

            result.AssertRight();
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsSendingPreReleaseInviteEmail()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var invite1 = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.PrereleaseViewer,
            Email = "test@test.com",
            EmailSent = false,
        };

        var invite2 = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.PrereleaseViewer,
            Email = "test2@test.com",
            EmailSent = false,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.UserReleaseInvites.AddRange(invite1, invite2);
            await context.SaveChangesAsync();
        }

        var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(MockBehavior.Strict);

        releaseChecklistService
            .Setup(s => s.GetErrors(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
            .ReturnsAsync(new List<ReleaseChecklistIssue>());

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        userResourceRoleNotificationService
            .Setup(mock =>
                mock.NotifyUserOfNewPreReleaseRole(invite1.Email, releaseVersion.Id, It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new EmailSendFailedException(""));

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                contentDbContext: context,
                releaseChecklistService: releaseChecklistService.Object,
                contentService: contentService.Object,
                userResourceRoleNotificationService: userResourceRoleNotificationService.Object
            );

            await Assert.ThrowsAsync<EmailSendFailedException>(async () =>
                await releaseService.CreateReleaseStatus(
                    releaseVersion.Id,
                    new ReleaseStatusCreateRequest
                    {
                        ApprovalStatus = ReleaseApprovalStatus.Approved,
                        InternalReleaseNote = "Test note",
                        PublishMethod = PublishMethod.Scheduled,
                        PublishScheduled = "2051-06-30",
                    }
                )
            );

            VerifyAllMocks(contentService, releaseChecklistService, userResourceRoleNotificationService);
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.HydrateReleaseForChecklist()
                .Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Assert that the failure to send emails prevented the release version from completing approval.
            // The release version should remain unchanged from the original release version.
            // Additionally, no new ReleaseStatus entries were added.
            saved.AssertDeepEqualTo(releaseVersion, ignoreProperties: [rv => rv.Release, rv => rv.Publication]);

            // Furthermore, we have proven that the Publisher was not informed of the change, as it
            // did not complete.
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_Approved_Scheduled_FailsWhileInformingPublisher()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var invite = new UserReleaseInvite
        {
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.PrereleaseViewer,
            Email = "test@test.com",
            EmailSent = false,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.UserReleaseInvites.Add(invite);
            await context.SaveChangesAsync();
        }

        var releaseChecklistService = new Mock<IReleaseChecklistService>(MockBehavior.Strict);
        var publishingService = new Mock<IPublishingService>(MockBehavior.Strict);
        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var preReleaseUserService = new Mock<IPreReleaseUserService>(MockBehavior.Strict);
        var userResourceRoleNotificationService = new Mock<IUserResourceRoleNotificationService>(MockBehavior.Strict);

        releaseChecklistService
            .Setup(s => s.GetErrors(It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)))
            .ReturnsAsync(new List<ReleaseChecklistIssue>());

        userResourceRoleNotificationService
            .Setup(mock =>
                mock.NotifyUserOfNewPreReleaseRole(invite.Email, releaseVersion.Id, It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        preReleaseUserService
            .Setup(mock => mock.MarkInviteEmailAsSent(It.Is<UserReleaseInvite>(i => i.Email == invite.Email)))
            .Returns(Task.CompletedTask);

        publishingService
            .Setup(s =>
                s.ReleaseChanged(
                    It.Is<ReleasePublishingKey>(key => key.ReleaseVersionId == releaseVersion.Id),
                    false,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(new BadRequestResult());

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var releaseService = BuildService(
                contentDbContext: context,
                releaseChecklistService: releaseChecklistService.Object,
                publishingService: publishingService.Object,
                contentService: contentService.Object,
                preReleaseUserService: preReleaseUserService.Object,
                userResourceRoleNotificationService: userResourceRoleNotificationService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    ApprovalStatus = ReleaseApprovalStatus.Approved,
                    InternalReleaseNote = "Test note",
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                }
            );

            VerifyAllMocks(
                contentService,
                publishingService,
                releaseChecklistService,
                preReleaseUserService,
                userResourceRoleNotificationService
            );

            result.AssertLeft();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.HydrateReleaseForChecklist()
                .Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Assert that the failure to notify the Publisher prevented the release version from completing approval.
            // The release version should remain unchanged from the original release version.
            // Additionally, no new ReleaseStatus entries were added.
            saved.AssertDeepEqualTo(releaseVersion, ignoreProperties: [rv => rv.Release, rv => rv.Publication]);

            // We have also shown that unfortunately the invite emails would have been sent out despite the
            // approval failing, but we have more importantly stopped a release version from only having been partially
            // approved.
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_HigherReview_DoesNotSendEmailIfNoReleaseApprovers()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var preReleaseUserService = new Mock<IPreReleaseUserService>(MockBehavior.Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(MockBehavior.Strict);

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            contentService
                .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            userReleaseRoleService
                .Setup(mock =>
                    mock.ListUserReleaseRolesByPublication(ReleaseRole.Approver, releaseVersion.Release.PublicationId)
                )
                .ReturnsAsync(new List<UserReleaseRole>());

            var releaseService = BuildService(
                contentDbContext: context,
                contentService: contentService.Object,
                preReleaseUserService: preReleaseUserService.Object,
                userReleaseRoleService: userReleaseRoleService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                    ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                    InternalReleaseNote = "Test internal note",
                }
            );

            VerifyAllMocks(contentService, preReleaseUserService, userReleaseRoleService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Assert that the release version has moved into Higher Level Review successfully.
            Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, saved.ApprovalStatus);

            // Assert a new ReleaseStatus entry is included.
            var savedStatus = Assert.Single(saved.ReleaseStatuses);
            Assert.Equal(releaseVersion.Id, savedStatus.ReleaseVersionId);
            Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, savedStatus.ApprovalStatus);
            Assert.Equal(_userId, savedStatus.CreatedById);
            Assert.Equal("Test internal note", savedStatus.InternalReleaseNote);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_HigherReview_SendsEmailIfReleaseApprovers()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userReleaseRole1 = new UserReleaseRole
        {
            User = _fixture.DefaultUser().WithEmail("test@test.com"),
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.Approver,
        };

        var userReleaseRole2 = new UserReleaseRole
        {
            User = _fixture.DefaultUser().WithEmail("test2@test.com"),
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.Approver,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(MockBehavior.Strict);
        var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            contentService
                .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            userReleaseRoleService
                .Setup(mock =>
                    mock.ListUserReleaseRolesByPublication(ReleaseRole.Approver, releaseVersion.Release.PublicationId)
                )
                .ReturnsAsync(ListOf(userReleaseRole1, userReleaseRole2));

            emailTemplateService
                .Setup(mock =>
                    mock.SendReleaseHigherReviewEmail(
                        userReleaseRole1.User.Email,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)
                    )
                )
                .Returns(Unit.Instance);

            emailTemplateService
                .Setup(mock =>
                    mock.SendReleaseHigherReviewEmail(
                        userReleaseRole2.User.Email,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)
                    )
                )
                .Returns(Unit.Instance);

            var releaseService = BuildService(
                contentDbContext: context,
                contentService: contentService.Object,
                userReleaseRoleService: userReleaseRoleService.Object,
                emailTemplateService: emailTemplateService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                    ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                    InternalReleaseNote = "Test internal note",
                }
            );

            VerifyAllMocks(contentService, userReleaseRoleService, emailTemplateService);
            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Assert that the release version has moved into Higher Level Review successfully.
            Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, saved.ApprovalStatus);

            // Assert a new ReleaseStatus entry is included.
            var savedStatus = Assert.Single(saved.ReleaseStatuses);
            Assert.Equal(releaseVersion.Id, savedStatus.ReleaseVersionId);
            Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, savedStatus.ApprovalStatus);
            Assert.Equal(_userId, savedStatus.CreatedById);
            Assert.Equal("Test internal note", savedStatus.InternalReleaseNote);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_HigherReview_SendsEmailIfPublicationReleaseApprover()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userPublicationApproverRole = new UserPublicationRole
        {
            User = _fixture.DefaultUser().WithEmail("test@test.com"),
            Publication = releaseVersion.Release.Publication,
            Role = PublicationRole.Allower,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            context.UserPublicationRoles.Add(userPublicationApproverRole);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var contentService = new Mock<IContentService>(MockBehavior.Strict);
            var userReleaseRoleService = new Mock<IUserReleaseRoleService>(MockBehavior.Strict);
            var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

            contentService
                .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            userReleaseRoleService
                .Setup(mock =>
                    mock.ListUserReleaseRolesByPublication(ReleaseRole.Approver, releaseVersion.Release.PublicationId)
                )
                .ReturnsAsync(new List<UserReleaseRole>());

            emailTemplateService
                .Setup(mock =>
                    mock.SendReleaseHigherReviewEmail(
                        userPublicationApproverRole.User.Email,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)
                    )
                )
                .Returns(Unit.Instance);

            var releaseService = BuildService(
                contentDbContext: context,
                contentService: contentService.Object,
                emailTemplateService: emailTemplateService.Object,
                userReleaseRoleService: userReleaseRoleService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                    ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                    InternalReleaseNote = "Test internal note",
                }
            );

            VerifyAllMocks(contentService, userReleaseRoleService, emailTemplateService);

            result.AssertRight();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Assert that the release version has moved into Higher Level Review successfully.
            Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, saved.ApprovalStatus);

            // Assert a new ReleaseStatus entry is included.
            var savedStatus = Assert.Single(saved.ReleaseStatuses);
            Assert.Equal(releaseVersion.Id, savedStatus.ReleaseVersionId);
            Assert.Equal(ReleaseApprovalStatus.HigherLevelReview, savedStatus.ApprovalStatus);
            Assert.Equal(_userId, savedStatus.CreatedById);
            Assert.Equal("Test internal note", savedStatus.InternalReleaseNote);
        }
    }

    [Fact]
    public async Task CreateReleaseStatus_HigherReview_FailsSendingEmail()
    {
        ReleaseVersion releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithRelease(_fixture.DefaultRelease().WithPublication(_fixture.DefaultPublication()));

        var userReleaseRole1 = new UserReleaseRole
        {
            User = _fixture.DefaultUser().WithEmail("test@test.com"),
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.Approver,
        };

        var userReleaseRole2 = new UserReleaseRole
        {
            User = _fixture.DefaultUser().WithEmail("test2@test.com"),
            ReleaseVersion = releaseVersion,
            Role = ReleaseRole.Approver,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.ReleaseVersions.Add(releaseVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IContentService>(MockBehavior.Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(MockBehavior.Strict);
        var emailTemplateService = new Mock<IEmailTemplateService>(MockBehavior.Strict);

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            contentService
                .Setup(mock => mock.GetContentBlocks<HtmlBlock>(releaseVersion.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            userReleaseRoleService
                .Setup(mock =>
                    mock.ListUserReleaseRolesByPublication(ReleaseRole.Approver, releaseVersion.Release.PublicationId)
                )
                .ReturnsAsync(ListOf(userReleaseRole1, userReleaseRole2));

            emailTemplateService
                .Setup(mock =>
                    mock.SendReleaseHigherReviewEmail(
                        userReleaseRole1.User.Email,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)
                    )
                )
                .Returns(Unit.Instance);

            emailTemplateService
                .Setup(mock =>
                    mock.SendReleaseHigherReviewEmail(
                        userReleaseRole2.User.Email,
                        It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id)
                    )
                )
                .Returns(new BadRequestResult());

            var releaseService = BuildService(
                contentDbContext: context,
                contentService: contentService.Object,
                userReleaseRoleService: userReleaseRoleService.Object,
                emailTemplateService: emailTemplateService.Object
            );

            var result = await releaseService.CreateReleaseStatus(
                releaseVersion.Id,
                new ReleaseStatusCreateRequest
                {
                    PublishMethod = PublishMethod.Scheduled,
                    PublishScheduled = "2051-06-30",
                    ApprovalStatus = ReleaseApprovalStatus.HigherLevelReview,
                    InternalReleaseNote = "Test internal note",
                }
            );

            VerifyAllMocks(contentService, userReleaseRoleService, emailTemplateService);
            result.AssertLeft();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var saved = await context
                .ReleaseVersions.Include(rv => rv.ReleaseStatuses)
                .SingleAsync(rv => rv.Id == releaseVersion.Id);

            // Assert that the failure to send emails prevented the release version from completing moving into
            // Higher Level Review.
            Assert.Equal(ReleaseApprovalStatus.Draft, saved.ApprovalStatus);

            // Assert no new ReleaseStatus entries were added.
            Assert.Empty(saved.ReleaseStatuses);
        }
    }

    public class ListReleaseStatuses : ReleaseApprovalServiceTests
    {
        private static readonly User User = new DataFixture().DefaultUser().WithEmail("test@test.com");

        [Fact]
        public async Task Success()
        {
            var (release, otherRelease) = _fixture
                .DefaultRelease()
                .ForIndex(
                    0,
                    r =>
                        r.SetVersions(
                            _fixture
                                .DefaultReleaseVersion()
                                .ForIndex(
                                    0,
                                    rv =>
                                        rv.SetVersion(0)
                                            .SetReleaseStatuses(
                                                _fixture
                                                    .DefaultReleaseStatus()
                                                    .WithCreatedBy(User)
                                                    .ForIndex(0, rs => rs.SetCreated(DateTime.UtcNow.AddDays(-4)))
                                                    .ForIndex(1, rs => rs.SetCreated(DateTime.UtcNow.AddDays(-3)))
                                                    .Generate(2)
                                            )
                                )
                                .ForIndex(
                                    1,
                                    rv =>
                                        rv.SetVersion(1)
                                            .SetReleaseStatuses(
                                                _fixture
                                                    .DefaultReleaseStatus()
                                                    .WithCreatedBy(User)
                                                    .ForIndex(0, rs => rs.SetCreated(DateTime.UtcNow.AddDays(-2)))
                                                    .ForIndex(1, rs => rs.SetCreated(DateTime.UtcNow.AddDays(-1)))
                                                    .Generate(2)
                                            )
                                )
                                .Generate(2)
                        )
                )
                .ForIndex(
                    1,
                    r =>
                        r.SetVersions(
                            _fixture
                                .DefaultReleaseVersion()
                                .WithReleaseStatuses(_fixture.DefaultReleaseStatus().WithCreatedBy(User).Generate(1))
                                .Generate(1)
                        )
                )
                .GenerateTuple2();

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Releases.AddRange(release, otherRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(contentDbContext);
                var result = await releaseService.ListReleaseStatuses(release.Versions[0].Id);

                var expectedReleaseStatuses = release
                    .Versions.SelectMany(rv => rv.ReleaseStatuses)
                    .OrderByDescending(rs => rs.Created)
                    .ToList();

                var viewModels = result.AssertRight();
                Assert.Equal(expectedReleaseStatuses.Count, viewModels.Count);

                Assert.All(
                    expectedReleaseStatuses.Zip(viewModels),
                    tuple =>
                    {
                        var (releaseStatus, viewModel) = tuple;

                        Assert.Multiple(
                            () => Assert.Equal(releaseStatus.Id, viewModel.ReleaseStatusId),
                            () => Assert.Equal(releaseStatus.InternalReleaseNote, viewModel.InternalReleaseNote),
                            () => Assert.Equal(releaseStatus.ApprovalStatus, viewModel.ApprovalStatus),
                            () => Assert.Equal(releaseStatus.Created, viewModel.Created),
                            () => Assert.Equal(releaseStatus.CreatedBy!.Email, viewModel.CreatedByEmail),
                            () => Assert.Equal(releaseStatus.ReleaseVersion.Version, viewModel.ReleaseVersion)
                        );
                    }
                );
            }
        }

        [Fact]
        public async Task ReleaseStatusHasNullCreatedValue_ResultHasNullCreatedValue()
        {
            Release release = _fixture
                .DefaultRelease()
                .WithVersions(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithReleaseStatuses(
                            _fixture.DefaultReleaseStatus().WithCreated().WithCreatedBy(User).Generate(1)
                        )
                        .Generate(1)
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId, updateTimestamps: false))
            {
                contentDbContext.Releases.Add(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(contentDbContext);
                var result = await releaseService.ListReleaseStatuses(release.Versions[0].Id);

                var viewModels = result.AssertRight();

                var viewModel = Assert.Single(viewModels);
                Assert.Null(viewModel.Created);
            }
        }

        [Fact]
        public async Task ReleaseStatusHasNullCreatedByValue_ResultHasNullCreatedByEmailValue()
        {
            Release release = _fixture
                .DefaultRelease()
                .WithVersions(
                    _fixture
                        .DefaultReleaseVersion()
                        .WithReleaseStatuses(_fixture.DefaultReleaseStatus().WithCreatedBy().Generate(1))
                        .Generate(1)
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                contentDbContext.Releases.Add(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(contentDbContext);
                var result = await releaseService.ListReleaseStatuses(release.Versions[0].Id);

                var viewModels = result.AssertRight();

                var viewModel = Assert.Single(viewModels);
                Assert.Null(viewModel.CreatedByEmail);
            }
        }

        [Fact]
        public async Task NoReleaseStatuses_ReturnsEmpty()
        {
            Release release = _fixture.DefaultRelease().WithVersions(_fixture.DefaultReleaseVersion().Generate(1));

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Releases.Add(release);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var releaseService = BuildService(context);
                var result = await releaseService.ListReleaseStatuses(release.Versions[0].Id);

                var viewModels = result.AssertRight();
                Assert.Empty(viewModels);
            }
        }
    }

    private static IOptions<ReleaseApprovalOptions> DefaultReleaseApprovalOptions()
    {
        return new ReleaseApprovalOptions
        {
            StageScheduledReleasesFunctionCronSchedule = "0 0 0 * * *",
            PublishScheduledReleasesFunctionCronSchedule = "0 30 9 * * *",
        }.ToOptionsWrapper();
    }

    private ReleaseApprovalService BuildService(
        ContentDbContext contentDbContext,
        TimeProvider? timeProvider = null,
        IPublishingService? publishingService = null,
        IReleaseFileRepository? releaseFileRepository = null,
        IReleaseFileService? releaseFileService = null,
        IReleaseChecklistService? releaseChecklistService = null,
        IContentService? contentService = null,
        IPreReleaseUserService? preReleaseUserService = null,
        IUserResourceRoleNotificationService? userResourceRoleNotificationService = null,
        IOptions<ReleaseApprovalOptions>? options = null,
        IUserReleaseRoleService? userReleaseRoleService = null,
        IEmailTemplateService? emailTemplateService = null
    )
    {
        var userService = AlwaysTrueUserService();

        userService.Setup(s => s.GetUserId()).Returns(_userId);

        return new ReleaseApprovalService(
            contentDbContext,
            timeProvider ?? new FakeTimeProvider(),
            userService.Object,
            publishingService ?? Mock.Of<IPublishingService>(MockBehavior.Strict),
            releaseChecklistService ?? Mock.Of<IReleaseChecklistService>(MockBehavior.Strict),
            contentService ?? Mock.Of<IContentService>(MockBehavior.Strict),
            preReleaseUserService ?? Mock.Of<IPreReleaseUserService>(MockBehavior.Strict),
            userResourceRoleNotificationService ?? Mock.Of<IUserResourceRoleNotificationService>(MockBehavior.Strict),
            releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
            releaseFileService ?? Mock.Of<IReleaseFileService>(MockBehavior.Strict),
            options ?? DefaultReleaseApprovalOptions(),
            userReleaseRoleService ?? Mock.Of<IUserReleaseRoleService>(MockBehavior.Strict),
            emailTemplateService ?? Mock.Of<IEmailTemplateService>(MockBehavior.Strict)
        );
    }
}
