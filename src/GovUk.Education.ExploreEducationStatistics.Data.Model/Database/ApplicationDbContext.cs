using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(int.MaxValue);
        }

        public DbSet<IndicatorMeta> IndicatorMeta { get; set; }
        public DbSet<CharacteristicMeta> CharacteristicMeta { get; set; }
        public DbSet<Release> Release { get; set; }
        public DbSet<GeographicData> GeographicData { get; set; }
        public DbSet<CharacteristicDataNational> CharacteristicDataNational { get; set; }
        public DbSet<CharacteristicDataLa> CharacteristicDataLa { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUnit(modelBuilder);
            ConfigureReleaseIndicatorMeta(modelBuilder);
            ConfigureReleaseCharacteristicMeta(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureRelease(modelBuilder);
            ConfigureTimePeriod(modelBuilder);
            ConfigureLevel(modelBuilder);
            ConfigureSchoolType(modelBuilder);
            ConfigureSchool(modelBuilder);
            ConfigureIndicators(modelBuilder);
            ConfigureCountry(modelBuilder);
            ConfigureLocalAuthority(modelBuilder);
            ConfigureRegion(modelBuilder);
            ConfigureCharacteristic(modelBuilder);
        }

        private static void ConfigureUnit(ModelBuilder modelBuilder)
        {
            var unitConverter = new EnumToStringConverter<Unit>();

            modelBuilder.Entity<IndicatorMeta>()
                .Property(data => data.Unit)
                .HasConversion(unitConverter);
        }

        private static void ConfigureReleaseIndicatorMeta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseIndicatorMeta>()
                .HasKey(releaseIndicatorMeta =>
                    new
                    {
                        releaseIndicatorMeta.ReleaseId,
                        releaseIndicatorMeta.IndicatorMetaId,
                        releaseIndicatorMeta.DataType
                    });

            modelBuilder.Entity<ReleaseIndicatorMeta>()
                .HasOne(releaseIndicatorMeta => releaseIndicatorMeta.IndicatorMeta)
                .WithMany(indicatorMeta => indicatorMeta.ReleaseIndicatorMetas)
                .HasForeignKey(releaseIndicatorMeta => releaseIndicatorMeta.IndicatorMetaId);

            modelBuilder.Entity<ReleaseIndicatorMeta>()
                .HasOne(releaseIndicatorMeta => releaseIndicatorMeta.Release)
                .WithMany(release => release.ReleaseIndicatorMetas)
                .HasForeignKey(releaseIndicatorMeta => releaseIndicatorMeta.ReleaseId);
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

        private static void ConfigureTimePeriod(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.TimePeriod);

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.TimePeriod);

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.TimePeriod);
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

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.SchoolLaEstab);
        }

        private static void ConfigureIndicators(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Indicators)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Indicators)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Indicators)
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

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.LocalAuthorityCode);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.LocalAuthority)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<LocalAuthority>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.LocalAuthorityCode)
                .HasComputedColumnSql("JSON_VALUE(LocalAuthority, '$.new_la_code')");

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.LocalAuthorityCode);
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

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.RegionCode);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Region)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Region>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.RegionCode)
                .HasComputedColumnSql("JSON_VALUE(Region, '$.region_code')");

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.RegionCode);
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
                .HasComputedColumnSql("JSON_VALUE(Characteristic, '$.characteristic_breakdown')");

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.CharacteristicName);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Characteristic)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Characteristic>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.CharacteristicName)
                .HasComputedColumnSql("JSON_VALUE(Characteristic, '$.characteristic_breakdown')");

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.CharacteristicName);
        }
    }
}