#nullable enable
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class LocationMigrationController : ControllerBase
{
    private readonly IPrivateBlobStorageService _blobStorageService;
    private readonly ContentDbContext _contentDbContext;
    private readonly StatisticsDbContext _statisticsDbContext;

    public LocationMigrationController(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IPrivateBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
        _statisticsDbContext = statisticsDbContext;
        _contentDbContext = contentDbContext;
    }

    public class LocationMigrationResult
    {
        public bool IsDryRun;
        public int NumLocationsMigrated;
    }

    [HttpPatch("bau/migrate-location-laestab")]
    public async Task<LocationMigrationResult> MigrateLocationLaEstab(
        [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var numLocationsMigrated = 0;

        var allBlobs = await _blobStorageService.ListBlobs(BlobContainers.PrivateReleaseFiles);
        var dataFiles = new List<BlobInfo>();

        foreach (var blob in allBlobs)
        {
            var file = _contentDbContext.Files.FirstOrDefault(f => f.Id == Guid.Parse(blob.FileName) && f.Type == FileType.Data);

            if (file is not null)
            {
                dataFiles.Add(blob);
            }
        }

        //CsvUtils.ForEachRow();

        // https://joshclose.github.io/CsvHelper/getting-started/#reading-a-csv-file
        using (var reader = new StreamReader("path\\to\\file.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<Foo>();
        }

        var schoolCodeFromFile = "";
        var existingLocationInDb = _statisticsDbContext.Location.Where(l => l.School_Code == locationFromFile);

        //foreach (var item in collection)
        //{
        //    numLocationsMigrated++;
        //}

        //if (!dryRun)
        //{
        //    await _contentDbContext.SaveChangesAsync(cancellationToken);
        //}

        return new LocationMigrationResult
        {
            IsDryRun = dryRun,
            NumLocationsMigrated = numLocationsMigrated,
        };
    }
}
