using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class ContentDbContextMockBuilder
{
    private readonly string _dbContextId = Guid.NewGuid().ToString();
    private ContentDbContext _inMemoryContentDbContext;

    public ContentDbContextMockBuilder()
    {
        // Create a db context for the ARRANGE phase
        _inMemoryContentDbContext = ContentDbUtils.InMemoryContentDbContext(_dbContextId);
    }

    public Asserter Assert => new(_inMemoryContentDbContext);

    /// <summary>
    /// Note: It is intended that this method is called during the creation of the SUT, at the last moment
    /// before the ACT phase. This is because the in-memory db context that was created for the ARRANGE phase
    /// is disposed and a new instance created. This ensures that any queries performed by the SUT are correctly
    /// rehydrating any child entities through .Include declarations.
    /// </summary>
    /// <returns></returns>
    public ContentDbContext Build()
    {
        // Dispose of the ARRANGE db context
        _inMemoryContentDbContext.Dispose();

        // Create a new db context for the ACT and ASSERT phase.
        _inMemoryContentDbContext = ContentDbUtils.InMemoryContentDbContext(_dbContextId);
        return _inMemoryContentDbContext;
    }

    public ContentDbContextMockBuilder With(ReleaseVersion releaseVersion)
    {
        if (_inMemoryContentDbContext.ReleaseVersions.Any(rv => rv.Id == releaseVersion.Id))
        {
            _inMemoryContentDbContext.ReleaseVersions.Update(releaseVersion);
        }
        else
        {
            _inMemoryContentDbContext.ReleaseVersions.Add(releaseVersion);
        }
        _inMemoryContentDbContext.SaveChanges();
        return this;
    }

    public ContentDbContextMockBuilder With(Publication publication)
    {
        if (_inMemoryContentDbContext.Publications.Any(p => p.Id == publication.Id))
        {
            _inMemoryContentDbContext.Publications.Update(publication);
        }
        else
        {
            _inMemoryContentDbContext.Publications.Add(publication);
        }
        _inMemoryContentDbContext.SaveChanges();
        return this;
    }

    public ContentDbContextMockBuilder WherePublication(Guid publicationId, Action<Publication> modifier)
    {
        var publication = _inMemoryContentDbContext.Publications.SingleOrDefault(p => p.Id == publicationId);
        if (publication == null)
            Xunit.Assert.Fail($"No publication was found with id {publicationId}");

        modifier(publication);
        _inMemoryContentDbContext.SaveChanges();
        return this;
    }

    public class Asserter(ContentDbContext contentDbContext)
    {
        public void PublicationsLatestPublishedReleaseVersionIdIs(
            Guid publicationId,
            Guid expectedLatestPublishedReleaseVersionId
        )
        {
            var publication = contentDbContext.Publications.SingleOrDefault(p => p.Id == publicationId);
            if (publication == null)
                Xunit.Assert.Fail($"No publication was found with id {publicationId}");

            Xunit.Assert.Equal(expectedLatestPublishedReleaseVersionId, publication.LatestPublishedReleaseVersionId);
        }
    }
}
