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

        public ContentDbContext(DbContextOptions<ContentDbContext> options, bool updateTimestamps = true) : base(options)
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
        public virtual DbSet<MethodologyVersionContent> MethodologyContent { get; set; }
        public virtual DbSet<PublicationMethodology> PublicationMethodologies { get; set; }
        public virtual DbSet<MethodologyFile> MethodologyFiles { get; set; }
        public virtual DbSet<Theme> Themes { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }
        public virtual DbSet<Publication> Publications { get; set; }
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
        public virtual DbSet<DataImport> DataImports { get; set; }
        public virtual DbSet<DataImportError> DataImportErrors { get; set; }
        public virtual DbSet<HtmlBlock> HtmlBlocks { get; set; }
        public virtual DbSet<MarkDownBlock> MarkDownBlocks { get; set; }
        public virtual DbSet<EmbedBlock> EmbedBlocks { get; set; }
        public virtual DbSet<EmbedBlockLink> EmbedBlockLinks { get; set; }
        public virtual DbSet<MethodologyNote> MethodologyNotes { get; set; }
        public virtual DbSet<Permalink> Permalinks { get; set; } = null!;
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<ReleaseContentSection> ReleaseContentSections { get; set; }
        public virtual DbSet<ReleaseContentBlock> ReleaseContentBlocks { get; set; }
        public virtual DbSet<Update> Update { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserPublicationRole> UserPublicationRoles { get; set; }
        public virtual DbSet<UserReleaseRole> UserReleaseRoles { get; set; }
        public virtual DbSet<GlossaryEntry> GlossaryEntries { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<UserReleaseInvite> UserReleaseInvites { get; set; }
        public virtual DbSet<UserPublicationInvite> UserPublicationInvites { get; set; }

        public virtual IQueryable<FreeTextRank> PublicationsFreeTextTable(string search) =>
            FromExpression(() => PublicationsFreeTextTable(search));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFreeTextTableSupport();

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

            modelBuilder.Entity<DataImport>()
                .HasOne(import => import.File)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<DataImportError>()
                .Property(importError => importError.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

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

            modelBuilder.Entity<MethodologyVersion>()
                .HasOne(m => m.MethodologyContent)
                .WithOne()
                .HasForeignKey<MethodologyVersionContent>(m => m.MethodologyVersionId);

            modelBuilder.Entity<MethodologyVersion>()
                .Property(m => m.Status)
                .HasConversion(new EnumToStringConverter<MethodologyStatus>());

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

            modelBuilder.Entity<MethodologyFile>()
                .HasOne(mf => mf.MethodologyVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<MethodologyFile>()
                .HasOne(mf => mf.File)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

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
                .WithMany()  // Ideally this would be WithOne, but we would need to fix existing data to do this
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Publication>()
                .HasOne(p => p.LatestPublishedRelease)
                .WithOne()
                .HasForeignKey<Publication>(p => p.LatestPublishedReleaseId)
                .IsRequired(false);

            modelBuilder.Entity<PublicationMethodology>()
                .HasKey(pm => new {pm.PublicationId, pm.MethodologyId});

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

            modelBuilder.Entity<Release>()
                .Property(r => r.TimePeriodCoverage)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(6);

            modelBuilder.Entity<Release>()
                .Property<List<Link>>("RelatedInformation")
                .IsRequired()
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<Link>>(v));

            modelBuilder.Entity<Release>()
                .Property(r => r.NotifiedOn)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<Release>()
                .HasIndex(r => new {r.PreviousVersionId, r.Version});

            modelBuilder.Entity<Release>()
                .HasOne(r => r.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Release>()
                .Property(r => r.Published)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<Release>()
                .HasQueryFilter(r => !r.SoftDeleted);

            modelBuilder.Entity<Release>()
                .Property(release => release.Type)
                .HasConversion(new EnumToStringConverter<ReleaseType>());

            modelBuilder.Entity<Release>()
                .HasIndex(release => release.Type);

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

            modelBuilder.Entity<ReleaseFile>()
                .HasOne(rf => rf.Release)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReleaseFile>()
                .HasOne(rf => rf.File)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<File>()
                .Property(f => f.ContentType)
                .HasMaxLength(255);

            modelBuilder.Entity<File>()
                .Property(b => b.Type)
                .HasConversion(new EnumToStringConverter<FileType>());

            modelBuilder.Entity<File>()
                .HasOne(b => b.Replacing)
                .WithOne()
                .HasForeignKey<File>(b => b.ReplacingId)
                .IsRequired(false);

            modelBuilder.Entity<File>()
                .HasOne(b => b.ReplacedBy)
                .WithOne()
                .HasForeignKey<File>(b => b.ReplacedById)
                .IsRequired(false);

            modelBuilder.Entity<File>()
                .Property(comment => comment.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<Release>()
                .HasOne(r => r.PreviousVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContentBlock>()
                .ToTable("ContentBlock")
                .HasDiscriminator<string>("Type");

            modelBuilder.Entity<ContentBlock>()
                .Property(block => block.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<ContentBlock>()
                .Property(comment => comment.Locked)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<ContentSection>()
                .Property(b => b.Type)
                .HasConversion(new EnumToStringConverter<ContentSectionType>());

            modelBuilder.Entity<Release>()
                .Property(release => release.NextReleaseDate)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<PartialDate>(v));

            modelBuilder.Entity<Release>()
                .Property(release => release.PublishScheduled)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            modelBuilder.Entity<Release>()
                .Property(release => release.ApprovalStatus)
                .HasConversion(new EnumToStringConverter<ReleaseApprovalStatus>());

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Heading)
                .HasColumnName("DataBlock_Heading");

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.HighlightName)
                .HasColumnName("DataBlock_HighlightName");

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.HighlightDescription)
                .HasColumnName("DataBlock_HighlightDescription");

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

            modelBuilder.Entity<HtmlBlock>()
                .Property(block => block.Body)
                .HasColumnName("Body");

            modelBuilder.Entity<EmbedBlockLink>()
                .Property(block => block.EmbedBlockId)
                .HasColumnName("EmbedBlockId");

            modelBuilder.Entity<EmbedBlockLink>()
                .HasOne(eb => eb.EmbedBlock)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MarkDownBlock>()
                .Property(block => block.Body)
                .HasColumnName("Body");

            modelBuilder.Entity<Permalink>()
                .Property(permalink => permalink.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Permalink>()
                .HasIndex(data => data.ReleaseId);

            modelBuilder.Entity<Permalink>()
                .HasIndex(data => data.SubjectId);

            modelBuilder.Entity<ReleaseContentSection>()
                .HasKey(item => new {item.ReleaseId, item.ContentSectionId});

            modelBuilder.Entity<ReleaseContentBlock>()
                .HasKey(item => new {item.ReleaseId, item.ContentBlockId});

            modelBuilder.Entity<User>();

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

            modelBuilder.Entity<UserPublicationInvite>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<PublicationRole>());

            modelBuilder.Entity<UserPublicationInvite>()
                .Property(invite => invite.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<GlossaryEntry>()
                .Property(rs => rs.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<GlossaryEntry>()
                .HasOne(rs => rs.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<KeyStatisticDataBlock>()
                .ToTable("KeyStatisticsDataBlock");

            modelBuilder.Entity<KeyStatisticDataBlock>()
                .HasOne<DataBlock>(ks => ks.DataBlock)
                .WithMany()
                // WARN: This is necessary - otherwise an automatically generated cascade delete is added for when an
                // associated data block is removed. That cascade delete _only_ removes the KeyStatisticsDataBlock
                // entry, leaving a KeyStatistics table entry, which should never happen.
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<KeyStatisticText>()
                .ToTable("KeyStatisticsText");
        }
    }
}
