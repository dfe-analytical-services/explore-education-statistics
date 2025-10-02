using System.Linq.Expressions;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Semver;
using JsonSerializer = System.Text.Json.JsonSerializer;

// ReSharper disable StringLiteralTypo
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

public class ContentDbContext : DbContext
{
    public ContentDbContext()
    {
        // We intentionally don't run `Configure` here as Moq would call this constructor, and we'd immediately get
        // a MockException from interacting with its fields e.g. from adding events listeners to `ChangeTracker`.
        // We can just rely on the variant which takes options instead as this is what gets used in real application
        // scenarios.
    }

    public ContentDbContext(DbContextOptions<ContentDbContext> options, bool updateTimestamps = true)
        : base(options)
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
    public virtual DbSet<Publication> Publications { get; set; }
    public virtual DbSet<ReleaseVersion> ReleaseVersions { get; set; }
    public virtual DbSet<Release> Releases { get; set; }
    public virtual DbSet<ReleaseStatus> ReleaseStatus { get; set; }
    public virtual DbSet<ReleaseFile> ReleaseFiles { get; set; }
    public virtual DbSet<File> Files { get; set; }
    public virtual DbSet<DataSetFileVersionGeographicLevel> DataSetFileVersionGeographicLevels { get; set; }
    public virtual DbSet<ContentSection> ContentSections { get; set; }
    public virtual DbSet<ContentBlock> ContentBlocks { get; set; }
    public virtual DbSet<KeyStatistic> KeyStatistics { get; set; }
    public virtual DbSet<KeyStatisticDataBlock> KeyStatisticsDataBlock { get; set; }
    public virtual DbSet<KeyStatisticText> KeyStatisticsText { get; set; }
    public virtual DbSet<DataBlock> DataBlocks { get; set; }
    public virtual DbSet<DataBlockParent> DataBlockParents { get; set; }
    public virtual DbSet<DataBlockVersion> DataBlockVersions { get; set; }
    public virtual DbSet<DataSetUpload> DataSetUploads { get; set; }
    public virtual DbSet<DataImport> DataImports { get; set; }
    public virtual DbSet<DataImportError> DataImportErrors { get; set; }
    public virtual DbSet<HtmlBlock> HtmlBlocks { get; set; }
    public virtual DbSet<EmbedBlock> EmbedBlocks { get; set; }
    public virtual DbSet<EmbedBlockLink> EmbedBlockLinks { get; set; }
    public virtual DbSet<FeaturedTable> FeaturedTables { get; set; }
    public virtual DbSet<MethodologyNote> MethodologyNotes { get; set; }
    public virtual DbSet<Organisation> Organisations { get; set; } = null!;
    public virtual DbSet<Permalink> Permalinks { get; set; } = null!;
    public virtual DbSet<Contact> Contacts { get; set; }
    public virtual DbSet<Update> Update { get; set; }
    public virtual DbSet<PublicationRedirect> PublicationRedirects { get; set; }
    public virtual DbSet<ReleaseRedirect> ReleaseRedirects { get; set; }
    public virtual DbSet<MethodologyRedirect> MethodologyRedirects { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserPublicationRole> UserPublicationRoles { get; set; }
    public virtual DbSet<UserReleaseRole> UserReleaseRoles { get; set; }
    public virtual DbSet<GlossaryEntry> GlossaryEntries { get; set; }
    public virtual DbSet<Comment> Comment { get; set; }
    public virtual DbSet<UserReleaseInvite> UserReleaseInvites { get; set; }
    public virtual DbSet<UserPublicationInvite> UserPublicationInvites { get; set; }
    public virtual DbSet<PageFeedback> PageFeedback { get; set; }
    public virtual DbSet<ReleasePublishingFeedback> ReleasePublishingFeedback { get; set; }
    public virtual DbSet<EducationInNumbersPage> EducationInNumbersPages { get; set; }
    public virtual DbSet<EinContentSection> EinContentSections { get; set; }
    public virtual DbSet<EinContentBlock> EinContentBlocks { get; set; }
    public virtual DbSet<EinTile> EinTiles { get; set; }

    [DbFunction]
    public virtual IQueryable<FreeTextRank> PublicationsFreeTextTable(string searchTerm) =>
        FromExpression(() => PublicationsFreeTextTable(searchTerm));

    [DbFunction]
    public virtual IQueryable<FreeTextRank> ReleaseFilesFreeTextTable(string searchTerm) =>
        FromExpression(() => ReleaseFilesFreeTextTable(searchTerm));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureComment(modelBuilder);
        ConfigureDataSetUpload(modelBuilder);
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
        ConfigureDataSetFileVersionGeographicLevel(modelBuilder);
        ConfigureContentBlock(modelBuilder);
        ConfigureContentSection(modelBuilder);
        ConfigureReleaseVersion(modelBuilder);
        ConfigureDataBlock(modelBuilder);
        ConfigureHtmlBlock(modelBuilder);
        ConfigureEmbedBlockLink(modelBuilder);
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
        ConfigurePageFeedback(modelBuilder);
        ConfigureReleasePublishingFeedback(modelBuilder);
        ConfigureEinContentBlock(modelBuilder);
        ConfigureEinTile(modelBuilder);

        // Apply model configuration for types which implement IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
        new FreeTextRank.Config().Configure(modelBuilder.Entity<FreeTextRank>());
    }

