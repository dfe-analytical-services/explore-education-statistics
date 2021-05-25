using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.UsersAndRolesMigrations
{
    public partial class EES2226_UsersAndRolesInitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceCodes",
                columns: table => new
                {
                    UserCode = table.Column<string>(maxLength: 200, nullable: false),
                    DeviceCode = table.Column<string>(maxLength: 200, nullable: false),
                    SubjectId = table.Column<string>(maxLength: 200, nullable: true),
                    ClientId = table.Column<string>(maxLength: 200, nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    Expiration = table.Column<DateTime>(nullable: false),
                    Data = table.Column<string>(maxLength: 50000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCodes", x => x.UserCode);
                });

            migrationBuilder.CreateTable(
                name: "PersistedGrants",
                columns: table => new
                {
                    Key = table.Column<string>(maxLength: 200, nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: false),
                    SubjectId = table.Column<string>(maxLength: 200, nullable: true),
                    ClientId = table.Column<string>(maxLength: 200, nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    Expiration = table.Column<DateTime>(nullable: true),
                    Data = table.Column<string>(maxLength: 50000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistedGrants", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserInvites",
                columns: table => new
                {
                    Email = table.Column<string>(nullable: false),
                    Accepted = table.Column<bool>(nullable: false),
                    RoleId = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvites", x => x.Email);
                    table.ForeignKey(
                        name: "FK_UserInvites_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "cf67b697-bddd-41bd-86e0-11b7e11d99b3", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "BAU User", "BAU USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "f9ddb43e-aa9e-41ed-837d-3062e130c425", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Analyst", "ANALYST" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "17e634f4-7a2b-4a23-8636-b079877b4232", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Prerelease User", "PRERELEASE USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { -2, "ApplicationAccessGranted", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -32, "DeleteAllReleaseAmendments", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -33, "ManageAllTaxonomy", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -34, "UpdateAllPublications", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -35, "MarkAllMethodologiesDraft", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -36, "AccessAllImports", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -37, "CancelAllFileImports", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -31, "MakeAmendmentsOfAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -13, "ApplicationAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -17, "PrereleasePagesAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -21, "CanViewPrereleaseContacts", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -25, "CreateAnyMethodology", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -26, "UpdateAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -27, "AccessAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -28, "ApproveAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -14, "AnalystPagesAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -30, "PublishAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -29, "ApproveAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -24, "AccessAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -3, "AccessAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -4, "AccessAllTopics", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -5, "MarkAllReleasesAsDraft", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -6, "SubmitAllReleasesToHigherReview", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -7, "ApproveAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -8, "UpdateAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -9, "CreateAnyPublication", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -10, "CreateAnyRelease", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -11, "ManageAnyUser", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -12, "ManageAnyMethodology", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -18, "AnalystPagesAccessGranted", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -19, "PrereleasePagesAccessGranted", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -20, "CanViewPrereleaseContacts", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -22, "CreateAnyMethodology", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -23, "UpdateAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -15, "ApplicationAccessGranted", "", "17e634f4-7a2b-4a23-8636-b079877b4232" },
                    { -16, "PrereleasePagesAccessGranted", "", "17e634f4-7a2b-4a23-8636-b079877b4232" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCodes_DeviceCode",
                table: "DeviceCodes",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCodes_Expiration",
                table: "DeviceCodes",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_Expiration",
                table: "PersistedGrants",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_SubjectId_ClientId_Type",
                table: "PersistedGrants",
                columns: new[] { "SubjectId", "ClientId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInvites_RoleId",
                table: "UserInvites",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DeviceCodes");

            migrationBuilder.DropTable(
                name: "PersistedGrants");

            migrationBuilder.DropTable(
                name: "UserInvites");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AspNetRoles");
        }
    }
}
