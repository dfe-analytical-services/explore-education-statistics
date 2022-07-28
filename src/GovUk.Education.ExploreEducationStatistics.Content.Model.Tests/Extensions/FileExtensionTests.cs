#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions.FileExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions
{
    public class FileExtensionTests
    {
        [Fact]
        public void BatchesPath()
        {
            var dataFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = Data
            };

            Assert.Equal($"{dataFile.RootPath}/data/batches/{dataFile.Id}/", dataFile.BatchesPath());
        }

        [Fact]
        public void BatchPath()
        {
            var dataFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = Data
            };

            Assert.Equal($"{dataFile.RootPath}/data/batches/{dataFile.Id}/{dataFile.Id}_000999",
                dataFile.BatchPath(999));
        }

        [Fact]
        public void Path()
        {
            var ancillaryFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            };

            var chartFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            };

            var dataFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = Data
            };

            var imageFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                Type = Image
            };

            var metaFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "data.meta.csv",
                Type = Metadata
            };

            var dataZipFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "data.zip",
                Type = DataZip
            };

            Assert.Equal($"{ancillaryFile.RootPath}/ancillary/{ancillaryFile.Id}", ancillaryFile.Path());
            Assert.Equal($"{chartFile.RootPath}/chart/{chartFile.Id}", chartFile.Path());
            Assert.Equal($"{dataFile.RootPath}/data/{dataFile.Id}", dataFile.Path());
            Assert.Equal($"{imageFile.RootPath}/image/{imageFile.Id}", imageFile.Path());
            Assert.Equal($"{metaFile.RootPath}/data/{metaFile.Id}", metaFile.Path());
            Assert.Equal($"{dataZipFile.RootPath}/data-zip/{dataZipFile.Id}", dataZipFile.Path());
        }

        [Fact]
        public void PublicPath()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            var ancillaryFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            };

            var chartFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            };

            var dataFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = Data
            };

            var imageFile = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                Type = Image
            };

            Assert.Equal($"{release.Id}/ancillary/{ancillaryFile.Id}",
                ancillaryFile.PublicPath(release));

            Assert.Equal($"{release.Id}/chart/{chartFile.Id}",
                chartFile.PublicPath(release));

            Assert.Equal($"{release.Id}/data/{dataFile.Id}",
                dataFile.PublicPath(release));

            Assert.Equal($"{release.Id}/image/{imageFile.Id}",
                imageFile.PublicPath(release));
        }

        [Fact]
        public void PublicPath_FileTypeIsNotAPublicFileType()
        {
            var release = new Release
            {
                Id = Guid.NewGuid()
            };

            EnumUtil.GetEnumValues<FileType>().ForEach(type =>
            {
                if (!PublicFileTypes.Contains(type))
                {
                    var file = new File
                    {
                        Type = type
                    };
                    Assert.Throws<ArgumentOutOfRangeException>(() => file.PublicPath(release));
                }
            });
        }
    }
}
