#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services
{
    public class MethodologyImageService : IMethodologyImageService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IUserService _userService;

        public MethodologyImageService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IUserService userService)
        {
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, FileStreamResult>> Stream(Guid methodologyVersionId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyFile>(q => q
                    .Include(mf => mf.File)
                    .Include(mf => mf.MethodologyVersion)
                    .Where(mf => mf.MethodologyVersionId == methodologyVersionId && mf.FileId == fileId))
                .OnSuccessDo(mf => _userService.CheckCanViewMethodologyVersion(mf.MethodologyVersion))
                .OnSuccessCombineWith(mf =>
                    _blobStorageService.DownloadToStream(PublicMethodologyFiles, mf.Path(), new MemoryStream()))
                .OnSuccess(methodologyFileAndStream =>
                {
                    var (methodologyFile, stream) = methodologyFileAndStream;
                    return new FileStreamResult(stream, methodologyFile.File.ContentType)
                    {
                        FileDownloadName = methodologyFile.File.Filename
                    };
                });
        }
    }
}
