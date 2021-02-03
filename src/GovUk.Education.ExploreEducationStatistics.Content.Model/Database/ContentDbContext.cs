using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

// ReSharper disable StringLiteralTypo
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext()
        {
        }

        public ContentDbContext(DbContextOptions<ContentDbContext> options)
            : base(options)
        {
        }

        private enum FilterItemName
        {
            Characteristic__Total,
            School_Type__Total
        }

        private enum IndicatorName
        {
            Unauthorised_absence_rate,
            Overall_absence_rate,
            Authorised_absence_rate
        }

        private enum SubjectName
        {
            AbsenceByCharacteristic
        }

        private static readonly Dictionary<SubjectName, Guid> SubjectIds = new Dictionary<SubjectName, Guid>
        {
            {
                SubjectName.AbsenceByCharacteristic, new Guid("803fbf56-600f-490f-8409-6413a891720d")
            }
        };

        private static readonly Dictionary<Guid, Dictionary<FilterItemName, Guid>> SubjectFilterItemIds =
            new Dictionary<Guid, Dictionary<FilterItemName, Guid>>
            {
                {
                    SubjectIds[SubjectName.AbsenceByCharacteristic], new Dictionary<FilterItemName, Guid>
                    {
                        {
                            FilterItemName.Characteristic__Total, new Guid("183f94c3-b5d7-4868-892d-c948e256744d")
                        },
                        {
                            FilterItemName.School_Type__Total, new Guid("cb9b57e8-9965-4cb6-b61a-acc6d34b32be")
                        }
                    }
                }
            };

        private static readonly Dictionary<Guid, Dictionary<IndicatorName, Guid>> SubjectIndicatorIds =
            new Dictionary<Guid, Dictionary<IndicatorName, Guid>>
            {
                {
                    SubjectIds[SubjectName.AbsenceByCharacteristic], new Dictionary<IndicatorName, Guid>
                    {
                        {
                            IndicatorName.Unauthorised_absence_rate, new Guid("ccfe716a-6976-4dc3-8fde-a026cd30f3ae")
                        },
                        {
                            IndicatorName.Overall_absence_rate, new Guid("92d3437a-0a62-4cd7-8dfb-bcceba7eef61")
                        },
                        {
                            IndicatorName.Authorised_absence_rate, new Guid("f9ae4976-7cd3-4718-834a-09349b6eb377")
                        }
                    }
                }
            };

        private const string CountryCodeEngland = "E92000001";

        public DbSet<Methodology> Methodologies { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<LegacyRelease> LegacyReleases { get; set; }
        public DbSet<ReleaseFile> ReleaseFiles { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<ContentSection> ContentSections { get; set; }
        public DbSet<ContentBlock> ContentBlocks { get; set; }
        public DbSet<DataBlock> DataBlocks { get; set; }
        public DbSet<Import> Imports { get; set; }
        public DbSet<ImportError> ImportErrors { get; set; }
        public DbSet<HtmlBlock> HtmlBlocks { get; set; }
        public DbSet<MarkDownBlock> MarkDownBlocks { get; set; }
        public DbSet<ReleaseType> ReleaseTypes { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ReleaseContentSection> ReleaseContentSections { get; set; }
        public DbSet<ReleaseContentBlock> ReleaseContentBlocks { get; set; }
        public DbSet<Update> Update { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserReleaseRole> UserReleaseRoles { get; set; }

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

            modelBuilder.Entity<Import>()
                .HasOne(import => import.File)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Import>()
                .HasOne(import => import.MetaFile)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Import>()
                .HasOne(import => import.ZipFile)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Import>()
                .Property(import => import.Status)
                .HasConversion(new EnumToStringConverter<ImportStatus>());

            modelBuilder.Entity<ImportError>()
                .Property(importError => importError.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Methodology>()
                .Property(b => b.Content)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));

            modelBuilder.Entity<Methodology>()
                .Property(b => b.Annexes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));

            modelBuilder.Entity<Methodology>()
                .Property(b => b.Status)
                .HasConversion(new EnumToStringConverter<MethodologyStatus>());

            modelBuilder.Entity<Publication>()
                .Property(p => p.LegacyPublicationUrl)
                .HasConversion(
                    p => p.ToString(),
                    p => new Uri(p));

            modelBuilder.Entity<Publication>()
                .OwnsOne(p => p.ExternalMethodology).ToTable("ExternalMethodology");

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

            modelBuilder.Entity<ReleaseFile>()
                .HasOne(r => r.Release)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<File>()
                .HasOne(r => r.Release)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<Release>()
                .HasOne(r => r.PreviousVersion)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ContentBlock>()
                .ToTable("ContentBlock")
                .HasDiscriminator<string>("Type");

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
                .Property(release => release.Status)
                .HasConversion(new EnumToStringConverter<ReleaseStatus>());

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Heading)
                .HasColumnName("DataBlock_Heading");

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.HighlightName)
                .HasColumnName("DataBlock_HighlightName");

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
                .Property(block => block.Summary)
                .HasColumnName("DataBlock_Summary")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<DataBlockSummary>(v));

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Table)
                .HasColumnName("DataBlock_Table")
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

            modelBuilder.Entity<UserReleaseRole>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<ReleaseRole>());

            modelBuilder.Entity<UserReleaseRole>()
                .HasQueryFilter(r => !r.SoftDeleted);

            modelBuilder.Entity<UserReleaseInvite>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<ReleaseRole>());

            modelBuilder.Entity<UserReleaseInvite>()
                .HasQueryFilter(r => !r.SoftDeleted);

            var analystMvcUser1Id = new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04");
            var analystMvcUser2Id = new Guid("6620bccf-2433-495e-995d-fc76c59d9c62");
            var analystMvcUser3Id = new Guid("b390b405-ef90-4b9d-8770-22948e53189a");
            var bauMvcUser1Id = new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd");
            var bauMvcUser2Id = new Guid("b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63");
            var prereleaseMvcUser1Id = new Guid("d5c85378-df85-482c-a1ce-09654dae567d");
            var prereleaseMvcUser2Id = new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050");

            var pupilAbsencePublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad");
            var dataAnalystContactId = new Guid("6256c8e5-9754-4873-90aa-cea429ab5b6c");
            var absenceReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5");
            var absenceMethodologyId = new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7");
            var pupilAbsenceRatesContentSectionId = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af");
            var pupilAbsenceSummaryContentSectionId = new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4");
            var pupilAbsenceKeyStatisticsSectionId = new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8");
            var pupilAbsenceKeyStatisticsSecondarySectionId = new Guid("30d74065-66b8-4843-9761-4578519e1394");
            var pupilAbsenceKeyStatisticsHeadlineSectionId = new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3");

            modelBuilder.Entity<ReleaseType>().HasData(
                new ReleaseType
                {
                    Id = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    Title = "Official Statistics"
                },
                new ReleaseType
                {
                    Id = new Guid("1821abb8-68b0-431b-9770-0bea65d02ff0"),
                    Title = "Ad Hoc"
                },
                new ReleaseType
                {
                    Id = new Guid("8becd272-1100-4e33-8a7d-1c0c4e3b42b8"),
                    Title = "National Statistics"
                });

            modelBuilder.Entity<Theme>().HasData(
                new Theme
                {
                    Id = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Title = "Pupils and schools",
                    Summary = "",
                    Slug = "pupils-and-schools"
                }
            );

            modelBuilder.Entity<Topic>().HasData(
                new Topic
                {
                    Id = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Title = "Pupil absence",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "pupil-absence"
                }
            );

            modelBuilder.Entity<Contact>().HasData(
                new Contact
                {
                    Id = dataAnalystContactId,
                    TeamName = "Test Team",
                    TeamEmail = "explore.statistics@education.gov.uk",
                    ContactName = "Data Analyst",
                    ContactTelNo = "01234100100"
                }
            );

            modelBuilder.Entity<Publication>().HasData(
                new Publication
                {
                    Id = pupilAbsencePublicationId,
                    Title = "Pupil absence in schools in England",
                    MethodologyId = absenceMethodologyId,
                    Summary = "",
                    TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Slug = "pupil-absence-in-schools-in-england",
                    DataSource = "",
                    ContactId = dataAnalystContactId
                }
            );

            modelBuilder.Entity<Release>().HasData(
                new Release
                {
                    Id = absenceReleaseId,
                    ReleaseName = "2016",
                    NextReleaseDate = new PartialDate
                    {
                        Day = "22",
                        Month = "3",
                        Year = "2019"
                    },
                    PublicationId = pupilAbsencePublicationId,
                    Published = new DateTime(2018, 4, 25, 9, 30, 0),
                    PublishScheduled = new DateTime(2018, 4, 25),
                    Status = ReleaseStatus.Approved,
                    Slug = "2016-17",
                    TimePeriodCoverage = AcademicYear,
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    Created = new DateTime(2017, 8, 1, 23, 59, 54, DateTimeKind.Utc),
                    CreatedById = bauMvcUser1Id,
                    PreviousVersionId = null
                }
            );

            modelBuilder.Entity<ContentSection>().HasData(
                new ContentSection
                {
                    Id = pupilAbsenceRatesContentSectionId,
                    Order = 1, Heading = "Pupil absence rates", Caption = "",
                    Type = ContentSectionType.Generic
                },

                new ContentSection
                {
                    Id = pupilAbsenceSummaryContentSectionId,
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.ReleaseSummary
                },

                new ContentSection
                {
                    Id = pupilAbsenceKeyStatisticsSectionId,
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatistics
                },

                new ContentSection
                {
                    Id = pupilAbsenceKeyStatisticsSecondarySectionId,
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatisticsSecondary
                },

                new ContentSection
                {
                    Id = pupilAbsenceKeyStatisticsHeadlineSectionId,
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.Headlines
                }
            );

            modelBuilder.Entity<ReleaseContentSection>().HasData(
                new ReleaseContentSection
                {
                    ContentSectionId = pupilAbsenceRatesContentSectionId,
                    ReleaseId = absenceReleaseId
                },

                new ReleaseContentSection
                {
                    ContentSectionId = pupilAbsenceSummaryContentSectionId,
                    ReleaseId = absenceReleaseId,
                },

                new ReleaseContentSection
                {
                    ContentSectionId = pupilAbsenceKeyStatisticsSectionId,
                    ReleaseId = absenceReleaseId,
                },

                new ReleaseContentSection
                {
                    ContentSectionId = pupilAbsenceKeyStatisticsSecondarySectionId,
                    ReleaseId = absenceReleaseId,
                },

                new ReleaseContentSection
                {
                    ContentSectionId = pupilAbsenceKeyStatisticsHeadlineSectionId,
                    ReleaseId = absenceReleaseId,
                }
            );

            modelBuilder.Entity<ReleaseContentBlock>().HasData(
                // absence key stats tile 1 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                    ReleaseId = absenceReleaseId,
                },
                // absence key stats aggregate table data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                    ReleaseId = absenceReleaseId,
                },
                // absence generic content data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                    ReleaseId = absenceReleaseId,
                }
            );

            modelBuilder.Entity<DataBlock>().HasData(
                // absence key statistics tile 1
                new DataBlock
                {
                    Id = new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                    ContentSectionId = pupilAbsenceKeyStatisticsSectionId,
                    Order = 1,
                    Name = "Key Stat 1",
                    Query = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Locations = new LocationQuery
                        {
                            Country = new List<string>
                            {
                                CountryCodeEngland
                            }
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2016,
                            StartCode = AcademicYear,
                            EndYear = 2016,
                            EndCode = AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                        }
                    },

                    Summary = new DataBlockSummary
                    {
                        DataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate)
                        },
                        DataDefinitionTitle = new List<string>
                        {
                            "What is overall absence?"
                        },
                        DataSummary = new List<string>
                        {
                            "Up from 4.6% in 2015/16",
                        },
                        DataDefinition = new List<string>
                        {
                            @"Total number of all authorised and unauthorised absences from possible school sessions for all pupils.",
                        }
                    },
                    Table = new TableBuilderConfiguration
                    {
                        TableHeaders = new TableHeaders
                        {
                            ColumnGroups = new List<List<TableHeader>>(),
                            Columns = new List<TableHeader>
                            {
                                new TableHeader("2016_AY", TableHeaderType.TimePeriod)
                            },
                            RowGroups = new List<List<TableHeader>>
                            {
                                new List<TableHeader>
                                {
                                    TableHeader.NewLocationHeader(GeographicLevel.Country, CountryCodeEngland)
                                }
                            },
                            Rows = new List<TableHeader>
                            {
                                new TableHeader(
                                    Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                        IndicatorName.Overall_absence_rate).ToString(), TableHeaderType.Indicator)
                            }
                        }
                    },
                    Charts = new List<IChart>()
                },
                // absence key statistics aggregate table
                new DataBlock
                {
                    Id = new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                    ContentSectionId = pupilAbsenceKeyStatisticsSecondarySectionId,
                    Name = "Key Stats aggregate table",
                    Order = 1,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Locations = new LocationQuery
                        {
                            Country = new List<string>
                            {
                                CountryCodeEngland
                            }
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = AcademicYear,
                            EndYear = 2016,
                            EndCode = AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate)
                        }
                    },

                    Summary = new DataBlockSummary
                    {
                        DataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate)
                        },
                        DataDefinitionTitle = new List<string>
                        {
                            "What is overall absence?",
                            "What is authorized absence?",
                            "What is unauthorized absence?"
                        },
                        DataSummary = new List<string>
                        {
                            "Up from 4.6% in 2015/16",
                            "Similar to previous years",
                            "Up from 1.1% in 2015/16"
                        },
                        DataDefinition = new List<string>
                        {
                            @"Total number of all authorised and unauthorised absences from possible school sessions for all pupils.",
                            @"Number of authorised absences as a percentage of the overall school population.",
                            @"Number of unauthorised absences as a percentage of the overall school population."
                        }
                    },
                    Table = new TableBuilderConfiguration
                    {
                        TableHeaders = new TableHeaders
                        {
                            ColumnGroups = new List<List<TableHeader>>(),
                            Columns = new List<TableHeader>
                            {
                                new TableHeader("2012_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2013_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2014_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2015_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2016_AY", TableHeaderType.TimePeriod)
                            },
                            RowGroups = new List<List<TableHeader>>
                            {
                                new List<TableHeader>
                                {
                                    TableHeader.NewLocationHeader(GeographicLevel.Country, CountryCodeEngland)
                                }
                            },
                            Rows = new List<TableHeader>
                            {
                                new TableHeader(
                                    Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                        IndicatorName.Authorised_absence_rate).ToString(), TableHeaderType.Indicator),
                                new TableHeader(
                                    Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                        IndicatorName.Unauthorised_absence_rate).ToString(), TableHeaderType.Indicator),
                                new TableHeader(
                                    Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                        IndicatorName.Overall_absence_rate).ToString(), TableHeaderType.Indicator)
                            }
                        }
                    },
                    Charts = new List<IChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfiguration>
                            {
                                ["major"] = new ChartAxisConfiguration
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfiguration
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Legend = new ChartLegend
                            {
                                Position = ChartLegendPosition.top,
                                Items = new List<ChartLegendItem>
                                {
                                    new ChartLegendItem
                                    {
                                        DataSet = new ChartLegendItemDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)

                                            }
                                        },
                                        Label = "Unauthorised absence rate",
                                        Colour = "#4763a5",
                                        Symbol = ChartLineSymbol.circle,
                                        LineStyle = ChartLineStyle.solid
                                    },
                                    new ChartLegendItem
                                    {
                                        DataSet = new ChartLegendItemDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                                            },
                                        },
                                        Label = "Overall absence rate",
                                        Colour = "#f5a450",
                                        Symbol = ChartLineSymbol.cross,
                                        LineStyle = ChartLineStyle.solid
                                    },
                                    new ChartLegendItem
                                    {
                                        DataSet = new ChartLegendItemDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                                            },
                                        },
                                        Label = "Authorised absence rate",
                                        Colour = "#005ea5",
                                        Symbol = ChartLineSymbol.diamond,
                                        LineStyle = ChartLineStyle.solid

                                    }
                                }
                            }
                        }
                    }
                },
                // absence generic data blocks used in content
                new DataBlock
                {
                    Id = new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                    ContentSectionId = pupilAbsenceRatesContentSectionId,
                    Name = "Generic data block - National",
                    Query = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Locations = new LocationQuery
                        {
                            Country = new List<string>
                            {
                                CountryCodeEngland
                            }
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = AcademicYear,
                            EndYear = 2016,
                            EndCode = AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate)
                        }
                    },
                    Table = new TableBuilderConfiguration
                    {
                        TableHeaders = new TableHeaders
                        {
                            ColumnGroups = new List<List<TableHeader>>(),
                            Columns = new List<TableHeader>
                            {
                                new TableHeader("2012_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2013_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2014_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2015_AY", TableHeaderType.TimePeriod),
                                new TableHeader("2016_AY", TableHeaderType.TimePeriod)
                            },
                            RowGroups = new List<List<TableHeader>>
                            {
                                new List<TableHeader>
                                {
                                    TableHeader.NewLocationHeader(GeographicLevel.Country, CountryCodeEngland)
                                }
                            },
                            Rows = new List<TableHeader>
                            {
                                new TableHeader(
                                    Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                        IndicatorName.Authorised_absence_rate).ToString(), TableHeaderType.Indicator),
                                new TableHeader(
                                    Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                        IndicatorName.Unauthorised_absence_rate).ToString(), TableHeaderType.Indicator),
                                new TableHeader(
                                    Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                        IndicatorName.Overall_absence_rate).ToString(), TableHeaderType.Indicator)
                            }
                        }
                    },
                    Charts = new List<IChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfiguration>
                            {
                                ["major"] = new ChartAxisConfiguration
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfiguration
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Legend = new ChartLegend
                            {
                                Position = ChartLegendPosition.top,
                                Items = new List<ChartLegendItem>
                                {
                                    new ChartLegendItem
                                    {
                                        DataSet = new ChartLegendItemDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                                            }
                                        },
                                        Label = "Unauthorised absence rate",
                                        Colour = "#4763a5",
                                        Symbol = ChartLineSymbol.circle,
                                        LineStyle = ChartLineStyle.solid
                                    },
                                    new ChartLegendItem
                                    {
                                        DataSet = new ChartLegendItemDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                                            }
                                        },
                                        Label = "Overall absence rate",
                                        Colour = "#f5a450",
                                        Symbol = ChartLineSymbol.cross,
                                        LineStyle = ChartLineStyle.solid
                                    },
                                    new ChartLegendItem
                                    {
                                        DataSet = new ChartLegendItemDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                                            }
                                        },
                                        Label = "Authorised absence rate",
                                        Colour = "#005ea5",
                                        Symbol = ChartLineSymbol.diamond,
                                        LineStyle = ChartLineStyle.solid
                                    }
                                }
                            }
                        }
                    }
                }
            );

            modelBuilder.Entity<Methodology>().HasData(
                new Methodology
                {
                    Id = absenceMethodologyId,
                    Title = "Pupil absence statistics: methodology",
                    Published = new DateTime(2018, 3, 22),
                    Updated = new DateTime(2019, 6, 26),
                    Slug = "pupil-absence-in-schools-in-england",
                    Status = MethodologyStatus.Draft,
                    Summary = "",
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Id = new Guid("5a7fd947-d131-475d-afcd-11ab2b1ece67"),
                            Heading = "1. Overview of absence statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("4d5ae97d-fa1c-4a09-a0a3-b28307fcfb09"),
                                    Body = System.IO.File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section1.html")
                                        ? System.IO.File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section1.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        }
                    },
                    Annexes = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Id = new Guid("0522bb29-1e0d-455a-88ef-5887f76fb069"),
                            Heading = "Annex A - Calculations",
                            Caption = "",
                            Order = 1,
                            Content = new List<ContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("8b90b3b2-f63d-4499-91aa-41ccae74e1c7"),
                                    Body = System.IO.File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexA.html")
                                        ? System.IO.File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexA.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        }
                    }
                }
            );

            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        Id = analystMvcUser1Id,
                        FirstName = "Analyst1",
                        LastName = "User1",
                        Email = "ees-analyst1@education.gov.uk"
                    },
                    new User
                    {
                        Id = analystMvcUser2Id,
                        FirstName = "Analyst2",
                        LastName = "User2",
                        Email = "ees-analyst2@education.gov.uk"
                    },
                    new User
                    {
                        Id = analystMvcUser3Id,
                        FirstName = "Analyst3",
                        LastName = "User3",
                        Email = "ees-analyst3@education.gov.uk"
                    },
                    new User
                    {
                        Id = bauMvcUser1Id,
                        FirstName = "Bau1",
                        LastName = "User1",
                        Email = "ees-bau1@education.gov.uk"
                    },
                    new User
                    {
                        Id = bauMvcUser2Id,
                        FirstName = "Bau2",
                        LastName = "User2",
                        Email = "ees-bau2@education.gov.uk"
                    },
                    new User
                    {
                        Id = prereleaseMvcUser1Id,
                        FirstName = "Prerelease1",
                        LastName = "User1",
                        Email = "ees-prerelease1@education.gov.uk"
                    },
                    new User
                    {
                        Id = prereleaseMvcUser2Id,
                        FirstName = "Prerelease2",
                        LastName = "User2",
                        Email = "ees-prerelease2@education.gov.uk"
                    }
                );

            modelBuilder.Entity<UserReleaseRole>()
                .HasData(
                    new UserReleaseRole
                    {
                        Id = new Guid("1501265c-979b-4cd4-8a55-00bfe909a2da"),
                        ReleaseId = absenceReleaseId,
                        UserId = analystMvcUser1Id,
                        Role = ReleaseRole.Contributor
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("086b1354-473c-48bb-9d30-0ac1963dc4cb"),
                        ReleaseId = absenceReleaseId,
                        UserId = analystMvcUser2Id,
                        Role = ReleaseRole.Lead
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("1851e50d-04ac-4e16-911b-3df3350c589b"),
                        ReleaseId = absenceReleaseId,
                        UserId = analystMvcUser2Id,
                        Role = ReleaseRole.Approver
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("69860a07-91d0-49d6-973d-98830fbbedfb"),
                        ReleaseId = absenceReleaseId,
                        UserId = prereleaseMvcUser1Id,
                        Role = ReleaseRole.PrereleaseViewer
                    }
                );
        }

        private static Guid FItem(Guid subjectId, FilterItemName filterItemName)
        {
            return SubjectFilterItemIds[subjectId][filterItemName];
        }

        private static Guid Indicator(Guid subjectId, IndicatorName indicatorName)
        {
            return SubjectIndicatorIds[subjectId][indicatorName];
        }
    }
}
