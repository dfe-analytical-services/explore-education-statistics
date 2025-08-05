#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

class FileInfo
{
    public readonly string Filename;

    // we have a possible list of matching mime types that we'd expect to see, that potentially change when run
    // on different operating systems, but are considered valid either way 
    public readonly string[] ExpectedMimeTypes;

    public FileInfo(string filename, params string[] expectedMimeTypes)
    {
        Filename = filename;
        ExpectedMimeTypes = expectedMimeTypes;
    }
}

public class FileTypeServiceTests
{
    private static readonly FileInfo Doc = new("test.doc", "application/msword");

    private static readonly FileInfo Docx = new("test.docx",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

    private static readonly FileInfo Ods = new("test.ods",
        "application/vnd.oasis.opendocument.spreadsheet");

    private static readonly FileInfo Odt = new("test.odt",
        "application/vnd.oasis.opendocument.text");

    private static readonly FileInfo Xls = new("test.xls", "application/vnd.ms-excel");
    private static readonly FileInfo Xlsx = new("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    private static readonly FileInfo Xlsx2 = new("test2.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    private static readonly FileInfo Csv = new("test.csv", "text/csv");
    private static readonly FileInfo SmallCsv = new("small-meta.csv", "text/csv");
    private static readonly FileInfo LongHeaderRowCsv = new("long-header-row.csv", "text/csv");
    private static readonly FileInfo Txt = new("test.txt", "text/plain");
    private static readonly FileInfo Pdf = new("test.pdf", "application/pdf");
    private static readonly FileInfo Bmp = new("test.bmp", "image/bmp");
    private static readonly FileInfo Gif = new("test.gif", "image/gif");
    private static readonly FileInfo Jpg = new("test.jpg", "image/jpeg");
    private static readonly FileInfo Png = new("test.png", "image/png");
    private static readonly FileInfo Svg = new("test.svg", "image/svg+xml");
    private static readonly FileInfo SvgNoXml = new("test-no-xml.svg", "image/svg+xml");
    private static readonly FileInfo Zip = new("test.zip", "application/x-compressed");

    private static readonly List<FileInfo> ImageTypes =
    [
        Bmp,
        Gif,
        Jpg,
        Png,
    ];

    private static readonly List<FileInfo> CsvTypes =
    [
        Csv,
        SmallCsv,
        LongHeaderRowCsv,
        Txt,
    ];

    private static readonly List<FileInfo> ZipTypes =
    [
        Zip,
    ];

    private static readonly List<FileInfo> AllTypes =
    [
        Doc,
        Docx,
        Ods,
        Odt,
        Xls,
        Xlsx,
        Xlsx2,
        Csv,
        SmallCsv,
        LongHeaderRowCsv,
        Txt,
        Pdf,
        Bmp,
        Gif,
        Jpg,
        Png,
        Zip,
    ];

    [Fact]
    public async Task GetMimeType_Doc()
    {
        await AssertMimeTypeCorrect(Doc);
    }

    [Fact]
    public async Task GetMimeType_Docx()
    {
        await AssertMimeTypeCorrect(Docx);
    }

    [Fact]
    public async Task GetMimeType_Ods()
    {
        await AssertMimeTypeCorrect(Ods);
    }

    [Fact]
    public async Task GetMimeType_Odt()
    {
        await AssertMimeTypeCorrect(Odt);
    }

    [Fact]
    public async Task GetMimeType_Xls()
    {
        await AssertMimeTypeCorrect(Xls);
    }

    [Fact]
    public async Task GetMimeType_Xlsx()
    {
        await AssertMimeTypeCorrect(Xlsx);
    }

    [Fact]
    public async Task GetMimeType_Xlsx2()
    {
        await AssertMimeTypeCorrect(Xlsx2);
    }

    [Fact]
    public async Task GetMimeType_Csv()
    {
        await AssertMimeTypeCorrect(Csv);
    }

    [Fact]
    public async Task GetMimeType_Pdf()
    {
        await AssertMimeTypeCorrect(Pdf);
    }

    [Fact]
    public async Task GetMimeType_Bmp()
    {
        await AssertMimeTypeCorrect(Bmp);
    }

    [Fact]
    public async Task GetMimeType_Jpg()
    {
        await AssertMimeTypeCorrect(Jpg);
    }

    [Fact]
    public async Task GetMimeType_Gif()
    {
        await AssertMimeTypeCorrect(Gif);
    }

    [Fact]
    public async Task GetMimeType_Png()
    {
        await AssertMimeTypeCorrect(Png);
    }

    [Fact]
    public async Task GetMimeType_Svg()
    {
        await AssertMimeTypeCorrect(Svg);
    }

    [Fact]
    public async Task GetMimeType_Svg_NoXml()
    {
        await AssertMimeTypeCorrect(SvgNoXml);
    }

    [Fact]
    public async Task GetMimeType_Txt()
    {
        await AssertMimeTypeCorrect(Txt);
    }

    [Fact]
    public async Task HasMatchingMimeType_ValidChartFileUploads()
    {
        // test that all image files are valid for chart file uploads and not others
        foreach (var type in AllTypes)
        {
            var expectedToSucceed = ImageTypes.Contains(type);
            await AssertHasMatchingMimeType(type, AllowedChartFileTypes.ToList(), expectedToSucceed);
        }
    }

    [Fact]
    public async Task HasMatchingMimeType_ValidAncillaryFileUploads()
    {
        // check that all types are valid for ancillary file uploads
        foreach (var type in AllTypes)
        {
            await AssertHasMatchingMimeType(type, AllowedAncillaryFileTypes.ToList(), true);
        }
    }

    [Fact]
    public async Task HasMatchingMimeType_ValidCsvFileUploads()
    {
        // check that csv file types (either explicitly detected as application/csv / text/csv or indirectly as
        // text/plain) are valid for data and metadata file uploads
        foreach (var type in AllTypes)
        {
            var expectedToSucceed = CsvTypes.Contains(type);
            await AssertHasMatchingMimeType(type, AllowedCsvMimeTypes.ToList(), expectedToSucceed);
        }
    }

    [Fact]
    public async Task HasMatchingMimeType_ValidZipFileUploads()
    {
        foreach (var type in AllTypes)
        {
            var expectedToSucceed = ZipTypes.Contains(type);
            await AssertHasMatchingMimeType(type, AllowedZipFileMimeTypes.ToList(), expectedToSucceed);
        }
    }

    [Fact]
    public void HasMatchingEncodingType_ValidCsv()
    {
        var allowedEncodingTypes = AllowedCsvEncodingTypes;

        var service = BuildService();

        foreach (var type in AllTypes)
        {
            var expectedToSucceed = CsvTypes.Contains(type);

            var fileInfo = type;

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources", fileInfo.Filename);

            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));

            var result = service.HasMatchingEncodingType(formFile.Object, allowedEncodingTypes);

            Assert.Equal(expectedToSucceed, result);
        }
    }

