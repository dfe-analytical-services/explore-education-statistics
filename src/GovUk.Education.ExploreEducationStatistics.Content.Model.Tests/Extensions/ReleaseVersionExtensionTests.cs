using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class ReleaseVersionExtensionTests
    {
        [Fact]
        public void AllFilesZipPath()
        {
            const string releaseSlug = "release-slug";
            const string publicationSlug = "publication-slug";

            var releaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Slug = releaseSlug,
                Publication = new Publication
                {
                    Slug = publicationSlug
                }
            };

            Assert.Equal($"{releaseVersion.Id}/zip/{publicationSlug}_{releaseSlug}.zip", releaseVersion.AllFilesZipPath());
        }
    }
}
