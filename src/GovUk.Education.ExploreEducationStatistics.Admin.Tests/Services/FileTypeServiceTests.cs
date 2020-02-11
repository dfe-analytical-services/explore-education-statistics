using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
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
        // ideally this would be detected as application/msword, but this is close enough
        private static readonly FileInfo Doc = new FileInfo("test.doc", "application/CDFV2");
        
        private static readonly FileInfo Docx = new FileInfo("test.docx", 
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        
        private static readonly FileInfo Ods = new FileInfo("test.ods", 
            "application/vnd.oasis.opendocument.spreadsheet");
        
        private static readonly FileInfo Odt = new FileInfo("test.odt", 
            "application/vnd.oasis.opendocument.text");
        
        // ideally this would be detected as application/msexcel or application/vnd.ms-excel, but this is close enough
        private static readonly FileInfo Xls = new FileInfo("test.xls", "application/CDFV2");
        private static readonly FileInfo Csv = new FileInfo("test.csv", "application/csv");
        private static readonly FileInfo Txt = new FileInfo("test.txt", "text/plain");
        private static readonly FileInfo Pdf = new FileInfo("test.pdf", "application/pdf");
        private static readonly FileInfo Bmp = new FileInfo("test.bmp", "image/bmp");
        private static readonly FileInfo Gif = new FileInfo("test.gif", "image/gif");
        private static readonly FileInfo Jpg = new FileInfo("test.jpg", "image/jpeg");
        private static readonly FileInfo Png = new FileInfo("test.png", "image/png");

        private static readonly List<FileInfo> ImageTypes = new List<FileInfo>
        {
            Bmp,
            Gif,
            Jpg,
            Png
        };
        
        private static readonly List<FileInfo> CsvTypes = new List<FileInfo>
        {
            Csv,
            Txt,
        };
        
        private static readonly List<FileInfo> AllTypes = new List<FileInfo>
        {
            Doc,
            Docx,
            Ods,
            Odt,
            Xls,
            Csv,
            Txt,
            Pdf,
            Bmp,
            Gif,
            Jpg,
            Png
        };
        
        [Fact]
        public void GetMimeType_Doc()
        {
            AssertMimeTypeCorrect(Doc);
        }
        
        [Fact]
        public void GetMimeType_Docx()
        {
            AssertMimeTypeCorrect(Docx);
        }
        
        [Fact]
        public void GetMimeType_Ods()
        {
            AssertMimeTypeCorrect(Ods);
        }
        
        [Fact]
        public void GetMimeType_Odt()
        {
            AssertMimeTypeCorrect(Odt);
        }
        
        [Fact]
        public void GetMimeType_Xls()
        {
            AssertMimeTypeCorrect(Xls);
        }
        
        [Fact]
        public void GetMimeType_Csv()
        {
            AssertMimeTypeCorrect(Csv);
        }
        
        [Fact]
        public void GetMimeType_Pdf()
        {
            AssertMimeTypeCorrect(Pdf);
        }
        
        [Fact]
        public void GetMimeType_Bmp()
        {
            AssertMimeTypeCorrect(Bmp);
        }
        
        [Fact]
        public void GetMimeType_Jpg()
        {
            AssertMimeTypeCorrect(Jpg);
        }
        
        [Fact]
        public void GetMimeType_Gif()
        {
            AssertMimeTypeCorrect(Gif);
        }
        
        [Fact]
        public void GetMimeType_Png()
        {
            AssertMimeTypeCorrect(Png);
        }
        
        [Fact]
        public void GetMimeType_Txt()
        {
            AssertMimeTypeCorrect(Txt);
        }
        
        [Fact]
        public void ValidChartFileUploads()
        {
            // test that all image files are valid for chart file uploads and not others
            AllTypes.ForEach(type =>
            {
                var expectedToSucceed = ImageTypes.Contains(type);
                AssertHasMatchingMimeType(type, ReleasesController.AllowedChartFileTypes.ToList(), expectedToSucceed);
            });
        }
        
        [Fact]
        public void ValidAncillaryFileUploads()
        {
            // check that all types are valid for ancillary file uploads
            AllTypes.ForEach(type =>
            {
                AssertHasMatchingMimeType(type, ReleasesController.AllowedAncillaryFileTypes.ToList(), true);
            });
        }
        
        [Fact]
        public void ValidCsvFileUploads()
        {
            // check that csv file types (either explicitly detected as application/csv / text/csv or indirectly as
            // text/plain) are valid for data and metadata file uploads
            AllTypes.ForEach(type =>
            {
                var expectedToSucceed = CsvTypes.Contains(type);
                AssertHasMatchingMimeType(type, FileStorageService.CsvMimeTypes.ToList(), expectedToSucceed);
            });
        }
        
        private static void AssertMimeTypeCorrect(FileInfo fileInfo)
        {
            var service = new FileTypeService();

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + fileInfo.Filename);
            
            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));
            
            var result = service.GetMimeType(formFile.Object);
            
            Assert.True(fileInfo.ExpectedMimeTypes.Contains(result), 
                "Expected " + result + " to be contained in the expected mime types list");
        }
        
        private static void AssertHasMatchingMimeType(FileInfo fileInfo, List<Regex> availableMimeTypes, 
            bool expectedToSucceed)
        {
            var service = new FileTypeService();

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + fileInfo.Filename);
            
            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));
            
            var result = service.HasMatchingMimeType(formFile.Object, availableMimeTypes);
            
            Assert.Equal(expectedToSucceed, result);
        }
    }
}