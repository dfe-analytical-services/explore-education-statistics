#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MimeDetective.Extensions;
using FileType = MimeDetective.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class FileTypeService(IConfiguration configuration) : IFileTypeService
    {
        public static readonly Regex[] AllowedCsvMimeTypes = {
            new ("^(application|text)/csv$"),
            new ("^text/plain$"),
        };

        public static readonly string[] AllowedCsvEncodingTypes = [
            "us-ascii",
            "utf-8",
        ];

        public async Task<string> GetMimeType(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var mimeType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeType;
        }

        public async Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes)
        {
            await using var stream = file.OpenReadStream();
            var mimeType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes)
        {
            using var stream = file.OpenReadStream();
            var encodingType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Contains(encodingType);
        }

        public async Task<bool> HasMatchingMimeType(Stream stream, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(stream);
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(Stream stream, IEnumerable<string> encodingTypes)
        {
            var encodingType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Contains(encodingType);
        }

        public async Task<bool> IsValidCsvFile(Stream stream)
        {
            var sampleBuffer = new byte[1024];
            var sampleBufferSize = await stream.ReadAsync(sampleBuffer, 0, 1024);

            var magicFilePath = configuration.GetValue<string>("MagicFilePath");

            // @MarkFix check this works rather than the previous 1000 line check
            // Check mime type
            var magicTypeContext = new Magic(MagicOpenFlags.MAGIC_MIME_TYPE, magicFilePath);
            var mimeType = magicTypeContext.Read(sampleBuffer, sampleBufferSize);
            if (!AllowedCsvMimeTypes.Any(pattern => pattern.Match(mimeType).Success))
            {
                return false;
            }

            // Check encoding
            var magicEncodingContext = new Magic(MagicOpenFlags.MAGIC_MIME_ENCODING, magicFilePath);
            var encodingType = magicEncodingContext.Read(sampleBuffer, sampleBufferSize);

            return AllowedCsvEncodingTypes.Contains(encodingType);
        }

        public Task<bool> IsValidCsvFile(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            return IsValidCsvFile(stream);
        }

        private async Task<FileType?> GetMimeTypeUsingMimeDetective(Stream stream)
        {
            // Mime Detective is much better at zip files
            return await stream.GetFileTypeAsync();
        }

        private string GuessMagicInfo(Stream fileStream, MagicOpenFlags flag)
        {
            using var reader = new StreamReader(fileStream);

            var dbPath = configuration.GetValue<string>("MagicFilePath");
            var magic = new Magic(flag, dbPath);

            var bufferSize = reader.BaseStream.Length >= 1024 ? 1024 : (int) reader.BaseStream.Length;
            return magic.Read(reader.BaseStream, bufferSize);
        }

        private bool IsMimeTypeNullOrZip(string mimeType)
        {
            return mimeType is "application/octet-stream" or "application/zip";
        }
    }
}
