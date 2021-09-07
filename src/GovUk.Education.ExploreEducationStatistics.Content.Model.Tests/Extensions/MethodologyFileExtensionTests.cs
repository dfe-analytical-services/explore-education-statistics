using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
                    Type = Image
                }
            };

            var result = methodologyFile.ToFileInfo(new BlobInfo
            (
                path: "Ignored",
                size: "500 B",
                contentType: "Ignored",
                contentLength: -1L,
                meta: new Dictionary<string, string>()
            ));

            Assert.Equal(methodologyFile.File.Id, result.Id);
            Assert.Equal("png", result.Extension);
            Assert.Equal("image.png", result.FileName);
            Assert.Equal("500 B", result.Size);
            Assert.Equal(Image, result.Type);
        }
    }
}
