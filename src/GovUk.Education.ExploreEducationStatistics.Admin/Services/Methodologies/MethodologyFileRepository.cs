using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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

        public async Task<File> Create(Guid methodologyId, string filename, FileType type)
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
                    // Mark any new files as already migrated while these flags temporarily exist
                    PrivateBlobPathMigrated = true,
                    PublicBlobPathMigrated = true,
                    RootPath = methodologyId,
                    Filename = filename,
                    Type = type
                }
            };

            var created = (await _contentDbContext.MethodologyFiles.AddAsync(methodologyFile)).Entity;
            await _contentDbContext.SaveChangesAsync();
            return created.File;
        }
    }
}