    private static void ConfigureComment(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Comment>()
            .Property(comment => comment.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder
            .Entity<Comment>()
            .Property(comment => comment.Updated)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
    }

    private static void ConfigureDataSetUpload(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<DataSetUpload>()
            .Property(upload => upload.ScreenerResult)
            .HasConversion(
                r => JsonSerializer.Serialize(r, (JsonSerializerOptions)null),
                r => JsonSerializer.Deserialize<DataSetScreenerResponse>(r, (JsonSerializerOptions)null)
            );

        modelBuilder
            .Entity<DataSetUpload>()
            .Property(m => m.Created)
            .HasConversion(d => d, d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
    }

    private static void ConfigureDataImport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataImport>().HasOne(import => import.File).WithOne().OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<DataImport>()
            .HasIndex(import => import.FileId)
            .IncludeProperties(import => new { import.Status });

        modelBuilder.Entity<DataImport>().HasOne(import => import.MetaFile).WithOne().OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DataImport>().HasOne(import => import.ZipFile).WithMany().OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<DataImport>()
            .Property(import => import.Status)
            .HasConversion(new EnumToStringConverter<DataImportStatus>());

        modelBuilder
            .Entity<DataImport>()
            .Property(import => import.GeographicLevels)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<HashSet<GeographicLevel>>(v)
            );
    }

