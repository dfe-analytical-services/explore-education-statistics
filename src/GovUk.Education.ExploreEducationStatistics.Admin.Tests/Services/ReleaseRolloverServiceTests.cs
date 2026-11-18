#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class ReleaseRolloverServiceTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly OrganisationsValidatorMockBuilder _organisationsValidator = new();
    private static readonly Guid UserId = Guid.NewGuid();

    public class CreateReleaseRolloverTests : ReleaseRolloverServiceTests
    {
        [Theory]
        [InlineData(RolloverMode.WithData)]
        [InlineData(RolloverMode.StructureOnly)]
        public async Task GivenEligibleSourceRelease_CreatesRollover(RolloverMode mode)
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var publishingOrganisations = _dataFixture.DefaultOrganisation().GenerateList(2);
            _organisationsValidator.WhereHasOrganisations([.. publishingOrganisations]);

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            var request = ValidRequest(publication.Id, sourceRelease.Id) with
            {
                Mode = mode,
                Label = "  initial   release   label  ",
                PublishingOrganisations = publishingOrganisations.Select(org => org.Id).ToArray(),
            };

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(request);

                var viewModel = result.AssertRight();
                Assert.Equal(releaseVersionViewModel, viewModel);
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var actualReleaseVersion = await context
                    .ReleaseVersions.Include(rv => rv.Release.Publication)
                    .Include(rv => rv.PublishingOrganisations)
                    .Include(rv => rv.Content)
                    .SingleAsync(rv => rv.Id == newReleaseVersionId);

                var actualRelease = actualReleaseVersion.Release;

                Assert.Equal(publication.Id, actualRelease.PublicationId);

                Assert.Equal(publication.Id, actualReleaseVersion.PublicationId);

                Assert.Equal(sourceRelease.Id, actualRelease.RolloverSourceReleaseId);
                Assert.NotEqual(sourceReleaseVersion.Id, actualRelease.RolloverSourceReleaseId);

                Assert.Equal(2024, actualRelease.Year);
                Assert.Equal(TimeIdentifier.AcademicYear, actualRelease.TimePeriodCoverage);
                Assert.Equal(request.Slug, actualRelease.Slug);

                // FormatReleaseLabel trims and collapses internal whitespace.
                Assert.Equal("initial release label", actualRelease.Label);

                Assert.Equal(ReleaseType.OfficialStatistics, actualReleaseVersion.Type);
                Assert.Equal(ReleaseApprovalStatus.Draft, actualReleaseVersion.ApprovalStatus);
                Assert.Equal(0, actualReleaseVersion.Version);
                Assert.True(actualReleaseVersion.Rollover);
                Assert.Equal(UserId, actualReleaseVersion.CreatedById);

                Assert.Equal(2, actualReleaseVersion.PublishingOrganisations.Count);
                Assert.All(
                    publishingOrganisations,
                    expected =>
                        Assert.Contains(
                            actualReleaseVersion.PublishingOrganisations,
                            actual => actual.Id == expected.Id
                        )
                );

                Assert.NotNull(actualReleaseVersion.HeadlinesSection);
                Assert.NotNull(actualReleaseVersion.KeyStatisticsSecondarySection);
                Assert.NotNull(actualReleaseVersion.RelatedDashboardsSection);
                Assert.NotNull(actualReleaseVersion.SummarySection);
                Assert.NotNull(actualReleaseVersion.WarningSection);

                var releaseSeriesItem = Assert.Single(
                    actualRelease.Publication.ReleaseSeries,
                    item => item.ReleaseId == actualRelease.Id
                );
                Assert.NotEqual(Guid.Empty, releaseSeriesItem.Id);
            }
        }

        [Fact]
        public async Task GivenSourceReleaseDoesNotExist_NotFound()
        {
            Publication publication = _dataFixture.DefaultPublication();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, Guid.NewGuid()));

            result.AssertNotFound();
        }

        [Fact]
        public async Task GivenRolloverSourceReleaseIdIsAReleaseVersionId_NotFound()
        {
            // Guards against conflating a ReleaseVersion id with its parent Release id - passing the
            // wrong kind of id must fail closed (404), not silently succeed against the wrong table.
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceReleaseVersion.Id));

            result.AssertNotFound();
        }

        [Fact]
        public async Task GivenSourceReleaseBelongsToDifferentPublication_BadRequest()
        {
            Publication publication = _dataFixture.DefaultPublication();
            Publication otherPublication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(otherPublication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.AddRange(publication, otherPublication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceRelease.Id));

            result.AssertBadRequest(RolloverSourceReleaseMustBeInSamePublication);
        }

        [Fact]
        public async Task GivenSourceReleaseHasNoPublishedVersion_BadRequest()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .WithPublished(null)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceRelease.Id));

            result.AssertBadRequest(RolloverSourceReleaseMustBePublished);
        }

        [Fact]
        public async Task GivenSourceReleaseIsEligible_ReleaseSlugIsValidated()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            var releaseVersionService = new ReleaseVersionServiceMockBuilder().WhereGetReleaseVersionReturns();
            var releaseSlugValidator = new ReleaseSlugValidatorMockBuilder();

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(
                contentDbContext: context2,
                releaseVersionService: releaseVersionService.Build(),
                releaseSlugValidator: releaseSlugValidator.Build()
            );

            var year = 2024;
            var timePeriodCoverage = TimeIdentifier.AcademicYear;
            var label = "initial";

            await sut.CreateReleaseRollover(
                ValidRequest(publication.Id, sourceRelease.Id) with
                {
                    Year = year,
                    TimePeriodCoverage = timePeriodCoverage,
                    Label = label,
                }
            );

            var expectedReleaseSlug = NamingUtils.CreateReleaseSlug(
                year: year,
                timePeriodCoverage: timePeriodCoverage,
                label: label
            );

            releaseSlugValidator.Assert.ValidateNewSlugWasCalled(
                expectedNewReleaseSlug: expectedReleaseSlug,
                expectedPublicationId: publication.Id,
                expectedReleaseId: null
            );
        }

        [Fact]
        public async Task GivenWithDataMode_CopiesDataBlocksFromTemplateReleaseAsNewDataBlocks()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            DataBlockVersion templateDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(sourceReleaseVersion)
                .Generate();

            DataBlock templateDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(templateDataBlockVersion)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                context.DataBlocks.Add(templateDataBlock);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(
                    ValidRequest(publication.Id, sourceRelease.Id) with
                    {
                        Mode = RolloverMode.WithData,
                    }
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var newDataBlockVersion = await context
                    .DataBlockVersions.Include(dataBlockVersion => dataBlockVersion.DataBlock)
                    .SingleAsync(dataBlockVersion => dataBlockVersion.ReleaseVersionId == newReleaseVersionId);

                // A brand-new DataBlock and DataBlockVersion in their own right, not sharing ids with the template.
                Assert.NotEqual(templateDataBlockVersion.Id, newDataBlockVersion.Id);
                Assert.NotEqual(templateDataBlockVersion.DataBlockId, newDataBlockVersion.DataBlockId);

                Assert.Equal(0, newDataBlockVersion.Version);
                Assert.Equal(templateDataBlockVersion.Heading, newDataBlockVersion.Heading);
                Assert.Equal(templateDataBlockVersion.Name, newDataBlockVersion.Name);
                Assert.Equal(templateDataBlockVersion.Source, newDataBlockVersion.Source);
                Assert.Equal(templateDataBlockVersion.Query.SubjectId, newDataBlockVersion.Query.SubjectId);
                Assert.Null(newDataBlockVersion.Updated);
                Assert.Null(newDataBlockVersion.Published);

                // The new DataBlock's LatestDraftVersion points at the new DataBlockVersion, not the template's.
                Assert.Equal(newDataBlockVersion.Id, newDataBlockVersion.DataBlock.LatestDraftVersionId);
            }
        }

        [Fact]
        public async Task GivenSourceReleaseHasPublishedVersionAndDraftAmendment_CopiesOnlyThePublishedVersionsDataBlocks()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion publishedSourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithVersion(0)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            // An unpublished amendment of the source release - must NOT be the one copied from.
            ReleaseVersion draftAmendmentReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithVersion(1)
                .WithPreviousVersion(publishedSourceReleaseVersion)
                .WithApprovalStatus(ReleaseApprovalStatus.Draft)
                .Generate();

            DataBlockVersion publishedDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(publishedSourceReleaseVersion)
                .WithName("Published version's data block")
                .Generate();

            DataBlock publishedDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(publishedDataBlockVersion)
                .Generate();

            DataBlockVersion draftAmendmentDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(draftAmendmentReleaseVersion)
                .WithName("Draft amendment's data block")
                .Generate();

            DataBlock draftAmendmentDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(draftAmendmentDataBlockVersion)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.AddRange(publishedSourceReleaseVersion, draftAmendmentReleaseVersion);
                context.DataBlocks.AddRange(publishedDataBlock, draftAmendmentDataBlock);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(
                    ValidRequest(publication.Id, sourceRelease.Id) with
                    {
                        Mode = RolloverMode.WithData,
                    }
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // SingleAsync also proves the draft amendment's data block was NOT also copied.
                var newDataBlockVersion = await context.DataBlockVersions.SingleAsync(dataBlockVersion =>
                    dataBlockVersion.ReleaseVersionId == newReleaseVersionId
                );

                Assert.Equal(publishedDataBlockVersion.Name, newDataBlockVersion.Name);
            }
        }

        [Fact]
        public async Task GivenSourceReleaseHasTwoPublishedVersions_CopiesDataBlocksFromTheLatestVersion()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion originalPublishedReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithVersion(0)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-60))
                .Generate();

            ReleaseVersion latestPublishedReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithVersion(1)
                .WithPreviousVersion(originalPublishedReleaseVersion)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-10))
                .Generate();

            DataBlockVersion originalDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(originalPublishedReleaseVersion)
                .WithName("Original published version's data block")
                .Generate();

            DataBlock originalDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(originalDataBlockVersion)
                .Generate();

            DataBlockVersion latestDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(latestPublishedReleaseVersion)
                .WithName("Latest published version's data block")
                .Generate();

            DataBlock latestDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(latestDataBlockVersion)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.AddRange(originalPublishedReleaseVersion, latestPublishedReleaseVersion);
                context.DataBlocks.AddRange(originalDataBlock, latestDataBlock);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(
                    ValidRequest(publication.Id, sourceRelease.Id) with
                    {
                        Mode = RolloverMode.WithData,
                    }
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // SingleAsync also proves the original version's data block was NOT also copied.
                var newDataBlockVersion = await context.DataBlockVersions.SingleAsync(dataBlockVersion =>
                    dataBlockVersion.ReleaseVersionId == newReleaseVersionId
                );

                Assert.Equal(latestDataBlockVersion.Name, newDataBlockVersion.Name);
            }
        }

        [Fact]
        public async Task GivenStructureOnlyMode_DoesNotCopyDataBlocks()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            DataBlockVersion templateDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(sourceReleaseVersion)
                .Generate();

            DataBlock templateDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(templateDataBlockVersion)
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                context.DataBlocks.Add(templateDataBlock);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(
                    ValidRequest(publication.Id, sourceRelease.Id) with
                    {
                        Mode = RolloverMode.StructureOnly,
                    }
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                Assert.False(
                    await context.DataBlockVersions.AnyAsync(dataBlockVersion =>
                        dataBlockVersion.ReleaseVersionId == newReleaseVersionId
                    )
                );
            }
        }

        [Fact]
        public async Task GivenWithDataMode_CopiesContentBlankingHtmlAndRelinkingDataBlocks()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            DataBlockVersion templateDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(sourceReleaseVersion)
                .Generate();

            DataBlock templateDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(templateDataBlockVersion)
                .Generate();

            HtmlBlock headlinesHtmlBlock = _dataFixture
                .DefaultHtmlBlock()
                .WithBody("Last year's headline")
                .WithOrder(1)
                .Generate();
            headlinesHtmlBlock.ReleaseVersionId = sourceReleaseVersion.Id;

            ContentSection headlinesSection = _dataFixture
                .DefaultContentSection(ContentSectionType.Headlines)
                .WithReleaseVersion(sourceReleaseVersion)
                .WithContentBlocks([headlinesHtmlBlock])
                .Generate();

            HtmlBlock genericHtmlBlock = _dataFixture.DefaultHtmlBlock().WithBody("Some text").WithOrder(1).Generate();
            genericHtmlBlock.ReleaseVersionId = sourceReleaseVersion.Id;

            DataBlockVersionLink genericDataBlockVersionLink = _dataFixture
                .DefaultDataVersionBlockVersionLink()
                .WithDataBlockVersion(templateDataBlockVersion)
                .WithReleaseVersion(sourceReleaseVersion)
                .WithOrder(2)
                .Generate();

            EmbedBlockLink genericEmbedBlockLink = _dataFixture.DefaultEmbedBlockLink().WithOrder(3).Generate();
            genericEmbedBlockLink.ReleaseVersionId = sourceReleaseVersion.Id;

            ContentSection genericSection = _dataFixture
                .DefaultContentSection(ContentSectionType.Generic)
                .WithReleaseVersion(sourceReleaseVersion)
                .WithHeading("Custom section")
                .WithOrder(5)
                .WithContentBlocks([genericHtmlBlock, genericDataBlockVersionLink, genericEmbedBlockLink])
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                context.DataBlocks.Add(templateDataBlock);
                context.ContentSections.AddRange(headlinesSection, genericSection);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(
                    ValidRequest(publication.Id, sourceRelease.Id) with
                    {
                        Mode = RolloverMode.WithData,
                    }
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var newDataBlockVersion = await context.DataBlockVersions.SingleAsync(dataBlockVersion =>
                    dataBlockVersion.ReleaseVersionId == newReleaseVersionId
                );

                var newHeadlinesSection = await context
                    .ContentSections.Include(section => section.Content)
                    .SingleAsync(section =>
                        section.ReleaseVersionId == newReleaseVersionId && section.Type == ContentSectionType.Headlines
                    );

                var newHeadlinesHtmlBlock = Assert.IsType<HtmlBlock>(Assert.Single(newHeadlinesSection.Content));
                Assert.Equal(string.Empty, newHeadlinesHtmlBlock.Body);
                Assert.Equal(headlinesHtmlBlock.Order, newHeadlinesHtmlBlock.Order);
                Assert.NotEqual(headlinesHtmlBlock.Id, newHeadlinesHtmlBlock.Id);

                var newGenericSection = await context
                    .ContentSections.Include(section => section.Content)
                    .SingleAsync(section =>
                        section.ReleaseVersionId == newReleaseVersionId && section.Type == ContentSectionType.Generic
                    );

                Assert.NotEqual(genericSection.Id, newGenericSection.Id);
                Assert.Equal(genericSection.Heading, newGenericSection.Heading);
                Assert.Equal(genericSection.Order, newGenericSection.Order);

                // The EmbedBlockLink is dropped - only the HtmlBlock and the relinked DataBlockVersionLink remain.
                Assert.Equal(2, newGenericSection.Content.Count);

                var newGenericHtmlBlock = Assert.IsType<HtmlBlock>(
                    Assert.Single(newGenericSection.Content, block => block is HtmlBlock)
                );
                Assert.Equal(string.Empty, newGenericHtmlBlock.Body);
                Assert.Equal(genericHtmlBlock.Order, newGenericHtmlBlock.Order);

                var newDataBlockVersionLink = Assert.IsType<DataBlockVersionLink>(
                    Assert.Single(newGenericSection.Content, block => block is DataBlockVersionLink)
                );
                Assert.Equal(newDataBlockVersion.Id, newDataBlockVersionLink.DataBlockVersionId);
                Assert.Equal(newDataBlockVersionLink.Id, newDataBlockVersionLink.DataBlockVersionId);
                Assert.Equal(genericDataBlockVersionLink.Order, newDataBlockVersionLink.Order);
            }
        }

        [Fact]
        public async Task GivenStructureOnlyMode_CopiesContentButDropsDataBlockVersionLinks()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            DataBlockVersion templateDataBlockVersion = _dataFixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(sourceReleaseVersion)
                .Generate();

            DataBlock templateDataBlock = _dataFixture
                .DefaultDataBlock()
                .WithLatestDraftVersion(templateDataBlockVersion)
                .Generate();

            HtmlBlock genericHtmlBlock = _dataFixture.DefaultHtmlBlock().WithBody("Some text").WithOrder(1).Generate();
            genericHtmlBlock.ReleaseVersionId = sourceReleaseVersion.Id;

            DataBlockVersionLink genericDataBlockVersionLink = _dataFixture
                .DefaultDataVersionBlockVersionLink()
                .WithDataBlockVersion(templateDataBlockVersion)
                .WithReleaseVersion(sourceReleaseVersion)
                .WithOrder(2)
                .Generate();

            ContentSection genericSection = _dataFixture
                .DefaultContentSection(ContentSectionType.Generic)
                .WithReleaseVersion(sourceReleaseVersion)
                .WithHeading("Custom section")
                .WithOrder(5)
                .WithContentBlocks([genericHtmlBlock, genericDataBlockVersionLink])
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                context.DataBlocks.Add(templateDataBlock);
                context.ContentSections.Add(genericSection);
                await context.SaveChangesAsync();
            }

            Guid? newReleaseVersionId = null;

            var releaseVersionViewModel = new ReleaseVersionViewModel();
            var releaseVersionServiceMock = new Mock<IReleaseVersionService>(Strict);
            releaseVersionServiceMock
                .Setup(rvs => rvs.GetRelease(It.IsAny<Guid>()))
                .ReturnsAsync(releaseVersionViewModel)
                .Callback((Guid releaseVersionId) => newReleaseVersionId = releaseVersionId);

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                var sut = BuildService(
                    contentDbContext: context,
                    releaseVersionService: releaseVersionServiceMock.Object
                );

                var result = await sut.CreateReleaseRollover(
                    ValidRequest(publication.Id, sourceRelease.Id) with
                    {
                        Mode = RolloverMode.StructureOnly,
                    }
                );

                result.AssertRight();
            }

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                // StructureOnly must not copy any data blocks at all, not just omit linking them into content.
                Assert.False(
                    await context.DataBlockVersions.AnyAsync(dataBlockVersion =>
                        dataBlockVersion.ReleaseVersionId == newReleaseVersionId
                    )
                );

                var newGenericSection = await context
                    .ContentSections.Include(section => section.Content)
                    .SingleAsync(section =>
                        section.ReleaseVersionId == newReleaseVersionId && section.Type == ContentSectionType.Generic
                    );

                Assert.NotEqual(genericSection.Id, newGenericSection.Id);
                Assert.Equal(genericSection.Heading, newGenericSection.Heading);
                Assert.Equal(genericSection.Order, newGenericSection.Order);

                // The DataBlockVersionLink is silently dropped - there's no id-map to resolve it against.
                var newGenericHtmlBlock = Assert.IsType<HtmlBlock>(Assert.Single(newGenericSection.Content));
                Assert.Equal(string.Empty, newGenericHtmlBlock.Body);
                Assert.Equal(genericHtmlBlock.Order, newGenericHtmlBlock.Order);
                Assert.NotEqual(genericHtmlBlock.Id, newGenericHtmlBlock.Id);
            }
        }

        [Fact]
        public async Task GivenReleaseRolloverDisabled_ReturnsForbid()
        {
            Publication publication = _dataFixture.DefaultPublication();

            Release sourceRelease = _dataFixture
                .DefaultRelease()
                .WithPublication(publication)
                .WithSlug("2023-24")
                .Generate();

            ReleaseVersion sourceReleaseVersion = _dataFixture
                .DefaultReleaseVersion()
                .WithRelease(sourceRelease)
                .WithApprovalStatus(ReleaseApprovalStatus.Approved)
                .WithPublished(DateTimeOffset.UtcNow.AddDays(-30))
                .Generate();

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.Releases.Add(sourceRelease);
                context.ReleaseVersions.Add(sourceReleaseVersion);
                await context.SaveChangesAsync();
            }

            await using var context2 = InMemoryApplicationDbContext(contextId);
            var sut = BuildService(contentDbContext: context2, enableReleaseRollover: false);

            var result = await sut.CreateReleaseRollover(ValidRequest(publication.Id, sourceRelease.Id));

            result.AssertForbidden();
        }
    }

    private ReleaseRolloverService BuildService(
        ContentDbContext contentDbContext,
        IReleaseVersionService? releaseVersionService = null,
        IReleaseSlugValidator? releaseSlugValidator = null,
        bool enableReleaseRollover = true
    )
    {
        var userService = AlwaysTrueUserService();

        userService.Setup(s => s.GetUserId()).Returns(UserId);

        return new ReleaseRolloverService(
            appOptions: new AppOptions { EnableReleaseRollover = enableReleaseRollover }.ToOptionsWrapper(),
            context: contentDbContext,
            userService: userService.Object,
            releaseVersionService: releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
            guidGenerator: new SequentialGuidGenerator(),
            organisationsValidator: _organisationsValidator.Build(),
            releaseSlugValidator: releaseSlugValidator ?? new ReleaseSlugValidatorMockBuilder().Build()
        );
    }

    private static ReleaseRolloverCreateRequest ValidRequest(Guid publicationId, Guid rolloverSourceReleaseId) =>
        new()
        {
            PublicationId = publicationId,
            RolloverSourceReleaseId = rolloverSourceReleaseId,
            Mode = RolloverMode.WithData,
            Year = 2024,
            TimePeriodCoverage = TimeIdentifier.AcademicYear,
            Label = "initial",
            Type = ReleaseType.OfficialStatistics,
            PublishingOrganisations = [],
        };
}
