#nullable enable
using System;
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
            await using var stream = file.OpenReadStream();
            var mimeType = GuessMagicInfo(stream,  MagicOpenFlags.MAGIC_MIME_TYPE);

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
            var mimeType = GuessMagicInfo(stream,  MagicOpenFlags.MAGIC_MIME_TYPE);

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
            var encodingType = GuessMagicInfo(stream,  MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Any(pattern => pattern.Equals(encodingType));
        }

        public async Task<bool> IsValidCsvFile(Stream stream)
        {
            var sampleBuffer = new byte[1024];
            var sampleBufferSize = await stream.ReadAsync(sampleBuffer, 0, 1024);

            var magicFilePath = _configuration.GetValue<string>("MagicFilePath");

            // @MarkFix do we need more than 1024 bytes? - like first 1000 lines like previously? - test it
            var magicTypeContext = new Magic(MagicOpenFlags.MAGIC_MIME_TYPE, magicFilePath);
            var mimeType = magicTypeContext.Read(sampleBuffer, sampleBufferSize);
            if (!AllowedCsvMimeTypes.Any(pattern => pattern.Match(mimeType).Success))
            {
                return false;
            }

            var magicEncodingContext = new Magic(MagicOpenFlags.MAGIC_MIME_ENCODING, magicFilePath);
            var encodingType = magicEncodingContext.Read(sampleBuffer, sampleBufferSize);

            return AllowedCsvEncodingTypes.Contains(encodingType);
        }

        public async Task<bool> IsValidCsvFile(Func<Task<Stream>> streamProvider)
        {
            await using var stream = await streamProvider.Invoke();
            return await IsValidCsvFile(stream);
        }

        private async Task<FileType?> GetMimeTypeUsingMimeDetective(Stream stream)
        {
            // Mime Detective is much better at zip files
            return await stream.GetFileTypeAsync();
        }

        private string GuessMagicInfo(Stream fileStream,  MagicOpenFlags flag)
        {
            var dbPath = _configuration.GetValue<string>("MagicFilePath");
            var magic = new Magic(flag, dbPath);

            // I have no idea why wrapping the Stream as a StreamReader is necessary, but it is.
            using var streamReader = new StreamReader(fileStream);
            return magic.Read(streamReader.BaseStream, 1024); // Read handles if buffer size is < 1024
        }

        private bool IsMimeTypeNullOrZip(string mimeType)
        {
            return mimeType == "application/octet-stream"
                   || mimeType == "application/zip";
        }
    }
}
