using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public sealed class PublicationReleaseOrderServiceTests : IDisposable
    {
        private readonly ContentDbContext _context;
        private readonly PublicationReleaseOrderService _sut;

        public PublicationReleaseOrderServiceTests()
        {
            _context = InMemoryApplicationDbContext();
            _sut = new(_context);
        }

        #region CreateForCreateLegacyRelease Tests
        [Fact]
        public async Task CreateForCreateLegacyRelease_NoPublicationId_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CreateForCreateLegacyRelease(Guid.Empty, Guid.NewGuid()));

            Assert.Equal($"Value cannot be null. (Parameter 'publicationId')", exception.Message);
        }

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
        public async Task CreateForCreateLegacyRelease_NoReleaseId_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CreateForCreateLegacyRelease(publicationId, Guid.Empty));

            Assert.Equal($"Value cannot be null. (Parameter 'releaseId')", exception.Message);
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
            var releaseOrder = publication.ReleaseOrders[0];

            Assert.Single(publication.ReleaseOrders);
            Assert.Equal(releaseId, releaseOrder.ReleaseId);
            Assert.True(releaseOrder.IsLegacy);
            Assert.False(releaseOrder.IsDraft);
            Assert.Equal(1, releaseOrder.Order);
        }
        #endregion CreateForCreateLegacyRelease Tests

        #region DeleteForDeleteLegacyRelease Tests
        [Fact]
        public async Task DeleteForDeleteLegacyRelease_NoPublicationId_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.DeleteForDeleteLegacyRelease(Guid.Empty, Guid.NewGuid()));

            Assert.Equal($"Value cannot be null. (Parameter 'publicationId')", exception.Message);
        }

        [Fact]
        public async Task DeleteForDeleteLegacyRelease_PublicationNotFound_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.DeleteForDeleteLegacyRelease(publicationId, Guid.NewGuid()));

            Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
        }

        [Fact]
        public async Task DeleteForDeleteLegacyRelease_NoReleaseId_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.DeleteForDeleteLegacyRelease(publicationId, Guid.Empty));

            Assert.Equal($"Value cannot be null. (Parameter 'releaseId')", exception.Message);
        }

        [Fact]
        public async Task DeleteForDeleteLegacyRelease_LegacyReleaseNotFound_ThrowsException()
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
                async () => await _sut.DeleteForDeleteLegacyRelease(publicationId, releaseId));

            Assert.Equal($"No matching LegacyRelease found with ID {releaseId}", exception.Message);
        }

        [Fact]
        public async Task DeleteForDeleteLegacyRelease_ReleaseOrderNotFound_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId,
                LegacyReleases = new()
                {
                    new() { Id = releaseId, Description = "Legacy Release 1" }
                },
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.DeleteForDeleteLegacyRelease(publicationId, releaseId));

            Assert.Equal($"No matching ReleaseOrder found for {nameof(LegacyRelease)} \"Legacy Release 1\"", exception.Message);
        }

        [Fact]
        public async Task DeleteForDeleteLegacyRelease()
        {
            // Arrange
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId,
                LegacyReleases = new()
                {
                    new() { Id = releaseId, Description = "Legacy Release 1" }
                },
                ReleaseOrders = new()
                {
                    new() { ReleaseId = releaseId },
                },
            });

            // Act
            await _sut.DeleteForDeleteLegacyRelease(publicationId, releaseId);

            // Assert
            var publication = await _context.Publications.FindAsync(publicationId);

            Assert.Empty(publication.ReleaseOrders);
        }

        [Fact]
        public async Task DeleteForDeleteLegacyRelease_ReleaseOrdersHaveBeenReset()
        {
            // Arrange
            var publicationId = Guid.NewGuid();
            var release1Id = Guid.NewGuid();
            var release2Id = Guid.NewGuid();
            var release3Id = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId,
                LegacyReleases = new()
                {
                    new() { Id = release1Id, Description = "Legacy Release 1" },
                    new() { Id = release2Id, Description = "Legacy Release 2" },
                    new() { Id = release3Id, Description = "Legacy Release 3" },
                },
                ReleaseOrders = new()
                {
                    new() { ReleaseId = release1Id, Order = 1 },
                    new() { ReleaseId = release2Id, Order = 2 },
                    new() { ReleaseId = release3Id, Order = 3 },
                }
            });

            // Act
            await _sut.DeleteForDeleteLegacyRelease(publicationId, release2Id);

            // Assert
            var publication = await _context.Publications.FindAsync(publicationId);

            Assert.Equal(2, publication.ReleaseOrders.Count);

            Assert.Equal(release1Id, publication.ReleaseOrders[0].ReleaseId);
            Assert.Equal(1, publication.ReleaseOrders[0].Order);

            Assert.Equal(release3Id, publication.ReleaseOrders[1].ReleaseId);
            Assert.Equal(2, publication.ReleaseOrders[1].Order);
        }
        #endregion DeleteForDeleteLegacyRelease Tests

        #region CreateForCreateRelease Tests
        [Fact]
        public async Task CreateForCreateRelease_NoPublicationId_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CreateForCreateRelease(Guid.Empty, Guid.NewGuid()));

            Assert.Equal($"Value cannot be null. (Parameter 'publicationId')", exception.Message);
        }

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
        public async Task CreateForCreateRelease_NoReleaseId_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CreateForCreateRelease(publicationId, Guid.Empty));

            Assert.Equal($"Value cannot be null. (Parameter 'releaseId')", exception.Message);
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
            var releaseOrder = publication.ReleaseOrders[0];

            Assert.Single(publication.ReleaseOrders);
            Assert.Equal(releaseId, releaseOrder.ReleaseId);
            Assert.False(releaseOrder.IsLegacy);
            Assert.True(releaseOrder.IsDraft);
            Assert.Equal(1, releaseOrder.Order);
        }
        #endregion CreateForCreateRelease Tests

        #region UpdateForPublishRelease Tests
        [Fact]
        public async Task UpdateForPublishRelease_NoPublicationId_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.UpdateForPublishRelease(Guid.Empty, Guid.NewGuid()));

            Assert.Equal($"Value cannot be null. (Parameter 'publicationId')", exception.Message);
        }

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
        public async Task UpdateForPublishRelease_NoReleaseId_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.UpdateForPublishRelease(publicationId, Guid.Empty));

            Assert.Equal($"Value cannot be null. (Parameter 'releaseId')", exception.Message);
        }

        [Fact]
        public async Task UpdateForPublishRelease_ReleaseOrderNotFound_ThrowsException()
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

            Assert.Equal($"No matching ReleaseOrder found for {nameof(Release)} with ID {releaseId}", exception.Message);
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
                ReleaseOrders = new()
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
        public async Task UpdateForPublishRelease_Amendment_OriginalReleaseOrderNotFound_ThrowsException()
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
                ReleaseOrders = new()
                {
                    new() { ReleaseId = releaseAmendmentId, IsAmendment = true },
                }
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.UpdateForPublishRelease(publicationId, releaseAmendmentId));

            Assert.Equal($"No matching ReleaseOrder for original {nameof(Release)} with ID {originalReleaseId} found", exception.Message);
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
                ReleaseOrders = new()
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

            Assert.Equal(2, publication.ReleaseOrders.Count);

            Assert.Equal(release1Id, publication.ReleaseOrders[0].ReleaseId);
            Assert.Equal(1, publication.ReleaseOrders[0].Order);

            Assert.Equal(release3Id, publication.ReleaseOrders[1].ReleaseId);
            Assert.Equal(2, publication.ReleaseOrders[1].Order);
            Assert.False(publication.ReleaseOrders[1].IsDraft);
            Assert.False(publication.ReleaseOrders[1].IsAmendment);
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
                ReleaseOrders = new()
                {
                    new() { ReleaseId = release1Id, IsDraft = true, Order = 1 },
                }
            });

            // Act
            await _sut.UpdateForPublishRelease(publicationId, release1Id);

            // Assert
            var publication = await _context.Publications.FindAsync(publicationId);

            Assert.Single(publication.ReleaseOrders);

            Assert.Equal(release1Id, publication.ReleaseOrders[0].ReleaseId);
            Assert.Equal(1, publication.ReleaseOrders[0].Order);
            Assert.False(publication.ReleaseOrders[0].IsDraft);
        }
        #endregion UpdateForPublishRelease Tests

        #region UpdateForUpdateCombinedReleaseOrder Tests
        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder_NoPublicationId_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.UpdateForUpdateCombinedReleaseOrder(Guid.Empty, new()));

            Assert.Equal($"Value cannot be null. (Parameter 'publicationId')", exception.Message);
        }

        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder_PublicationNotFound_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, new()));

            Assert.Equal($"No matching Publication found with ID {publicationId}", exception.Message);
        }

        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder_NoReleaseId_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            var updates = new List<CombinedReleaseUpdateOrderViewModel>
            {
                new(),
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, updates));

            Assert.Equal($"Update with order 0 must have a release ID specified", exception.Message);
        }

        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder_NewOrderMissing_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            var updates = new List<CombinedReleaseUpdateOrderViewModel>
            {
                new() { Id = releaseId },
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApplicationException>(
                async () => await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, updates));

            Assert.Equal($"Updated order for release with ID {releaseId} must be greater than 0", exception.Message);
        }

        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder_AmendmentReleaseNotFound_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            var updates = new List<CombinedReleaseUpdateOrderViewModel>
            {
                new() { Id = releaseId, Order = 1, IsAmendment = true },
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, updates));

            Assert.Equal($"No matching amendment for {nameof(Release)} with ID {releaseId} found", exception.Message);
        }

        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder_Amendment_OriginalReleaseOrderNotFound_ThrowsException()
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

            var updates = new List<CombinedReleaseUpdateOrderViewModel>
            {
                new() { Id = releaseAmendmentId, Order = 1, IsAmendment = true },
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, updates));

            Assert.Equal($"No matching ReleaseOrder for original {nameof(Release)} with ID {originalReleaseId} found", exception.Message);
        }

        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder_Amendment_OriginalReleaseOrderUpdated()
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
                ReleaseOrders = new()
                {
                    new() { ReleaseId = originalReleaseId, Order = 1 },
                    new() { ReleaseId = releaseAmendmentId, Order = 1 },
                }
            });

            var updates = new List<CombinedReleaseUpdateOrderViewModel>
            {
                new() { Id = releaseAmendmentId, Order = 5, IsAmendment = true },
            };

            // Act
            await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, updates);

            // Assert
            var publication = await _context.Publications.FindAsync(publicationId);
            var releaseOrders = publication.ReleaseOrders;
            var originalReleaseOrder = publication.ReleaseOrders[0];
            var amendmentReleaseOrder = publication.ReleaseOrders[1];

            Assert.Equal(2, releaseOrders.Count);
            Assert.Equal(5, originalReleaseOrder.Order);
            Assert.Equal(5, amendmentReleaseOrder.Order);
        }

        [Theory]
        [InlineData(true, nameof(LegacyRelease))]
        [InlineData(false, nameof(Release))]
        public async Task UpdateForUpdateCombinedReleaseOrder_ReleaseOrderNotFound_ThrowsException(
            bool isLegacy,
            string entityTypeName)
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            var updates = new List<CombinedReleaseUpdateOrderViewModel>
            {
                new() { Id = Guid.NewGuid(), Order = 1, IsLegacy = isLegacy },
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, updates));

            Assert.Equal($"No matching ReleaseOrder found for {entityTypeName} with ID {updates[0].Id}", exception.Message);
        }

        [Fact]
        public async Task UpdateForUpdateCombinedReleaseOrder()
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
                ReleaseOrders = new()
                {
                    new() { ReleaseId = legacyReleaseId, Order = 1, IsLegacy = true },
                    new() { ReleaseId = eesReleaseId, Order = 2, IsLegacy = false },
                }
            });

            var updates = new List<CombinedReleaseUpdateOrderViewModel>
            {
                new() { Id = legacyReleaseId, Order = 2, IsLegacy = true },
                new() { Id = eesReleaseId, Order = 1, IsLegacy = false },
            };

            // Act
            await _sut.UpdateForUpdateCombinedReleaseOrder(publicationId, updates);

            // Assert
            var publication = await _context.Publications.FindAsync(publicationId);
            var legacyReleaseOrder = publication.ReleaseOrders[0];
            var eesReleaseOrder = publication.ReleaseOrders[1];

            Assert.Equal(2, publication.ReleaseOrders.Count);

            Assert.Equal(2, legacyReleaseOrder.Order);
            Assert.Equal(1, eesReleaseOrder.Order);
        }
        #endregion UpdateForUpdateCombinedReleaseOrder Tests

        #region CreateForAmendRelease Tests
        [Fact]
        public async Task CreateForAmendRelease_NoPublicationId_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CreateForAmendRelease(Guid.Empty, Guid.NewGuid()));

            Assert.Equal($"Value cannot be null. (Parameter 'publicationId')", exception.Message);
        }

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
        public async Task CreateForAmendRelease_NoReleaseAmendmentId_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CreateForAmendRelease(publicationId, Guid.Empty));

            Assert.Equal($"Value cannot be null. (Parameter 'releaseAmendmentId')", exception.Message);
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
        public async Task CreateForAmendRelease_OriginalReleaseOrderNotFound_ThrowsException()
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

            Assert.Equal($"No matching ReleaseOrder for original {nameof(Release)} with ID {previousVersionId} found", exception.Message);
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
                ReleaseOrders = new()
                {
                    new() { ReleaseId = originalReleaseId, IsDraft = true, Order = 1 },
                }
            });

            // Act
            await _sut.CreateForAmendRelease(publicationId, releaseAmendmentId);

            // Assert
            var publication = await _context.Publications.FindAsync(publicationId);
            var amendmentReleaseOrder = publication.ReleaseOrders[1];

            Assert.Equal(releaseAmendmentId, amendmentReleaseOrder.ReleaseId);
            Assert.False(amendmentReleaseOrder.IsLegacy);
            Assert.True(amendmentReleaseOrder.IsDraft);
            Assert.True(amendmentReleaseOrder.IsAmendment);
            Assert.Equal(1, amendmentReleaseOrder.Order);
        }
        #endregion CreateForAmendRelease Tests

        #region DeleteForDeleteRelease Tests
        [Fact]
        public async Task DeleteForDeleteRelease_NoPublicationId_ThrowsException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.DeleteForDeleteRelease(Guid.Empty, Guid.NewGuid()));

            Assert.Equal($"Value cannot be null. (Parameter 'publicationId')", exception.Message);
        }

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
        public async Task DeleteForDeleteRelease_NoReleaseId_ThrowsException()
        {
            // Arrange
            var publicationId = Guid.NewGuid();

            _context.Publications.Add(new()
            {
                Id = publicationId,
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.DeleteForDeleteRelease(publicationId, Guid.Empty));

            Assert.Equal($"Value cannot be null. (Parameter 'releaseId')", exception.Message);
        }

        [Fact]
        public async Task DeleteForDeleteRelease_ReleaseOrderNotFound_ThrowsException()
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

            Assert.Equal($"No matching ReleaseOrder found for {nameof(Release)} amendment with ID {releaseId}", exception.Message);
        }

        [Fact]
        public async Task DeleteForDeleteRelease_RemainingReleaseOrderValuesSequential()
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
                ReleaseOrders = new()
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
            var releaseOrders = publication.ReleaseOrders;

            Assert.Equal(3, publication.ReleaseOrders.Count);
            Assert.Equal(1, releaseOrders[0].Order);
            Assert.Equal(2, releaseOrders[1].Order);
            Assert.Equal(3, releaseOrders[2].Order);
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
                ReleaseOrders = new()
                {
                    new() { ReleaseId = releaseId },
                },
            });

            // Act
            await _sut.DeleteForDeleteRelease(publicationId, releaseId);

            // Assert
            var publication = await _context.Publications.FindAsync(publicationId);

            Assert.Empty(publication.ReleaseOrders);
        }
        #endregion DeleteForDeleteRelease Tests

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
