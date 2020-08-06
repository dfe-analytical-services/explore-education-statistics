using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TeamName = table.Column<string>(nullable: true),
                    TeamEmail = table.Column<string>(nullable: true),
                    ContactName = table.Column<string>(nullable: true),
                    ContactTelNo = table.Column<string>(nullable: true)
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
                    Status = table.Column<string>(nullable: false, defaultValue: "Draft"),
                    Published = table.Column<DateTime>(nullable: true),
                    PublishScheduled = table.Column<DateTime>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: true),
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
                    Description = table.Column<string>(nullable: true),
                    ThemeId = table.Column<Guid>(nullable: false),
                    Summary = table.Column<string>(nullable: true)
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
                    TopicId = table.Column<Guid>(nullable: false),
                    ContactId = table.Column<Guid>(nullable: true)
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
                    PreviousVersionId = table.Column<Guid>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    ReleaseName = table.Column<string>(nullable: true),
                    Published = table.Column<DateTime>(nullable: true),
                    PublishScheduled = table.Column<DateTime>(nullable: true),
                    Slug = table.Column<string>(nullable: true),
                    PublicationId = table.Column<Guid>(nullable: false),
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
                name: "ReleaseFileReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: true),
                    Filename = table.Column<string>(nullable: true),
                    ReleaseFileType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseFileReferences_Releases_ReleaseId",
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

            migrationBuilder.CreateTable(
                name: "ReleaseFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    ReleaseFileReferenceId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseFiles_ReleaseFileReferences_ReleaseFileReferenceId",
                        column: x => x.ReleaseFileReferenceId,
                        principalTable: "ReleaseFileReferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseFiles_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Contacts",
                columns: new[] { "Id", "ContactName", "ContactTelNo", "TeamEmail", "TeamName" },
                values: new object[] { new Guid("6256c8e5-9754-4873-90aa-cea429ab5b6c"), "Data Analyst", "01234100100", "explore.statistics@education.gov.uk", "Test Team" });

            migrationBuilder.InsertData(
                table: "ContentSections",
                columns: new[] { "Id", "Caption", "Heading", "Order", "Type" },
                values: new object[,]
                {
                    { new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"), "", "Pupil absence rates", 1, "Generic" },
                    { new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"), "", "", 1, "ReleaseSummary" },
                    { new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"), "", "", 1, "KeyStatistics" },
                    { new Guid("30d74065-66b8-4843-9761-4578519e1394"), "", "", 1, "KeyStatisticsSecondary" },
                    { new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"), "", "", 1, "Headlines" }
                });

            migrationBuilder.InsertData(
                table: "Methodologies",
                columns: new[] { "Id", "Annexes", "Content", "InternalReleaseNote", "LastUpdated", "PublishScheduled", "Published", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"), "[{\"Id\":\"0522bb29-1e0d-455a-88ef-5887f76fb069\",\"Order\":1,\"Heading\":\"Annex A - Calculations\",\"Caption\":\"\",\"Content\":[{\"Type\":\"HtmlBlock\",\"Body\":\"<p>\\n  The following calculations are used to produce absence National Statistics:\\n</p>\\n<dl class=\\\"govuk-list\\\">\\n  <dt>Percentage of sessions missed due to overall absence</dt>\\n  <dd>(Total overall absence sessions / Total sessions possible) X 100</dd>\\n  <dt>Percentage of sessions missed due to authorised absence</dt>\\n  <dd>(Total authorised absence sessions / Total sessions possible) X 100</dd>\\n  <dt>Percentage of sessions missed due to unauthorised absence</dt>\\n  <dd>(Total unauthorised absence sessions / Total sessions possible) X 100</dd>\\n  <dt>Percentage of pupils with one or more session of absence by reason</dt>\\n  <dd>\\n    (Number of enrolments with one or more session of absence for a reason /\\n    Number of enrolments) X 100\\n  </dd>\\n  <dt>\\n    Percentage of overall absence for which persistent absentees are responsible\\n  </dt>\\n  <dd>\\n    (Overall absence sessions for persistent absentees / Total overall absence\\n    sessions) X 100\\n  </dd>\\n  <dt>\\n    Percentage of authorised absence for which persistent absentees are\\n    responsibe\\n  </dt>\\n  <dd>\\n    (Authorised absence session for persistent absentees / Total overall absence\\n    sessions) X 100\\n  </dd>\\n  <dt>\\n    Percentage of unauthorised absence for which persistent absentees are\\n    responsible\\n  </dt>\\n  <dd>\\n    (unauthorised absence sessions for persistent absentees / Total overall\\n    absence sessions) X 100\\n  </dd>\\n  <dt>Distribution of reasons for absence</dt>\\n  <dd>(Absence for this reason / Total overall absence with reasons) X 100</dd>\\n  <dt>Absence rates by reason</dt>\\n  <dd>(Absence for the reason / Total session possible) X 100</dd>\\n</dl>\\n\",\"Id\":\"8b90b3b2-f63d-4499-91aa-41ccae74e1c7\",\"Order\":0,\"Comments\":null}],\"Release\":null}]", "[{\"Id\":\"5a7fd947-d131-475d-afcd-11ab2b1ece67\",\"Order\":1,\"Heading\":\"1. Overview of absence statistics\",\"Caption\":\"\",\"Content\":[{\"Type\":\"HtmlBlock\",\"Body\":\"<h3 id=\\\"section1-1\\\">1.1 Pupil attendance requirements for schools</h3>\\n<p>\\n  All maintained schools are required to provide 2 possible sessions per day,\\n  morning and afternoon, to all pupils.\\n</p>\\n<p>\\n  The length of each session, break and the school day is determined by the\\n  school’s governing body.\\n</p>\\n<p>\\n  Schools must meet for at least 380 sessions or 190 days during any school year\\n  to educate their pupils.\\n</p>\\n<p>\\n  If a school is prevented from meeting for 1 or more sessions because of an\\n  unavoidable event, it should find a practical way of holding extra sessions.\\n</p>\\n<p>\\n  However, if it cannot find a practical way of doing this then it’s not\\n  required to make up the lost sessions.\\n</p>\\n<p>\\n  Academy and free school funding agreements state that the duration of the\\n  school day and sessions are the responsibility of the academy trust.\\n</p>\\n<p>\\n  Schools are required to take attendance registers twice a day - once at the\\n  start of the first morning session and once during the second afternoon\\n  session.\\n</p>\\n<p>\\n  In their register, schools are required to record whether pupils are:<!-- -->\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>absent</li>\\n  <li>attending an approved educational activity</li>\\n  <li>present</li>\\n  <li>unable to attend due to exceptional circumstances</li>\\n</ul>\\n<p>\\n  Where a pupil of compulsory school age is absent, schools have a\\n  responsibility to:<!-- -->\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>ascertain the reason</li>\\n  <li>ensure the proper safeguarding action is taken</li>\\n  <li>\\n    indicate in their register whether the absence is authorised by the school\\n    or unauthorised\\n  </li>\\n  <li>\\n    identify the correct code to use before entering it on to the school’s\\n    electronic register, or management information system which is then used to\\n    download data to the school census. A code set of these is available in\\n    <a href=\\\"#annex-sections-heading-2\\\">Annex B</a>\\n  </li>\\n</ul>\\n<p>\\n  The parent of every child of compulsory school age is required to ensure their\\n  child receives a suitable full-time education for their ability, age, aptitude\\n  and any special education needs they may have either by regular attendance at\\n  school or otherwise.\\n</p>\\n<p>\\n  Failure of a parent to secure regular attendance of their school registered\\n  child of compulsory school age can lead to a penalty notice or prosecution.\\n</p>\\n<p>\\n  Local authorities (LAs) and schools have legal responsibilities regarding\\n  accurate recording of a pupil’s attendance.\\n</p>\\n<p>\\n  For further information:<!-- -->\\n  <a href=\\\"https://www.gov.uk/government/publications/school-attendance\\\"\\n    >School attendance: guidance for schools</a\\n  >.<!-- -->\\n</p>\\n<h3 id=\\\"section1-2\\\">1.2 Uses and users</h3>\\n<p>\\n  The data used to publish absence statistics is collected via the school census\\n  which is used by a variety of companies and organisations including:<!-- -->\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>mainstream and specialist media companies</li>\\n  <li>housing websites such as Rightmove and Zoopla</li>\\n  <li>specialist publications such as the good schools guide</li>\\n  <li>data analysis organisations such the Fischer Family Trust</li>\\n  <li>\\n    academic research and think tank organisations such as Durham University and\\n    the Education Policy Institute\\n  </li>\\n  <li>\\n    central government organisations such as DfE, Ofsted and other government\\n    departments\\n  </li>\\n</ul>\\n<p>\\n  The published data is also used in answers to parliamentary questions and\\n  public enquiries - including those made under the Freedom of Information Act.\\n</p>\\n<h3 id=\\\"section1-3\\\">1.3 Current termly publications</h3>\\n<p>\\n  DfE publishes termly pupil absence data and statistics via the following 3\\n  National Statistics releases each year:\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>autumn term data and statistics - published in May</li>\\n  <li>autumn and spring terms data and statistics - published in October</li>\\n  <li>full year data and statistics - published in March</li>\\n</ul>\\n<p>\\n  Only the full year absence release gives a definitive view of pupil absence.\\n</p>\\n<p>\\n  Termly publications can be affected significantly by term length with findings\\n  from these releases for indicative purposes only and the results should be\\n  treated with caution.\\n</p>\\n<p>\\n  DfE’s annual absence releases have been badged as National Statistics since\\n  the 1999/00 academic year publication.\\n</p>\\n<p>\\n  The termly and two-term combined releases were badged as National Statistics\\n  slightly later.\\n</p>\\n<p>\\n  The combined autumn and spring term release was badged as national statistics\\n  from the autumn 2006 and spring 2007 publication and the single term releases\\n  were badged as National Statistics as of the autumn term 2009 publication.\\n</p>\\n<h4>Historical publications</h4>\\n<p>\\n  Prior to the 2012/13 academic year DfE also published spring term only absence\\n  data. However, this was discontinued as it was deemed no longer necessary and\\n  of the least importance to users.\\n</p>\\n<p>The last spring term release was published on 30 August 2012:</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>\\n    <a\\n      href=\\\"https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-spring-term-2012\\\"\\n      >Pupil absence in schools in England: spring term 2012</a\\n    >\\n  </li>\\n</ul>\\n<p>\\n  For the 2005/06 academic year, due to the transition of absence collection\\n  between the Absence in Schools Survey and the school census, absence\\n  information was published for secondary schools only as a National Statistics\\n  release.\\n</p>\\n<p>\\n  Absence data for 2005/06 were not released on a termly basis as this was the\\n  first year of collection.\\n</p>\\n<p>\\n  For academic years 1999/00 to 2004/05, annual pupil absence information was\\n  collected via the Absence in Schools Survey and published as an annual\\n  National Statistics release.\\n</p>\\n<p>\\n  For academic years 1993/94 to 1998/99, annual pupil absence information was\\n  collected via the Absence in Schools Survey and published via Statistical\\n  bulletins. Links to all absence publications can be found in<!-- -->\\n  <a href=\\\"#annex-sections-heading-3\\\">Annex C</a>.\\n</p>\\n<h3 id=\\\"section1-4\\\">1.4 Key absence measures</h3>\\n<p>\\n  DfE monitors pupil absence levels using two key measures - overall absence\\n  rate and persistent absence (PA) rate.\\n</p>\\n<p>\\n  These key measures are calculated for pupils who are of compulsory school age\\n  - ie aged between 5 and 15 as at the start of the academic year (31 August).\\n</p>\\n<p>\\n  Absence information is reported as totals or rates across a period, usually by\\n  school term or academic year.\\n</p>\\n<p>\\n  Overall absence is the aggregated total of all authorised and unauthorised\\n  absences.\\n</p>\\n<p>\\n  Authorised absence is absence with permission from a teacher or other\\n  authorised school representative - including absences where a satisfactory\\n  explanation has been provided. For example, through illness.\\n</p>\\n<p>\\n  Unauthorised absence is absence without permission from the school. This\\n  includes all unexplained or unjustified absences and arrivals after\\n  registration has closed.\\n</p>\\n<p>\\n  For further information:<!-- -->\\n  <a href=\\\"#section3-1\\\">3.1 Overall absence methodology</a>.\\n</p>\\n<p>\\n  Persistent absence is when a pupil enrolment’s overall absence equates to 10%\\n  or more of their possible sessions.\\n</p>\\n<p>\\n  For further information:<!-- -->\\n  <a href=\\\"#section3-2\\\">3.2 Persistent absence methodology</a>.\\n</p>\\n<h3 id=\\\"section1-5\\\">1.5 Cohort used in absence measures</h3>\\n<p>\\n  Absence information is collected and disseminated at enrolment level rather\\n  than pupil level.\\n</p>\\n<p>\\n  This means where a pupil has moved school throughout the year, theyll be\\n  counted more than once as they have recorded attendance and absence at more\\n  than one school.\\n</p>\\n<p>\\n  This allows for schools to be held accountable for pupil absences, as the\\n  absence is attached to enrolments at a particular school, not the individual\\n  pupil.\\n</p>\\n<p>\\n  All the enrolments at a school over the period in question are included in the\\n  absence measures, not just the pupils on roll at a particular date.\\n</p>\\n<p>\\n  Schools only record absence for the period a pupil is on roll at their school.\\n</p>\\n<p>\\n  The number of pupil enrolments is approximately 4% higher than the number of\\n  pupils.\\n</p>\\n<h3 id=\\\"section1-6\\\">1.6 The school year (five half terms vs six half terms)</h3>\\n<p>\\n  Generally, the academic year is made up of three terms - autumn, spring and\\n  summer.\\n</p>\\n<p>\\n  Each term has two parts (half-terms) which are usually separated by a half\\n  term break.\\n</p>\\n<p>\\n  Since the 2012/13 academic year, pupil absence information has been collected\\n  for the full academic year (ie all six half terms).\\n</p>\\n<p>\\n  However, prior to this absence information was collected for the first five\\n  half terms only, meaning absences in the second half of the summer term were\\n  not collected.\\n</p>\\n<p>\\n  Since the 2012/13 academic year, DfE's key absence indicators have been based\\n  on the full academic year’s (ie six half terms) data.\\n</p>\\n<p>\\n  However, as we're unable to rework time series tables or provide any\\n  historical six half term absence levels DfE continued to publish a full set of\\n  absence information for the first five half terms up to and including the\\n  2013/14 academic year.\\n</p>\\n<p>\\n  Following this, a single csv file based on data for five half terms has been\\n  published alongside the annual absence publications so longer term-time\\n  comparisons can still be made.\\n</p>\\n<p>\\n  To account for high levels of study leave and other authorised absences for\\n  pupils aged 15 in the second half of the summer term, all possible sessions\\n  and absences relating to this period for 15 year olds (as at the start of the\\n  academic year) are removed prior to any analysis being undertaken and are not\\n  included in any published statistics.\\n</p>\\n<table class=\\\"govuk-table\\\">\\n  <caption class=\\\"govuk-table__caption\\\">\\n    Table 1: State-funded primary, secondary and special schools - pupils of\\n    compulsory school age pupil and enrolment numbers comparison\\n  </caption>\\n  <thead>\\n    <tr>\\n      <th scope=\\\"col\\\" class=\\\"govuk-table__header govuk-table__header--numeric\\\">\\n        Academic year\\n      </th>\\n      <th scope=\\\"col\\\" class=\\\"govuk-table__header govuk-table__header--numeric\\\">\\n        Pupil numbers as at January each year<sup>1</sup>\\n      </th>\\n      <th scope=\\\"col\\\" class=\\\"govuk-table__header govuk-table__header--numeric\\\">\\n        Enrolment numbers across full academic year\\n      </th>\\n      <th scope=\\\"col\\\" class=\\\"govuk-table__header govuk-table__header--numeric\\\">\\n        Percentage difference\\n      </th>\\n    </tr>\\n  </thead>\\n  <tbody>\\n    <tr>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">2012/13</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,230,420</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,477,725</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">4.0</td>\\n    </tr>\\n    <tr>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">2013/14</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,300,105</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,554,005</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">4.0</td>\\n    </tr>\\n    <tr>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">2014/15</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,381,940</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,642,755</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">4.0</td>\\n    </tr>\\n    <tr>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">2015/16</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,484,725</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">6,737,190</td>\\n      <td class=\\\"govuk-table__cell govuk-table__cell--numeric\\\">3.9</td>\\n    </tr>\\n  </tbody>\\n  <tfoot>\\n    <tr>\\n      <td colspan=\\\"4\\\">\\n        <sup>1</sup> Pupils with a sole or dual main registration, aged between\\n        5 and 15 who are not boarders as of the January school census each year.\\n      </td>\\n    </tr>\\n  </tfoot>\\n</table>\\n<p>\\n  In published absence statistics, pupil enrolments who first enrolled at a\\n  school within the second half of the summer term are not included.\\n</p>\\n<p>\\n  This is to ensure the same cohorts of enrolments are included in both the five\\n  and six half term absence measures.\\n</p>\\n<h3 id=\\\"section1-7\\\">\\n  1.7 Published geographical and characteristics breakdowns\\n</h3>\\n<p>\\n  DfE routinely publishes pupil absence information at national, local authority\\n  and school level - including breakdowns by pupil characteristics.\\n</p>\\n<p>\\n  The autumn term absence publication provides high level information designed\\n  to give an early indication on absence levels and the effect of winter\\n  illness. This includes:\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>authorised, unauthorised and overall absence rates</li>\\n  <li>absence broken down by reason</li>\\n  <li>\\n    the number of pupils with one or more sessions of absence for different\\n    reasons\\n  </li>\\n  <li>information on persistent absence</li>\\n</ul>\\n<p>\\n  The combined autumn and spring term publication includes similar information\\n  to that of the autumn term. However, it also includes absence levels broken\\n  down by pupil:\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>ethnic group</li>\\n  <li>first language</li>\\n  <li>free school meal eligibility</li>\\n  <li>gender</li>\\n  <li>national curriculum year group</li>\\n  <li>special educational need</li>\\n</ul>\\n<p>\\n  The full academic year's absence publication includes combined absence #\\n  information for the autumn, spring and summer terms.\\n</p>\\n<p>\\n  It’s the largest publication and includes similar breakdowns to that of the\\n  combined autumn and spring term publication (as outlined above) as well as\\n  persistent absence broken down by reason for absence and pupil characteristic.\\n</p>\\n<p>Additional breakdowns included in this full year release relate to the:</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>distribution of enrolments by length of overall absence</li>\\n  <li>percentage of enrolments by their overall absence</li>\\n  <li>number of schools by the percentage of persistent absentees</li>\\n</ul>\\n<p>\\n  In this publication, information is also provided at district level, based on\\n  Income Deprivation Affecting Children Index (IDACI) and by degree of rurality.\\n</p>\\n<p>\\n  In addition, from 2015/16 onwards, characteristics include free school meal\\n  eligibility in the last six years.\\n</p>\\n<p>\\n  The Income Deprivation Affecting Children Index (IDACI) is provided by the\\n  Department for Communities and Local Government (CLG).\\n</p>\\n<p>\\n  The index measures the proportion of all children aged 0 to 15 living in\\n  income deprived families and is based on Lower-layer Super Output Areas\\n  (LSOAs) in England.\\n</p>\\n<p>\\n  Each LSOA is given a rank between 1 and 32,844 where the LSOA with the rank of\\n  1 is the most deprived LSOA and the LSOA with the rank of 32,844 is the most\\n  deprived.\\n</p>\\n<p>\\n  IDACI is a subset of the Income Deprivation Domain of the Index of Multiple\\n  Deprivation 2015 which measures the proportion of the population in an area\\n  experiencing deprivation relating to low income.\\n</p>\\n<p>\\n  The definition of low income used includes both those people that are\\n  out-of-work and those that are in work but who have low earnings (and who\\n  satisfy the respective means tests).\\n</p>\\n<p>\\n  For further information about IDACI:<!-- -->\\n  <a\\n    href=\\\"https://www.gov.uk/government/statistics/english-indices-of-deprivation-2015\\\"\\n    >English indices of deprivation 2015</a\\n  >.\\n</p>\\n<p>IDACI bands from 2014/15 are based on 2015 IDACI scores.</p>\\n<p>\\n  IDACI bands for 2010/11 to 2013/14 are based on 2010 IDACI scores and those\\n  for 2007/08 to 2009/10 are based on 2007 IDACI scores.\\n</p>\\n<p>\\n  Care should be taken when comparing IDACI tables based on different IDACI\\n  scores.\\n</p>\\n<p>\\n  The Rural and Urban Area Classification is a product of a joint project to\\n  produce a single and consistent classification of urban and rural areas. The\\n  project was sponsored by a number of government departments.\\n</p>\\n<p>\\n  The rural and urban definitions classify output areas, wards and super output\\n  areas by aggregating the underlying hectare grid squares classifications for\\n  the measures of settlement size and sparsity.\\n</p>\\n<p>\\n  Up to eight classes of output areas could be distinguished - four settlement\\n  types (urban, town and fringe, village, hamlet and isolated dwelling) in\\n  either a sparse or less sparse regional setting.\\n</p>\\n<p>\\n  Absence data by degree of rurality from 2014/15 has been analysed based on the\\n  2011 Rural and Urban Area Classification, whereas equivalent data for previous\\n  years was analysed based on the 2004 Rural and Urban Area Classification.\\n</p>\\n<p>\\n  For further information about Rural and Urban Area Classification 2011:<!-- -->\\n  <a\\n    href=\\\"https://www.gov.uk/government/statistics/2011-rural-urban-classification-of-local-authority-and-other-higher-level-geographies-for-statistical-purposes\\\"\\n    >2011 Rural-Urban Classification of Local Authorities and other\\n    geographies</a\\n  >.\\n</p>\\n<p>\\n  A full list of published absence breakdowns (as of the latest academic year's\\n  releases) is available in<!-- -->\\n  <a href=\\\"#annex-sections-heading-4\\\">Annex D</a>.\\n</p>\\n<p>\\n  From 2015/16 onwards, published tables on characteristics breakdowns include\\n  figures for pupils with unclassified or missing characteristics information.\\n</p>\\n<p>\\n  This represents a small proportion of all pupils and the figures should be\\n  interpreted with caution.\\n</p>\\n<p>\\n  For some characteristics, like free school meals eligibility, pupils with\\n  unclassified or missing characteristics information have been found to have a\\n  low average number of sessions possible, which might explain more variability\\n  in absence rates which use the number of possible sessions as a denominator.\\n</p>\\n<h3 id=\\\"section1-8\\\">1.8 Underlying data provided alongside publications</h3>\\n<p>\\n  From the 2009/10 academic year, each National Statistics release has been\\n  accompanied by underlying data, including national, local authority and school\\n  level information.\\n</p>\\n<p>\\n  Alongside the underlying data there's an accompanying document (metadata)\\n  which provides further information on the contents of these files.\\n</p>\\n<p>\\n  This data is released under the terms of the<!-- -->\\n  <a\\n    href=\\\"http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/\\\"\\n    >Open Government License</a\\n  >\\n  <!-- -->and is intended to meet at least 3 stars for<!-- -->\\n  <a\\n    href=\\\"https://www.gov.uk/government/publications/2010-to-2015-government-policy-government-transparency-and-accountability/2010-to-2015-government-policy-government-transparency-and-accountability#appendix-3-releasing-data-in-open-and-anonymised-formats\\\"\\n    >Open Data</a\\n  >.\\n</p>\\n<p>\\n  Following the<!-- -->\\n  <a\\n    href=\\\"https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/467936/Absence_statistics_changes_-_consultation_response.pdf\\\"\\n    >Consultation on improvements to pupil absence statistics</a\\n  >, results published in October 2015, releases are now accompanied by time\\n  series underlying data, containing additional breakdowns and data from 2006/07\\n  to the latest year.\\n</p>\\n<p>\\n  This additional data is intended to provide users with all information in one\\n  place and give them the option of producing their own analysis.\\n</p>\\n<h3 id=\\\"section1-9\\\">1.9 Suppression of absence data</h3>\\n<p>\\n  The<!-- -->\\n  <a href=\\\"https://www.statisticsauthority.gov.uk/code-of-practice/the-code/\\\"\\n    >Code of Practice for Statistics</a\\n  >\\n  <!-- -->requires reasonable steps are taken to ensure all published or\\n  disseminated statistics produced by DfE protect confidentiality.<!-- -->\\n</p>\\n<p>\\n  To do this totals are rounded and small numbers are suppressed according to\\n  the following rules:<!-- -->\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>\\n    enrolment numbers at national and regional levels are rounded to the nearest\\n    5. Local authority totals across school types are also rounded to the\\n    nearest 5 to prevent disclosure of any supressed values\\n  </li>\\n  <li>\\n    enrolment numbers of 1 or 2 are suppressed to protect pupil confidentiality\\n  </li>\\n  <li>\\n    where the numerator or denominator of any percentage calculated on enrolment\\n    numbers of 1 or 2, the percentage is suppressed consistent with<!-- -->\\n    <a\\n      href=\\\"http://media.education.gov.uk/assets/files/policy%20statement%20on%20confidentiality.pdf\\\"\\n      >DfE's Statistical Policy Statement on Confidentiality</a\\n    >\\n  </li>\\n  <li>where any number is shown as 0 - the original figure was also 0</li>\\n</ul>\\n<table class=\\\"govuk-table\\\">\\n  <thead>\\n    <tr>\\n      <th colspan=\\\"2\\\">Symbols used to identify this in published tables:</th>\\n    </tr>\\n  </thead>\\n  <tbody>\\n    <tr>\\n      <td>0</td>\\n      <td>Zero</td>\\n    </tr>\\n    <tr>\\n      <td>x</td>\\n      <td>Small number suppressed to preserve confidentiality</td>\\n    </tr>\\n    <tr>\\n      <td>.</td>\\n      <td>Not applicable</td>\\n    </tr>\\n    <tr>\\n      <td>..</td>\\n      <td>Not available</td>\\n    </tr>\\n  </tbody>\\n</table>\\n<h3 id=\\\"section1-10\\\">1.10 Other related publications</h3>\\n<p>\\n  Pupil absence information is also available in the following publications:\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>\\n    other National Statistics releases published by DfE:\\n    <ul class=\\\"govuk-list govuk-list--bullet\\\">\\n      <li>\\n        <a\\n          href=\\\"https://www.gov.uk/government/collections/statistics-children-in-need\\\"\\n          >Statistics: children in need and child protection</a\\n        >\\n      </li>\\n      <li>\\n        <a\\n          href=\\\"https://www.gov.uk/government/collections/statistics-looked-after-children\\\"\\n          >Statistics: looked-after children</a\\n        >\\n      </li>\\n      <li>\\n        <a href=\\\"https://www.compare-school-performance.service.gov.uk/\\\"\\n          >school and college performance tables published via Find and compare\\n          schools in England</a\\n        >\\n      </li>\\n    </ul>\\n  </li>\\n  <li>\\n    other reports published by DfE:\\n    <ul class=\\\"govuk-list govuk-list--bullet\\\">\\n      <li>\\n        <a\\n          href=\\\"https://www.gov.uk/government/publications/absence-and-attainment-at-key-stages-2-and-4-2013-to-2014\\\"\\n          >Absence and attainment at key stages 2 and 4: 2013 to 2014</a\\n        >\\n      </li>\\n    </ul>\\n  </li>\\n</ul>\\n<p>\\n  In addition historical pupil absence data is available in the following\\n  discontinued publications:\\n</p>\\n<ul class=\\\"govuk-list govuk-list--bullet\\\">\\n  <li>\\n    <a\\n      href=\\\"https://www.gov.uk/government/collections/statistics-special-educational-needs-sen\\\"\\n      >Statistics: special educational needs (SEN)</a\\n    >\\n    <!-- -->- DfE data up to and including 2013/14\\n  </li>\\n  <li>\\n    <a href=\\\"https://www.ons.gov.uk/help/localstatistics\\\"\\n      >Neighbourhood Statistics (NeSS)</a\\n    >\\n    <!-- -->- ONS small area tables - 2006/07 to 2012/13 inclusive\\n  </li>\\n</ul>\\n<h3 id=\\\"section1-11\\\">1.11 Devolved administration statistics on absence</h3>\\n<p>DfE collects and reports on absence information from schools in England.</p>\\n<p>For information for Wales, Scotland and Northern Ireland:</p>\\n<h3 class=\\\"govuk-heading-s\\\">Wales</h3>\\n<ul class=\\\"govuk-list-bullet\\\">\\n  <li>\\n    email:<!-- -->\\n    <a href=\\\"mailto:school.stats@wales.gsi.gov.uk\\\"\\n      >school.stats@wales.gsi.gov.uk</a\\n    >\\n  </li>\\n  <li>\\n    visit:<!-- -->\\n    <a href=\\\"https://gov.wales/statistics-and-research\\\"\\n      >GOV.WALES – Statistics and research</a\\n    >\\n  </li>\\n</ul>\\n<h3 class=\\\"govuk-heading-s\\\">Scotland</h3>\\n<ul class=\\\"govuk-list-bullet\\\">\\n  <li>\\n    email:<!-- -->\\n    <a href=\\\"mailto:school.stats@wales.gsi.gov.uk\\\"\\n      >school.stats@wales.gsi.gov.uk</a\\n    >\\n  </li>\\n  <li>\\n    visit:<!-- -->\\n    <a href=\\\"https://www2.gov.scot/Topics/Statistics/Browse/School-Education\\\"\\n      >GOV.SCOT – School Education</a\\n    >\\n  </li>\\n</ul>\\n<h3 class=\\\"govuk-heading-s\\\">Nothern Ireland</h3>\\n<ul class=\\\"govuk-list-bullet\\\">\\n  <li>\\n    email:<!-- -->\\n    <a href=\\\"mailto:statistics@deni.gov.uk\\\">statistics@deni.gov.uk</a>\\n  </li>\\n  <li>\\n    visit:<!-- -->\\n    <a\\n      href=\\\"https://www.education-ni.gov.uk/topics/statistics-and-research/statistics\\\"\\n      >Department of Education – Statistics and research</a\\n    >\\n  </li>\\n</ul>\\n\",\"Id\":\"4d5ae97d-fa1c-4a09-a0a3-b28307fcfb09\",\"Order\":0,\"Comments\":null}],\"Release\":null}]", null, new DateTime(2019, 6, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "pupil-absence-in-schools-in-england", "", "Pupil absence statistics: methodology" });

            migrationBuilder.InsertData(
                table: "ReleaseTypes",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { new Guid("8becd272-1100-4e33-8a7d-1c0c4e3b42b8"), "National Statistics" },
                    { new Guid("1821abb8-68b0-431b-9770-0bea65d02ff0"), "Ad Hoc" },
                    { new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"), "Official Statistics" }
                });

            migrationBuilder.InsertData(
                table: "Themes",
                columns: new[] { "Id", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "pupils-and-schools", "", "Pupils and schools" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName" },
                values: new object[,]
                {
                    { new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04"), "analyst1@example.com", "Analyst1", "User1" },
                    { new Guid("6620bccf-2433-495e-995d-fc76c59d9c62"), "analyst2@example.com", "Analyst2", "User2" },
                    { new Guid("b390b405-ef90-4b9d-8770-22948e53189a"), "analyst3@example.com", "Analyst3", "User3" },
                    { new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"), "bau1@example.com", "Bau1", "User1" },
                    { new Guid("b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63"), "bau2@example.com", "Bau2", "User2" },
                    { new Guid("d5c85378-df85-482c-a1ce-09654dae567d"), "prerelease1@example.com", "Prerelease1", "User1" },
                    { new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050"), "prerelease2@example.com", "Prerelease2", "User2" }
                });

            migrationBuilder.InsertData(
                table: "ContentBlock",
                columns: new[] { "Id", "ContentSectionId", "Order", "Type", "DataBlock_Charts", "DataBlock_Heading", "DataBlock_HighlightName", "Name", "DataBlock_Query", "Source", "DataBlock_Summary", "DataBlock_Table" },
                values: new object[,]
                {
                    { new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"), new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"), 0, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"f9ae4976-7cd3-4718-834a-09349b6eb377_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}},\"Type\":\"line\",\"LegendHeight\":0,\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null}]", null, null, "Generic data block - National", "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Locations\":{\"GeographicLevel\":null,\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"PlanningArea\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null},\"IncludeGeoJson\":null}", null, null, "{\"TableHeaders\":{\"ColumnGroups\":[],\"Columns\":[{\"Level\":null,\"Value\":\"2012_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2013_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2014_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2015_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2016_AY\",\"Type\":\"TimePeriod\"}],\"RowGroups\":[[{\"Level\":\"country\",\"Value\":\"E92000001\",\"Type\":\"Location\"}]],\"Rows\":[{\"Level\":null,\"Value\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Type\":\"Indicator\"},{\"Level\":null,\"Value\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Type\":\"Indicator\"},{\"Level\":null,\"Value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Type\":\"Indicator\"}]}}" },
                    { new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"), new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"), 1, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}},\"Type\":\"line\",\"LegendHeight\":0,\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null}]", null, null, "Key Stat 1", "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2016,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"Indicators\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"Locations\":{\"GeographicLevel\":null,\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"PlanningArea\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null},\"IncludeGeoJson\":null}", null, "{\"DataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\"],\"DataSummary\":[\"Up from 4.6% in 2015/16\"],\"DataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils.\"],\"DataDefinitionTitle\":[\"What is overall absence?\"]}", "{\"TableHeaders\":{\"ColumnGroups\":[],\"Columns\":[{\"Level\":null,\"Value\":\"2016_AY\",\"Type\":\"TimePeriod\"}],\"RowGroups\":[[{\"Level\":\"country\",\"Value\":\"E92000001\",\"Type\":\"Location\"}]],\"Rows\":[{\"Level\":null,\"Value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Type\":\"Indicator\"}]}}" },
                    { new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"), new Guid("30d74065-66b8-4843-9761-4578519e1394"), 1, "DataBlock", "[{\"Legend\":\"top\",\"Labels\":{\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Unauthorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#4763a5\",\"symbol\":\"circle\",\"LineStyle\":\"solid\"},\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Overall absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#f5a450\",\"symbol\":\"cross\",\"LineStyle\":\"solid\"},\"f9ae4976-7cd3-4718-834a-09349b6eb377_183f94c3-b5d7-4868-892d-c948e256744d_cb9b57e8-9965-4cb6-b61a-acc6d34b32be_____\":{\"Label\":\"Authorised absence rate\",\"Value\":null,\"Name\":null,\"Unit\":\"%\",\"Colour\":\"#005ea5\",\"symbol\":\"diamond\",\"LineStyle\":\"solid\"}},\"Axes\":{\"major\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[{\"Indicator\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null},{\"Indicator\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"Location\":null,\"TimePeriod\":null,\"Config\":null}],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"School Year\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":null,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null},\"minor\":{\"Name\":null,\"Type\":\"major\",\"GroupBy\":\"timePeriod\",\"SortBy\":null,\"SortAsc\":true,\"DataSets\":[],\"ReferenceLines\":[],\"Visible\":true,\"Title\":\"Absence Rate\",\"Unit\":null,\"ShowGrid\":true,\"Label\":null,\"Min\":0,\"Max\":null,\"Size\":null,\"TickConfig\":\"default\",\"TickSpacing\":null}},\"Type\":\"line\",\"LegendHeight\":0,\"Title\":null,\"Alt\":null,\"Height\":0,\"Width\":null}]", null, null, "Key Stats aggregate table", "{\"SubjectId\":\"803fbf56-600f-490f-8409-6413a891720d\",\"TimePeriod\":{\"StartYear\":2012,\"StartCode\":\"AY\",\"EndYear\":2016,\"EndCode\":\"AY\"},\"Filters\":[\"183f94c3-b5d7-4868-892d-c948e256744d\",\"cb9b57e8-9965-4cb6-b61a-acc6d34b32be\"],\"BoundaryLevel\":null,\"Indicators\":[\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\"],\"Locations\":{\"GeographicLevel\":null,\"Country\":[\"E92000001\"],\"Institution\":null,\"LocalAuthority\":null,\"LocalAuthorityDistrict\":null,\"LocalEnterprisePartnership\":null,\"MultiAcademyTrust\":null,\"MayoralCombinedAuthority\":null,\"OpportunityArea\":null,\"ParliamentaryConstituency\":null,\"PlanningArea\":null,\"Region\":null,\"RscRegion\":null,\"Sponsor\":null,\"Ward\":null},\"IncludeGeoJson\":null}", null, "{\"DataKeys\":[\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\"],\"DataSummary\":[\"Up from 4.6% in 2015/16\",\"Similar to previous years\",\"Up from 1.1% in 2015/16\"],\"DataDefinition\":[\"Total number of all authorised and unauthorised absences from possible school sessions for all pupils.\",\"Number of authorised absences as a percentage of the overall school population.\",\"Number of unauthorised absences as a percentage of the overall school population.\"],\"DataDefinitionTitle\":[\"What is overall absence?\",\"What is authorized absence?\",\"What is unauthorized absence?\"]}", "{\"TableHeaders\":{\"ColumnGroups\":[],\"Columns\":[{\"Level\":null,\"Value\":\"2012_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2013_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2014_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2015_AY\",\"Type\":\"TimePeriod\"},{\"Level\":null,\"Value\":\"2016_AY\",\"Type\":\"TimePeriod\"}],\"RowGroups\":[[{\"Level\":\"country\",\"Value\":\"E92000001\",\"Type\":\"Location\"}]],\"Rows\":[{\"Level\":null,\"Value\":\"f9ae4976-7cd3-4718-834a-09349b6eb377\",\"Type\":\"Indicator\"},{\"Level\":null,\"Value\":\"ccfe716a-6976-4dc3-8fde-a026cd30f3ae\",\"Type\":\"Indicator\"},{\"Level\":null,\"Value\":\"92d3437a-0a62-4cd7-8dfb-bcceba7eef61\",\"Type\":\"Indicator\"}]}}" }
                });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "Description", "Slug", "Summary", "ThemeId", "Title" },
                values: new object[] { new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"), null, "pupil-absence", "", new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"), "Pupil absence" });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "ContactId", "DataSource", "Description", "LegacyPublicationUrl", "MethodologyId", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new Guid("6256c8e5-9754-4873-90aa-cea429ab5b6c"), "", null, null, new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"), "pupil-absence-in-schools-in-england", "", "Pupil absence in schools in England", new Guid("67c249de-1cca-446e-8ccb-dcdac542f460") });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Created", "CreatedById", "DataLastPublished", "InternalReleaseNote", "NextReleaseDate", "PreviousVersionId", "PublicationId", "PublishScheduled", "Published", "RelatedInformation", "ReleaseName", "Slug", "SoftDeleted", "Status", "TimePeriodCoverage", "TypeId", "Version" },
                values: new object[] { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new DateTime(2017, 8, 1, 23, 59, 54, 0, DateTimeKind.Utc), new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd"), null, null, "{\"Year\":\"2019\",\"Month\":\"3\",\"Day\":\"22\"}", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2018, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 4, 25, 9, 30, 0, 0, DateTimeKind.Unspecified), null, "2016", "2016-17", false, "Approved", "AY", new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"), 0 });

            migrationBuilder.InsertData(
                table: "ReleaseContentBlocks",
                columns: new[] { "ReleaseId", "ContentBlockId" },
                values: new object[,]
                {
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef") }
                });

            migrationBuilder.InsertData(
                table: "ReleaseContentSections",
                columns: new[] { "ReleaseId", "ContentSectionId" },
                values: new object[,]
                {
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("30d74065-66b8-4843-9761-4578519e1394") },
                    { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3") }
                });

            migrationBuilder.InsertData(
                table: "UserReleaseRoles",
                columns: new[] { "Id", "ReleaseId", "Role", "SoftDeleted", "UserId" },
                values: new object[,]
                {
                    { new Guid("1501265c-979b-4cd4-8a55-00bfe909a2da"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "Contributor", false, new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04") },
                    { new Guid("086b1354-473c-48bb-9d30-0ac1963dc4cb"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "Lead", false, new Guid("6620bccf-2433-495e-995d-fc76c59d9c62") },
                    { new Guid("1851e50d-04ac-4e16-911b-3df3350c589b"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "Approver", false, new Guid("6620bccf-2433-495e-995d-fc76c59d9c62") },
                    { new Guid("69860a07-91d0-49d6-973d-98830fbbedfb"), new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "PrereleaseViewer", false, new Guid("d5c85378-df85-482c-a1ce-09654dae567d") }
                });

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
                name: "IX_LegacyReleases_PublicationId",
                table: "LegacyReleases",
                column: "PublicationId");

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
                name: "IX_ReleaseFileReferences_ReleaseId",
                table: "ReleaseFileReferences",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFiles_ReleaseFileReferenceId",
                table: "ReleaseFiles",
                column: "ReleaseFileReferenceId");

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
                name: "ExternalMethodology");

            migrationBuilder.DropTable(
                name: "LegacyReleases");

            migrationBuilder.DropTable(
                name: "ReleaseContentBlocks");

            migrationBuilder.DropTable(
                name: "ReleaseContentSections");

            migrationBuilder.DropTable(
                name: "ReleaseFiles");

            migrationBuilder.DropTable(
                name: "Update");

            migrationBuilder.DropTable(
                name: "UserReleaseInvites");

            migrationBuilder.DropTable(
                name: "UserReleaseRoles");

            migrationBuilder.DropTable(
                name: "ContentBlock");

            migrationBuilder.DropTable(
                name: "ReleaseFileReferences");

            migrationBuilder.DropTable(
                name: "ContentSections");

            migrationBuilder.DropTable(
                name: "Releases");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "ReleaseTypes");

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
