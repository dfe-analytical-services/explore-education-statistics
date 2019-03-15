using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AttributeMeta> AttributeMeta { get; set; }
        public DbSet<CharacteristicMeta> CharacteristicMeta { get; set; }
        public DbSet<Release> Release { get; set; }
        public DbSet<GeographicData> GeographicData { get; set; }
        public DbSet<CharacteristicDataNational> CharacteristicDataNational { get; set; }
        public DbSet<CharacteristicDataLa> CharacteristicDataLa { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUnit(modelBuilder);
            ConfigureReleaseAttributeMeta(modelBuilder);
            ConfigureReleaseCharacteristicMeta(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureRelease(modelBuilder);
            ConfigureYear(modelBuilder);
            ConfigureLevel(modelBuilder);
            ConfigureSchoolType(modelBuilder);
            ConfigureSchool(modelBuilder);
            ConfigureAttributes(modelBuilder);
            ConfigureCountry(modelBuilder);
            ConfigureLocalAuthority(modelBuilder);
            ConfigureRegion(modelBuilder);
            ConfigureCharacteristic(modelBuilder);
        }

        private static void ConfigureUnit(ModelBuilder modelBuilder)
        {
            var unitConverter = new EnumToStringConverter<Unit>();

            modelBuilder.Entity<AttributeMeta>()
                .Property(data => data.Unit)
                .HasConversion(unitConverter);
        }

        private static void ConfigureReleaseAttributeMeta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseAttributeMeta>()
                .HasKey(releaseAttributeMeta =>
                    new
                    {
                        releaseAttributeMeta.ReleaseId,
                        releaseAttributeMeta.AttributeMetaId,
                        releaseAttributeMeta.DataType
                    });

            modelBuilder.Entity<ReleaseAttributeMeta>()
                .HasOne(releaseAttributeMeta => releaseAttributeMeta.AttributeMeta)
                .WithMany(attributeMeta => attributeMeta.ReleaseAttributeMetas)
                .HasForeignKey(releaseAttributeMeta => releaseAttributeMeta.AttributeMetaId);

            modelBuilder.Entity<ReleaseAttributeMeta>()
                .HasOne(releaseAttributeMeta => releaseAttributeMeta.Release)
                .WithMany(release => release.ReleaseAttributeMetas)
                .HasForeignKey(releaseAttributeMeta => releaseAttributeMeta.ReleaseId);
        }

        private static void ConfigureReleaseCharacteristicMeta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseCharacteristicMeta>()
                .HasKey(releaseCharacteristicMeta =>
                    new
                    {
                        releaseCharacteristicMeta.ReleaseId,
                        releaseCharacteristicMeta.CharacteristicMetaId,
                        releaseCharacteristicMeta.DataType
                    });

            modelBuilder.Entity<ReleaseCharacteristicMeta>()
                .HasOne(releaseCharacteristicMeta => releaseCharacteristicMeta.CharacteristicMeta)
                .WithMany(characteristicMeta => characteristicMeta.ReleaseCharacteristicMetas)
                .HasForeignKey(releaseCharacteristicMeta => releaseCharacteristicMeta.CharacteristicMetaId);

            modelBuilder.Entity<ReleaseCharacteristicMeta>()
                .HasOne(releaseCharacteristicMeta => releaseCharacteristicMeta.Release)
                .WithMany(release => release.ReleaseCharacteristicMetas)
                .HasForeignKey(releaseCharacteristicMeta => releaseCharacteristicMeta.ReleaseId);
        }

        private static void ConfigurePublication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Release>()
                .HasIndex(data => data.PublicationId);

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.PublicationId);

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.PublicationId);

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.PublicationId);
        }

        private static void ConfigureRelease(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.ReleaseId);

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.ReleaseId);

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.ReleaseId);
        }

        private static void ConfigureYear(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.Year);

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.Year);

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.Year);
        }

        private static void ConfigureLevel(ModelBuilder modelBuilder)
        {
            var levelConverter = new EnumToStringConverter<Level>();

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Level)
                .HasConversion(levelConverter);

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.Level);

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Level)
                .HasConversion(levelConverter);

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.Level);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Level)
                .HasConversion(levelConverter);

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.Level);
        }

        private static void ConfigureSchoolType(ModelBuilder modelBuilder)
        {
            var schoolTypeConverter = new EnumToStringConverter<SchoolType>();

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.SchoolType);

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.SchoolType);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.SchoolType);
        }

        private static void ConfigureSchool(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.School)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<School>(v));

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.SchoolLaEstab)
                .HasComputedColumnSql("JSON_VALUE(School, '$.laestab')");
        }

        private static void ConfigureAttributes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Attributes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Attributes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Attributes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
        }

        private static void ConfigureCountry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Country)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Country>(v));

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Country)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Country>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Country)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Country>(v));
        }

        private static void ConfigureLocalAuthority(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.LocalAuthority)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<LocalAuthority>(v));

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.LocalAuthorityCode)
                .HasComputedColumnSql("JSON_VALUE(LocalAuthority, '$.new_la_code')");

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.LocalAuthority)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<LocalAuthority>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.LocalAuthorityCode)
                .HasComputedColumnSql("JSON_VALUE(LocalAuthority, '$.new_la_code')");
        }

        private static void ConfigureRegion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Region)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Region>(v));

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.RegionCode)
                .HasComputedColumnSql("JSON_VALUE(Region, '$.region_code')");

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Region)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Region>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.RegionCode)
                .HasComputedColumnSql("JSON_VALUE(Region, '$.region_code')");
        }

        private static void ConfigureCharacteristic(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Characteristic)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Characteristic>(v));

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.CharacteristicName)
                .HasComputedColumnSql("JSON_VALUE(Characteristic, '$.characteristic_1')");

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.CharacteristicName);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Characteristic)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Characteristic>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.CharacteristicName)
                .HasComputedColumnSql("JSON_VALUE(Characteristic, '$.characteristic_1')");

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.CharacteristicName);
        }
    }
}