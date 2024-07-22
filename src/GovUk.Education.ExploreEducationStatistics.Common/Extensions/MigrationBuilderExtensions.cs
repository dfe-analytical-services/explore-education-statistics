#nullable enable
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class MigrationBuilderExtensions
    {
        public static bool IsEnvironment(
            this MigrationBuilder _,
            string environmentName) =>
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == environmentName;

        public static void SqlFromFile(
            this MigrationBuilder migrationBuilder,
            string migrationsPath,
            string filename,
            bool suppressTransaction = false)
        {
            var file = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                $"{migrationsPath}{Path.DirectorySeparatorChar}{filename}");

            migrationBuilder.Sql(File.ReadAllText(file), suppressTransaction);
        }

        public static void SqlFromFileByLine(
            this MigrationBuilder migrationBuilder,
            string migrationsPath,
            string filename)
        {
            var file = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                $"{migrationsPath}{Path.DirectorySeparatorChar}{filename}");

            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                migrationBuilder.Sql(line);
            }
        }
    }
}
