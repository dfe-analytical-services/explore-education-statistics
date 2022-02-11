using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using MimeDetective.Extensions;
using FileType = MimeDetective.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class FileTypeService : IFileTypeService
    {
        public async Task<string> GetMimeType(IFormFile file)
        {
            // The Mime project is generally very good at determining mime types from file contents
            var mimeType = GetMimeTypeUsingMimeProject(file.OpenReadStream());

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeType;
        }

        public async Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GetMimeTypeUsingMimeProject(file.OpenReadStream());

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes)
        {
            var encodingType = GetMimeEncoding(file.OpenReadStream());
            return encodingTypes.Any(pattern => pattern.Equals(encodingType));
        }

        public async Task<bool> HasMatchingMimeType(Stream stream, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GetMimeTypeUsingMimeProject(stream);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(stream);
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(Stream stream, IEnumerable<string> encodingTypes)
        {
            var encodingType = GetMimeEncoding(stream);
            return encodingTypes.Any(pattern => pattern.Equals(encodingType));
        }

        private string GetMimeTypeUsingMimeProject(Stream stream)
        {
            // The Mime project is generally very good at determining mime types from file contents
            return GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);
        }

        private async Task<FileType> GetMimeTypeUsingMimeDetective(Stream stream)
        {
            // Mime Detective is much better at zip files
            return await stream.GetFileTypeAsync();
        }

        private string GetMimeEncoding(Stream stream)
        {
            return GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_ENCODING);
        }

        private string GuessMagicInfo(Stream fileStream, MagicOpenFlags flag)
        {
            using var reader = new StreamReader(fileStream);
            var magic = new Magic(flag);
            var bufferSize = reader.BaseStream.Length >= 1024 ? 1024 : (int) reader.BaseStream.Length;
            return magic.Read(reader.BaseStream, bufferSize);
        }

        private bool IsMimeTypeNullOrZip(string mimeType)
        {
            return mimeType == null ||
                   mimeType == "application/octet-stream" ||
                   mimeType == "application/zip";
        }
    }
}
