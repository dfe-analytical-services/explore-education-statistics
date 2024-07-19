#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MimeDetective.Extensions;
using FileType = MimeDetective.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class FileTypeService : IFileTypeService
    {
        public static readonly Regex[] AllowedCsvMimeTypes = {
            new ("^(application|text)/csv$"),
            new ("^text/plain$")
        };

        public static readonly string[] AllowedCsvEncodingTypes = [
            "us-ascii",
            "utf-8"
        ];

        private readonly IConfiguration _configuration;

        public FileTypeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetMimeType(IFormFile file)
        {
            // The Mime project is generally very good at determining mime types from file contents
            var mimeType = GuessMagicInfo(file,  MagicOpenFlags.MAGIC_MIME_TYPE);

            if (mimeType is "application/octet-stream" or "application/zip") // i.e. is zip file
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
            var encodingType = GuessMagicInfo(file,  MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Contains(encodingType);
        }

        public async Task<bool> IsValidCsvFile(Stream stream)
        {
            var sampleBuffer = new byte[1024];
            var sampleBufferSize = await stream.ReadAsync(sampleBuffer, 0, 1024);

            var magicFilePath = _configuration.GetValue<string>("MagicFilePath");

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

        public async Task<bool> IsValidZipFile(IFormFile zipFile)
        {
            return await HasMatchingMimeType(zipFile, FileTypeValidationUtils.AllowedArchiveMimeTypes)
                && HasMatchingEncodingType(zipFile, FileTypeValidationUtils.AllowedArchiveEncodingTypes);
        }

        private async Task<FileType?> GetMimeTypeUsingMimeDetective(Stream stream)
        {
            // Mime Detective is much better at zip files
            return await stream.GetFileTypeAsync();
        }

        private string GuessMagicInfo(IFormFile file, MagicOpenFlags flag)
        {
            var dbPath = _configuration.GetValue<string>("MagicFilePath");
            var magic = new Magic(flag, dbPath);

            using var fileStream = file.OpenReadStream();
            return magic.Read(fileStream, 1024); // magic.Read handles if buffer size is < 1024
        }
    }
}