    [Fact]
    public void HasMatchingEncodingType_ValidZip()
    {
        var fileInfo = Zip;

        var service = BuildService();

        var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", fileInfo.Filename);

        var formFile = new Mock<IFormFile>();
        formFile
            .Setup(f => f.OpenReadStream())
            .Returns(() => File.OpenRead(filePath));

        var result = service.HasMatchingEncodingType(formFile.Object, AllowedZipFileEncodingTypes);

        Assert.True(result);
    }

    [Fact]
    public async Task IsValidCsvFile_AllTypes()
    {
        var service = BuildService();

        foreach (var type in AllTypes)
        {
            var expectedToSucceed = CsvTypes.Contains(type);

            var fileInfo = type;

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources", fileInfo.Filename);

            await using var stream = File.OpenRead(filePath);

            var result = await service.HasCsvFileType(stream);

            Assert.Equal(expectedToSucceed, result);
        }
    }

    [Fact]
    public async Task IsValidZipFile_AllTypes()
    {
        var service = BuildService();

        foreach (var type in AllTypes)
        {
            var expectedToSucceed = ZipTypes.Contains(type);

            var fileInfo = type;

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Resources", fileInfo.Filename);

            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));

            var result = await service.HasZipFileType(formFile.Object);

            Assert.Equal(expectedToSucceed, result);
        }
    }

    private static async Task AssertMimeTypeCorrect(FileInfo fileInfo)
    {
        var service = BuildService();

        var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", fileInfo.Filename);

        var formFile = new Mock<IFormFile>();
        formFile
            .Setup(f => f.OpenReadStream())
            .Returns(() => File.OpenRead(filePath));

        var result = await service.GetMimeType(formFile.Object);

        Assert.True(fileInfo.ExpectedMimeTypes.Contains(result),
            "Expected " + result + " to be contained in the expected mime types list");
    }

    private static async Task AssertHasMatchingMimeType(FileInfo fileInfo, List<Regex> availableMimeTypes,
        bool expectedToSucceed)
    {
        var service = BuildService();

        var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources", fileInfo.Filename);

        var formFile = new Mock<IFormFile>();
        formFile
            .Setup(f => f.OpenReadStream())
            .Returns(() => File.OpenRead(filePath));

        var result = await service.HasMatchingMimeType(formFile.Object, availableMimeTypes);

        Assert.Equal(expectedToSucceed, result);
    }

    private static IConfiguration DefaultConfiguration()
    {
        return new ConfigurationBuilder().Build();
    }

    private static FileTypeService BuildService(IConfiguration? configuration = null)
    {
        return new FileTypeService(
            configuration ?? DefaultConfiguration()
        );
    }
}
