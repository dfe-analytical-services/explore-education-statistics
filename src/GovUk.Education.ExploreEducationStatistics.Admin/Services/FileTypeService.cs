using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using MimeDetective.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileTypeService : IFileTypeService
    {
        public string GetMimeType(IFormFile file)
        {
            // The Mime project is generally very good at determining mime types from file contents
            var mimeType = GuessMagicInfo(file.OpenReadStream(), MagicOpenFlags.MAGIC_MIME_TYPE);

            if (mimeType != null && mimeType != "application/octet-stream")
            {
                return mimeType;
            }
            
            // However, it does not determine zipped types particularly well, like some of the open doc and spreadsheet
            // formats.  Mime Detective is much better at doing this.
            var fileType = file.OpenReadStream().GetFileType();

            return fileType?.Mime ?? mimeType;
        }
        
        public string GetMimeEncoding(IFormFile file)
        {
            return GuessMagicInfo(file.OpenReadStream(), MagicOpenFlags.MAGIC_MIME_ENCODING);
        }
        
        public bool HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GetMimeType(file);
            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }
        
        public bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes)
        {
            var encodingType = GetMimeEncoding(file);
            return encodingTypes.Any(pattern => pattern.Equals(encodingType));
        }
        
        private string GuessMagicInfo(Stream fileStream, MagicOpenFlags flag)
        {
            using var reader = new StreamReader(fileStream);
            var magic = new Magic(flag);
            var bufferSize = reader.BaseStream.Length >= 1024 ? 1024 : (int) reader.BaseStream.Length;
            return magic.Read(reader.BaseStream, bufferSize);
        }
    }
}