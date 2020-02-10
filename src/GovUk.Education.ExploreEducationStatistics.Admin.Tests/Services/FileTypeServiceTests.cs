using System.IO;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FileTypeServiceTests
    {
        [Fact]
        public void GetMimeType_Doc()
        {
            AssertMimeTypeCorrect("test.doc", "application/CDFV2");
        }
        
        [Fact]
        public void GetMimeType_Docx()
        {
            AssertMimeTypeCorrect("test.docx", 
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        }
        
        [Fact]
        public void GetMimeType_Ods()
        {
            AssertMimeTypeCorrect("test.ods", 
                "application/vnd.oasis.opendocument.spreadsheet");
        }
        
        [Fact]
        public void GetMimeType_Odt()
        {
            AssertMimeTypeCorrect("test.odt", "application/vnd.oasis.opendocument.text");
        }
        
        [Fact]
        public void GetMimeType_Xls()
        {
            AssertMimeTypeCorrect("test.xls", "application/CDFV2");
        }
        
        [Fact]
        public void GetMimeType_Csv()
        {
            AssertMimeTypeCorrect("test.csv", "application/csv");
        }
        
        [Fact]
        public void GetMimeType_Pdf()
        {
            AssertMimeTypeCorrect("test.pdf", "application/pdf");
        }
        
        [Fact]
        public void GetMimeType_Rtf()
        {
            AssertMimeTypeCorrect("test.rtf", "text/plain");
        }
        
        [Fact]
        public void GetMimeType_Bmp()
        {
            AssertMimeTypeCorrect("test.bmp", "image/bmp");
        }
        
        [Fact]
        public void GetMimeType_Jpg()
        {
            AssertMimeTypeCorrect("test.jpg", "image/jpeg");
        }
        
        [Fact]
        public void GetMimeType_Gif()
        {
            AssertMimeTypeCorrect("test.gif", "image/gif");
        }
        
        [Fact]
        public void GetMimeType_Png()
        {
            AssertMimeTypeCorrect("test.png", "image/png");
        }
        
        [Fact]
        public void GetMimeType_Txt()
        {
            AssertMimeTypeCorrect("test.txt", "text/plain");
        }
        
        private static void AssertMimeTypeCorrect(string filename, string expectedMimeType)
        {
            var service = new FileTypeService();

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources" + Path.DirectorySeparatorChar + filename);
            
            var formFile = new Mock<IFormFile>();
            formFile
                .Setup(f => f.OpenReadStream())
                .Returns(() => File.OpenRead(filePath));
            
            var result = service.GetMimeType(formFile.Object);
            
            Assert.Equal(expectedMimeType, result);
        }
    }
}