    private static void ConfigureDataImportError(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<DataImportError>()
            .Property(importError => importError.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }

    private static void ConfigureMethodology(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Methodology>()
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

        modelBuilder
            .Entity<MethodologyVersionContent>()
            .Property(m => m.Content)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<ContentSection>>(v)
            );

        modelBuilder
            .Entity<MethodologyVersionContent>()
            .Property(m => m.Annexes)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<ContentSection>>(v)
            );
    }

    private static void ConfigureMethodologyVersion(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<MethodologyVersion>()
            .HasOne(m => m.MethodologyContent)
            .WithOne()
            .HasForeignKey<MethodologyVersionContent>(m => m.MethodologyVersionId);

        modelBuilder
            .Entity<MethodologyVersion>()
            .Property(m => m.Status)
            .HasConversion(new EnumToStringConverter<MethodologyApprovalStatus>());

        modelBuilder
            .Entity<MethodologyVersion>()
            .Property(m => m.PublishingStrategy)
            .HasConversion(new EnumToStringConverter<MethodologyPublishingStrategy>());

        modelBuilder
            .Entity<MethodologyVersion>()
            .Property(m => m.Created)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder.Entity<MethodologyVersion>().HasOne(m => m.CreatedBy).WithMany().OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureMethodologyStatus(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<MethodologyStatus>()
            .Property(rs => rs.Created)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<MethodologyStatus>()
            .HasOne(rs => rs.CreatedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<MethodologyStatus>()
            .Property(rs => rs.ApprovalStatus)
            .HasConversion(new EnumToStringConverter<MethodologyApprovalStatus>());
    }

    private static void ConfigureMethodologyFile(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<MethodologyFile>()
            .HasOne(mf => mf.MethodologyVersion)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MethodologyFile>().HasOne(mf => mf.File).WithMany().OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureMethodologyNote(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<MethodologyNote>()
            .Property(n => n.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder.Entity<MethodologyNote>().HasOne(m => m.CreatedBy).WithMany().OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<MethodologyNote>()
            .Property(n => n.DisplayDate)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder
            .Entity<MethodologyNote>()
            .Property(n => n.Updated)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<MethodologyNote>()
            .HasOne(m => m.UpdatedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }

    private static void ConfigurePublication(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Publication>().OwnsOne(p => p.ExternalMethodology).ToTable("ExternalMethodology");

        modelBuilder
            .Entity<Publication>()
            .Property(n => n.Updated)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<Publication>()
            .HasOne(p => p.Contact)
            .WithMany() // Ideally this would be WithOne, but we would need to fix existing data to do this
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Publication>()
            .HasOne(p => p.LatestPublishedReleaseVersion)
            .WithOne()
            .HasForeignKey<Publication>(p => p.LatestPublishedReleaseVersionId)
            .IsRequired(false);

        modelBuilder
            .Entity<Publication>()
            .Property(p => p.ReleaseSeries)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<ReleaseSeriesItem>>(v)
            );
    }

    private static void ConfigurePublicationMethodology(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PublicationMethodology>().HasKey(pm => new { pm.PublicationId, pm.MethodologyId });

        modelBuilder
            .Entity<PublicationMethodology>()
            .HasOne(pm => pm.Publication)
            .WithMany(p => p.Methodologies)
            .HasForeignKey(pm => pm.PublicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<PublicationMethodology>()
            .HasOne(pm => pm.Methodology)
            .WithMany(m => m.Publications)
            .HasForeignKey(pm => pm.MethodologyId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureReleaseStatus(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ReleaseStatus>()
            .Property(rs => rs.Created)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder.Entity<ReleaseStatus>().HasOne(rs => rs.CreatedBy).WithMany().OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<ReleaseStatus>()
            .Property(rs => rs.ApprovalStatus)
            .HasConversion(new EnumToStringConverter<ReleaseApprovalStatus>());
    }

    private static void ConfigureReleaseFile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReleaseFile>(entity =>
        {
            entity.HasQueryFilter(e => !e.ReleaseVersion.SoftDeleted);
            entity.HasOne(rf => rf.ReleaseVersion).WithMany().OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(rf => rf.File).WithMany().OnDelete(DeleteBehavior.NoAction);
            entity
                .Property(rf => rf.FilterSequence)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<FilterSequenceEntry>>(v)
                );
            entity
                .Property(rf => rf.IndicatorSequence)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<IndicatorGroupSequenceEntry>>(v)
                );
            entity
                .Property(rf => rf.Published)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
            entity
                .Property(f => f.PublicApiDataSetVersion)
                .HasMaxLength(20)
                .HasConversion(v => v.ToString(), v => SemVersion.Parse(v, SemVersionStyles.Strict, 20));

            entity.HasIndex(rf => new { rf.ReleaseVersionId, rf.FileId }).IsUnique();

            entity
                .HasIndex(rf => new
                {
                    rf.ReleaseVersionId,
                    rf.PublicApiDataSetId,
                    rf.PublicApiDataSetVersion,
                })
                .IsUnique();
        });
    }

    private static void ConfigureFile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<File>(entity =>
        {
            entity.Property(f => f.ContentType).HasMaxLength(255);
            entity.Property(f => f.Type).HasConversion(new EnumToStringConverter<FileType>()).HasMaxLength(25);
            entity.HasIndex(f => f.Type);
            entity.HasOne(f => f.Replacing).WithOne().HasForeignKey<File>(f => f.ReplacingId).IsRequired(false);
            entity.HasOne(f => f.ReplacedBy).WithOne().HasForeignKey<File>(f => f.ReplacedById).IsRequired(false);
            entity
                .Property(f => f.Created)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
            entity
                .Property(p => p.DataSetFileMeta)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<DataSetFileMeta>(v)
                );
            entity
                .Property(p => p.FilterHierarchies)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<DataSetFileFilterHierarchy>>(v)
                );
        });
    }

    private static void ConfigureDataSetFileVersionGeographicLevel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataSetFileVersionGeographicLevel>(entity =>
        {
            entity.HasKey(gl => new { gl.DataSetFileVersionId, gl.GeographicLevel });

            entity
                .Property(gl => gl.GeographicLevel)
                .HasMaxLength(6)
                .HasConversion(new EnumToEnumValueConverter<GeographicLevel>());
        });
    }

    private static void ConfigureContentBlock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentBlock>(entity =>
        {
            entity.ToTable("ContentBlock").HasDiscriminator<string>("Type");
            entity.Property("Type").HasMaxLength(25);
            entity.HasIndex("Type");
            entity
                .Property(e => e.Created)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
            entity
                .Property(e => e.Locked)
                .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
        });
    }

    private static void ConfigureContentSection(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentSection>(entity =>
        {
            entity
                .Property(e => e.Type)
                .HasConversion(new EnumToStringConverter<ContentSectionType>())
                .HasMaxLength(25);
            entity.HasIndex(e => e.Type);
        });
    }

    private static void ConfigureReleaseVersion(ModelBuilder modelBuilder)
    {
        // TODO This will be removed in EES-5659. It's been added to prevent multiple delete cascade paths
        modelBuilder
            .Entity<ReleaseVersion>()
            .HasOne(rv => rv.Publication)
            .WithMany(p => p.ReleaseVersions)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<ReleaseVersion>()
            .Property<List<Link>>("RelatedInformation")
            .IsRequired()
            .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<Link>>(v));

        modelBuilder
            .Entity<ReleaseVersion>()
            .Property(rv => rv.NotifiedOn)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder.Entity<ReleaseVersion>().HasIndex(rv => new { rv.PreviousVersionId, rv.Version });

        modelBuilder.Entity<ReleaseVersion>().HasOne(rv => rv.CreatedBy).WithMany().OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<ReleaseVersion>()
            .Property(rv => rv.Published)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<ReleaseVersion>()
            .HasMany(rv => rv.PublishingOrganisations)
            .WithMany()
            .UsingEntity(
                "ReleaseVersionPublishingOrganisations",
                rv => rv.HasOne(typeof(Organisation)).WithMany().HasForeignKey("OrganisationId"),
                o => o.HasOne(typeof(ReleaseVersion)).WithMany().HasForeignKey("ReleaseVersionId")
            );

        modelBuilder.Entity<ReleaseVersion>().HasQueryFilter(rv => !rv.SoftDeleted);

        modelBuilder
            .Entity<ReleaseVersion>()
            .Property(rv => rv.Type)
            .HasConversion(new EnumToStringConverter<ReleaseType>());

        modelBuilder.Entity<ReleaseVersion>().HasIndex(rv => rv.Type);

        modelBuilder
            .Entity<ReleaseVersion>()
            .Property(rv => rv.NextReleaseDate)
            .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<PartialDate>(v));

        modelBuilder
            .Entity<ReleaseVersion>()
            .Property(rv => rv.PublishScheduled)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<ReleaseVersion>()
            .Property(rv => rv.ApprovalStatus)
            .HasConversion(new EnumToStringConverter<ReleaseApprovalStatus>());

        modelBuilder
            .Entity<ReleaseVersion>()
            .HasOne(rv => rv.PreviousVersion)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureDataBlock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataBlock>().Property(block => block.Heading).HasColumnName("DataBlock_Heading");

        modelBuilder
            .Entity<DataBlock>()
            .Property(block => block.Query)
            .HasColumnName("DataBlock_Query")
            .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<FullTableQuery>(v));

        modelBuilder
            .Entity<DataBlock>()
            .Property(block => block.Charts)
            .HasColumnName("DataBlock_Charts")
            .HasConversion(v => JsonConvert.SerializeObject(v), v => JsonConvert.DeserializeObject<List<IChart>>(v));

        modelBuilder
            .Entity<DataBlock>()
            .Property(block => block.Table)
            .HasColumnName("DataBlock_Table")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<TableBuilderConfiguration>(v)
            );
    }

    private static void ConfigureHtmlBlock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HtmlBlock>().Property(block => block.Body).HasColumnName("Body");
    }

    private static void ConfigureEmbedBlockLink(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmbedBlockLink>().Property(block => block.EmbedBlockId).HasColumnName("EmbedBlockId");

        modelBuilder.Entity<EmbedBlockLink>().HasOne(eb => eb.EmbedBlock).WithOne().OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureFeaturedTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeaturedTable>().HasOne(ft => ft.DataBlock).WithOne();
    }

    private static void ConfigurePermalink(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Permalink>()
            .Property(permalink => permalink.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder.Entity<Permalink>().HasIndex(data => data.ReleaseVersionId);

        modelBuilder.Entity<Permalink>().HasIndex(data => data.SubjectId);
    }

    private static void ConfigureRedirects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PublicationRedirect>().HasKey(pr => new { pr.PublicationId, pr.Slug });

        modelBuilder.Entity<ReleaseRedirect>().HasKey(rr => new { rr.ReleaseId, rr.Slug });

        modelBuilder.Entity<MethodologyRedirect>().HasKey(mr => new { mr.MethodologyVersionId, mr.Slug });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        // Map IdentityRole to the existing table, but exclude it from migrations for this DbContext
        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
            entity.Metadata.SetIsTableExcludedFromMigrations(true);
        });

        modelBuilder.Entity<User>().HasOne(c => c.CreatedBy).WithMany().OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>().HasOne(e => e.Role).WithMany().OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureUserPublicationRole(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<UserPublicationRole>()
            .Property(upr => upr.Created)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<UserPublicationRole>()
            .HasOne(upr => upr.CreatedBy)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<UserPublicationRole>()
            .Property(upr => upr.Role)
            .HasConversion(new EnumToStringConverter<PublicationRole>());

        // This will be changed when we start introducing the use of the NEW publication roles in the
        // UI, in STEP 9 (EES-6196) of the Permissions Rework. For now, we want to
        // filter out any usage of the NEW roles.
        var unusedRoles = new[] { PublicationRole.Approver, PublicationRole.Drafter };

        modelBuilder.Entity<UserPublicationRole>().HasQueryFilter(upr => !unusedRoles.Contains(upr.Role));
    }

    private static void ConfigureUserReleaseRole(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<UserReleaseRole>()
            .Property(userReleaseRole => userReleaseRole.Created)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<UserReleaseRole>()
            .Property(r => r.Role)
            .HasConversion(new EnumToStringConverter<ReleaseRole>());
    }

    private static void ConfigureUserReleaseInvite(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<UserReleaseInvite>()
            .Property(uri => uri.Role)
            .HasConversion(new EnumToStringConverter<ReleaseRole>());

        modelBuilder
            .Entity<UserReleaseInvite>()
            .Property(invite => invite.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder
            .Entity<UserReleaseInvite>()
            .Property(invite => invite.Updated)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<UserReleaseInvite>()
            .HasIndex(uri => new
            {
                uri.ReleaseVersionId,
                uri.Email,
                uri.Role,
            })
            .IsUnique();
    }

    private static void ConfigureUserPublicationInvite(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<UserPublicationInvite>()
            .Property(upi => upi.Role)
            .HasConversion(new EnumToStringConverter<PublicationRole>());

        modelBuilder
            .Entity<UserPublicationInvite>()
            .Property(invite => invite.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder
            .Entity<UserPublicationInvite>()
            .HasIndex(upi => new
            {
                upi.PublicationId,
                upi.Email,
                upi.Role,
            })
            .IsUnique();
    }

    private static void ConfigureGlossaryEntry(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<GlossaryEntry>()
            .Property(rs => rs.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder.Entity<GlossaryEntry>().HasOne(rs => rs.CreatedBy).WithMany().OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureKeyStatisticsDataBlock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyStatisticDataBlock>().ToTable("KeyStatisticsDataBlock");

        modelBuilder
            .Entity<KeyStatisticDataBlock>()
            .HasOne(ks => ks.DataBlock)
            .WithMany()
            // WARN: This is necessary - otherwise an automatically generated cascade delete is added for when an
            // associated data block is removed. That cascade delete _only_ removes the KeyStatisticsDataBlock
            // entry, leaving a KeyStatistics table entry, which should never happen.
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureDataBlockParent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataBlockParent>().ToTable("DataBlocks");

        modelBuilder
            .Entity<DataBlockParent>()
            .HasOne(dataBlock => dataBlock.LatestDraftVersion)
            .WithOne()
            .HasForeignKey<DataBlockParent>(version => version.LatestDraftVersionId);

        modelBuilder
            .Entity<DataBlockParent>()
            .HasOne(dataBlock => dataBlock.LatestPublishedVersion)
            .WithOne()
            .HasForeignKey<DataBlockParent>(version => version.LatestPublishedVersionId)
            .IsRequired(false);
    }

    private static void ConfigureDataBlockVersion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataBlockVersion>().ToTable("DataBlockVersions");

        modelBuilder.Entity<DataBlockVersion>().Property(f => f.DataBlockParentId).HasColumnName("DataBlockId");

        modelBuilder
            .Entity<DataBlockVersion>()
            .Property(invite => invite.Published)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<DataBlockVersion>()
            .Property(invite => invite.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder
            .Entity<DataBlockVersion>()
            .Property(invite => invite.Updated)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        // Automatically include the backing ContentBlock of type "DataBlock" whenever we retrieve
        // DataBlockVersions, as DataBlockVersions encapsulate their backing ContentBlocks and will replace them
        // entirely in EES-4640.
        modelBuilder
            .Entity<DataBlockVersion>()
            .Navigation(dataBlockVersion => dataBlockVersion.ContentBlock)
            .AutoInclude();
    }

    private static void ConfigureKeyStatisticsText(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyStatisticText>().ToTable("KeyStatisticsText");
    }

    private static void ConfigurePageFeedback(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PageFeedback>().Property(feedback => feedback.Url).IsRequired().HasMaxLength(2000);

        modelBuilder.Entity<PageFeedback>().Property(feedback => feedback.UserAgent).HasMaxLength(250);

        modelBuilder.Entity<PageFeedback>().Property(feedback => feedback.Context).HasMaxLength(2000);

        modelBuilder.Entity<PageFeedback>().Property(feedback => feedback.Issue).HasMaxLength(2000);

        modelBuilder.Entity<PageFeedback>().Property(feedback => feedback.Intent).HasMaxLength(2000);

        modelBuilder
            .Entity<PageFeedback>()
            .Property(feedback => feedback.Response)
            .HasConversion(new EnumToStringConverter<PageFeedbackResponse>())
            .IsRequired()
            .HasMaxLength(50);
    }

    private static void ConfigureReleasePublishingFeedback(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .HasOne(rf => rf.ReleaseVersion)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .Property(feedback => feedback.EmailToken)
            .IsRequired()
            .HasMaxLength(55);

        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .Property(feedback => feedback.UserPublicationRole)
            .HasConversion(new EnumToStringConverter<PublicationRole>())
            .IsRequired();

        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .Property(feedback => feedback.Response)
            .HasConversion(new EnumToStringConverter<ReleasePublishingFeedbackResponse>())
            .HasMaxLength(50);

        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .Property(feedback => feedback.EmailToken)
            .IsRequired()
            .HasMaxLength(55);

        modelBuilder.Entity<ReleasePublishingFeedback>().HasIndex(feedback => feedback.EmailToken).IsUnique();

        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .Property(feedback => feedback.Created)
            .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .Property(feedback => feedback.FeedbackReceived)
            .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder
            .Entity<ReleasePublishingFeedback>()
            .Property(feedback => feedback.AdditionalFeedback)
            .HasMaxLength(2000);
    }

    private static void ConfigureEinContentBlock(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<EinContentBlock>()
            .HasDiscriminator<string>("Type")
            .HasValue<EinHtmlBlock>("HtmlBlock")
            .HasValue<EinTileGroupBlock>("TileGroupBlock");
    }

    private static void ConfigureEinTile(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<EinTile>()
            .HasDiscriminator<string>("Type")
            .HasValue<EinFreeTextStatTile>("FreeTextStatTile");
    }
}

/// <summary>
/// This is used to disambiguate between SQL Server and PostgreSQL's EF extension methods of the same name.
/// </summary>
public static class BuilderExtensions
{
    public static IndexBuilder<TEntity> IncludeProperties<TEntity>(
        this IndexBuilder<TEntity> indexBuilder,
        Expression<Func<TEntity, object?>> includeExpression
    )
    {
        return SqlServerIndexBuilderExtensions.IncludeProperties(indexBuilder, includeExpression);
    }
}
