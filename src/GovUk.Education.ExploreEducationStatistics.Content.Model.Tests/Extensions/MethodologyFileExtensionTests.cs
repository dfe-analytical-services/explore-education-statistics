using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class MethodologyFileExtensionTests
    {
        [Fact]
        public void Path()
        {
            var methodologyFile = new MethodologyFile
            {
                File = new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            Assert.Equal(methodologyFile.File.Path(), methodologyFile.Path());
        }
    }
}
