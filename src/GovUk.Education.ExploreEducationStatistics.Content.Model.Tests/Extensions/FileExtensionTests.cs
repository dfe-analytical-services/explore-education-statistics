using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions.FileExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions;

public class FileExtensionTests
{
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
        var releaseVersionId = Guid.NewGuid();

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

        Assert.Equal($"{releaseVersionId}/ancillary/{ancillaryFile.Id}",
            ancillaryFile.PublicPath(releaseVersionId: releaseVersionId));

        Assert.Equal($"{releaseVersionId}/chart/{chartFile.Id}",
            chartFile.PublicPath(releaseVersionId: releaseVersionId));

        Assert.Equal($"{releaseVersionId}/data/{dataFile.Id}",
            dataFile.PublicPath(releaseVersionId: releaseVersionId));

        Assert.Equal($"{releaseVersionId}/image/{imageFile.Id}",
            imageFile.PublicPath(releaseVersionId: releaseVersionId));
    }

    [Fact]
    public void PublicPath_FileTypeIsNotAPublicFileType()
    {
        EnumUtil.GetEnums<FileType>().ForEach(type =>
        {
            if (!PublicFileTypes.Contains(type))
            {
                var file = new File
                {
                    Type = type
                };
                Assert.Throws<ArgumentOutOfRangeException>(() => file.PublicPath(releaseVersionId: Guid.NewGuid()));
            }
        });
    }

    [Theory]
    [InlineData(1, "1 B")]
    [InlineData(1024, "1 Kb")]
    [InlineData(1048576, "1 Mb")]
    [InlineData(1073741824, "1 Gb")]
    [InlineData(1099511627776, "1 Tb")]
    public void DisplaySize_FromFile_ReturnsStringOfCorrectSizeAndUnit(
        long contentLength,
        string expectedDisplaySize)
    {
        var file = new File
        {
            ContentLength = contentLength
        };

        Assert.Equal(expectedDisplaySize, file.DisplaySize());
    }

    [Theory]
    [InlineData(1, "1 B")]
    [InlineData(1024, "1 Kb")]
    [InlineData(1048576, "1 Mb")]
    [InlineData(1073741824, "1 Gb")]
    [InlineData(1099511627776, "1 Tb")]
    public void DisplaySize_FromNumber_ReturnsStringOfCorrectSizeAndUnit(
        long contentLength,
        string expectedDisplaySize)
    {
        Assert.Equal(expectedDisplaySize, contentLength.DisplaySize());
    }
}
