#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class ReleaseExtensionTests
    {
        [Fact]
        public void AllFilesZipPath()
        {
            const string releaseSlug = "release-slug";
            const string publicationSlug = "publication-slug";

            var release = new Release
            {
                Id = Guid.NewGuid(),
                Slug = releaseSlug,
                Publication = new Publication
                {
                    Slug = publicationSlug
                }
            };

            Assert.Equal($"{release.Id}/zip/{publicationSlug}_{releaseSlug}.zip", release.AllFilesZipPath());
        }
    }
}
