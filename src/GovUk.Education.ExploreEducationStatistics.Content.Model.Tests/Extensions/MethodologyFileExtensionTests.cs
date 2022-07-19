#nullable enable
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

        [Fact]
        public void ToFileInfo()
        {
            var methodologyFile = new MethodologyFile
            {
                MethodologyVersion = new MethodologyVersion(),
                File = new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    ContentLength = 10240,
                    Type = Image
                }
            };

            var result = methodologyFile.ToFileInfo();

            Assert.Equal(methodologyFile.File.Id, result.Id);
            Assert.Equal("png", result.Extension);
            Assert.Equal("image.png", result.FileName);
            Assert.Equal("10 Kb", result.Size);
            Assert.Equal(Image, result.Type);
        }
    }
}
