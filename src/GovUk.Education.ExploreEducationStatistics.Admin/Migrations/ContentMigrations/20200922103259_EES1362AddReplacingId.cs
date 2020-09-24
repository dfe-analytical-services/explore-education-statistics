﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1362AddReplacingId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReplacedById",
                table: "ReleaseFileReferences",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplacingId",
                table: "ReleaseFileReferences",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFileReferences_ReplacedById",
                table: "ReleaseFileReferences",
                column: "ReplacedById",
                unique: true,
                filter: "[ReplacedById] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFileReferences_ReplacingId",
                table: "ReleaseFileReferences",
                column: "ReplacingId",
                unique: true,
                filter: "[ReplacingId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFileReferences_ReleaseFileReferences_ReplacedById",
                table: "ReleaseFileReferences",
                column: "ReplacedById",
                principalTable: "ReleaseFileReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFileReferences_ReleaseFileReferences_ReplacingId",
                table: "ReleaseFileReferences",
                column: "ReplacingId",
                principalTable: "ReleaseFileReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFileReferences_ReleaseFileReferences_ReplacedById",
                table: "ReleaseFileReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFileReferences_ReleaseFileReferences_ReplacingId",
                table: "ReleaseFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_ReleaseFileReferences_ReplacedById",
                table: "ReleaseFileReferences");

            migrationBuilder.DropIndex(
                name: "IX_ReleaseFileReferences_ReplacingId",
                table: "ReleaseFileReferences");

            migrationBuilder.DropColumn(
                name: "ReplacedById",
                table: "ReleaseFileReferences");

            migrationBuilder.DropColumn(
                name: "ReplacingId",
                table: "ReleaseFileReferences");
        }
    }
}
