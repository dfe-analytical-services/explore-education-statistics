#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies
{
    public class MethodologyFileRepository : IMethodologyFileRepository
    {
        private readonly ContentDbContext _contentDbContext;

        private static readonly List<FileType> SupportedFileTypes = new()
        {
            Image
        };

        public MethodologyFileRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<MethodologyFile> Create(Guid methodologyVersionId,
            string filename,
            long contentLength,
            string contentType,
            FileType type,
            Guid createdById)
        {
            if (!SupportedFileTypes.Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot create file for file type");
            }

            var methodologyFile = new MethodologyFile
            {
                MethodologyVersionId = methodologyVersionId,
                File = new File
                {
                    CreatedById = createdById,
                    RootPath = methodologyVersionId,
                    Filename = filename,
                    ContentLength = contentLength,
                    ContentType = contentType,
                    Type = type
                }
            };

            var created = (await _contentDbContext.MethodologyFiles.AddAsync(methodologyFile)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created;
        }

        public async Task<Either<ActionResult, File>> CheckFileExists(Guid methodologyVersionId,
            Guid fileId,
            params FileType[] allowedFileTypes)
        {
            // Ensure file is linked to the version by getting the MethodologyFile first
            var methodologyFile = await Get(methodologyVersionId, fileId);

            if (methodologyFile == null)
            {
                return new NotFoundResult();
            }

            if (allowedFileTypes.Any() && !allowedFileTypes.Contains(methodologyFile.File.Type))
            {
                return ValidationUtils.ValidationActionResult(FileTypeInvalid);
            }

            return methodologyFile.File;
        }

        public async Task Delete(Guid methodologyVersionId, Guid fileId)
        {
            var methodologyFile = await Get(methodologyVersionId, fileId);
            if (methodologyFile != null)
            {
                _contentDbContext.MethodologyFiles.Remove(methodologyFile);
                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task<MethodologyFile?> Get(Guid methodologyVersionId, Guid fileId)
        {
            return await _contentDbContext.MethodologyFiles
                .Include(methodologyFile => methodologyFile.File)
                .SingleOrDefaultAsync(methodologyFile =>
                    methodologyFile.MethodologyVersionId == methodologyVersionId
                    && methodologyFile.FileId == fileId);
        }

        public async Task<List<MethodologyFile>> GetByFileType(Guid methodologyVersionId, params FileType[] types)
        {
            return await _contentDbContext.MethodologyFiles
                .Include(f => f.File)
                .Where(methodologyFile =>
                    methodologyFile.MethodologyVersionId == methodologyVersionId
                    && types.Contains(methodologyFile.File.Type))
                .ToListAsync();
        }

        public Task<List<MethodologyFile>> GetByFile(Guid fileId)
        {
            return _contentDbContext
                .MethodologyFiles
                .AsQueryable()
                .Where(f => f.FileId == fileId)
                .ToListAsync();
        }
    }
}
