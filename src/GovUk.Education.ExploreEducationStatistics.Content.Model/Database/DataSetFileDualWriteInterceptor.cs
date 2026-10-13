#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

/// <summary>
/// Mirrors changes to Data and Metadata <see cref="File"/> rows and to the <see cref="ReleaseFile"/> rows of Data files
/// into the <see cref="DataSetFile"/>, <see cref="DataSetFileVersion"/> and
/// <see cref="ReleaseVersionDataSetFileVersion"/> tables, in the same SaveChanges.
/// </summary>
/// <remarks>
/// TODO EES-5105 Remove once the DataSetFile tables are the source of truth.
///
/// Files and ReleaseFiles remain the source of truth, so this must never cause the save of them to fail.
/// Anything which can't be mirrored is skipped (and logged), and is reported as drift by the BAU check.
/// </remarks>
public class DataSetFileDualWriteInterceptor : SaveChangesInterceptor
{
    public static readonly DataSetFileDualWriteInterceptor Instance = new();

    // Keeps the number of parameters in each Contains query well below SQL Server's limit of 2100
    private const int QueryBatchSize = 1000;

    private DataSetFileDualWriteInterceptor() { }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is ContentDbContext context)
        {
            // Runs synchronously as every query is executed synchronously when async is false
            MirrorChanges(context, async: false, CancellationToken.None).GetAwaiter().GetResult();
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is ContentDbContext context)
        {
            await MirrorChanges(context, async: true, cancellationToken);
        }

        return result;
    }

    private static async Task MirrorChanges(ContentDbContext context, bool async, CancellationToken cancellationToken)
    {
        var autoDetectChangesEnabled = context.ChangeTracker.AutoDetectChangesEnabled;

        // SaveChanges only detects changes after calling interceptors.
        // Detect them once here, then stop the ChangeTracker / Local lookups below from detecting them repeatedly.
        if (autoDetectChangesEnabled)
        {
            context.ChangeTracker.DetectChanges();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        try
        {
            var mirror = new Mirror(context, async, cancellationToken);

            await mirror.MirrorDataFiles();
            await mirror.MirrorMetaFiles();
            await mirror.MirrorReleaseFiles();
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;
        }
    }

    private class Mirror(ContentDbContext context, bool async, CancellationToken cancellationToken)
    {
        private readonly ILogger _logger = context
            .GetService<ILoggerFactory>()
            .CreateLogger<DataSetFileDualWriteInterceptor>();

        private readonly List<EntityEntry<File>> _fileEntries = context
            .ChangeTracker.Entries<File>()
            .Where(entry => entry.Entity.Type is FileType.Data or FileType.Metadata)
            .ToList();

        public async Task MirrorDataFiles()
        {
            var changedEntries = _fileEntries
                .Where(entry => entry.Entity.Type == FileType.Data && IsChanged(entry.State))
                .ToList();

            if (changedEntries.Count == 0)
            {
                return;
            }

            var addedEntries = changedEntries
                .Where(entry => entry.State == EntityState.Added)
                .Where(entry =>
                {
                    var file = entry.Entity;

                    if (
                        file.DataSetFileId is not null
                        && file.DataSetFileVersion is not null
                        && file.SubjectId is not null
                    )
                    {
                        return true;
                    }

                    _logger.LogWarning(
                        "Not mirroring data file {FileId} as it has no data set file id, version or subject id",
                        file.Id
                    );
                    return false;
                })
                .ToList();

            var modifiedEntries = changedEntries.Where(entry => entry.State == EntityState.Modified).ToList();
            var deletedEntries = changedEntries.Where(entry => entry.State == EntityState.Deleted).ToList();

            var replacementIds = addedEntries
                .Concat(modifiedEntries)
                .SelectMany(entry => new[] { entry.Entity.ReplacingId, entry.Entity.ReplacedById })
                .OfType<Guid>();

            var versions = await FindTrackedOrStored<DataSetFileVersion>(
                changedEntries.Select(entry => entry.Entity.Id).Concat(replacementIds)
            );

            // Versions which will exist once this save completes, so are safe to reference by foreign key
            var existingVersionIds = versions
                .Where(pair => !IsDeleted(pair.Value))
                .Select(pair => pair.Key)
                .Concat(addedEntries.Select(entry => entry.Entity.Id))
                .Except(deletedEntries.Select(entry => entry.Entity.Id))
                .ToHashSet();

            // Update() marks every property as modified, so compare values to avoid querying DataSetFiles needlessly.
            // This also means the importer, which has no permissions on DataSetFiles, never queries it.
            var dataSetFileIdChangedEntries = modifiedEntries.Where(entry =>
            {
                var property = entry.Property(f => f.DataSetFileId);
                return property.IsModified && property.OriginalValue != property.CurrentValue;
            });

            await AddMissingDataSetFiles(
                addedEntries.Concat(dataSetFileIdChangedEntries).Select(entry => entry.Entity)
            );

            var metaFiles = await FindMetaFiles(addedEntries.Select(entry => entry.Entity.SubjectId!.Value));

            foreach (var entry in addedEntries)
            {
                var file = entry.Entity;

                // Already tracked if SaveChanges is being called again after failing
                if (versions.TryGetValue(file.Id, out var version))
                {
                    CopyDataFile(entry, version, existingVersionIds, onlyModified: false);
                    continue;
                }

                version = new DataSetFileVersion { Id = file.Id };
                CopyDataFile(entry, version, existingVersionIds, onlyModified: false);

                if (metaFiles.TryGetValue(file.SubjectId!.Value, out var metaFile))
                {
                    CopyMetaFile(metaFile, version);
                }

                context.DataSetFileVersions.Add(version);
            }

            foreach (var entry in modifiedEntries)
            {
                if (versions.TryGetValue(entry.Entity.Id, out var version) && !IsDeleted(version))
                {
                    CopyDataFile(entry, version, existingVersionIds, onlyModified: true);
                }
                else
                {
                    _logger.LogWarning("Not mirroring data file {FileId} as it has no version", entry.Entity.Id);
                }
            }

            foreach (var entry in deletedEntries)
            {
                if (versions.TryGetValue(entry.Entity.Id, out var version))
                {
                    context.DataSetFileVersions.Remove(version);
                }
                else
                {
                    _logger.LogWarning("Not mirroring data file {FileId} as it has no version", entry.Entity.Id);
                }
            }

            await RemoveOrphanedDataSetFiles(
                deletedEntries
                    .Select(entry => versions.GetValueOrDefault(entry.Entity.Id)?.DataSetFileId)
                    .OfType<Guid>()
            );
        }

        public async Task MirrorMetaFiles()
        {
            var changedEntries = _fileEntries
                .Where(entry => entry.Entity.Type == FileType.Metadata && IsChanged(entry.State))
                .Where(entry => entry.Entity.SubjectId is not null)
                .ToList();

            if (changedEntries.Count == 0)
            {
                return;
            }

            var versions = await FindVersionsBySubjectId(changedEntries.Select(entry => entry.Entity.SubjectId!.Value));

            foreach (var entry in changedEntries)
            {
                var metaFile = entry.Entity;

                // A missing version is expected, e.g. when the data file was deleted first in an earlier save.
                // Where the meta file is saved before its data file, it's mirrored when the data file is.
                if (!versions.TryGetValue(metaFile.SubjectId!.Value, out var version))
                {
                    continue;
                }

                if (entry.State == EntityState.Deleted)
                {
                    if (version.MetaFileId == metaFile.Id)
                    {
                        version.MetaFileId = null;
                        version.MetaFilename = null;
                        version.MetaContentType = null;
                        version.MetaContentLength = null;
                    }

                    continue;
                }

                if (version.MetaFileId is not null && version.MetaFileId != metaFile.Id)
                {
                    _logger.LogWarning(
                        "Not mirroring meta file {FileId} as version {VersionId} already has meta file {MetaFileId}",
                        metaFile.Id,
                        version.Id,
                        version.MetaFileId
                    );
                    continue;
                }

                CopyMetaFile(metaFile, version);
            }
        }

        public async Task MirrorReleaseFiles()
        {
            var changedEntries = context
                .ChangeTracker.Entries<ReleaseFile>()
                .Where(entry => IsChanged(entry.State))
                .ToList();

            if (changedEntries.Count == 0)
            {
                return;
            }

            var links = await FindTrackedOrStored<ReleaseVersionDataSetFileVersion>(
                changedEntries.Select(entry => entry.Entity.Id)
            );

            var addedEntries = changedEntries.Where(entry => entry.State == EntityState.Added).ToList();

            // Only Data files have versions, so this also filters out the links of other file types
            var versions = await FindTrackedOrStored<DataSetFileVersion>(
                addedEntries.Select(entry => entry.Entity.FileId)
            );

            foreach (var entry in changedEntries)
            {
                var releaseFile = entry.Entity;
                links.TryGetValue(releaseFile.Id, out var link);

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (!versions.TryGetValue(releaseFile.FileId, out var version) || IsDeleted(version))
                        {
                            break;
                        }

                        // Already tracked if SaveChanges is being called again after failing
                        if (link is null)
                        {
                            link = new ReleaseVersionDataSetFileVersion
                            {
                                Id = releaseFile.Id,
                                DataSetFileVersionId = releaseFile.FileId,
                            };
                            CopyReleaseFile(entry, link, onlyModified: false);
                            context.ReleaseVersionDataSetFileVersions.Add(link);
                        }
                        else
                        {
                            CopyReleaseFile(entry, link, onlyModified: false);
                        }

                        break;

                    case EntityState.Modified when link is not null:
                        CopyReleaseFile(entry, link, onlyModified: true);
                        break;

                    case EntityState.Deleted when link is not null:
                        context.ReleaseVersionDataSetFileVersions.Remove(link);
                        break;
                }
            }
        }

        private void CopyDataFile(
            EntityEntry<File> entry,
            DataSetFileVersion version,
            HashSet<Guid> existingVersionIds,
            bool onlyModified
        )
        {
            var file = entry.Entity;

            bool ShouldCopy<TProperty>(Expression<Func<File, TProperty>> property) =>
                !onlyModified || entry.Property(property).IsModified;

            if (ShouldCopy(f => f.DataSetFileId) && file.DataSetFileId is not null)
            {
                version.DataSetFileId = file.DataSetFileId.Value;
            }

            if (ShouldCopy(f => f.DataSetFileVersion) && file.DataSetFileVersion is not null)
            {
                version.Version = file.DataSetFileVersion.Value;
            }

            if (ShouldCopy(f => f.SubjectId) && file.SubjectId is not null)
            {
                version.SubjectId = file.SubjectId.Value;
            }

            if (ShouldCopy(f => f.RootPath))
            {
                version.RootPath = file.RootPath;
            }

            if (ShouldCopy(f => f.Filename))
            {
                version.Filename = file.Filename;
            }

            if (ShouldCopy(f => f.ContentType))
            {
                version.ContentType = file.ContentType;
            }

            if (ShouldCopy(f => f.ContentLength))
            {
                version.ContentLength = file.ContentLength;
            }

            if (ShouldCopy(f => f.DataSetFileMeta))
            {
                version.DataSetFileMeta = CloneJson(file.DataSetFileMeta);
            }

            if (ShouldCopy(f => f.FilterHierarchies))
            {
                version.FilterHierarchies = CloneJson(file.FilterHierarchies);
            }

            if (ShouldCopy(f => f.ReplacedById))
            {
                version.ReplacedById = ExistingVersionId(file.ReplacedById, existingVersionIds, file.Id);
            }

            if (ShouldCopy(f => f.ReplacingId))
            {
                version.ReplacingId = ExistingVersionId(file.ReplacingId, existingVersionIds, file.Id);
            }

            if (ShouldCopy(f => f.Created))
            {
                version.Created = file.Created;
            }

            if (ShouldCopy(f => f.CreatedById))
            {
                version.CreatedById = file.CreatedById;
            }
        }

        private static void CopyMetaFile(File metaFile, DataSetFileVersion version)
        {
            version.MetaFileId = metaFile.Id;
            version.MetaFilename = metaFile.Filename;
            version.MetaContentType = metaFile.ContentType;
            version.MetaContentLength = metaFile.ContentLength;
        }

        private static void CopyReleaseFile(
            EntityEntry<ReleaseFile> entry,
            ReleaseVersionDataSetFileVersion link,
            bool onlyModified
        )
        {
            var releaseFile = entry.Entity;

            bool ShouldCopy<TProperty>(Expression<Func<ReleaseFile, TProperty>> property) =>
                !onlyModified || entry.Property(property).IsModified;

            if (ShouldCopy(rf => rf.ReleaseVersionId))
            {
                link.ReleaseVersionId = releaseFile.ReleaseVersionId;
            }

            if (ShouldCopy(rf => rf.Name))
            {
                link.Name = releaseFile.Name;
            }

            if (ShouldCopy(rf => rf.Summary))
            {
                link.Summary = releaseFile.Summary;
            }

            if (ShouldCopy(rf => rf.Order))
            {
                link.Order = releaseFile.Order;
            }

            if (ShouldCopy(rf => rf.PublicApiCompatible))
            {
                link.PublicApiCompatible = releaseFile.PublicApiCompatible;
            }

            if (ShouldCopy(rf => rf.PublicApiDataSetId))
            {
                link.PublicApiDataSetId = releaseFile.PublicApiDataSetId;
            }

            if (ShouldCopy(rf => rf.PublicApiDataSetVersion))
            {
                link.PublicApiDataSetVersion = releaseFile.PublicApiDataSetVersion;
            }

            if (ShouldCopy(rf => rf.FilterSequence))
            {
                link.FilterSequence = CloneJson(releaseFile.FilterSequence);
            }

            if (ShouldCopy(rf => rf.IndicatorSequence))
            {
                link.IndicatorSequence = CloneJson(releaseFile.IndicatorSequence);
            }

            if (ShouldCopy(rf => rf.Published))
            {
                link.Published = releaseFile.Published;
            }
        }

        private Guid? ExistingVersionId(Guid? versionId, HashSet<Guid> existingVersionIds, Guid fileId)
        {
            if (versionId is null || existingVersionIds.Contains(versionId.Value))
            {
                return versionId;
            }

            _logger.LogWarning(
                "Not mirroring replacement link from data file {FileId} to {ReplacementFileId} as it has no version",
                fileId,
                versionId
            );
            return null;
        }

        private async Task AddMissingDataSetFiles(IEnumerable<File> files)
        {
            var filesByDataSetFileId = files
                .Where(file => file.DataSetFileId is not null)
                .GroupBy(file => file.DataSetFileId!.Value)
                .ToDictionary(group => group.Key, group => group.ToList());

            var dataSetFiles = await FindTrackedOrStored<DataSetFile>(filesByDataSetFileId.Keys);

            foreach (var (dataSetFileId, dataSetFileFiles) in filesByDataSetFileId)
            {
                if (!dataSetFiles.ContainsKey(dataSetFileId))
                {
                    context.DataSetFiles.Add(
                        new DataSetFile { Id = dataSetFileId, Created = dataSetFileFiles.Min(file => file.Created) }
                    );
                }
            }
        }

        private async Task RemoveOrphanedDataSetFiles(IEnumerable<Guid> dataSetFileIds)
        {
            var candidateIds = dataSetFileIds.ToHashSet();

            if (candidateIds.Count == 0)
            {
                return;
            }

            var storedVersionIds = await ToListAsync(
                context
                    .DataSetFileVersions.AsNoTracking()
                    .Where(version => candidateIds.Contains(version.DataSetFileId))
                    .Select(version => new { version.Id, version.DataSetFileId })
            );

            var trackedVersionEntries = context.ChangeTracker.Entries<DataSetFileVersion>().ToList();

            var deletedVersionIds = trackedVersionEntries
                .Where(entry => entry.State == EntityState.Deleted)
                .Select(entry => entry.Entity.Id)
                .ToHashSet();

            var remainingDataSetFileIds = storedVersionIds
                .Where(version => !deletedVersionIds.Contains(version.Id))
                .Select(version => version.DataSetFileId)
                .Concat(
                    trackedVersionEntries
                        .Where(entry => entry.State != EntityState.Deleted)
                        .Select(entry => entry.Entity.DataSetFileId)
                )
                .ToHashSet();

            var orphanedIds = candidateIds.Except(remainingDataSetFileIds).ToList();

            var dataSetFiles = await FindTrackedOrStored<DataSetFile>(orphanedIds);

            context.DataSetFiles.RemoveRange(dataSetFiles.Values);
        }

        /// <summary>
        /// Find the Metadata files of each subject, preferring tracked files to stored ones.
        /// </summary>
        private async Task<Dictionary<Guid, File>> FindMetaFiles(IEnumerable<Guid> subjectIds)
        {
            var subjectIdSet = subjectIds.ToHashSet();

            if (subjectIdSet.Count == 0)
            {
                return [];
            }

            var trackedEntries = _fileEntries
                .Where(entry => entry.Entity.Type == FileType.Metadata)
                .Where(entry =>
                    entry.Entity.SubjectId is not null && subjectIdSet.Contains(entry.Entity.SubjectId.Value)
                )
                .ToList();

            var metaFiles = trackedEntries
                .Where(entry => entry.State != EntityState.Deleted)
                .GroupBy(entry => entry.Entity.SubjectId!.Value)
                .ToDictionary(group => group.Key, group => group.First().Entity);

            var trackedIds = trackedEntries.Select(entry => entry.Entity.Id).ToHashSet();
            var untrackedSubjectIds = subjectIdSet.Except(metaFiles.Keys).ToList();

            foreach (var batch in untrackedSubjectIds.Chunk(QueryBatchSize))
            {
                var storedMetaFiles = await ToListAsync(
                    context
                        .Files.AsNoTracking()
                        .Where(file => file.Type == FileType.Metadata)
                        .Where(file => file.SubjectId.HasValue && batch.Contains(file.SubjectId.Value))
                );

                foreach (var metaFile in storedMetaFiles.Where(file => !trackedIds.Contains(file.Id)))
                {
                    metaFiles.TryAdd(metaFile.SubjectId!.Value, metaFile);
                }
            }

            return metaFiles;
        }

        /// <summary>
        /// Find the version of each subject, preferring tracked versions to stored ones.
        /// </summary>
        private async Task<Dictionary<Guid, DataSetFileVersion>> FindVersionsBySubjectId(IEnumerable<Guid> subjectIds)
        {
            var subjectIdSet = subjectIds.ToHashSet();

            var trackedEntries = context
                .ChangeTracker.Entries<DataSetFileVersion>()
                .Where(entry => subjectIdSet.Contains(entry.Entity.SubjectId))
                .ToList();

            var versions = trackedEntries
                .Where(entry => entry.State != EntityState.Deleted)
                .GroupBy(entry => entry.Entity.SubjectId)
                .ToDictionary(group => group.Key, group => group.First().Entity);

            var trackedIds = trackedEntries.Select(entry => entry.Entity.Id).ToHashSet();
            var untrackedSubjectIds = subjectIdSet.Except(versions.Keys).ToList();

            foreach (var batch in untrackedSubjectIds.Chunk(QueryBatchSize))
            {
                var storedVersions = await ToListAsync(
                    context.DataSetFileVersions.Where(version => batch.Contains(version.SubjectId))
                );

                foreach (var version in storedVersions.Where(version => !trackedIds.Contains(version.Id)))
                {
                    versions.TryAdd(version.SubjectId, version);
                }
            }

            return versions;
        }

        /// <summary>
        /// Find entities by id, preferring tracked entities (including those being deleted) to stored ones.
        /// </summary>
        private async Task<Dictionary<Guid, TEntity>> FindTrackedOrStored<TEntity>(IEnumerable<Guid> ids)
            where TEntity : class
        {
            var entities = new Dictionary<Guid, TEntity>();
            var untrackedIds = new List<Guid>();

            foreach (var id in ids.Distinct())
            {
                var entry = context.Set<TEntity>().Local.FindEntry(id);

                if (entry is null)
                {
                    untrackedIds.Add(id);
                }
                else
                {
                    entities[id] = entry.Entity;
                }
            }

            foreach (var batch in untrackedIds.Chunk(QueryBatchSize))
            {
                var storedEntities = await ToListAsync(
                    context
                        .Set<TEntity>()
                        .IgnoreQueryFilters()
                        .Where(entity => batch.Contains(EF.Property<Guid>(entity, "Id")))
                );

                foreach (var entity in storedEntities)
                {
                    entities[context.Entry(entity).Property<Guid>("Id").CurrentValue] = entity;
                }
            }

            return entities;
        }

        private bool IsDeleted(object entity) => context.Entry(entity).State == EntityState.Deleted;

        private async Task<List<T>> ToListAsync<T>(IQueryable<T> query) =>
            async ? await query.ToListAsync(cancellationToken) : query.ToList();

        private static bool IsChanged(EntityState state) =>
            state is EntityState.Added or EntityState.Modified or EntityState.Deleted;

        private static T? CloneJson<T>(T? value) =>
            value is null ? default : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
    }
}
