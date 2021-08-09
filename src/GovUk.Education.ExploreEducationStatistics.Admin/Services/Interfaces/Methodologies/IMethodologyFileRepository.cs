using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies
{
    public interface IMethodologyFileRepository
    {
        public Task<MethodologyFile> Create(
            Guid methodologyId,
            string filename,
            FileType type,
            Guid createdById);

        public Task<Either<ActionResult, File>> CheckFileExists(Guid methodologyId,
            Guid fileId,
            params FileType[] allowedFileTypes);

        public Task Delete(Guid methodologyId, Guid fileId);

        public Task<MethodologyFile> Get(Guid methodologyId, Guid fileId);

        public Task<List<MethodologyFile>> GetByFileType(Guid methodologyId, params FileType[] types);
        
        public Task<List<MethodologyFile>> GetByFile(Guid fileId);
    }
}
