using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
public sealed class PublicationReleaseSeriesViewServiceTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly PublicationReleaseSeriesViewService _sut;

    public PublicationReleaseSeriesViewServiceTests()
    {
        _context = InMemoryApplicationDbContext();
        _sut = new(_context);
    }

    #region CreateForCreateLegacyRelease Tests
    [Fact]
    public async Task CreateForCreateLegacyRelease_PublicationNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.CreateForCreateLegacyRelease(publicationId, Guid.NewGuid()));

        Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
    }

    [Fact]
    public async Task CreateForCreateLegacyRelease()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId
        });

        // Act
        await _sut.CreateForCreateLegacyRelease(publicationId, releaseId);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);
        var releaseSeriesItem = publication.ReleaseSeriesView[0];

        Assert.Single(publication.ReleaseSeriesView);
        Assert.Equal(releaseId, releaseSeriesItem.ReleaseId);
        Assert.True(releaseSeriesItem.IsLegacy);
        Assert.False(releaseSeriesItem.IsDraft);
        Assert.Equal(1, releaseSeriesItem.Order);
    }
    #endregion CreateForCreateLegacyRelease Tests

    #region DeleteForDeleteLegacyRelease Tests
    [Fact]
    public async Task DeleteForDeleteLegacyRelease_LegacyReleaseNotFound_ThrowsException()
    {
        // Arrange
        var legacyReleaseId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.DeleteForDeleteLegacyRelease(legacyReleaseId));

        Assert.Equal($"No matching LegacyRelease found with ID {legacyReleaseId}", exception.Message);
    }

    [Fact]
    public async Task DeleteForDeleteLegacyRelease_PublicationNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var legacyReleaseId = Guid.NewGuid();

        _context.LegacyReleases.Add(new()
        {
            Id = legacyReleaseId,
            PublicationId = publicationId,
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.DeleteForDeleteLegacyRelease(legacyReleaseId));

        Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
    }

    [Fact]
    public async Task DeleteForDeleteLegacyRelease_ReleaseSeriesItemNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var legacyReleaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            LegacyReleases = new()
            {
                new() { Id = legacyReleaseId, Description = "Legacy Release 1" }
            },
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.DeleteForDeleteLegacyRelease(legacyReleaseId));

        Assert.Equal($"No matching ReleaseSeriesItem found for {nameof(LegacyRelease)} \"Legacy Release 1\"", exception.Message);
    }

    [Fact]
    public async Task DeleteForDeleteLegacyRelease()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var legacyReleaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            LegacyReleases = new()
            {
                new() { Id = legacyReleaseId, Description = "Legacy Release 1" }
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = legacyReleaseId },
            },
        });

        // Act
        await _sut.DeleteForDeleteLegacyRelease(legacyReleaseId);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);

        Assert.Empty(publication.ReleaseSeriesView);
    }

    [Fact]
    public async Task DeleteForDeleteLegacyRelease_ReleaseSeriesHasBeenReset()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var legacyRelease1Id = Guid.NewGuid();
        var legacyRelease2Id = Guid.NewGuid();
        var legacyRelease3Id = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            LegacyReleases = new()
            {
                new() { Id = legacyRelease1Id, Description = "Legacy Release 1" },
                new() { Id = legacyRelease2Id, Description = "Legacy Release 2" },
                new() { Id = legacyRelease3Id, Description = "Legacy Release 3" },
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = legacyRelease1Id, Order = 1 },
                new() { ReleaseId = legacyRelease2Id, Order = 2 },
                new() { ReleaseId = legacyRelease3Id, Order = 3 },
            }
        });

        // Act
        await _sut.DeleteForDeleteLegacyRelease(legacyRelease2Id);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);

        Assert.Equal(2, publication.ReleaseSeriesView.Count);

        Assert.Equal(legacyRelease1Id, publication.ReleaseSeriesView[0].ReleaseId);
        Assert.Equal(1, publication.ReleaseSeriesView[0].Order);

        Assert.Equal(legacyRelease3Id, publication.ReleaseSeriesView[1].ReleaseId);
        Assert.Equal(2, publication.ReleaseSeriesView[1].Order);
    }
    #endregion DeleteForDeleteLegacyRelease Tests

    #region CreateForCreateRelease Tests
    [Fact]
    public async Task CreateForCreateRelease_PublicationNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.CreateForCreateRelease(publicationId, Guid.NewGuid()));

        Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
    }

    [Fact]
    public async Task CreateForCreateRelease()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId
        });

        // Act
        await _sut.CreateForCreateRelease(publicationId, releaseId);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);
        var releaseSeriesItem = publication.ReleaseSeriesView[0];

        Assert.Single(publication.ReleaseSeriesView);
        Assert.Equal(releaseId, releaseSeriesItem.ReleaseId);
        Assert.False(releaseSeriesItem.IsLegacy);
        Assert.True(releaseSeriesItem.IsDraft);
        Assert.Equal(1, releaseSeriesItem.Order);
    }
    #endregion CreateForCreateRelease Tests

    #region UpdateForPublishRelease Tests
    [Fact]
    public async Task UpdateForPublishRelease_PublicationNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForPublishRelease(publicationId, Guid.NewGuid()));

        Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
    }

    [Fact]
    public async Task UpdateForPublishRelease_ReleaseSeriesItemNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForPublishRelease(publicationId, releaseId));

        Assert.Equal($"No matching ReleaseSeriesItem found for {nameof(Release)} with ID {releaseId}", exception.Message);
    }

    [Fact]
    public async Task UpdateForPublishRelease_Amendment_ReleaseNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = releaseId, IsAmendment = true },
            }
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForPublishRelease(publicationId, releaseId));

        Assert.Equal($"No matching amendment for {nameof(Release)} with ID {releaseId} found", exception.Message);
    }

    [Fact]
    public async Task UpdateForPublishRelease_Amendment_OriginalReleaseSeriesItemNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var originalReleaseId = Guid.NewGuid();
        var releaseAmendmentId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = originalReleaseId },
                new() { Id = releaseAmendmentId, PreviousVersionId = originalReleaseId },
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = releaseAmendmentId, IsAmendment = true },
            }
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForPublishRelease(publicationId, releaseAmendmentId));

        Assert.Equal($"No matching ReleaseSeriesItem for original {nameof(Release)} with ID {originalReleaseId} found", exception.Message);
    }

    [Fact]
    public async Task UpdateForPublishRelease_Amendment()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var release1Id = Guid.NewGuid();
        var release2Id = Guid.NewGuid();
        var release3Id = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = release1Id },
                new() { Id = release2Id },
                new() { Id = release3Id, PreviousVersionId = release2Id },
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = release1Id, Order = 1 },
                new() { ReleaseId = release2Id, Order = 2 },
                new() { ReleaseId = release3Id, Order = 2, IsDraft = true, IsAmendment = true },
            }
        });

        // Act
        await _sut.UpdateForPublishRelease(publicationId, release3Id);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);

        Assert.Equal(2, publication.ReleaseSeriesView.Count);

        Assert.Equal(release1Id, publication.ReleaseSeriesView[0].ReleaseId);
        Assert.Equal(1, publication.ReleaseSeriesView[0].Order);

        Assert.Equal(release3Id, publication.ReleaseSeriesView[1].ReleaseId);
        Assert.Equal(2, publication.ReleaseSeriesView[1].Order);
        Assert.False(publication.ReleaseSeriesView[1].IsDraft);
        Assert.False(publication.ReleaseSeriesView[1].IsAmendment);
    }

    [Fact]
    public async Task UpdateForPublishRelease()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var release1Id = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = release1Id },
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = release1Id, IsDraft = true, Order = 1 },
            }
        });

        // Act
        await _sut.UpdateForPublishRelease(publicationId, release1Id);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);

        Assert.Single(publication.ReleaseSeriesView);

        Assert.Equal(release1Id, publication.ReleaseSeriesView[0].ReleaseId);
        Assert.Equal(1, publication.ReleaseSeriesView[0].Order);
        Assert.False(publication.ReleaseSeriesView[0].IsDraft);
    }
    #endregion UpdateForPublishRelease Tests

    #region UpdateForUpdateReleaseSeries Tests
    [Fact]
    public async Task UpdateForUpdateReleaseSeries_PublicationNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForUpdateReleaseSeries(publicationId, new()));

        Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
    }

    [Fact]
    public async Task UpdateForUpdateReleaseSeries_NewOrderMissing_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId
        });

        var updates = new List<ReleaseSeriesItemUpdateRequest>
        {
            new() { Id = releaseId },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            async () => await _sut.UpdateForUpdateReleaseSeries(publicationId, updates));

        Assert.Equal($"Updated order for release with ID {releaseId} must be greater than 0", exception.Message);
    }

    [Fact]
    public async Task UpdateForUpdateReleaseSeries_AmendmentReleaseNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId
        });

        var updates = new List<ReleaseSeriesItemUpdateRequest>
        {
            new() { Id = releaseId, Order = 1, IsAmendment = true },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForUpdateReleaseSeries(publicationId, updates));

        Assert.Equal($"No matching amendment for {nameof(Release)} with ID {releaseId} found", exception.Message);
    }

    [Fact]
    public async Task UpdateForUpdateReleaseSeries_Amendment_OriginalReleaseSeriesItemNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var originalReleaseId = Guid.NewGuid();
        var releaseAmendmentId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = originalReleaseId },
                new() { Id = releaseAmendmentId, PreviousVersionId = originalReleaseId },
            }
        });

        var updates = new List<ReleaseSeriesItemUpdateRequest>
        {
            new() { Id = releaseAmendmentId, Order = 1, IsAmendment = true },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForUpdateReleaseSeries(publicationId, updates));

        Assert.Equal($"No matching ReleaseSeriesItem for original {nameof(Release)} with ID {originalReleaseId} found", exception.Message);
    }

    [Fact]
    public async Task UpdateForUpdateReleaseSeries_Amendment_OriginalReleaseSeriesItemUpdated()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var originalReleaseId = Guid.NewGuid();
        var releaseAmendmentId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = originalReleaseId },
                new() { Id = releaseAmendmentId, PreviousVersionId = originalReleaseId },
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = originalReleaseId, Order = 1 },
                new() { ReleaseId = releaseAmendmentId, Order = 1 },
            }
        });

        var updates = new List<ReleaseSeriesItemUpdateRequest>
        {
            new() { Id = releaseAmendmentId, Order = 5, IsAmendment = true },
        };

        // Act
        await _sut.UpdateForUpdateReleaseSeries(publicationId, updates);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);
        var releaseSeries = publication.ReleaseSeriesView;
        var originalReleaseSeriesItem = publication.ReleaseSeriesView[0];
        var amendmentReleaseSeriesItem = publication.ReleaseSeriesView[1];

        Assert.Equal(2, releaseSeries.Count);
        Assert.Equal(5, originalReleaseSeriesItem.Order);
        Assert.Equal(5, amendmentReleaseSeriesItem.Order);
    }

    [Theory]
    [InlineData(true, nameof(LegacyRelease))]
    [InlineData(false, nameof(Release))]
    public async Task UpdateForUpdateReleaseSeries_ReleaseSeriesItemNotFound_ThrowsException(
        bool isLegacy,
        string entityTypeName)
    {
        // Arrange
        var publicationId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId
        });

        var updates = new List<ReleaseSeriesItemUpdateRequest>
        {
            new() { Id = Guid.NewGuid(), Order = 1, IsLegacy = isLegacy },
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.UpdateForUpdateReleaseSeries(publicationId, updates));

        Assert.Equal($"No matching ReleaseSeriesItem found for {entityTypeName} with ID {updates[0].Id}", exception.Message);
    }

    [Fact]
    public async Task UpdateForUpdateReleaseSeries()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var eesReleaseId = Guid.NewGuid();
        var legacyReleaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            LegacyReleases = new()
            {
                new() { Id = legacyReleaseId }
            },
            Releases = new()
            {
                new() { Id = eesReleaseId }
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = legacyReleaseId, Order = 1, IsLegacy = true },
                new() { ReleaseId = eesReleaseId, Order = 2, IsLegacy = false },
            }
        });

        var updates = new List<ReleaseSeriesItemUpdateRequest>
        {
            new() { Id = legacyReleaseId, Order = 2, IsLegacy = true },
            new() { Id = eesReleaseId, Order = 1, IsLegacy = false },
        };

        // Act
        await _sut.UpdateForUpdateReleaseSeries(publicationId, updates);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);
        var legacyReleaseSeriesItem = publication.ReleaseSeriesView[0];
        var eesReleaseSeriesItem = publication.ReleaseSeriesView[1];

        Assert.Equal(2, publication.ReleaseSeriesView.Count);

        Assert.Equal(2, legacyReleaseSeriesItem.Order);
        Assert.Equal(1, eesReleaseSeriesItem.Order);
    }
    #endregion UpdateForUpdateReleaseSeries Tests

    #region CreateForAmendRelease Tests
    [Fact]
    public async Task CreateForAmendRelease_PublicationNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.CreateForAmendRelease(publicationId, Guid.NewGuid()));

        Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
    }

    [Fact]
    public async Task CreateForAmendRelease_ReleaseAmendmentNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseAmendmentId = Guid.NewGuid();
        var originalReleaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id =  Guid.NewGuid(), PreviousVersionId = originalReleaseId }
            },
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.CreateForAmendRelease(publicationId, releaseAmendmentId));

        Assert.Equal($"No matching amendment for {nameof(Release)} with ID {releaseAmendmentId} found", exception.Message);
    }

    [Fact]
    public async Task CreateForAmendRelease_OriginalReleaseSeriesItemNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseAmendmentId = Guid.NewGuid();
        var previousVersionId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = releaseAmendmentId, PreviousVersionId = previousVersionId }
            },
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.CreateForAmendRelease(publicationId, releaseAmendmentId));

        Assert.Equal($"No matching ReleaseSeriesItem for original {nameof(Release)} with ID {previousVersionId} found", exception.Message);
    }

    [Fact]
    public async Task CreateForAmendRelease()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var originalReleaseId = Guid.NewGuid();
        var releaseAmendmentId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = originalReleaseId },
                new() { Id = releaseAmendmentId, PreviousVersionId = originalReleaseId },
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = originalReleaseId, IsDraft = true, Order = 1 },
            }
        });

        // Act
        await _sut.CreateForAmendRelease(publicationId, releaseAmendmentId);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);
        var amendmentReleaseSeriesItem = publication.ReleaseSeriesView[1];

        Assert.Equal(releaseAmendmentId, amendmentReleaseSeriesItem.ReleaseId);
        Assert.False(amendmentReleaseSeriesItem.IsLegacy);
        Assert.True(amendmentReleaseSeriesItem.IsDraft);
        Assert.True(amendmentReleaseSeriesItem.IsAmendment);
        Assert.Equal(1, amendmentReleaseSeriesItem.Order);
    }
    #endregion CreateForAmendRelease Tests

    #region DeleteForDeleteRelease Tests
    [Fact]
    public async Task DeleteForDeleteRelease_PublicationNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.DeleteForDeleteRelease(publicationId, Guid.NewGuid()));

        Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
    }

    [Fact]
    public async Task DeleteForDeleteRelease_ReleaseSeriesItemNotFound_ThrowsException()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = releaseId },
            },
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.DeleteForDeleteRelease(publicationId, releaseId));

        Assert.Equal($"No matching ReleaseSeriesItem found for {nameof(Release)} amendment with ID {releaseId}", exception.Message);
    }

    [Fact]
    public async Task DeleteForDeleteRelease_RemainingReleaseSeriesItemValuesSequential()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new(),
                new() { Id = releaseId },
                new(),
                new(),
            },
            ReleaseSeriesView = new()
            {
                new() { Order = 1 },
                new() { ReleaseId = releaseId, Order = 2 },
                new() { Order = 3 },
                new() { Order = 4 },
            },
        });

        // Act
        await _sut.DeleteForDeleteRelease(publicationId, releaseId);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);
        var releaseSeries = publication.ReleaseSeriesView;

        Assert.Equal(3, publication.ReleaseSeriesView.Count);
        Assert.Equal(1, releaseSeries[0].Order);
        Assert.Equal(2, releaseSeries[1].Order);
        Assert.Equal(3, releaseSeries[2].Order);
    }

    [Fact]
    public async Task DeleteForDeleteRelease()
    {
        // Arrange
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();

        _context.Publications.Add(new()
        {
            Id = publicationId,
            Releases = new()
            {
                new() { Id = releaseId },
            },
            ReleaseSeriesView = new()
            {
                new() { ReleaseId = releaseId },
            },
        });

        // Act
        await _sut.DeleteForDeleteRelease(publicationId, releaseId);

        // Assert
        var publication = await _context.Publications.FindAsync(publicationId);

        Assert.Empty(publication.ReleaseSeriesView);
    }
    #endregion DeleteForDeleteRelease Tests

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
