using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public class EinTilePersistenceTests
{
    /// <summary>
    /// Education in Numbers pages are global, so a tile can reference a Release in any Theme and will
    /// outlive it. Deleting the Release has to null the reference rather than be blocked by it, and the
    /// database has to do that itself - nothing loads the tiles before ReleaseVersionService deletes a
    /// Release, so a client-side behaviour such as ClientSetNull would leave the delete to fail against
    /// FK_EinTiles_Releases_ReleaseId.
    /// </summary>
    [Fact]
    public void ApiQueryStatTileReleaseReferenceIsNulledByTheDatabaseOnDelete()
    {
        using var context = InMemoryContentDbContext();

        var releaseForeignKey = context
            .Model.FindEntityType(typeof(EinApiQueryStatTile))!
            .GetForeignKeys()
            .Single(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Release));

        Assert.Equal(DeleteBehavior.SetNull, releaseForeignKey.DeleteBehavior);

        // A cascading action can only be left to the database while the reference stays optional.
        Assert.False(releaseForeignKey.IsRequired);
    }

    [Fact]
    public async Task DeletingReleaseNullsApiQueryStatTileReleaseReference()
    {
        var contextId = Guid.NewGuid().ToString();

        var release = new Release
        {
            Id = Guid.NewGuid(),
            Slug = "2024-25",
            Year = 2024,
            TimePeriodCoverage = TimeIdentifier.AcademicYear,
            PublicationId = Guid.NewGuid(),
        };

        var tile = new EinApiQueryStatTile
        {
            Id = Guid.NewGuid(),
            Title = "Pupil numbers",
            EinParentBlockId = Guid.NewGuid(),
            ReleaseId = release.Id,
        };

        await using (var context = InMemoryContentDbContext(contextId))
        {
            context.Releases.Add(release);
            context.EinTiles.Add(tile);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var releaseToDelete = await context.Releases.SingleAsync(r => r.Id == release.Id);

            // Load the tile so that the in-memory provider, which does not enforce foreign keys, applies
            // the configured delete behaviour. Against SQL Server the constraint does this without the
            // tile being tracked, which is what ReleaseVersionService relies on.
            await context.EinTiles.OfType<EinApiQueryStatTile>().Where(t => t.ReleaseId == release.Id).LoadAsync();

            context.Releases.Remove(releaseToDelete);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            // The tile survives the Release it referenced, with the reference cleared
            var retrievedTile = await context.EinTiles.OfType<EinApiQueryStatTile>().SingleAsync(t => t.Id == tile.Id);

            Assert.Null(retrievedTile.ReleaseId);
            Assert.Equal("Pupil numbers", retrievedTile.Title);
        }
    }
}
