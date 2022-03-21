using System;
using System.Collections.Generic;
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

        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
        {
            Configure();
        }

        private void Configure()
        {
            ChangeTracker.StateChanged += DbContextUtils.UpdateTimestamps;
            ChangeTracker.Tracked += DbContextUtils.UpdateTimestamps;
        }

        public DbSet<Methodology> Methodologies { get; set; }
        public DbSet<MethodologyVersion> MethodologyVersions { get; set; }
        public DbSet<PublicationMethodology> PublicationMethodologies { get; set; }
        public DbSet<MethodologyFile> MethodologyFiles { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<ReleaseStatus> ReleaseStatus { get; set; }
        public DbSet<LegacyRelease> LegacyReleases { get; set; }
        public DbSet<ReleaseFile> ReleaseFiles { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<ContentSection> ContentSections { get; set; }
        public DbSet<ContentBlock> ContentBlocks { get; set; }
        public DbSet<DataBlock> DataBlocks { get; set; }
        public DbSet<DataImport> DataImports { get; set; }
        public DbSet<DataImportError> DataImportErrors { get; set; }
        public DbSet<HtmlBlock> HtmlBlocks { get; set; }
        public DbSet<MarkDownBlock> MarkDownBlocks { get; set; }
        public DbSet<MethodologyNote> MethodologyNotes { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ReleaseContentSection> ReleaseContentSections { get; set; }
        public DbSet<ReleaseContentBlock> ReleaseContentBlocks { get; set; }
        public DbSet<Update> Update { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPublicationRole> UserPublicationRoles { get; set; }
        public DbSet<UserReleaseRole> UserReleaseRoles { get; set; }
        public DbSet<GlossaryEntry> GlossaryEntries { get; set; }

        public DbSet<Comment> Comment { get; set; }
        public DbSet<UserReleaseInvite> UserReleaseInvites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
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
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

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

            modelBuilder.Entity<MethodologyVersion>()
                .Property(m => m.Content)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));

            modelBuilder.Entity<MethodologyVersion>()
                .Property(m => m.Annexes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));

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
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

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
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

            modelBuilder.Entity<MethodologyNote>()
                .HasOne(m => m.UpdatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            modelBuilder.Entity<Publication>()
                .Property(p => p.LegacyPublicationUrl)
                .HasConversion(
                    p => p.ToString(),
                    p => new Uri(p));

            modelBuilder.Entity<Publication>()
                .OwnsOne(p => p.ExternalMethodology)
                .ToTable("ExternalMethodology");

            modelBuilder.Entity<Publication>()
                .Property(n => n.Published)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

            modelBuilder.Entity<Publication>()
                .Property(n => n.Updated)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

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
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<Link>>(v));

            modelBuilder.Entity<Release>()
                .HasIndex(r => new {r.PreviousVersionId, r.Version});

            modelBuilder.Entity<Release>()
                .HasOne(r => r.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Release>()
                .HasQueryFilter(r => !r.SoftDeleted);

            modelBuilder.Entity<Release>()
                .Property(release => release.Type)
                .HasConversion(new EnumToEnumValueConverter<ReleaseType>());

            modelBuilder.Entity<Release>()
                .HasIndex(release => release.Type);

            modelBuilder.Entity<ReleaseStatus>()
                .Property(rs => rs.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue
                        ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                        : (DateTime?) null);

            modelBuilder.Entity<ReleaseStatus>()
                .HasOne(rs => rs.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ReleaseStatus>()
                .Property(rs => rs.NotifiedOn)
                .HasConversion(
                    v => v,
                    v => v.HasValue
                        ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                        : (DateTime?) null);

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
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

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
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

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
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

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
                // TODO EES-3212 rename back to DataBlock_Query
                .HasColumnName("DataBlock_QueryMigrated")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<ObservationQueryContext>(v));

            // TODO EES-3212 remove these temporary fields related to the migration from Location codes to id's.
            modelBuilder.Entity<DataBlock>()
                .Property(block => block.TableHeaderCountChanged)
                .HasColumnName("DataBlock_TableHeaderCountChanged")
                .HasDefaultValue(false)
                .IsRequired();
            modelBuilder.Entity<DataBlock>()
                .Property(block => block.LocationsMigrated)
                .HasColumnName("DataBlock_LocationsMigrated")
                .HasDefaultValue(true)
                .IsRequired();

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Charts)
                // TODO EES-3212 rename back to DataBlock_Charts
                .HasColumnName("DataBlock_ChartsMigrated")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<IChart>>(v));

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Summary)
                .HasColumnName("DataBlock_Summary")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<DataBlockSummary>(v));

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Table)
                // TODO EES-3212 rename this back to DataBlock_Table
                .HasColumnName("DataBlock_TableMigrated")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<TableBuilderConfiguration>(v));

            modelBuilder.Entity<HtmlBlock>()
                .Property(block => block.Body)
                .HasColumnName("Body");

            modelBuilder.Entity<MarkDownBlock>()
                .Property(block => block.Body)
                .HasColumnName("Body");

            modelBuilder.Entity<ReleaseContentSection>()
                .HasKey(item => new {item.ReleaseId, item.ContentSectionId});

            modelBuilder.Entity<ReleaseContentBlock>()
                .HasKey(item => new {item.ReleaseId, item.ContentBlockId});

            modelBuilder.Entity<User>();

            modelBuilder.Entity<UserPublicationRole>()
                .Property(r => r.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<UserPublicationRole>()
                .HasOne(r => r.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserPublicationRole>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<PublicationRole>());

            modelBuilder.Entity<UserReleaseRole>()
                .Property(userReleaseRole => userReleaseRole.Created)
                .HasConversion(
                    v => v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?) null);

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
    }
}
