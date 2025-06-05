using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class ContentDbContextMockBuilder
{
    private readonly ContentDbContext _inMemoryContentDbContext = ContentDbUtils.InMemoryContentDbContext();

    public Asserter Assert => new(_inMemoryContentDbContext);
    public ContentDbContext Build()
    {
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
        public void PublicationsLatestPublishedReleaseVersionIdIs(Guid publicationId, Guid expectedLatestPublishedReleaseVersionId)
        {
            var publication = contentDbContext.Publications.SingleOrDefault(p => p.Id == publicationId);
            if (publication == null)
                Xunit.Assert.Fail($"No publication was found with id {publicationId}");
            
            Xunit.Assert.Equal(expectedLatestPublishedReleaseVersionId, publication.LatestPublishedReleaseVersionId);
        }
    }
}
