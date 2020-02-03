using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileTypeService : IFileTypeService
    {
        private readonly string _magicFilePath;

        public FileTypeService(IFileProvider fileProvider)
        {
            var magicFile = fileProvider.GetFileInfo("magic.mgc");
            _magicFilePath = magicFile.PhysicalPath;
        }

        public string GetMimeType(IFormFile file)
        {
            return GuessMagicInfo(file.OpenReadStream(), MagicOpenFlags.MAGIC_MIME_TYPE);
        }
        
        public string GetMimeEncoding(IFormFile file)
        {
            return GuessMagicInfo(file.OpenReadStream(), MagicOpenFlags.MAGIC_MIME_ENCODING);
        }
        
        public bool HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GuessMagicInfo(file.OpenReadStream(), MagicOpenFlags.MAGIC_MIME_TYPE);
            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }
        
        public bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes)
        {
            var encodingType = GuessMagicInfo(file.OpenReadStream(), MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Any(pattern => pattern.Equals(encodingType));
        }
        
        private string GuessMagicInfo(Stream fileStream, MagicOpenFlags flag)
        {
            using var reader = new StreamReader(fileStream);
            var magic = new Magic(flag, _magicFilePath);
            var bufferSize = reader.BaseStream.Length >= 1024 ? 1024 : (int) reader.BaseStream.Length;
            return magic.Read(reader.BaseStream, bufferSize);
        }
    }
}