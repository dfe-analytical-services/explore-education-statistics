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

        private static readonly List<FileType> SupportedFileTypes = new List<FileType>
        {
            Image
        };

        public MethodologyFileRepository(ContentDbContext contentDbContext)
        {
            _contentDbContext = contentDbContext;
        }

        public async Task<MethodologyFile> Create(Guid methodologyId,
            string filename,
            FileType type,
            Guid createdById)
        {
            if (!SupportedFileTypes.Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Cannot create file for file type");
            }

            var methodologyFile = new MethodologyFile
            {
                MethodologyId = methodologyId,
                File = new File
                {
                    Created = DateTime.UtcNow,
                    CreatedById = createdById,
                    RootPath = methodologyId,
                    Filename = filename,
                    Type = type
                }
            };

            var created = (await _contentDbContext.MethodologyFiles.AddAsync(methodologyFile)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created;
        }

        public async Task<Either<ActionResult, File>> CheckFileExists(Guid methodologyId,
            Guid fileId,
            params FileType[] allowedFileTypes)
        {
            // Ensure file is linked to the Methodology by getting the MethodologyFile first
            var methodologyFile = await Get(methodologyId, fileId);

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

        public async Task Delete(Guid methodologyId, Guid fileId)
        {
            var methodologyFile = await Get(methodologyId, fileId);
            if (methodologyFile != null)
            {
                _contentDbContext.MethodologyFiles.Remove(methodologyFile);
                await _contentDbContext.SaveChangesAsync();
            }
        }

        public async Task<MethodologyFile> Get(Guid methodologyId, Guid fileId)
        {
            return await _contentDbContext.MethodologyFiles
                .Include(methodologyFile => methodologyFile.File)
                .SingleOrDefaultAsync(methodologyFile =>
                    methodologyFile.MethodologyId == methodologyId
                    && methodologyFile.FileId == fileId);
        }

        public async Task<List<MethodologyFile>> GetByFileType(Guid methodologyId, params FileType[] types)
        {
            return await _contentDbContext.MethodologyFiles
                .Include(f => f.File)
                .Where(methodologyFile =>
                    methodologyFile.MethodologyId == methodologyId
                    && types.Contains(methodologyFile.File.Type))
                .ToListAsync();
        }

        public Task<List<MethodologyFile>> GetMethodologyLinksToFile(Guid fileId)
        {
            return _contentDbContext
                .MethodologyFiles
                .Where(f => f.FileId == fileId)
                .ToListAsync();
        }
    }
}
