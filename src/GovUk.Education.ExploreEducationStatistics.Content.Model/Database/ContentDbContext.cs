using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

// ReSharper disable StringLiteralTypo
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext()
        {
            // We intentionally don't run `Configure` here as Moq would call this constructor
            // and we'd immediately get a MockException from interacting with its fields
            // e.g. from adding events listeners to `ChangeTracker`.
            // We can just rely on the variants which take options instead as these
            // are what get used in real application scenarios.
        }

        public ContentDbContext(DbContextOptions<ContentDbContext> options) : this(options, true)
        {
        }

        public ContentDbContext(DbContextOptions<ContentDbContext> options, bool updateTimestamps = true) :
            base(options)
        {
            Configure(updateTimestamps);
        }

        private void Configure(bool updateTimestamps = true)
        {
            if (updateTimestamps)
            {
                ChangeTracker.StateChanged += DbContextUtils.UpdateTimestamps;
                ChangeTracker.Tracked += DbContextUtils.UpdateTimestamps;
            }
        }

        public virtual DbSet<Methodology> Methodologies { get; set; }
        public virtual DbSet<MethodologyVersion> MethodologyVersions { get; set; }
        public virtual DbSet<MethodologyStatus> MethodologyStatus { get; set; }
        public virtual DbSet<MethodologyVersionContent> MethodologyContent { get; set; }
        public virtual DbSet<PublicationMethodology> PublicationMethodologies { get; set; }
        public virtual DbSet<MethodologyFile> MethodologyFiles { get; set; }
        public virtual DbSet<Theme> Themes { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }
        public virtual DbSet<Publication> Publications { get; set; }
        public virtual DbSet<ReleaseVersion> ReleaseVersions { get; set; }
        public virtual DbSet<Release> Releases { get; set; }
        public virtual DbSet<ReleaseStatus> ReleaseStatus { get; set; }
        public virtual DbSet<LegacyRelease> LegacyReleases { get; set; }
        public virtual DbSet<ReleaseFile> ReleaseFiles { get; set; }
        public virtual DbSet<File> Files { get; set; }
        public virtual DbSet<ContentSection> ContentSections { get; set; }
        public virtual DbSet<ContentBlock> ContentBlocks { get; set; }
        public virtual DbSet<KeyStatistic> KeyStatistics { get; set; }
        public virtual DbSet<KeyStatisticDataBlock> KeyStatisticsDataBlock { get; set; }
        public virtual DbSet<KeyStatisticText> KeyStatisticsText { get; set; }
        public virtual DbSet<DataBlock> DataBlocks { get; set; }
        public virtual DbSet<DataBlockParent> DataBlockParents { get; set; }
        public virtual DbSet<DataBlockVersion> DataBlockVersions { get; set; }
        public virtual DbSet<DataImport> DataImports { get; set; }
        public virtual DbSet<DataImportError> DataImportErrors { get; set; }
        public virtual DbSet<HtmlBlock> HtmlBlocks { get; set; }
        public virtual DbSet<MarkDownBlock> MarkDownBlocks { get; set; }
        public virtual DbSet<EmbedBlock> EmbedBlocks { get; set; }
        public virtual DbSet<EmbedBlockLink> EmbedBlockLinks { get; set; }
        public virtual DbSet<FeaturedTable> FeaturedTables { get; set; }
        public virtual DbSet<MethodologyNote> MethodologyNotes { get; set; }
        public virtual DbSet<Permalink> Permalinks { get; set; } = null!;
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Update> Update { get; set; }
        public virtual DbSet<PublicationRedirect> PublicationRedirects { get; set; }
        public virtual DbSet<MethodologyRedirect> MethodologyRedirects { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserPublicationRole> UserPublicationRoles { get; set; }
        public virtual DbSet<UserReleaseRole> UserReleaseRoles { get; set; }
        public virtual DbSet<GlossaryEntry> GlossaryEntries { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<UserReleaseInvite> UserReleaseInvites { get; set; }
        public virtual DbSet<UserPublicationInvite> UserPublicationInvites { get; set; }

        [DbFunction]
        public virtual IQueryable<FreeTextRank> PublicationsFreeTextTable(string searchTerm) =>
            FromExpression(() => PublicationsFreeTextTable(searchTerm));

        [DbFunction]
        public virtual IQueryable<FreeTextRank> ReleaseFilesFreeTextTable(string searchTerm) =>
            FromExpression(() => ReleaseFilesFreeTextTable(searchTerm));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureComment(modelBuilder);
            ConfigureDataImport(modelBuilder);
            ConfigureDataImportError(modelBuilder);
            ConfigureMethodology(modelBuilder);
            ConfigureMethodologyContent(modelBuilder);
            ConfigureMethodologyVersion(modelBuilder);
            ConfigureMethodologyStatus(modelBuilder);
            ConfigureMethodologyFile(modelBuilder);
            ConfigureMethodologyNote(modelBuilder);
            ConfigureRedirects(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigurePublicationMethodology(modelBuilder);
            ConfigureReleaseStatus(modelBuilder);
            ConfigureReleaseFile(modelBuilder);
            ConfigureFile(modelBuilder);
            ConfigureContentBlock(modelBuilder);
            ConfigureContentSection(modelBuilder);
            ConfigureRelease(modelBuilder);
            ConfigureDataBlock(modelBuilder);
            ConfigureHtmlBlock(modelBuilder);
            ConfigureEmbedBlockLink(modelBuilder);
            ConfigureMarkdownBlock(modelBuilder);
            ConfigureFeaturedTable(modelBuilder);
            ConfigurePermalink(modelBuilder);
            ConfigureUser(modelBuilder);
            ConfigureUserPublicationRole(modelBuilder);
            ConfigureUserReleaseRole(modelBuilder);
            ConfigureUserReleaseInvite(modelBuilder);
            ConfigureUserPublicationInvite(modelBuilder);
            ConfigureGlossaryEntry(modelBuilder);
            ConfigureKeyStatisticsDataBlock(modelBuilder);
            ConfigureKeyStatisticsText(modelBuilder);
            ConfigureDataBlockParent(modelBuilder);
            ConfigureDataBlockVersion(modelBuilder);

            // Apply model configuration for types which implement IEntityTypeConfiguration

            new FreeTextRank.Config().Configure(modelBuilder.Entity<FreeTextRank>());
        }

        private static void ConfigureComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .Property(comment => comment.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Comment>()
                .Property(comment => comment.Updated)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
        }

        private static void ConfigureDataImport(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataImport>()
                .HasOne(import => import.File)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DataImport>()
                .HasIndex(import => import.FileId)
                .IncludeProperties(
                    import => new { import.Status });

            modelBuilder.Entity<DataImport>()
                .HasOne(import => import.MetaFile)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DataImport>()
                .HasOne(import => import.ZipFile)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DataImport>()
                .Property(import => import.Status)
                .HasConversion(new EnumToStringConverter<DataImportStatus>());

            modelBuilder.Entity<DataImport>()
                .Property(import => import.GeographicLevels)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<HashSet<GeographicLevel>>(v));
        }

        private static void ConfigureDataImportError(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataImportError>()
                .Property(importError => importError.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }

        private static void ConfigureMethodology(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Methodology>()
                .HasOne(m => m.LatestPublishedVersion)
                .WithOne()
                .HasForeignKey<Methodology>(m => m.LatestPublishedVersionId)
                .IsRequired(false);
        }

        private static void ConfigureMethodologyContent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MethodologyVersionContent>().ToTable("MethodologyVersions");
            modelBuilder.Entity<MethodologyVersionContent>().Property<Guid>("MethodologyVersionId");
            modelBuilder.Entity<MethodologyVersionContent>().HasKey("MethodologyVersionId");

            modelBuilder.Entity<MethodologyVersionContent>()
                .Property(m => m.Content)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));

            modelBuilder.Entity<MethodologyVersionContent>()
                .Property(m => m.Annexes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));
        }

        private static void ConfigureMethodologyVersion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MethodologyVersion>()
                .HasOne(m => m.MethodologyContent)
                .WithOne()
                .HasForeignKey<MethodologyVersionContent>(m => m.MethodologyVersionId);

            modelBuilder.Entity<MethodologyVersion>()
                .Property(m => m.Status)
                .HasConversion(new EnumToStringConverter<MethodologyApprovalStatus>());

            modelBuilder.Entity<MethodologyVersion>()
                .Property(m => m.PublishingStrategy)
                .HasConversion(new EnumToStringConverter<MethodologyPublishingStrategy>());

            modelBuilder.Entity<MethodologyVersion>()
                .Property(m => m.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<MethodologyVersion>()
                .HasOne(m => m.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureMethodologyStatus(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MethodologyStatus>()
                .Property(rs => rs.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue
                        ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                        : null);

            modelBuilder.Entity<MethodologyStatus>()
                .HasOne(rs => rs.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MethodologyStatus>()
                .Property(rs => rs.ApprovalStatus)
                .HasConversion(new EnumToStringConverter<MethodologyApprovalStatus>());
        }

        private static void ConfigureMethodologyFile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MethodologyFile>()
                .HasOne(mf => mf.MethodologyVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MethodologyFile>()
                .HasOne(mf => mf.File)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureMethodologyNote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MethodologyNote>()
                .Property(n => n.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<MethodologyNote>()
                .HasOne(m => m.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MethodologyNote>()
                .Property(n => n.DisplayDate)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<MethodologyNote>()
                .Property(n => n.Updated)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<MethodologyNote>()
                .HasOne(m => m.UpdatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);
        }

        private static void ConfigurePublication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Publication>()
                .OwnsOne(p => p.ExternalMethodology)
                .ToTable("ExternalMethodology");

            modelBuilder.Entity<Publication>()
                .Property(n => n.Updated)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<Publication>()
                .HasOne(p => p.Contact)
                .WithMany() // Ideally this would be WithOne, but we would need to fix existing data to do this
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Publication>()
                .HasOne(p => p.LatestPublishedReleaseVersion)
                .WithOne()
                .HasForeignKey<Publication>(p => p.LatestPublishedReleaseVersionId)
                .IsRequired(false);

            modelBuilder.Entity<Publication>()
                .Property(p => p.ReleaseSeries)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ReleaseSeriesItem>>(v));
        }

        private static void ConfigurePublicationMethodology(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PublicationMethodology>()
                .HasKey(pm => new { pm.PublicationId, pm.MethodologyId });

            modelBuilder.Entity<PublicationMethodology>()
                .HasOne(pm => pm.Publication)
                .WithMany(p => p.Methodologies)
                .HasForeignKey(pm => pm.PublicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PublicationMethodology>()
                .HasOne(pm => pm.Methodology)
                .WithMany(m => m.Publications)
                .HasForeignKey(pm => pm.MethodologyId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureReleaseStatus(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseStatus>()
                .Property(rs => rs.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue
                        ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                        : null);

            modelBuilder.Entity<ReleaseStatus>()
                .HasOne(rs => rs.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReleaseStatus>()
                .Property(rs => rs.ApprovalStatus)
                .HasConversion(new EnumToStringConverter<ReleaseApprovalStatus>());
        }

        private static void ConfigureReleaseFile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseFile>(entity =>
            {
                entity.HasQueryFilter(e => !e.ReleaseVersion.SoftDeleted);
                entity.HasOne(rf => rf.ReleaseVersion)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(rf => rf.File)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        private static void ConfigureFile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<File>(entity =>
            {
                entity.Property(e => e.ContentType)
                    .HasMaxLength(255);
                entity.Property(e => e.Type)
                    .HasConversion(new EnumToStringConverter<FileType>())
                    .HasMaxLength(25);
                entity.HasIndex(e => e.Type);
                entity.HasOne(b => b.Replacing)
                    .WithOne()
                    .HasForeignKey<File>(b => b.ReplacingId)
                    .IsRequired(false);
                entity.HasOne(b => b.ReplacedBy)
                    .WithOne()
                    .HasForeignKey<File>(b => b.ReplacedById)
                    .IsRequired(false);
                entity.Property(e => e.Created)
                    .HasConversion(
                        v => v,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
            });
        }

        private static void ConfigureContentBlock(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContentBlock>(entity =>
            {
                entity.ToTable("ContentBlock")
                    .HasDiscriminator<string>("Type");
                entity.Property("Type")
                    .HasMaxLength(25);
                entity.HasIndex("Type");
                entity.Property(e => e.Created)
                    .HasConversion(
                        v => v,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
                entity.Property(e => e.Locked)
                    .HasConversion(
                        v => v,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
            });
        }

        private static void ConfigureContentSection(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContentSection>(entity =>
            {
                entity.Property(e => e.Type)
                    .HasConversion(new EnumToStringConverter<ContentSectionType>())
                    .HasMaxLength(25);
                entity.HasIndex(e => e.Type);
            });
        }

        private static void ConfigureRelease(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseVersion>()
                .Property(rv => rv.TimePeriodCoverage)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(6);

            modelBuilder.Entity<ReleaseVersion>()
                .Property<List<Link>>("RelatedInformation")
                .IsRequired()
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<Link>>(v));

            modelBuilder.Entity<ReleaseVersion>()
                .Property(rv => rv.NotifiedOn)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<ReleaseVersion>()
                .HasIndex(rv => new { rv.PreviousVersionId, rv.Version });

            modelBuilder.Entity<ReleaseVersion>()
                .HasOne(rv => rv.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReleaseVersion>()
                .Property(rv => rv.Published)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<ReleaseVersion>()
                .HasQueryFilter(rv => !rv.SoftDeleted);

            modelBuilder.Entity<ReleaseVersion>()
                .Property(rv => rv.Type)
                .HasConversion(new EnumToStringConverter<ReleaseType>());

            modelBuilder.Entity<ReleaseVersion>()
                .HasIndex(rv => rv.Type);

            modelBuilder.Entity<ReleaseVersion>()
                .Property(rv => rv.NextReleaseDate)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<PartialDate>(v));

            modelBuilder.Entity<ReleaseVersion>()
                .Property(rv => rv.PublishScheduled)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<ReleaseVersion>()
                .Property(rv => rv.ApprovalStatus)
                .HasConversion(new EnumToStringConverter<ReleaseApprovalStatus>());

            modelBuilder.Entity<ReleaseVersion>()
                .HasOne(rv => rv.PreviousVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureDataBlock(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Heading)
                .HasColumnName("DataBlock_Heading");

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Query)
                .HasColumnName("DataBlock_Query")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<ObservationQueryContext>(v));

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Charts)
                .HasColumnName("DataBlock_Charts")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<IChart>>(v));

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Table)
                .HasColumnName("DataBlock_Table")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<TableBuilderConfiguration>(v));
        }

        private static void ConfigureHtmlBlock(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HtmlBlock>()
                .Property(block => block.Body)
                .HasColumnName("Body");
        }

        private static void ConfigureEmbedBlockLink(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmbedBlockLink>()
                .Property(block => block.EmbedBlockId)
                .HasColumnName("EmbedBlockId");

            modelBuilder.Entity<EmbedBlockLink>()
                .HasOne(eb => eb.EmbedBlock)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureMarkdownBlock(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarkDownBlock>()
                .Property(block => block.Body)
                .HasColumnName("Body");
        }

        private static void ConfigureFeaturedTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeaturedTable>()
                .HasOne(ft => ft.DataBlock)
                .WithOne();
        }

        private static void ConfigurePermalink(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permalink>()
                .Property(permalink => permalink.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Permalink>()
                .HasIndex(data => data.ReleaseVersionId);

            modelBuilder.Entity<Permalink>()
                .HasIndex(data => data.SubjectId);
        }

        private static void ConfigureRedirects(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PublicationRedirect>()
                .HasKey(pr => new { pr.PublicationId, pr.Slug });

            modelBuilder.Entity<MethodologyRedirect>()
                .HasKey(mr => new { mr.MethodologyVersionId, mr.Slug });
        }

        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
        }

        private static void ConfigureUserPublicationRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPublicationRole>()
                .Property(r => r.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<UserPublicationRole>()
                .HasOne(r => r.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserPublicationRole>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<PublicationRole>());

            modelBuilder.Entity<UserPublicationRole>()
                .HasQueryFilter(p => p.Deleted == null);
        }

        private static void ConfigureUserReleaseRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserReleaseRole>()
                .Property(userReleaseRole => userReleaseRole.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<UserReleaseRole>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<ReleaseRole>());

            modelBuilder.Entity<UserReleaseRole>()
                .HasQueryFilter(r =>
                    !r.SoftDeleted
                    && r.Deleted == null);
        }

        private static void ConfigureUserReleaseInvite(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserReleaseInvite>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<ReleaseRole>());

            modelBuilder.Entity<UserReleaseInvite>()
                .HasQueryFilter(r => !r.SoftDeleted);

            modelBuilder.Entity<UserReleaseInvite>()
                .Property(invite => invite.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<UserReleaseInvite>()
                .Property(invite => invite.Updated)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
        }

        private static void ConfigureUserPublicationInvite(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPublicationInvite>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<PublicationRole>());

            modelBuilder.Entity<UserPublicationInvite>()
                .Property(invite => invite.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }

        private static void ConfigureGlossaryEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GlossaryEntry>()
                .Property(rs => rs.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<GlossaryEntry>()
                .HasOne(rs => rs.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureKeyStatisticsDataBlock(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeyStatisticDataBlock>()
                .ToTable("KeyStatisticsDataBlock");

            modelBuilder.Entity<KeyStatisticDataBlock>()
                .HasOne(ks => ks.DataBlock)
                .WithMany()
                // WARN: This is necessary - otherwise an automatically generated cascade delete is added for when an
                // associated data block is removed. That cascade delete _only_ removes the KeyStatisticsDataBlock
                // entry, leaving a KeyStatistics table entry, which should never happen.
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureDataBlockParent(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataBlockParent>()
                .ToTable("DataBlocks");

            modelBuilder.Entity<DataBlockParent>()
                .HasOne(dataBlock => dataBlock.LatestDraftVersion)
                .WithOne()
                .HasForeignKey<DataBlockParent>(version => version.LatestDraftVersionId);

            modelBuilder.Entity<DataBlockParent>()
                .HasOne(dataBlock => dataBlock.LatestPublishedVersion)
                .WithOne()
                .HasForeignKey<DataBlockParent>(version => version.LatestPublishedVersionId)
                .IsRequired(false);
        }

        private static void ConfigureDataBlockVersion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataBlockVersion>()
                .ToTable("DataBlockVersions");

            modelBuilder.Entity<DataBlockVersion>()
                .Property(f => f.DataBlockParentId)
                .HasColumnName("DataBlockId");

            modelBuilder.Entity<DataBlockVersion>()
                .Property(invite => invite.Published)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<DataBlockVersion>()
                .Property(invite => invite.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<DataBlockVersion>()
                .Property(invite => invite.Updated)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            // Automatically include the backing ContentBlock of type "DataBlock" whenever we retrieve
            // DataBlockVersions, as DataBlockVersions encapsulate their backing ContentBlocks and will replace them
            // entirely in EES-4640.
            modelBuilder.Entity<DataBlockVersion>()
                .Navigation(dataBlockVersion => dataBlockVersion.ContentBlock)
                .AutoInclude();
        }

        private static void ConfigureKeyStatisticsText(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeyStatisticText>()
                .ToTable("KeyStatisticsText");
        }
    }
}
