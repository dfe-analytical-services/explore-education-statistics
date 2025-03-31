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
        _inMemoryContentDbContext.ReleaseVersions.Add(releaseVersion);
        _inMemoryContentDbContext.SaveChanges();
        return this;
    }

    public ContentDbContextMockBuilder With(Publication publication)
    {
        _inMemoryContentDbContext.Publications.Add(publication);
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
