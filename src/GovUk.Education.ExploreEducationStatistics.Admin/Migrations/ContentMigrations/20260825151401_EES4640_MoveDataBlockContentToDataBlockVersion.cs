using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    // ReSharper disable once InconsistentNaming
    public partial class EES4640_MoveDataBlockContentToDataBlockVersion : Migration
    {
        private const string MigrationId = "20260825151401";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the foreign keys and indexes that will be affected by the migration. The unique index on
            //    DataBlockVersions.ContentBlockId is deliberately left in place until step 7, as the data migration
            //    in step 5 joins on that column.
            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlock_ContentSections_ContentSectionId",
                table: "ContentBlock"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlock_ReleaseVersions_ReleaseVersionId",
                table: "ContentBlock"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_DataBlockVersions_ContentBlock_ContentBlockId",
                table: "DataBlockVersions"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedTables_ContentBlock_DataBlockId",
                table: "FeaturedTables"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedTables_DataBlocks_DataBlockParentId",
                table: "FeaturedTables"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_KeyStatisticsDataBlock_ContentBlock_DataBlockId",
                table: "KeyStatisticsDataBlock"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_KeyStatisticsDataBlock_DataBlocks_DataBlockParentId",
                table: "KeyStatisticsDataBlock"
            );

            migrationBuilder.DropIndex(name: "IX_FeaturedTables_DataBlockId", table: "FeaturedTables");

            migrationBuilder.DropIndex(name: "IX_FeaturedTables_DataBlockParentId", table: "FeaturedTables");

            migrationBuilder.DropIndex(
                name: "IX_KeyStatisticsDataBlock_DataBlockParentId",
                table: "KeyStatisticsDataBlock"
            );

            // 2. Add the six content columns to DataBlockVersions. They are nullable to begin with so that the
            //    existing data can be copied across before they are made non-nullable.
            migrationBuilder.AddColumn<string>(
                name: "Heading",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Query",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Charts",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Table",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: true
            );

            // 3. Add the DataBlockVersionId foreign key column to ContentBlock. It is nullable as only ContentBlocks
            //    of type "DataBlockVersionLink" populate it (table-per-hierarchy).
            migrationBuilder.AddColumn<Guid>(
                name: "DataBlockVersionId",
                table: "ContentBlock",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlock_DataBlockVersionId",
                table: "ContentBlock",
                column: "DataBlockVersionId",
                unique: true,
                filter: "[DataBlockVersionId] IS NOT NULL"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlock_DataBlockVersions_DataBlockVersionId",
                table: "ContentBlock",
                column: "DataBlockVersionId",
                principalTable: "DataBlockVersions",
                principalColumn: "Id"
            );

            // 4. Add the new DataBlockVersionId columns to FeaturedTables and KeyStatisticsDataBlock, replacing their
            //    DataBlockParentId columns (dropped in step 7).
            //
            //    Note that the meaning of their existing DataBlockId columns changes as well - these used to
            //    reference the ContentBlock of type "DataBlock", and now reference the parent DataBlock. Both columns
            //    are therefore re-pointed by the data migration in step 5, rather than DataBlockParentId simply being
            //    renamed.
            //
            //    The new columns are nullable to begin with so that they can be populated before being made
            //    non-nullable.
            migrationBuilder.AddColumn<Guid>(
                name: "DataBlockVersionId",
                table: "FeaturedTables",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "DataBlockVersionId",
                table: "KeyStatisticsDataBlock",
                type: "uniqueidentifier",
                nullable: true
            );

            // 5. Copy data across from the old columns to the new columns (old columns will be dropped in step 7).
            migrationBuilder.SqlFromFile(
                ContentMigrationsPath,
                $"{MigrationId}_{nameof(EES4640_MoveDataBlockContentToDataBlockVersion)}.sql"
            );

            // 6. Make the new required columns non-nullable now that the content has been copied across.
            migrationBuilder.AlterColumn<string>(
                name: "Heading",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Query",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Charts",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Table",
                table: "DataBlockVersions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "DataBlockVersionId",
                table: "FeaturedTables",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "DataBlockVersionId",
                table: "KeyStatisticsDataBlock",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true
            );

            // 7. Drop the old columns now that the content has been copied across.
            migrationBuilder.DropIndex(name: "IX_DataBlockVersions_ContentBlockId", table: "DataBlockVersions");

            migrationBuilder.DropColumn(name: "ContentBlockId", table: "DataBlockVersions");

            migrationBuilder.DropColumn(name: "DataBlock_Charts", table: "ContentBlock");

            migrationBuilder.DropColumn(name: "DataBlock_Heading", table: "ContentBlock");

            migrationBuilder.DropColumn(name: "DataBlock_Query", table: "ContentBlock");

            migrationBuilder.DropColumn(name: "DataBlock_Table", table: "ContentBlock");

            migrationBuilder.DropColumn(name: "Name", table: "ContentBlock");

            migrationBuilder.DropColumn(name: "Source", table: "ContentBlock");

            migrationBuilder.DropColumn(name: "DataBlockParentId", table: "FeaturedTables");

            migrationBuilder.DropColumn(name: "DataBlockParentId", table: "KeyStatisticsDataBlock");

            // 8. Delete the ContentBlocks belonging to DataBlocks that aren't placed in a ContentSection. These still
            //    have the old "DataBlock" discriminator, as step 5 only converted those in ContentSections into
            //    "DataBlockVersionLink"s.
            migrationBuilder.Sql(
                """
                DELETE FROM ContentBlock
                WHERE [Type] = 'DataBlock'
                  AND ContentSectionId IS NULL;
                """
            );

            // 9. Make the ContentSectionId foreign key non-nullable, as all ContentBlocks are now guaranteed to be
            //    placed in a ContentSection.
            migrationBuilder.AlterColumn<Guid>(
                name: "ContentSectionId",
                table: "ContentBlock",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true
            );

            // 10. Recreate the indexes and foreign keys that were dropped in step 1, now pointing at the new columns.
            migrationBuilder.CreateIndex(
                name: "IX_FeaturedTables_DataBlockId",
                table: "FeaturedTables",
                column: "DataBlockId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedTables_DataBlockVersionId",
                table: "FeaturedTables",
                column: "DataBlockVersionId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_KeyStatisticsDataBlock_DataBlockVersionId",
                table: "KeyStatisticsDataBlock",
                column: "DataBlockVersionId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlock_ContentSections_ContentSectionId",
                table: "ContentBlock",
                column: "ContentSectionId",
                principalTable: "ContentSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlock_ReleaseVersions_ReleaseVersionId",
                table: "ContentBlock",
                column: "ReleaseVersionId",
                principalTable: "ReleaseVersions",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedTables_DataBlockVersions_DataBlockVersionId",
                table: "FeaturedTables",
                column: "DataBlockVersionId",
                principalTable: "DataBlockVersions",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedTables_DataBlocks_DataBlockId",
                table: "FeaturedTables",
                column: "DataBlockId",
                principalTable: "DataBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KeyStatisticsDataBlock_DataBlockVersions_DataBlockVersionId",
                table: "KeyStatisticsDataBlock",
                column: "DataBlockVersionId",
                principalTable: "DataBlockVersions",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_KeyStatisticsDataBlock_DataBlocks_DataBlockId",
                table: "KeyStatisticsDataBlock",
                column: "DataBlockId",
                principalTable: "DataBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
