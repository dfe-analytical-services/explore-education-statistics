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
            var mimeType = await GetMimeType(file);
            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes)
        {
            using var stream = file.OpenReadStream();
            var encodingType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Contains(encodingType);
        }

        public async Task<bool> IsValidCsvFile(Stream stream)
        {
            var sampleBuffer = new byte[1024];
            var sampleBufferSize = await stream.ReadAsync(sampleBuffer, 0, 1024);

            var magicFilePath = configuration.GetValue<string>("MagicFilePath");

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

        private async Task<FileType?> GetMimeTypeUsingMimeDetective(Stream stream)
        {
            // Mime Detective is much better at zip files
            return await stream.GetFileTypeAsync();
        }

        private string GuessMagicInfo(Stream stream, MagicOpenFlags flag)
        {
            var magicFilePath = configuration.GetValue<string>("MagicFilePath");
            var magicContext = new Magic(flag, magicFilePath);

            // Wrapping in a StreamReader is necessary with DeflateStreams - stream of a file in a zip - in order
            // for magicContext.Read to work correctly. Why? No idea.
            using var reader = new StreamReader(stream);
            return magicContext.Read(reader.BaseStream, 1024); // Read method hands if size < 1024
        }

        private bool IsMimeTypeNullOrZip(string mimeType)
        {
            return mimeType is "application/octet-stream" or "application/zip";
        }
    }
}
