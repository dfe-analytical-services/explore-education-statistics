using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2226_ContentInitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TeamName = table.Column<string>(nullable: false),
                    TeamEmail = table.Column<string>(nullable: false),
                    ContactName = table.Column<string>(nullable: false),
                    ContactTelNo = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Heading = table.Column<string>(nullable: true),
                    Caption = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Methodologies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: false),
                    Published = table.Column<DateTime>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Annexes = table.Column<string>(nullable: true),
                    InternalReleaseNote = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Methodologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Summary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentBlock",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContentSectionId = table.Column<Guid>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    DataBlock_Heading = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DataBlock_HighlightName = table.Column<string>(nullable: true),
                    DataBlock_HighlightDescription = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    DataBlock_Query = table.Column<string>(nullable: true),
                    DataBlock_Charts = table.Column<string>(nullable: true),
                    DataBlock_Summary = table.Column<string>(nullable: true),
                    DataBlock_Table = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBlock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentBlock_ContentSections_ContentSectionId",
                        column: x => x.ContentSectionId,
                        principalTable: "ContentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    ThemeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topics_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RootPath = table.Column<Guid>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    ReplacedById = table.Column<Guid>(nullable: true),
                    ReplacingId = table.Column<Guid>(nullable: true),
                    SourceId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Files_ReplacedById",
                        column: x => x.ReplacedById,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Files_ReplacingId",
                        column: x => x.ReplacingId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Files_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContentBlockId = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<Guid>(nullable: true),
                    LegacyCreatedBy = table.Column<string>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comment_ContentBlock_ContentBlockId",
                        column: x => x.ContentBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comment_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DataSource = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true),
                    MethodologyId = table.Column<Guid>(nullable: true),
                    LegacyPublicationUrl = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: true),
                    TopicId = table.Column<Guid>(nullable: false),
                    ContactId = table.Column<Guid>(nullable: true),
                    Updated = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publications_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Publications_Methodologies_MethodologyId",
                        column: x => x.MethodologyId,
                        principalTable: "Methodologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Publications_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    StagePercentageComplete = table.Column<int>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    MetaFileId = table.Column<Guid>(nullable: false),
                    ZipFileId = table.Column<Guid>(nullable: true),
                    Rows = table.Column<int>(nullable: false),
                    NumBatches = table.Column<int>(nullable: false),
                    RowsPerBatch = table.Column<int>(nullable: false),
                    TotalRows = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataImports_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataImports_Files_MetaFileId",
                        column: x => x.MetaFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataImports_Files_ZipFileId",
                        column: x => x.ZipFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MethodologyFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MethodologyId = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodologyFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodologyFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MethodologyFiles_Methodologies_MethodologyId",
                        column: x => x.MethodologyId,
                        principalTable: "Methodologies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExternalMethodology",
                columns: table => new
                {
                    PublicationId = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalMethodology", x => x.PublicationId);
                    table.ForeignKey(
                        name: "FK_ExternalMethodology_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LegacyReleases",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Url = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    PublicationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyReleases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegacyReleases_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<Guid>(nullable: false),
                    PreviousVersionId = table.Column<Guid>(nullable: true),
                    Version = table.Column<int>(nullable: false),
                    ReleaseName = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: true),
                    PublishScheduled = table.Column<DateTime>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    PublicationId = table.Column<Guid>(nullable: false),
                    PreReleaseAccessList = table.Column<string>(nullable: true),
                    MetaGuidance = table.Column<string>(nullable: true),
                    SoftDeleted = table.Column<bool>(nullable: false),
                    TypeId = table.Column<Guid>(nullable: true),
                    TimePeriodCoverage = table.Column<string>(maxLength: 6, nullable: false),
                    Status = table.Column<string>(nullable: false),
                    InternalReleaseNote = table.Column<string>(nullable: true),
                    NextReleaseDate = table.Column<string>(nullable: true),
                    RelatedInformation = table.Column<string>(nullable: true),
                    DataLastPublished = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Releases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Releases_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Releases_Releases_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalTable: "Releases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Releases_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Releases_ReleaseTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ReleaseTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPublicationRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    PublicationId = table.Column<Guid>(nullable: false),
                    Role = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPublicationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPublicationRoles_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPublicationRoles_Publications_PublicationId",
                        column: x => x.PublicationId,
                        principalTable: "Publications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPublicationRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataImportError",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataImportId = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImportError", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataImportError_DataImports_DataImportId",
                        column: x => x.DataImportId,
                        principalTable: "DataImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseContentBlocks",
                columns: table => new
                {
                    ReleaseId = table.Column<Guid>(nullable: false),
                    ContentBlockId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseContentBlocks", x => new { x.ReleaseId, x.ContentBlockId });
                    table.ForeignKey(
                        name: "FK_ReleaseContentBlocks_ContentBlock_ContentBlockId",
                        column: x => x.ContentBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseContentBlocks_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseContentSections",
                columns: table => new
                {
                    ReleaseId = table.Column<Guid>(nullable: false),
                    ContentSectionId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseContentSections", x => new { x.ReleaseId, x.ContentSectionId });
                    table.ForeignKey(
                        name: "FK_ReleaseContentSections_ContentSections_ContentSectionId",
                        column: x => x.ContentSectionId,
                        principalTable: "ContentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseContentSections_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleaseFiles_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Update",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    On = table.Column<DateTime>(nullable: false),
                    Reason = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Update", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Update_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserReleaseInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    Role = table.Column<string>(nullable: false),
                    Accepted = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<Guid>(nullable: false),
                    SoftDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReleaseInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReleaseInvites_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserReleaseInvites_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserReleaseRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    Role = table.Column<string>(nullable: false),
                    SoftDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReleaseRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReleaseRoles_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserReleaseRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"), "Official Statistics" });

            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("1821abb8-68b0-431b-9770-0bea65d02ff0"), "Ad Hoc" });

            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] { "Id", "Title" },
                values: new object[] { new Guid("8becd272-1100-4e33-8a7d-1c0c4e3b42b8"), "National Statistics" });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ContentBlockId",
                table: "Comment",
                column: "ContentBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_CreatedById",
                table: "Comment",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlock_ContentSectionId",
                table: "ContentBlock",
                column: "ContentSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataImportError_DataImportId",
                table: "DataImportError",
                column: "DataImportId");

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_FileId",
                table: "DataImports",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_MetaFileId",
                table: "DataImports",
                column: "MetaFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_ZipFileId",
                table: "DataImports",
                column: "ZipFileId",
                unique: true,
                filter: "[ZipFileId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Files_CreatedById",
                table: "Files",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ReplacedById",
                table: "Files",
                column: "ReplacedById",
                unique: true,
                filter: "[ReplacedById] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ReplacingId",
                table: "Files",
                column: "ReplacingId",
                unique: true,
                filter: "[ReplacingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Files_SourceId",
                table: "Files",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyReleases_PublicationId",
                table: "LegacyReleases",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyFiles_FileId",
                table: "MethodologyFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyFiles_MethodologyId",
                table: "MethodologyFiles",
                column: "MethodologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_ContactId",
                table: "Publications",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_MethodologyId",
                table: "Publications",
                column: "MethodologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_TopicId",
                table: "Publications",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseContentBlocks_ContentBlockId",
                table: "ReleaseContentBlocks",
                column: "ContentBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseContentSections_ContentSectionId",
                table: "ReleaseContentSections",
                column: "ContentSectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFiles_FileId",
                table: "ReleaseFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFiles_ReleaseId",
                table: "ReleaseFiles",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_CreatedById",
                table: "Releases",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_PublicationId",
                table: "Releases",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_TypeId",
                table: "Releases",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_PreviousVersionId_Version",
                table: "Releases",
                columns: new[] { "PreviousVersionId", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_Topics_ThemeId",
                table: "Topics",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Update_ReleaseId",
                table: "Update",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_CreatedById",
                table: "UserPublicationRoles",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_PublicationId",
                table: "UserPublicationRoles",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPublicationRoles_UserId",
                table: "UserPublicationRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseInvites_CreatedById",
                table: "UserReleaseInvites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseInvites_ReleaseId",
                table: "UserReleaseInvites",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_ReleaseId",
                table: "UserReleaseRoles",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReleaseRoles_UserId",
                table: "UserReleaseRoles",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "DataImportError");

            migrationBuilder.DropTable(
                name: "ExternalMethodology");

            migrationBuilder.DropTable(
                name: "LegacyReleases");

            migrationBuilder.DropTable(
                name: "MethodologyFiles");

            migrationBuilder.DropTable(
                name: "ReleaseContentBlocks");

            migrationBuilder.DropTable(
                name: "ReleaseContentSections");

            migrationBuilder.DropTable(
                name: "ReleaseFiles");

            migrationBuilder.DropTable(
                name: "Update");

            migrationBuilder.DropTable(
                name: "UserPublicationRoles");

            migrationBuilder.DropTable(
                name: "UserReleaseInvites");

            migrationBuilder.DropTable(
                name: "UserReleaseRoles");

            migrationBuilder.DropTable(
                name: "DataImports");

            migrationBuilder.DropTable(
                name: "ContentBlock");

            migrationBuilder.DropTable(
                name: "Releases");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "ContentSections");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "ReleaseTypes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "Methodologies");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
