#nullable enable
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
            Guid methodologyVersionId,
            string filename,
            long contentLength,
            string contentType,
            FileType type,
            Guid createdById);

        public Task<Either<ActionResult, File>> CheckFileExists(Guid methodologyVersionId,
            Guid fileId,
            params FileType[] allowedFileTypes);

        public Task Delete(Guid methodologyVersionId, Guid fileId);

        public Task<MethodologyFile?> Get(Guid methodologyVersionId, Guid fileId);

        public Task<List<MethodologyFile>> GetByFileType(Guid methodologyVersionId, params FileType[] types);

        public Task<List<MethodologyFile>> GetByFile(Guid fileId);
    }
}
