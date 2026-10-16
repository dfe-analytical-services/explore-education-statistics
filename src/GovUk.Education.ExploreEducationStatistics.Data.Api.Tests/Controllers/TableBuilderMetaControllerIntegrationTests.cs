#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Fixtures.Optimised;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class TableBuilderMetaControllerIntegrationTestsFixture : OptimisedDataApiCollectionFixture
{
    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        // Always miss the cache so subject meta is built from the database on every request
        serviceModifications.ReplaceService(Mock.Of<IPublicBlobCacheService>(MockBehavior.Loose));

        // Not used by these endpoints, but resolved by the controller's dependencies
        serviceModifications.ReplaceService(Mock.Of<IPublicBlobStorageService>(MockBehavior.Strict));
    }
}

[CollectionDefinition(nameof(TableBuilderMetaControllerIntegrationTestsFixture))]
public class TableBuilderMetaControllerIntegrationTestsCollection
    : ICollectionFixture<TableBuilderMetaControllerIntegrationTestsFixture>;

[Collection(nameof(TableBuilderMetaControllerIntegrationTestsFixture))]
public class TableBuilderMetaControllerIntegrationTests(TableBuilderMetaControllerIntegrationTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    [Fact]
    public async Task GetSubjectMeta_DraftAmendmentPairedIndicatorsNotVisible()
    {
        var dataSet = await SeedPublishedVersionWithDraftAmendment();

        var response = await fixture.CreateClient().GetAsync($"/api/meta/subject/{dataSet.SubjectId}");

        var viewModel = response.AssertOk<SubjectMetaViewModel>();

        AssertParentIndicatorIds(
            viewModel,
            dataSet.IndicatorIds,
            expectedParentIdsByChildId: new Dictionary<Guid, Guid>
            {
                { dataSet.IndicatorIds[1], dataSet.IndicatorIds[0] },
            }
        );
    }

    [Fact]
    public async Task GetSubjectMeta_AmendmentPairedIndicatorsVisibleOncePublished()
    {
        var dataSet = await SeedPublishedVersionWithDraftAmendment();

        // Publish the amendment, as the Publisher does
        var contentDbContext = fixture.GetContentDbContext();
        var amendment = await contentDbContext.ReleaseVersions.SingleAsync(rv => rv.Id == dataSet.Amendment.Id);
        amendment.Published = DateTimeOffset.UtcNow.AddMinutes(-1);
        await contentDbContext.SaveChangesAsync();

        var response = await fixture.CreateClient().GetAsync($"/api/meta/subject/{dataSet.SubjectId}");

        var viewModel = response.AssertOk<SubjectMetaViewModel>();

        AssertParentIndicatorIds(
            viewModel,
            dataSet.IndicatorIds,
            expectedParentIdsByChildId: new Dictionary<Guid, Guid>
            {
                { dataSet.IndicatorIds[3], dataSet.IndicatorIds[2] },
            }
        );
    }

    private record SeededDataSet(Guid SubjectId, ReleaseVersion Amendment, List<Guid> IndicatorIds);

    /// <summary>
    /// Seeds a data set linked to a published release version and its draft amendment, where the published
    /// version pairs indicators 0 and 1, and the draft amendment pairs indicators 2 and 3 instead.
    /// </summary>
    private async Task<SeededDataSet> SeedPublishedVersionWithDraftAmendment()
    {
        Publication publication = DataFixture
            .DefaultPublication()
            .WithReleases([DataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true)]);

        var publishedVersion = publication.Releases[0].Versions.Single(rv => rv.Published is not null);
        var amendment = publication.Releases[0].Versions.Single(rv => rv.Published is null);

        Subject subject = DataFixture.DefaultSubject();
        IndicatorGroup indicatorGroup = DataFixture.DefaultIndicatorGroup(indicatorCount: 4).WithSubject(subject);
        var indicatorIds = indicatorGroup.Indicators.Select(indicator => indicator.Id).ToList();

        var file = DataFixture.DefaultFile().WithType(FileType.Data).WithSubjectId(subject.Id).Generate();

        ReleaseFile publishedReleaseFile = DataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(publishedVersion)
            .WithFile(file)
            .WithPairedIndicators([new PairedIndicatorEntry(indicatorIds[0], indicatorIds[1])]);

        ReleaseFile amendmentReleaseFile = DataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(amendment)
            .WithFile(file)
            .WithPairedIndicators([new PairedIndicatorEntry(indicatorIds[2], indicatorIds[3])]);

        await fixture
            .GetContentDbContext()
            .AddTestData(context =>
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.AddRange(publishedReleaseFile, amendmentReleaseFile);
            });

        await fixture
            .GetStatisticsDbContext()
            .AddTestData(context =>
            {
                context.IndicatorGroup.Add(indicatorGroup);
                context.ReleaseSubject.AddRange(
                    new ReleaseSubject
                    {
                        ReleaseVersion = new Model.ReleaseVersion { Id = publishedVersion.Id },
                        Subject = subject,
                    },
                    new ReleaseSubject
                    {
                        ReleaseVersion = new Model.ReleaseVersion { Id = amendment.Id },
                        Subject = subject,
                    }
                );
            });

        return new SeededDataSet(SubjectId: subject.Id, Amendment: amendment, IndicatorIds: indicatorIds);
    }

    private static void AssertParentIndicatorIds(
        SubjectMetaViewModel viewModel,
        List<Guid> indicatorIds,
        Dictionary<Guid, Guid> expectedParentIdsByChildId
    )
    {
        var options = viewModel.Indicators.Values.SelectMany(group => group.Options).ToList();

        Assert.Equal(indicatorIds.Count, options.Count);

        Assert.All(
            options,
            option =>
                Assert.Equal(
                    expectedParentIdsByChildId.TryGetValue(option.Value, out var parentId) ? (Guid?)parentId : null,
                    option.ParentIndicatorId
                )
        );
    }
}
