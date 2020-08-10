using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations
{
    public partial class UsersAndRolesInitialCreate : Migration
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
                values: new object[,]
                {
                    { "cf67b697-bddd-41bd-86e0-11b7e11d99b3", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "BAU User", "BAU USER" },
                    { "f9ddb43e-aa9e-41ed-837d-3062e130c425", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Analyst", "ANALYST" },
                    { "17e634f4-7a2b-4a23-8636-b079877b4232", "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "Prerelease User", "PRERELEASE USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "analyst1@example.com", false, "Analyst1", "User1", true, null, "ANALYST1@EXAMPLE.COM", "ANALYST1@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "analyst1@example.com" },
                    { "6620bccf-2433-495e-995d-fc76c59d9c62", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "analyst2@example.com", false, "Analyst2", "User2", true, null, "ANALYST2@EXAMPLE.COM", "ANALYST2@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "analyst2@example.com" },
                    { "b390b405-ef90-4b9d-8770-22948e53189a", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "analyst3@example.com", false, "Analyst3", "User3", true, null, "ANALYST3@EXAMPLE.COM", "ANALYST3@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "analyst3@example.com" },
                    { "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "bau1@example.com", false, "Bau1", "User1", true, null, "BAU1@EXAMPLE.COM", "BAU1@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "bau1@example.com" },
                    { "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "bau2@example.com", false, "Bau2", "User2", true, null, "BAU2@EXAMPLE.COM", "BAU2@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "bau2@example.com" },
                    { "d5c85378-df85-482c-a1ce-09654dae567d", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "prerelease1@example.com", false, "Prerelease1", "User1", true, null, "PRERELEASE1@EXAMPLE.COM", "PRERELEASE1@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "prerelease1@example.com" },
                    { "ee9a02c1-b3f9-402c-9e9b-4fb78d737050", 0, "85d6c75e-a6c8-4c7e-b4d0-8ee70a4879d3", "prerelease2@example.com", false, "Prerelease2", "User2", true, null, "PRERELEASE2@EXAMPLE.COM", "PRERELEASE2@EXAMPLE.COM", null, null, false, "V7ZOUEOGN2HGZDN3HKPNIHLSUWWUHTA6", false, "prerelease2@example.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { -24, "AccessAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -17, "PrereleasePagesAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -34, "UpdateAllPublications", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -33, "ManageAllTaxonomy", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -32, "DeleteAllReleaseAmendments", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -31, "MakeAmendmentsOfAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -30, "PublishAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -29, "ApproveAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -21, "CanViewPrereleaseContacts", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -23, "UpdateAllMethodologies", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -22, "CreateAnyMethodology", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -20, "CanViewPrereleaseContacts", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -19, "PrereleasePagesAccessGranted", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -18, "AnalystPagesAccessGranted", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -12, "ManageAnyMethodology", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -14, "AnalystPagesAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -11, "ManageAnyUser", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -9, "CreateAnyPublication", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -8, "UpdateAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -7, "ApproveAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -6, "SubmitAllReleasesToHigherReview", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -5, "MarkAllReleasesAsDraft", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -4, "AccessAllTopics", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -3, "AccessAllReleases", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -2, "ApplicationAccessGranted", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -25, "CreateAnyMethodology", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -26, "UpdateAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -27, "AccessAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -28, "ApproveAllMethodologies", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { -15, "ApplicationAccessGranted", "", "17e634f4-7a2b-4a23-8636-b079877b4232" },
                    { -16, "PrereleasePagesAccessGranted", "", "17e634f4-7a2b-4a23-8636-b079877b4232" },
                    { -10, "CreateAnyRelease", "", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { -13, "ApplicationAccessGranted", "", "f9ddb43e-aa9e-41ed-837d-3062e130c425" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId" },
                values: new object[,]
                {
                    { "OpenIdConnect", "5zzTEeAYz71aVPJ1ho1VGW3cYk7_qcQpkDqYYxbH3po", "OpenIdConnect", "e7f7c82e-aaf3-43db-a5ab-755678f67d04" },
                    { "OpenIdConnect", "s5vNxMDGwRCvg3MTtLEDomZqOKl7cvv2f8PW5NvJzbw", "OpenIdConnect", "ee9a02c1-b3f9-402c-9e9b-4fb78d737050" },
                    { "OpenIdConnect", "uLGzMPaxGz0nY6nbff7wkBP7ly2iLdephomGPFOP0k8", "OpenIdConnect", "d5c85378-df85-482c-a1ce-09654dae567d" },
                    { "OpenIdConnect", "EKTK7hPGgxGVxRSBjgTv51XVJhtMo91sIcADfjSuJjw", "OpenIdConnect", "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63" },
                    { "OpenIdConnect", "cb3XrjF6BLuMZ5P3aRo8wBobF7tAshdk2gF0X5Qm68o", "OpenIdConnect", "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd" },
                    { "OpenIdConnect", "ces_f2I3zCjGZ9HUprWF3RiQgswrKvPFAY1Lwu_KI6M", "OpenIdConnect", "b390b405-ef90-4b9d-8770-22948e53189a" },
                    { "OpenIdConnect", "RLdgJMsfN6QVjpCbkaOYIpzh6DA3QpRfnBcfIx46uDM", "OpenIdConnect", "6620bccf-2433-495e-995d-fc76c59d9c62" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[,]
                {
                    { "b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { "d5c85378-df85-482c-a1ce-09654dae567d", "17e634f4-7a2b-4a23-8636-b079877b4232" },
                    { "e7f7c82e-aaf3-43db-a5ab-755678f67d04", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { "6620bccf-2433-495e-995d-fc76c59d9c62", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { "b390b405-ef90-4b9d-8770-22948e53189a", "f9ddb43e-aa9e-41ed-837d-3062e130c425" },
                    { "b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63", "cf67b697-bddd-41bd-86e0-11b7e11d99b3" },
                    { "ee9a02c1-b3f9-402c-9e9b-4fb78d737050", "17e634f4-7a2b-4a23-8636-b079877b4232" }
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
