using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class RestorePublicationAndReleaseGuids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("0b81528a-200c-499b-a542-3b7462966c26"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("11d0111b-cf26-416d-98c3-168bb8401fdd"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("1511d1f6-7b6e-4c8e-9002-9d3fe65e33f1"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("18af7ad3-6dc2-4dec-b438-e451ab7e1cab"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("2b4e08a5-cef0-4d79-a1a9-8412b6a4ed02"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("2c06838d-342e-419e-ab90-91b965e1e382"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("37ba6d3f-2c5e-48cc-9ec9-596140159937"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("3cc8cc9a-319b-425a-aca1-16624bda7968"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("48fe71bd-74a1-41e8-a02a-52bec450596c"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("54b9cff3-1c83-4c56-83d3-83213c6d34c1"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("5a1850d4-140f-4db3-af5e-e6d0ce3727e7"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("78a28be8-17c0-461e-baad-603d825e13de"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("83f1a9ea-3f7c-4184-ab5a-f8dc22078b17"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("8b6c8afa-0a90-4998-b157-81a7a52103d1"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("943f1242-992a-4e58-9a7a-531336d20b7c"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("a9ce8158-d0f0-4123-afe2-109789c3982b"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("bf1047be-eae0-4bed-8cd0-83f562c77cdf"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("bf39fc39-fb96-4704-b6fe-d7f7568588bf"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("bfaed167-e8db-4db8-b7f5-b00635f47ffa"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("c4578279-63b4-466d-8e60-e26e15948ed2"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("f48cce88-af53-4b1a-aac2-cc2269d80801"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("fedc1701-9b9f-421d-b549-0443922dcbd4"));

            migrationBuilder.DeleteData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("ba24b6e8-900a-4d7a-925a-c9138cf112b3"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("91fdcf75-4814-4077-b7ca-7e07f415fa30"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("e64c4fdc-6067-49f4-a698-347484eaf360"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f00aa2c8-f642-475f-8f0e-f469aa975b3f"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("18b4a3e4-bacc-4ace-9e43-7caa90b006ea"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("226bac6d-e6ae-4f0b-9dbf-a4d0e97a20fb"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("24b9005b-ed8a-4f48-8a6f-55fa23882832"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("420c8ba4-af05-465c-a8eb-abce0a369fca"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("5614a5ff-2507-4a7d-b99f-3328dd9c4068"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("5a24198d-ab19-47c6-96f2-6faca416cbb7"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("77c3370c-71ef-4b5c-958d-135982eeb5ad"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("ad285753-cdd4-42fc-9b4b-165fd9f07185"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("e0231ff8-5160-485d-85e6-9cbd29695327"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("eea2b9a1-8944-4638-b7d1-4f94d89c8594"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("254688cb-ebd8-4b4c-9ec0-8efa67e164ee"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("2bb3a68d-330c-4cc4-a584-797e30f1288f"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("74e93f2c-e009-490b-9467-3c48429dab9f"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("9c0637ec-db61-46ee-9b63-86349c19a24e"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("c5ca8fa6-8cbb-4126-ae83-c6e2c1babcfc"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3a9b9782-64e9-4c47-8996-0d6503fac26b"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"));

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("d63daa75-5c3e-48bf-a232-f232e0d13898"), null, null, null, "30-hours-free-childcare", "", "30 hours free childcare", new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"), null, null, null, "early-years-foundation-stage-profile-results", "", "Early years foundation stage profile results", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), null, null, null, "permanent-and-fixed-period-exclusions-in-england", "", "Permanent and fixed-period exclusions in England", new Guid("77941b7d-bbd6-4069-9107-565af89e2dec") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)", null, new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "pupil-absence-in-schools-in-england", "", "Pupil absence in schools in England", new Guid("67c249de-1cca-446e-8ccb-dcdac542f460") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), null, null, null, "school-pupils-and-their-characteristics", "", "School and pupils and their characteristics", new Guid("e50ba9fd-9f19-458c-aceb-4422f0c7d1ba") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"), null, null, null, "secondary-and-primary-schools-applications-and-offers", "", "Secondary and primary schools applications and offers", new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), null, null, null, "gcse-and-equivalent-results", "", "GCSE and equivalent results", new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b") });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"), "2008 to 2009", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("313435b3-fe56-4b92-8e13-670dbf510062"), "January 2017", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("e3c1db23-8a8f-47fe-b2cd-8e677db700a2"), "January 2016", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("5e244416-6f2a-4d22-bea4-c22a229befef"), "January 2015", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("398ba8c6-3ea0-49da-8645-ceb3c7fb9860"), "January 2014", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("e6b36ee8-ef66-4864-a4b3-9047ee3da338"), "January 2013", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("181ec43e-cf22-4cab-a128-0a5702468566"), "January 2012", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("b086ba70-703c-40dd-aaef-d2e19335188e"), "January 2011", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("dc8b0d8c-08bb-47cc-b3a1-9e6ac9c2c268"), "January 2010", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"), "2014 to 2015", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"), "2012 to 2013", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"), "2013 to 2014", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"), "2010 to 2011", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"), "2009 to 2010", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"), "2015 to 2016", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"), "2014 to 2015", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"), "2013 to 2014", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"), "2012 to 2013", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"), "2011 to 2012", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"), "2010 to 2011", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"), "2009 to 2010", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"), "2011 to 2012", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics" });

            migrationBuilder.InsertData(
                table: "Methodologies",
                columns: new[] { "Id", "Annexes", "Content", "PublicationId", "Published", "Summary", "Title" },
                values: new object[] { new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"), "[{\"Order\":1,\"Heading\":\"Annex A - Glossary\",\"Caption\":\"\",\"Content\":[]},{\"Order\":2,\"Heading\":\"Annex B - Calculations\",\"Caption\":\"\",\"Content\":[]},{\"Order\":3,\"Heading\":\"Annex C - School attendance codes\",\"Caption\":\"\",\"Content\":[]},{\"Order\":4,\"Heading\":\"Annex D - Links to pupil absence national statistics and data\",\"Caption\":\"\",\"Content\":[]},{\"Order\":5,\"Heading\":\"Annex E - Standard breakdowns\",\"Caption\":\"\",\"Content\":[]},{\"Order\":6,\"Heading\":\"Annex F - Timeline\",\"Caption\":\"\",\"Content\":[]}]", "[{\"Order\":1,\"Heading\":\"1. Overview of absence statistics\",\"Caption\":\"\",\"Content\":[]},{\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[]},{\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[]},{\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[]},{\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[]},{\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[]},{\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[]}]", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Find out about the methodology behind pupil absence statistics and data and how and why they're collected and published.", "Pupil absence statistics: methodology" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This statistical publication provides the number of schools and pupils in schools in England, using data from the January 2018School Census.\\n\\n Breakdowns are given for school types as well as for pupil characteristics including free school meal eligibility, English as an additional languageand ethnicity.This release also contains information about average class sizes.\\n\\n SEN tables previously provided in thispublication will be published in the statistical publication ‘Special educational needs in England: January 2018’ scheduled for release on 26July 2018.\\n\\n Cross border movement tables will be added to this publication later this year.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}", new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), new DateTime(2018, 5, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "January 2018", "january-2018", "Statistics on pupils in schools in England as collected in the January 2018 school census.", "Schools, pupils and their characteristics: January 2018" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This statistical first release (SFR) reports on absence of pupils of compulsory school age in state-funded primary, secondary and special schools during the 2016/17 academic year. Information on absence in pupil referral units, and for pupils aged four, is also included. The Department uses two key measures to monitor pupil absence – overall and persistent absence. Absence by reason and pupils characteristics is also included in this release. Figures are available at national, regional, local authority and school level. Figures held in this release are used for policy development as key indicators in behaviour and school attendance policy. Schools and local authorities also use the statistics to compare their local absence rates to regional and national averages for different pupil groups.\"}]},{\"Order\":2,\"Heading\":\"Absence rates\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The overall absence rate across state-funded primary, secondary and special schools increased from 4.6 per cent in 2015/16 to 4.7 per cent in 2016/17. In primary schools the overall absence rate stayed the same at 4 per cent and the rate in secondary schools increased from 5.2 per cent to 5.4 per cent. Absence in special schools is much higher at 9.7 per cent in 2016/17\\n\\nThe increase in overall absence rate has been driven by an increase in the unauthorised absence rate across state-funded primary, secondary and special schools - which increased from 1.1 per cent to 1.3 per cent between 2015/16 and 2016/17.\\n\\nLooking at longer-term trends, overall and authorised absence rates have been fairly stable over recent years after decreasing gradually between 2006/07 and 2013/14. Unauthorised absence rates have not varied much since 2006/07, however the unauthorised absence rate is now at its highest since records began, at 1.3 per cent.\\n\\nThis increase in unauthorised absence is due to an increase in absence due to family holidays that were not agreed by the school. The authorised absence rate has not changed since last year, at 3.4 per cent. Though in primary schools authorised absence rates have been decreasing across recent years.\\n\\nThe total number of days missed due to overall absence across state-funded primary, secondary and special schools has increased since last year, from 54.8 million in 2015/16 to 56.7 million in 2016/17. This partly reflects the rise in the total number of pupil enrolments, the average number of days missed per enrolment has increased very slightly from 8.1 days in 2015/16 to 8.2 days in 2016/17.\\n\\nIn 2016/17, 91.8 per cent of pupils in state-funded primary, state-funded secondary and special schools missed at least one session during the school year, this is similar to the previous year (91.7 per cent in 2015/16).\"},{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2017\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":[{\"Indicators\":[\"23\",\"26\",\"28\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"Absence Rate\"},\"Type\":\"line\"}],\"Summary\":null,\"Tables\":[{\"indicators\":[\"23\",\"26\",\"28\"]}]}]},{\"Order\":3,\"Heading\":\"Persistent absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The percentage of enrolments in state-funded primary and state-funded secondary schools that were classified as persistent absentees in 2016/17 was 10.8 per cent. This is up from the equivalent figure of 10.5 per cent in 2015/16 (see Figure 2).\\n\\nIn 2016/17, persistent absentees accounted for 37.6 per cent of all absence compared to 36.6 per cent in 2015/16. Longer term, there has been a decrease in the proportion of absence that persistent absentees account for – down from 43.3 per cent in 2011/12.\\n\\nThe overall absence rate for persistent absentees across all schools was 18.1 per cent, nearly four times higher than the rate for all pupils. This is a slight increase from 2015/16, when the overall absence rate for persistent absentees was 17.6 per cent.\\n\\nPersistent absentees account for almost a third, 31.6 per cent, of all authorised absence and more than half, 53.8 per cent of all unauthorised absence. The rate of illness absences is almost four times higher for persistent absentees compared to other pupils, at 7.6 per cent and 2.0 per cent respectively.\"}]},{\"Order\":4,\"Heading\":\"Reasons for absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":null,\"Body\":\"Within this release absence by reason is broken down in three different ways:\\n\\nDistribution of absence by reason: The proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences.\\n\\nRate of absence by reason: The rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions.\\n\\nOne or more sessions missed due to each reason: The number of pupil enrolments missing at least one session due to each reason.\"}]},{\"Order\":5,\"Heading\":\"Distribution of absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Nearly half of all pupils (48.9 per cent) were absent for five days or fewer across state-funded primary, secondary and special schools in 2016/17, down from 49.1 per cent in 2015/16.\\n\\n4.3 per cent of pupil enrolments had more than 25 days of absence in 2016/17 (the same as in 2015/16). These pupil enrolments accounted for 23.5 per cent of days missed. 8.2 per cent of pupil enrolments had no absence during 2016/17.\\n\\nPer pupil enrolment, the average total absence in primary schools was 7.2 days, compared to 16.9 days in special schools and 9.3 days in secondary schools.\\n\\nWhen looking at absence rates across terms for primary, secondary and special schools, the overall absence rate is lowest in the autumn term and highest in the summer term. The authorised rate is highest in the spring term and lowest in the summer term, and the unauthorised rate is highest in the summer term.\"}]},{\"Order\":6,\"Heading\":\"Absence by pupil characteristics\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The patterns of absence rates for pupils with different characteristics have been consistent across recent years.\\n\\n### Gender\\n\\nThe overall absence rates across state-funded primary, secondary and special schools were very similar for boys and girls, at 4.7 per cent and 4.6 per cent respectively. The persistent absence rates were also similar, at 10.9 per cent for boys and 10.6 per cent for girls.\\n\\n### Free school meals (FSM) eligibility\\n\\nAbsence rates are higher for pupils who are known to be eligible for and claiming free school meals. The overall absence rate for these pupils was 7.3 per cent, compared to 4.2 per cent for non FSM pupils. The persistent absence rate for pupils who were eligible for FSM was more than twice the rate for those pupils not eligible for FSM.\\n\\n### National curriculum year group\\n\\nPupils in national curriculum year groups 3 and 4 had the lowest overall absence rates at 3.9 and 4 per cent respectively. Pupils in national curriculum year groups 10 and 11 had the highest overall absence rate at 6.1 per cent and 6.2 per cent respectively. This trend is repeated for persistent absence.\\n\\n### Special educational need (SEN)\\n\\nPupils with a statement of special educational needs (SEN) or education healthcare plan (EHC) had an overall absence rate of 8.2 per cent compared to 4.3 per cent for those with no identified SEN. The percentage of pupils with a statement of SEN or an EHC plan that are persistent absentees was more than two times higher than the percentage for pupils with no identified SEN.\\n\\n### Ethnic group\\n\\nThe highest overall absence rates were for Traveller of Irish Heritage and Gypsy/ Roma pupils at 18.1 per cent and 12.9 per cent respectively. Overall absence rates for pupils of a Chinese and Black African ethnicity were substantially lower than the national average of 4.7 per cent at 2.4 per cent and 2.9 per cent respectively. A similar pattern is seen in persistent absence rates; Traveller of Irish heritage pupils had the highest rate at 64 per cent and Chinese pupils had the lowest rate at 3.1 per cent.\"}]},{\"Order\":7,\"Heading\":\"Absence for four year olds\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The overall absence rate for four year olds in 2016/17 was 5.1 per cent which is lower than the rate of 5.2 per cent which it has been for the last two years.\\n\\nAbsence recorded for four year olds is not treated as 'authorised' or 'unauthorised' and is therefore reported as overall absence only.\"}]},{\"Order\":8,\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The overall absence rate for pupil referral units in 2016/17 was 33.9 per cent, compared to 32.6 per cent in 2015/16. The percentage of enrolments in pupil referral units who were persistent absentees was 73.9 per cent in 2016/17, compared to 72.5 per cent in 2015/16.\"}]},{\"Order\":9,\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"There is variation in overall and persistent absence rates across state-funded primary, secondary and special schools by region and local authority. Similarly to last year, the three regions with the highest overall absence rate across all state-funded primary, secondary and special schools are the North East (4.9 per cent), Yorkshire and the Humber (4.9 per cent) and the South West (4.8 per cent), with Inner and Outer London having the lowest overall absence rate (4.4 per cent). The region with the highest persistent absence rate is Yorkshire and the Humber, where 11.9 per cent of pupil enrolments are persistent absentees, with Outer London having the lowest rate of persistent absence (at 10.0 per cent).\\n\\nAbsence information at local authority district level is also published within this release, in the accompanying underlying data files.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2016\",\"endYear\":\"2017\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"23\",\"26\",\"28\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days \\n  * overall and unauthorised absence rates up on previous year \\n * unauthorised rise due to higher rates of unauthorised holidays \\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":null}", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas. 

", "Pupil absence data and statistics for schools in England" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":null},{\"Order\":2,\"Heading\":\"Absence rates\",\"Caption\":\"\",\"Content\":null},{\"Order\":3,\"Heading\":\"Persistent absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":4,\"Heading\":\"Distribution of absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":5,\"Heading\":\"Absence for four year olds\",\"Caption\":\"\",\"Content\":null},{\"Order\":6,\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":7,\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\",\"Content\":null}]", "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}", new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2015 to 2016", "2015-16", "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.", "Pupil absence data and statistics for schools in England" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This National Statistics release reports on permanent and fixed period exclusions from state-funded primary, state-funded secondary and special schools during the 2016/17 academic year as reported in the School Census. This release also includes school level exclusions figures for state-funded primary, secondary and special schools and national level figures on permanent and fixed-period exclusions from pupil referral units. All figures in this release are based on unrounded data; therefore, constituent parts may not add up due to rounding.\\n\\nAn Exclusions statistics guide, which provides historical information on exclusion statistics, technical background information to the figures and data collection, and definitions of key terms should be referenced alongside this release.\\n\\nIn this publication: The following tables are included in the statistical publication\\n\\n*   national tables (Excel .xls and open format)\\n\\n*   local authority (LA) tables\\n\\n*   underlying data (open format .csv and metadata .txt)\\n\\nThe underlying data is accompanied by a metadata document that describes underlying data files.\\n\\nWe welcome feedback on any aspect of this document at [schools.statistics@education.gov.uk](#)\"}]},{\"Order\":2,\"Heading\":\"Permanent exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":\"Permanent exclusion rate definition\",\"Body\":\"A permanent exclusion refers to a pupil who is excluded and who will not come back to that school (unless the exclusion is overturned). The number of permanent exclusions across all state-funded primary, secondary and special schools has increased from 6,685 in 2015/16 to 7,720 in 2016/17. This corresponds to around 40.6 permanent exclusions per day in 2016/17, up from an average of 35.2 per day in 2015/16.\"},{\"Type\":\"DataBlock\",\"Heading\":\"Chart showing permanent exclusions in England\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"The rate of permanent exclusions across all state-funded primary, secondary and special schools has also increased from 0.08 per cent to 0.10 per cent of pupil enrolments, which is equivalent to around 10 pupils per 10,000.\\n\\nMost (83 per cent) permanent exclusions occurred in secondary schools. The rate of permanent exclusions in secondary schools increased from 0.17 per cent in 2015/16 to 0.20 per cent in 2016/17, which is equivalent to around 20 pupils per 10,000.\\n\\nThe rate of permanent exclusions also rose in primary schools, at 0.03 per cent, but decreased in special schools from 0.08 per cent in 2015/16 to 0.07 per cent in 2016/17. Looking at longer-term trends, the rate of permanent exclusions across all state-funded primary, secondary and special schools followed a generally downward trend from 2006/07 when the rate was 0.12 per cent until 2012/13, and has been rising again since then, although rates are still lower now than in 2006/07.\"}]},{\"Order\":3,\"Heading\":\"Fixed-period exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":\"Fixed-period exclusion rate definition\",\"Body\":\"Fixed-period exclusion refers to a pupil who is excluded from a school for a set period of time. A fixed-period exclusion can involve a part of the school day and it does not have to be for a continuous period. A pupil may be excluded for one or more fixed periods up to a maximum of 45 school days in a single academic year. This total includes exclusions from previous schools covered by the exclusion legislation. A pupil may receive more than one fixed-period exclusion, so pupils with repeat exclusions can inflate fixed-period exclusion rates.\"},{\"Type\":\"DataBlock\",\"Heading\":\"Chart showing fixed-period exclusions in England\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"The number of fixed-period exclusions across all state-funded primary, secondary and special schools has increased from 339,360 in 2015/16 to 381,865 in 2016/17. This corresponds to around 2,010 fixed-period exclusions per day1 in 2016/17, up from an average of 1,786 per day in 2015/16.\\n\\nThere were increases in the number and rate of fixed-period exclusions for state-funded primary and secondary schools and special schools:\"}]},{\"Order\":4,\"Heading\":\"Number and length of fixed-period exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":\"Enrolments with one or more fixed-period exclusion definition\",\"Body\":\"Pupils with one or more fixed-period exclusion refer to pupil enrolments that had at least one fixed-period exclusion across the full academic year. It includes those with repeated fixed-period exclusions.\"},{\"Type\":\"MarkDownBlock\",\"Body\":\"In state-funded primary, secondary and special schools, there were 183,475 pupil enrolments, 2.29 per cent, with at least one fixed term exclusion in 2016/17, up from 167,125 pupil enrolments, 2.11 per cent, in 2015/16.\\n\\nOf those pupils with at least one fixed-period exclusion, 59.1 per cent were excluded only on one occasion, and 1.5 per cent received 10 or more fixed-period exclusions during the year. The percentage of pupils with at least one fixed-period exclusion that went on to receive a permanent one was 3.5 per cent.\\n\\nThe average length of fixed-period exclusions across state-funded primary, secondary and special schools in 2016/17 was 2.1 days, slightly shorter than in 2015/16.\\n\\nThe highest proportion of fixed-period exclusions (46.6 per cent) lasted for only one day. Only 2.0 per cent of fixed-period exclusions lasted for longer than one week and longer fixed-period exclusions were more prevalent in secondary schools.\"}]},{\"Order\":5,\"Heading\":\"Reasons for exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Persistent disruptive behaviour remained the most common reason for permanent exclusions in state-funded primary, secondary and special schools - accounting for 2,755 (35.7 per cent) of all permanent exclusions in 2016/17. This is equivalent to 3 permanent exclusions per 10,000 pupils. However, in special schools alone, the most common reason for exclusion was physical assault against and adult, which made up 37.8 per cent of all permanent exclusions and 28.1 per cent of all fixed-period exclusions.\\n\\nAll reasons except bullying and theft saw an increase in permanent exclusions since last year. The most common reasons - persistent disruptive behaviour, physical assault against a pupil and other reasons had the largest increases.\\n\\nPersistent disruptive behaviour is also the most common reason for fixed-period exclusions. The 108,640 fixed-period exclusions for persistent disruptive behaviour in state-funded primary, secondary and special schools made up 28.4 per cent of all fixed-period exclusions, up from 27.7 per cent in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.\\n\\nAll reasons saw an increase in fixed-period exclusions since last year. Persistent disruptive behaviour and other reasons saw the biggest increases.\"}]},{\"Order\":6,\"Heading\":\"Exclusions by pupil characteristics\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"In 2016/17 we saw a similar pattern by pupil characteristics to previous years. The groups that we usually expect to have higher rates are the ones that have increased exclusions since last year e.g. boys, pupils with special educational needs, pupils known to be eligible for and claiming free school meals and national curriculum years 9 and 10.\\n\\n**Age, national curriculum year group and gender**\\n\\n*   Over half of all permanent (57.2 per cent) and fixed-period (52.6 per cent) exclusions occur in national curriculum year 9 or above.\\n\\n*   A quarter (25.0 per cent) of all permanent exclusions were for pupils aged 14, and pupils of this age group also had the highest rate of fixed-period exclusion, and the highest rate of pupils receiving one or more fixed-period exclusion.\\n\\n*   The permanent exclusion rate for boys (0.15 per cent) was over three times higher than that for girls (0.04 per cent) and the fixed-period exclusion rate was almost three times higher (6.91 compared with 2.53 per cent).\\n\\n**Free school meals (FSM) eligibility**\\n\\n*   Pupils known to be eligible for and claiming free school meals (FSM) had a permanent exclusion rate of 0.28 per cent and fixed period exclusion rate of 12.54 per cent - around four times higher than those who are not eligible (0.07 and 3.50 per cent respectively).\\n\\n*   Pupils known to be eligible for and claiming free school meals (FSM) accounted for 40.0 per cent of all permanent exclusions and 36.7 per cent of all fixed-period exclusions. Special educational need (SEN)\\n\\n*   Pupils with identified special educational needs (SEN) accounted for around half of all permanent exclusions (46.7 per cent) and fixed-period exclusions (44.9 per cent).\\n\\n*   Pupils with SEN support had the highest permanent exclusion rate at 0.35 per cent. This was six times higher than the rate for pupils with no SEN (0.06 per cent).\\n\\n*   Pupils with an Education, Health and Care (EHC) plan or with a statement of SEN had the highest fixed-period exclusion rate at 15.93 per cent - over five times higher than pupils with no SEN (3.06 per cent).\\n\\n**Ethnic group**\\n\\n*   Pupils of Gypsy/Roma and Traveller of Irish Heritage ethnic groups had the highest rates of both permanent and fixed-period exclusions, but as the population is relatively small these figures should be treated with some caution.\\n\\n*   Black Caribbean pupils had a permanent exclusion rate nearly three times higher (0.28 per cent) than the school population as a whole (0.10 per cent). Pupils of Asian ethnic groups had the lowest rates of permanent and fixed-period exclusion.\"}]},{\"Order\":7,\"Heading\":\"Independent exclusion reviews\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Independent review Panel definition: Parents (and pupils if aged over 18) are able to request a review of a permanent exclusion. An independent review panel’s role is to review the decision of the governing body not to reinstate a permanently excluded pupil. The panel must consider the interests and circumstances of the excluded pupil, including the circumstances in which the pupil was excluded and have regard to the interests of other pupils and people working at the school.\\n\\nIn 2016/17 in maintained primary, secondary and special schools and academies there were 560 reviews lodged with independent review panels of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement.\"}]},{\"Order\":8,\"Heading\":\"Exclusions from pupil referral units\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The rate of permanent exclusion in pupil referral units decreased from 0.14 per cent in 2015/16 to 0.13 in 2016/17. After an increase from 2013/14 to 2014/15, permanent exclusions rates have remained fairly steady. There were 25,815 fixed-period exclusions in pupil referral units in 2016/17, up from 23,400 in 2015/16. The fixed period exclusion rate has been steadily increasing since 2013/14.\\n\\nThe percentage of pupil enrolments in pupil referral units who one or more fixed-period exclusion was 59.17 per cent in 2016/17, up from 58.15 per cent in 2015/16.\"}]},{\"Order\":9,\"Heading\":\"Exclusions by local authority\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"There is considerable variation in the permanent and fixed-period exclusion rate at local authority level (see accompanying maps on the web page).\\n\\nThe regions with the highest overall rates of permanent exclusion across state-funded primary, secondary and special schools are the West Midlands and the North West (at 0.14 per cent). The regions with the lowest rates are the South East (at 0.06 per cent) and Yorkshire and the Humber (at 0.07 per cent).\\n\\nThe region with the highest fixed-period exclusion rate is Yorkshire and the Humber (at 7.22 per cent), whilst the lowest rate was seen in Outer London (3.49 per cent).\\n\\nThese regions also had the highest and lowest rates of exclusion in the previous academic year.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"perm_excl_rate\",\"perm_excl\",\"fixed_excl_rate\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall rate of permanent exclusions has increased from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17 \\n * number of exclusions has also increased, from 6,685 to 7,720 \\n * overall rate of fixed period exclusions increased, from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17 \\n * number of exclusions has also increased, from 339,360 to 381,865. \\n\"}},\"Tables\":null}", new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of permanent and fixed-period exclusion subject areas. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-heading-9)", "Permanent and fixed period exclusions" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This release shows results for GCSE and equivalent Key Stage 4 (KS4) qualifications in 2018 across a range of measures, broken down by pupil characteristics and education institutions. Results are also provided on schools below the floor standards and meeting the coasting definition.  \\n\\nThis is an update to Provisional figures released in October 2018. Users should be careful when comparing headline measures to results in previous years given recent methodological changes \\n\\nFigures are available at national, regional, local authority, and school level. Figures held in this release are used for policy development and count towards the secondary performance tables. Schools and local authorities also use the statistics to compare their local performance to regional and national averages for different pupil groups.\"}]},{\"Order\":2,\"Heading\":\"School performance for 2018\",\"Caption\":\"School performance for 2018 shows small increases across all headline measures compared to 2017\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Average headline performance measures over time\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Results for 2018 show an increases across all headline measures compared to 2017. **When drawing comparison over time, however, it is very important to note any changes to methodology or data changes underpinning these measures**. For example, changes in Attainment 8 may have been affected by the introduction of further reformed GCSEs graded on the 9-1 scale which have a higher maximum score than unreformed GCSEs. Similarly, in 2016 there were significant changes to the Attainment in English and Maths measure. \\n\\nThese results cover state-funded schools but results for all schools are available in the supporting tables and show slightly lower performance across all headline measures on average. Differences between the figures for all schools and state-funded schools are primarily due to the impact of unapproved and unregulated qualifications such as international GCSEs taken more commonly in independent schools. These qualification are not included in school performance tables. \\n\\nThere are five primary headline measures used throughout this report: \\n * **Attainment8** - measures the average achievement of pupils in up to 8 qualifications (including English and Maths). \\n * **Attainment in English & Maths (9-5)** - measures the percentage of pupils achieving a grade 5 or above in both English and maths.\\n * **EBacc Entries** – measure the percentage of pupils reaching the English Baccalaureate (EBacc) attainment threshold in core academic subjects at key stage 4. The EBacc is made up of English, maths, science, a language, and history or geography. \\n * **EBacc Average Point Score (APS)** – measures pupils’ point scores across the five pillars of the EBacc, ensuring the attainment of all pupils is recognised. New measure from 2018, replacing the previous threshold EBacc attainment measure. \\n * **Progress** - measures the progress a pupil makes from the end of key stage 2 to the end of key stage 4. It compares pupils’ Attainment 8 score with the average for all pupils nationally who had a similar starting point. Progress 8 is a relative measure, therefore the national average Progress 8 score for mainstream schools is very close to zero. \"}]},{\"Order\":3,\"Heading\":\"Schools meeting the coasting and floor standard\",\"Caption\":\"Over 250 schools failed to support pupils to fulfil their potential in 2018\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"There is wide variation in the percentage of schools meeting the coasting and floor standard by region\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"The floor and coasting standards give measures of whether schools are helping pupils to fulfil their potential based on progress measures. The floor standard is based on results in the most recent year, whereas the Coasting definition looks at slightly different measures over the past three years. Only state-funded mainstream schools are covered by these measures, subject to certain eligibility criteria. \\n* **11.6%** of eligible schools were below the floor standard in 2018. This represents 346 schools\\n* **9.2%** of eligible schools met the coasting definition in 2018. This represents 257 schools \\n* **161** schools were both coating and below the floor standard \\n* due to methodological changes no directly comparable measures exist for previous years \\n\"}]},{\"Order\":4,\"Heading\":\"Pupil characteristics\",\"Caption\":\"Disadvantaged pupils and those with Special Education Needs continue to do less well than their peers\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Average headline scores by pupil characteristics\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Breakdowns by pupil characteristics show that across all headline measures: \\n* girls continue to do better than boys \\n* non-disadvantaged pupils continue to do better than disadvantaged pupils \\n* pupils with no identified Special Educational Needs (SEN) continue to do better perform than SEN pupils \\nIn general the pattern of attainment gaps for Attainment 8 in 2018 remained the same as in 2017 although differences in Attainment 8 scores widened slightly across all groups. This is to be expected due to changes to reformed GCSEs in 2018, meaning more points are available for higher scores.  \\n\\nDue to changes in performance measures over time, comparability over time is complicated. As such, for disadvantaged pupils is recommended to use to disadvantage gap index instead with is more resilient to changes in grading systems over time. The gap between disadvantaged pupils and others, measured using the gap index, has remained broadly stable, widening by 0.6% in 2018, and narrowing by 9.5% since 2011.\"},{\"Type\":\"DataBlock\",\"Heading\":\"Disadvantage attainment gap index\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null}]},{\"Order\":5,\"Heading\":\"Headline performance\",\"Caption\":\"Results across headline performance measures vary by ethnicity\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Average headline scores by pupil ethnicity\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Results across headline measures differ by ethnicity with Chinese pupils in particular achieving scores above the national average. \\n\\nPerformance across headline measures increased for all major ethnic groups from 2017 to 2018, with the exception of EBacc entries for white pupils were there was a small decrease. \\n\\nWithin the more detailed ethnic groupings, pupils from an Indian background are the highest performing group in key stage 4 headline measures other than Chinese pupils. Gypsy/Roma pupils and traveller of Irish heritage pupils are the lowest performing groups. \\n\\nFor context, White pupils made up 75.8% of pupils at the end of key stage 4 in 2018, 10.6% were Asian, 5.5% were black, 4.7% were mixed, 0.4% were Chinese. The remainder are in smaller breakdowns or unclassified.\"}]},{\"Order\":6,\"Heading\":\"Local authority\",\"Caption\":\"Performance by local authority varies considerably \",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Performance varies considerably across the country – for Attainment 8 score per pupil there is nearly a 23 point gap between the poorest and highest performing areas. The highest performing local authorities are concentrated in London and the south with the majority of the lowest performing local authorities are located in the northern and midland regions with average Attainment 8 score per pupil show that. This is similar to patterns seen in recent years and against other performance measures. \"}]},{\"Order\":7,\"Heading\":\"Pupil subject areas\",\"Caption\":\"Pupil subject entries are highest for science and humanities and continue to increase\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Pupil subject entries are highest for science and humanities and continue to increase\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"It is compulsory for pupils to study English and Maths at key stage 4 in state-funded schools.  \\n ### Science\\nIt is compulsory for schools to teach Science at Key Stage 4. For these subjects, the proportion of pupils entering continues to increase.  \\n\\n In 2018, 68.0% of the cohort entered the new combined science pathway rather than the individual science subjects like Chemistry, Biology, Physics or Computer Science. The general pattern is for pupils with higher prior attainment tend to take single sciences; those with lower prior attainment to opt for the combined science pathway; and those with the lowest prior attainment to take no science qualifications. \\n ### Humanities \\nThe proportion of pupils entering EBacc humanities continued to increase in 2018, to 78.3% in state-funded schools, a rise of 1.5 percentage points since 2017. This was driven by small increases in entries across the majority of prior attainment groups for geography, and small increases in entries for pupils with low and average prior attainment for history. In history, the slight increase in entries from pupils with low and average prior attainment groups was counter-balanced by continued decreases in proportion of entries for high prior attainers. This trend has continued since 2016. \\n ### Languages \\n Entries to EBacc languages continued to decrease in 2018 to 46.1%, a fall of 1.3 percentage points compared to 2017. This was the fourth year in a row that entries have fallen. There were decreases across the majority of prior attainment bands but the largest drop occurred for pupils with higher prior attainment.. This decrease in entries for pupils with high prior attainment between 2018 and 2017 is much smaller than the drop that occurred between 2016 and 2017. Some of this drop can be explained by pupils who entered a language qualification early in a subject that was subsequently reformed in 2018. This was the case for over 3,500 pupils, whose language result did not count in 2018 performance tables.  \\n ### Art and design subjects \\n The percentage of pupils entering at least one arts subject decreased in 2018, by 2.2 percentage points compared to equivalent data in 2017. 44.3% of pupils in state-funded schools entered at least one arts subject. This is the third consecutive year that a fall in entries has occurred. \"}]},{\"Order\":8,\"Heading\":\"Schools performance\",\"Caption\":\"Across state-funded schools performance is typically higher in converter academies, the most common school type\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Across state-funded schools performance is typically higher in converter academies, the most common school type\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Schools in England can be divided into state-funded and independent schools (funded by fees paid by attendees). Independent schools are considered separately, because the department holds state-funded schools accountable for their performance.  \\n\\n The vast majority of pupils in state-funded schools are in either academies (68%) or LA maintained schools (29%). *Converter academies* were high performing schools that chose to convert to academies and have on average higher attainment across the headline measures. *Sponsored academies* were schools that were low performing prior to conversion and tend to perform below the average for state-funded schools.  \\n\\n Between 2017 and 2018 EBacc entry remained stable for sponsored academies, with an increase of 0.1 percentage points to 30.1%. EBacc entry fell marginally for converter academies by 0.3 percentage points (from 44.2% to 43.8%). Over the same period, EBacc entry in local authority maintained schools increased by 0.2 percentage points to 37.0%.\"}]},{\"Order\":9,\"Heading\":\"Attainment\",\"Caption\":\"Multi-academy trust schools generally perform below national averages, but typically face greater challenges.\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Academies are state schools directly funded by the government, each belonging to a trust. Multi-Academy Trusts (MATs) can be responsible for a group of academies and cover around 13.6% of state-funded mainstream pupils. Most MATs are responsible for between 3 and 5 schools but just over 10% cover 11 or more schools.  \\n\\nGenerally speaking MATs are typically more likely to cover previously poor-performing schools and pupils are more likely to have lower prior attainment, be disadvantaged, have special educational needs (SEN) or have English as an additional language (EAL) than the national average. \\n\\nThe number of eligible MATs included in Key Stage 4 measures increased from 62 in 2017 to 85 in 2018. This is an increase from 384 to 494 schools, and from 54,356 to 69,169 pupils. \"},{\"Type\":\"DataBlock\",\"Heading\":\"Performance in MATs compared to national average\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"On Progress8 measures, in 2018, 32.9% of MATs were below the national average and 7.1% well below average. 29.4% were not above or below the national average by a statistically significant amount. \\n\\nEntry rate in EBacc is lower in MATs compared to the national average – in 2018 43.5% of MATs had an entry rate higher than the national average of 39.1%. The EBacc average point score is also lower in MATs – 32.9% of MATs had an APS higher than the national average. \\n\\nAnalysis by characteristics shows that in 2018 disadvantaged pupils in MATs made more progress than the national average for disadvantaged. However, non-disadvantaged pupils, SEN and non-SEN pupils, pupils with English as a first language and high prior attainment pupils made less progress than the national average for their respective group.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * average Attainment8 scores remained stable compared to 2017s \\n * percentage of pupils achieving 5 or above in English and Maths increased \\n * EBacc entry increased slightly \\n * over 250 schools met the coasting definition in 2018\"}},\"Tables\":null}", new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), new DateTime(2018, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", @"This statistical first release (SFR) provides information on the achievements in GCSE examinations and other qualifications of young people in academic year 2016 to 2017. This typically covers those starting the academic year aged 15. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-content-6) 

[Find out more about our GCSE and equivalent results methodology and terminology](#extra-information-sections-heading-1)", "GCSE and equivalent results in England, 2016 to 2017" });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("4fca874d-98b8-4c79-ad20-d698fb0af7dc"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("33ff3f17-0671-41e9-b404-5661ab8a9476"), new DateTime(2018, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Updated exclusion rates for Gypsy/Roma pupils, to include extended ethnicity categories within the headcount (Gypsy, Roma and other Gypsy/Roma).", new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"), new DateTime(2017, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document.", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"), new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("51bd1e2f-2669-4708-b300-799b6be9ec9a"), new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("aa4c0f33-cdf4-4df9-9540-18472d46a301"), new DateTime(2018, 6, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amended title of table 8e in attachment 'Schools pupils and their characteristics 2018 - LA tables'.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("4bd0f73b-ef2b-4901-839a-80cbf8c0871f"), new DateTime(2018, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Removed unrelated extra material from table 7c in attachment 'Schools pupils and their characteristics 2018 - LA tables'.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("7f911a4e-7a56-4f6f-92a6-bd556a9bcfd3"), new DateTime(2018, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Added cross-border movement local authority level and underlying data tables.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("d008b331-af29-4c7e-bb8a-5a2005aa0131"), new DateTime(2018, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Added open document version of 'Schools pupils and their characteristics 2018 - Cross-border movement local authority tables'.", new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("8900bab9-74ec-4b5d-8be1-648ff4870167"), new DateTime(2018, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("181ec43e-cf22-4cab-a128-0a5702468566"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("313435b3-fe56-4b92-8e13-670dbf510062"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("398ba8c6-3ea0-49da-8645-ceb3c7fb9860"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("5e244416-6f2a-4d22-bea4-c22a229befef"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("b086ba70-703c-40dd-aaef-d2e19335188e"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("dc8b0d8c-08bb-47cc-b3a1-9e6ac9c2c268"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("e3c1db23-8a8f-47fe-b2cd-8e677db700a2"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("e6b36ee8-ef66-4864-a4b3-9047ee3da338"));

            migrationBuilder.DeleteData(
                table: "Link",
                keyColumn: "Id",
                keyValue: new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"));

            migrationBuilder.DeleteData(
                table: "Methodologies",
                keyColumn: "Id",
                keyValue: new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d63daa75-5c3e-48bf-a232-f232e0d13898"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("33ff3f17-0671-41e9-b404-5661ab8a9476"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("4bd0f73b-ef2b-4901-839a-80cbf8c0871f"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("4fca874d-98b8-4c79-ad20-d698fb0af7dc"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("51bd1e2f-2669-4708-b300-799b6be9ec9a"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("7f911a4e-7a56-4f6f-92a6-bd556a9bcfd3"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("8900bab9-74ec-4b5d-8be1-648ff4870167"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("aa4c0f33-cdf4-4df9-9540-18472d46a301"));

            migrationBuilder.DeleteData(
                table: "Update",
                keyColumn: "Id",
                keyValue: new Guid("d008b331-af29-4c7e-bb8a-5a2005aa0131"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"));

            migrationBuilder.DeleteData(
                table: "Releases",
                keyColumn: "Id",
                keyValue: new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"));

            migrationBuilder.DeleteData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"));

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("e64c4fdc-6067-49f4-a698-347484eaf360"), null, null, null, "30-hours-free-childcare", "", "30 hours free childcare", new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("f00aa2c8-f642-475f-8f0e-f469aa975b3f"), null, null, null, "early-years-foundation-stage-profile-results", "", "Early years foundation stage profile results", new Guid("17b2e32c-ed2f-4896-852b-513cdf466769") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), null, null, null, "permanent-and-fixed-period-exclusions-in-england", "", "Permanent and fixed-period exclusions in England", new Guid("77941b7d-bbd6-4069-9107-565af89e2dec") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)", null, new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "pupil-absence-in-schools-in-england", "", "Pupil absence in schools in England", new Guid("67c249de-1cca-446e-8ccb-dcdac542f460") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), null, null, null, "school-pupils-and-their-characteristics", "", "School and pupils and their characteristics", new Guid("e50ba9fd-9f19-458c-aceb-4422f0c7d1ba") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("91fdcf75-4814-4077-b7ca-7e07f415fa30"), null, null, null, "secondary-and-primary-schools-applications-and-offers", "", "Secondary and primary schools applications and offers", new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c") });

            migrationBuilder.InsertData(
                table: "Publications",
                columns: new[] { "Id", "DataSource", "Description", "NextUpdate", "Slug", "Summary", "Title", "TopicId" },
                values: new object[] { new Guid("3a9b9782-64e9-4c47-8996-0d6503fac26b"), null, null, null, "gcse-and-equivalent-results", "", "GCSE and equivalent results", new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b") });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("fedc1701-9b9f-421d-b549-0443922dcbd4"), "2008 to 2009", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("18af7ad3-6dc2-4dec-b438-e451ab7e1cab"), "January 2017", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("3cc8cc9a-319b-425a-aca1-16624bda7968"), "January 2016", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("5a1850d4-140f-4db3-af5e-e6d0ce3727e7"), "January 2015", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("37ba6d3f-2c5e-48cc-9ec9-596140159937"), "January 2014", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("0b81528a-200c-499b-a542-3b7462966c26"), "January 2013", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("48fe71bd-74a1-41e8-a02a-52bec450596c"), "January 2012", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("2c06838d-342e-419e-ab90-91b965e1e382"), "January 2011", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("943f1242-992a-4e58-9a7a-531336d20b7c"), "January 2010", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("bf39fc39-fb96-4704-b6fe-d7f7568588bf"), "2014 to 2015", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("1511d1f6-7b6e-4c8e-9002-9d3fe65e33f1"), "2012 to 2013", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("11d0111b-cf26-416d-98c3-168bb8401fdd"), "2013 to 2014", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("a9ce8158-d0f0-4123-afe2-109789c3982b"), "2010 to 2011", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("78a28be8-17c0-461e-baad-603d825e13de"), "2009 to 2010", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("c4578279-63b4-466d-8e60-e26e15948ed2"), "2015 to 2016", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("bf1047be-eae0-4bed-8cd0-83f562c77cdf"), "2014 to 2015", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("bfaed167-e8db-4db8-b7f5-b00635f47ffa"), "2013 to 2014", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("8b6c8afa-0a90-4998-b157-81a7a52103d1"), "2012 to 2013", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("83f1a9ea-3f7c-4184-ab5a-f8dc22078b17"), "2011 to 2012", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("2b4e08a5-cef0-4d79-a1a9-8412b6a4ed02"), "2010 to 2011", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("54b9cff3-1c83-4c56-83d3-83213c6d34c1"), "2009 to 2010", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010" });

            migrationBuilder.InsertData(
                table: "Link",
                columns: new[] { "Id", "Description", "PublicationId", "Url" },
                values: new object[] { new Guid("f48cce88-af53-4b1a-aac2-cc2269d80801"), "2011 to 2012", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics" });

            migrationBuilder.InsertData(
                table: "Methodologies",
                columns: new[] { "Id", "Annexes", "Content", "PublicationId", "Published", "Summary", "Title" },
                values: new object[] { new Guid("ba24b6e8-900a-4d7a-925a-c9138cf112b3"), "[{\"Order\":1,\"Heading\":\"Annex A - Glossary\",\"Caption\":\"\",\"Content\":[]},{\"Order\":2,\"Heading\":\"Annex B - Calculations\",\"Caption\":\"\",\"Content\":[]},{\"Order\":3,\"Heading\":\"Annex C - School attendance codes\",\"Caption\":\"\",\"Content\":[]},{\"Order\":4,\"Heading\":\"Annex D - Links to pupil absence national statistics and data\",\"Caption\":\"\",\"Content\":[]},{\"Order\":5,\"Heading\":\"Annex E - Standard breakdowns\",\"Caption\":\"\",\"Content\":[]},{\"Order\":6,\"Heading\":\"Annex F - Timeline\",\"Caption\":\"\",\"Content\":[]}]", "[{\"Order\":1,\"Heading\":\"1. Overview of absence statistics\",\"Caption\":\"\",\"Content\":[]},{\"Order\":2,\"Heading\":\"2. National Statistics badging\",\"Caption\":\"\",\"Content\":[]},{\"Order\":3,\"Heading\":\"3. Methodology\",\"Caption\":\"\",\"Content\":[]},{\"Order\":4,\"Heading\":\"4. Data collection\",\"Caption\":\"\",\"Content\":[]},{\"Order\":5,\"Heading\":\"5. Data processing\",\"Caption\":\"\",\"Content\":[]},{\"Order\":6,\"Heading\":\"6. Data quality\",\"Caption\":\"\",\"Content\":[]},{\"Order\":7,\"Heading\":\"7. Contacts\",\"Caption\":\"\",\"Content\":[]}]", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), new DateTime(2018, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Find out about the methodology behind pupil absence statistics and data and how and why they're collected and published.", "Pupil absence statistics: methodology" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("9c0637ec-db61-46ee-9b63-86349c19a24e"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This statistical publication provides the number of schools and pupils in schools in England, using data from the January 2018School Census.\\n\\n Breakdowns are given for school types as well as for pupil characteristics including free school meal eligibility, English as an additional languageand ethnicity.This release also contains information about average class sizes.\\n\\n SEN tables previously provided in thispublication will be published in the statistical publication ‘Special educational needs in England: January 2018’ scheduled for release on 26July 2018.\\n\\n Cross border movement tables will be added to this publication later this year.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}", new Guid("cdc016da-04d0-4571-a1d9-61fdb01037bb"), new DateTime(2018, 5, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "January 2018", "january-2018", "Statistics on pupils in schools in England as collected in the January 2018 school census.", "Schools, pupils and their characteristics: January 2018" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("c5ca8fa6-8cbb-4126-ae83-c6e2c1babcfc"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This statistical first release (SFR) reports on absence of pupils of compulsory school age in state-funded primary, secondary and special schools during the 2016/17 academic year. Information on absence in pupil referral units, and for pupils aged four, is also included. The Department uses two key measures to monitor pupil absence – overall and persistent absence. Absence by reason and pupils characteristics is also included in this release. Figures are available at national, regional, local authority and school level. Figures held in this release are used for policy development as key indicators in behaviour and school attendance policy. Schools and local authorities also use the statistics to compare their local absence rates to regional and national averages for different pupil groups.\"}]},{\"Order\":2,\"Heading\":\"Absence rates\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The overall absence rate across state-funded primary, secondary and special schools increased from 4.6 per cent in 2015/16 to 4.7 per cent in 2016/17. In primary schools the overall absence rate stayed the same at 4 per cent and the rate in secondary schools increased from 5.2 per cent to 5.4 per cent. Absence in special schools is much higher at 9.7 per cent in 2016/17\\n\\nThe increase in overall absence rate has been driven by an increase in the unauthorised absence rate across state-funded primary, secondary and special schools - which increased from 1.1 per cent to 1.3 per cent between 2015/16 and 2016/17.\\n\\nLooking at longer-term trends, overall and authorised absence rates have been fairly stable over recent years after decreasing gradually between 2006/07 and 2013/14. Unauthorised absence rates have not varied much since 2006/07, however the unauthorised absence rate is now at its highest since records began, at 1.3 per cent.\\n\\nThis increase in unauthorised absence is due to an increase in absence due to family holidays that were not agreed by the school. The authorised absence rate has not changed since last year, at 3.4 per cent. Though in primary schools authorised absence rates have been decreasing across recent years.\\n\\nThe total number of days missed due to overall absence across state-funded primary, secondary and special schools has increased since last year, from 54.8 million in 2015/16 to 56.7 million in 2016/17. This partly reflects the rise in the total number of pupil enrolments, the average number of days missed per enrolment has increased very slightly from 8.1 days in 2015/16 to 8.2 days in 2016/17.\\n\\nIn 2016/17, 91.8 per cent of pupils in state-funded primary, state-funded secondary and special schools missed at least one session during the school year, this is similar to the previous year (91.7 per cent in 2015/16).\"},{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2012\",\"endYear\":\"2017\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":[{\"Indicators\":[\"23\",\"26\",\"28\"],\"XAxis\":{\"title\":\"School Year\"},\"YAxis\":{\"title\":\"Absence Rate\"},\"Type\":\"line\"}],\"Summary\":null,\"Tables\":[{\"indicators\":[\"23\",\"26\",\"28\"]}]}]},{\"Order\":3,\"Heading\":\"Persistent absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The percentage of enrolments in state-funded primary and state-funded secondary schools that were classified as persistent absentees in 2016/17 was 10.8 per cent. This is up from the equivalent figure of 10.5 per cent in 2015/16 (see Figure 2).\\n\\nIn 2016/17, persistent absentees accounted for 37.6 per cent of all absence compared to 36.6 per cent in 2015/16. Longer term, there has been a decrease in the proportion of absence that persistent absentees account for – down from 43.3 per cent in 2011/12.\\n\\nThe overall absence rate for persistent absentees across all schools was 18.1 per cent, nearly four times higher than the rate for all pupils. This is a slight increase from 2015/16, when the overall absence rate for persistent absentees was 17.6 per cent.\\n\\nPersistent absentees account for almost a third, 31.6 per cent, of all authorised absence and more than half, 53.8 per cent of all unauthorised absence. The rate of illness absences is almost four times higher for persistent absentees compared to other pupils, at 7.6 per cent and 2.0 per cent respectively.\"}]},{\"Order\":4,\"Heading\":\"Reasons for absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":null,\"Body\":\"Within this release absence by reason is broken down in three different ways:\\n\\nDistribution of absence by reason: The proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences.\\n\\nRate of absence by reason: The rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions.\\n\\nOne or more sessions missed due to each reason: The number of pupil enrolments missing at least one session due to each reason.\"}]},{\"Order\":5,\"Heading\":\"Distribution of absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Nearly half of all pupils (48.9 per cent) were absent for five days or fewer across state-funded primary, secondary and special schools in 2016/17, down from 49.1 per cent in 2015/16.\\n\\n4.3 per cent of pupil enrolments had more than 25 days of absence in 2016/17 (the same as in 2015/16). These pupil enrolments accounted for 23.5 per cent of days missed. 8.2 per cent of pupil enrolments had no absence during 2016/17.\\n\\nPer pupil enrolment, the average total absence in primary schools was 7.2 days, compared to 16.9 days in special schools and 9.3 days in secondary schools.\\n\\nWhen looking at absence rates across terms for primary, secondary and special schools, the overall absence rate is lowest in the autumn term and highest in the summer term. The authorised rate is highest in the spring term and lowest in the summer term, and the unauthorised rate is highest in the summer term.\"}]},{\"Order\":6,\"Heading\":\"Absence by pupil characteristics\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The patterns of absence rates for pupils with different characteristics have been consistent across recent years.\\n\\n### Gender\\n\\nThe overall absence rates across state-funded primary, secondary and special schools were very similar for boys and girls, at 4.7 per cent and 4.6 per cent respectively. The persistent absence rates were also similar, at 10.9 per cent for boys and 10.6 per cent for girls.\\n\\n### Free school meals (FSM) eligibility\\n\\nAbsence rates are higher for pupils who are known to be eligible for and claiming free school meals. The overall absence rate for these pupils was 7.3 per cent, compared to 4.2 per cent for non FSM pupils. The persistent absence rate for pupils who were eligible for FSM was more than twice the rate for those pupils not eligible for FSM.\\n\\n### National curriculum year group\\n\\nPupils in national curriculum year groups 3 and 4 had the lowest overall absence rates at 3.9 and 4 per cent respectively. Pupils in national curriculum year groups 10 and 11 had the highest overall absence rate at 6.1 per cent and 6.2 per cent respectively. This trend is repeated for persistent absence.\\n\\n### Special educational need (SEN)\\n\\nPupils with a statement of special educational needs (SEN) or education healthcare plan (EHC) had an overall absence rate of 8.2 per cent compared to 4.3 per cent for those with no identified SEN. The percentage of pupils with a statement of SEN or an EHC plan that are persistent absentees was more than two times higher than the percentage for pupils with no identified SEN.\\n\\n### Ethnic group\\n\\nThe highest overall absence rates were for Traveller of Irish Heritage and Gypsy/ Roma pupils at 18.1 per cent and 12.9 per cent respectively. Overall absence rates for pupils of a Chinese and Black African ethnicity were substantially lower than the national average of 4.7 per cent at 2.4 per cent and 2.9 per cent respectively. A similar pattern is seen in persistent absence rates; Traveller of Irish heritage pupils had the highest rate at 64 per cent and Chinese pupils had the lowest rate at 3.1 per cent.\"}]},{\"Order\":7,\"Heading\":\"Absence for four year olds\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The overall absence rate for four year olds in 2016/17 was 5.1 per cent which is lower than the rate of 5.2 per cent which it has been for the last two years.\\n\\nAbsence recorded for four year olds is not treated as 'authorised' or 'unauthorised' and is therefore reported as overall absence only.\"}]},{\"Order\":8,\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The overall absence rate for pupil referral units in 2016/17 was 33.9 per cent, compared to 32.6 per cent in 2015/16. The percentage of enrolments in pupil referral units who were persistent absentees was 73.9 per cent in 2016/17, compared to 72.5 per cent in 2015/16.\"}]},{\"Order\":9,\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"There is variation in overall and persistent absence rates across state-funded primary, secondary and special schools by region and local authority. Similarly to last year, the three regions with the highest overall absence rate across all state-funded primary, secondary and special schools are the North East (4.9 per cent), Yorkshire and the Humber (4.9 per cent) and the South West (4.8 per cent), with Inner and Outer London having the lowest overall absence rate (4.4 per cent). The region with the highest persistent absence rate is Yorkshire and the Humber, where 11.9 per cent of pupil enrolments are persistent absentees, with Outer London having the lowest rate of persistent absence (at 10.0 per cent).\\n\\nAbsence information at local authority district level is also published within this release, in the accompanying underlying data files.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":{\"subjectId\":1,\"geographicLevel\":\"National\",\"countries\":null,\"localAuthorities\":null,\"regions\":null,\"startYear\":\"2016\",\"endYear\":\"2017\",\"filters\":[\"1\",\"2\"],\"indicators\":[\"23\",\"26\",\"28\"]},\"Charts\":null,\"Summary\":{\"dataKeys\":[\"23\",\"26\",\"28\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * pupils missed on average 8.2 school days \\n  * overall and unauthorised absence rates up on previous year \\n * unauthorised rise due to higher rates of unauthorised holidays \\n * 10% of pupils persistently absent during 2016/17\"}},\"Tables\":null}", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas. 

", "Pupil absence data and statistics for schools in England" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("2bb3a68d-330c-4cc4-a584-797e30f1288f"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":null},{\"Order\":2,\"Heading\":\"Absence rates\",\"Caption\":\"\",\"Content\":null},{\"Order\":3,\"Heading\":\"Persistent absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":4,\"Heading\":\"Distribution of absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":5,\"Heading\":\"Absence for four year olds\",\"Caption\":\"\",\"Content\":null},{\"Order\":6,\"Heading\":\"Pupil referral unit absence\",\"Caption\":\"\",\"Content\":null},{\"Order\":7,\"Heading\":\"Pupil absence by local authority\",\"Caption\":\"\",\"Content\":null}]", "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\"\"}},\"Tables\":null}", new Guid("6a8551af-a700-4c45-a8ac-aa2269f961f8"), new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2015 to 2016", "2015-16", "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.", "Pupil absence data and statistics for schools in England" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("74e93f2c-e009-490b-9467-3c48429dab9f"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This National Statistics release reports on permanent and fixed period exclusions from state-funded primary, state-funded secondary and special schools during the 2016/17 academic year as reported in the School Census. This release also includes school level exclusions figures for state-funded primary, secondary and special schools and national level figures on permanent and fixed-period exclusions from pupil referral units. All figures in this release are based on unrounded data; therefore, constituent parts may not add up due to rounding.\\n\\nAn Exclusions statistics guide, which provides historical information on exclusion statistics, technical background information to the figures and data collection, and definitions of key terms should be referenced alongside this release.\\n\\nIn this publication: The following tables are included in the statistical publication\\n\\n*   national tables (Excel .xls and open format)\\n\\n*   local authority (LA) tables\\n\\n*   underlying data (open format .csv and metadata .txt)\\n\\nThe underlying data is accompanied by a metadata document that describes underlying data files.\\n\\nWe welcome feedback on any aspect of this document at [schools.statistics@education.gov.uk](#)\"}]},{\"Order\":2,\"Heading\":\"Permanent exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":\"Permanent exclusion rate definition\",\"Body\":\"A permanent exclusion refers to a pupil who is excluded and who will not come back to that school (unless the exclusion is overturned). The number of permanent exclusions across all state-funded primary, secondary and special schools has increased from 6,685 in 2015/16 to 7,720 in 2016/17. This corresponds to around 40.6 permanent exclusions per day in 2016/17, up from an average of 35.2 per day in 2015/16.\"},{\"Type\":\"DataBlock\",\"Heading\":\"Chart showing permanent exclusions in England\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"The rate of permanent exclusions across all state-funded primary, secondary and special schools has also increased from 0.08 per cent to 0.10 per cent of pupil enrolments, which is equivalent to around 10 pupils per 10,000.\\n\\nMost (83 per cent) permanent exclusions occurred in secondary schools. The rate of permanent exclusions in secondary schools increased from 0.17 per cent in 2015/16 to 0.20 per cent in 2016/17, which is equivalent to around 20 pupils per 10,000.\\n\\nThe rate of permanent exclusions also rose in primary schools, at 0.03 per cent, but decreased in special schools from 0.08 per cent in 2015/16 to 0.07 per cent in 2016/17. Looking at longer-term trends, the rate of permanent exclusions across all state-funded primary, secondary and special schools followed a generally downward trend from 2006/07 when the rate was 0.12 per cent until 2012/13, and has been rising again since then, although rates are still lower now than in 2006/07.\"}]},{\"Order\":3,\"Heading\":\"Fixed-period exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":\"Fixed-period exclusion rate definition\",\"Body\":\"Fixed-period exclusion refers to a pupil who is excluded from a school for a set period of time. A fixed-period exclusion can involve a part of the school day and it does not have to be for a continuous period. A pupil may be excluded for one or more fixed periods up to a maximum of 45 school days in a single academic year. This total includes exclusions from previous schools covered by the exclusion legislation. A pupil may receive more than one fixed-period exclusion, so pupils with repeat exclusions can inflate fixed-period exclusion rates.\"},{\"Type\":\"DataBlock\",\"Heading\":\"Chart showing fixed-period exclusions in England\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"The number of fixed-period exclusions across all state-funded primary, secondary and special schools has increased from 339,360 in 2015/16 to 381,865 in 2016/17. This corresponds to around 2,010 fixed-period exclusions per day1 in 2016/17, up from an average of 1,786 per day in 2015/16.\\n\\nThere were increases in the number and rate of fixed-period exclusions for state-funded primary and secondary schools and special schools:\"}]},{\"Order\":4,\"Heading\":\"Number and length of fixed-period exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"InsetTextBlock\",\"Heading\":\"Enrolments with one or more fixed-period exclusion definition\",\"Body\":\"Pupils with one or more fixed-period exclusion refer to pupil enrolments that had at least one fixed-period exclusion across the full academic year. It includes those with repeated fixed-period exclusions.\"},{\"Type\":\"MarkDownBlock\",\"Body\":\"In state-funded primary, secondary and special schools, there were 183,475 pupil enrolments, 2.29 per cent, with at least one fixed term exclusion in 2016/17, up from 167,125 pupil enrolments, 2.11 per cent, in 2015/16.\\n\\nOf those pupils with at least one fixed-period exclusion, 59.1 per cent were excluded only on one occasion, and 1.5 per cent received 10 or more fixed-period exclusions during the year. The percentage of pupils with at least one fixed-period exclusion that went on to receive a permanent one was 3.5 per cent.\\n\\nThe average length of fixed-period exclusions across state-funded primary, secondary and special schools in 2016/17 was 2.1 days, slightly shorter than in 2015/16.\\n\\nThe highest proportion of fixed-period exclusions (46.6 per cent) lasted for only one day. Only 2.0 per cent of fixed-period exclusions lasted for longer than one week and longer fixed-period exclusions were more prevalent in secondary schools.\"}]},{\"Order\":5,\"Heading\":\"Reasons for exclusions\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Persistent disruptive behaviour remained the most common reason for permanent exclusions in state-funded primary, secondary and special schools - accounting for 2,755 (35.7 per cent) of all permanent exclusions in 2016/17. This is equivalent to 3 permanent exclusions per 10,000 pupils. However, in special schools alone, the most common reason for exclusion was physical assault against and adult, which made up 37.8 per cent of all permanent exclusions and 28.1 per cent of all fixed-period exclusions.\\n\\nAll reasons except bullying and theft saw an increase in permanent exclusions since last year. The most common reasons - persistent disruptive behaviour, physical assault against a pupil and other reasons had the largest increases.\\n\\nPersistent disruptive behaviour is also the most common reason for fixed-period exclusions. The 108,640 fixed-period exclusions for persistent disruptive behaviour in state-funded primary, secondary and special schools made up 28.4 per cent of all fixed-period exclusions, up from 27.7 per cent in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.\\n\\nAll reasons saw an increase in fixed-period exclusions since last year. Persistent disruptive behaviour and other reasons saw the biggest increases.\"}]},{\"Order\":6,\"Heading\":\"Exclusions by pupil characteristics\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"In 2016/17 we saw a similar pattern by pupil characteristics to previous years. The groups that we usually expect to have higher rates are the ones that have increased exclusions since last year e.g. boys, pupils with special educational needs, pupils known to be eligible for and claiming free school meals and national curriculum years 9 and 10.\\n\\n**Age, national curriculum year group and gender**\\n\\n*   Over half of all permanent (57.2 per cent) and fixed-period (52.6 per cent) exclusions occur in national curriculum year 9 or above.\\n\\n*   A quarter (25.0 per cent) of all permanent exclusions were for pupils aged 14, and pupils of this age group also had the highest rate of fixed-period exclusion, and the highest rate of pupils receiving one or more fixed-period exclusion.\\n\\n*   The permanent exclusion rate for boys (0.15 per cent) was over three times higher than that for girls (0.04 per cent) and the fixed-period exclusion rate was almost three times higher (6.91 compared with 2.53 per cent).\\n\\n**Free school meals (FSM) eligibility**\\n\\n*   Pupils known to be eligible for and claiming free school meals (FSM) had a permanent exclusion rate of 0.28 per cent and fixed period exclusion rate of 12.54 per cent - around four times higher than those who are not eligible (0.07 and 3.50 per cent respectively).\\n\\n*   Pupils known to be eligible for and claiming free school meals (FSM) accounted for 40.0 per cent of all permanent exclusions and 36.7 per cent of all fixed-period exclusions. Special educational need (SEN)\\n\\n*   Pupils with identified special educational needs (SEN) accounted for around half of all permanent exclusions (46.7 per cent) and fixed-period exclusions (44.9 per cent).\\n\\n*   Pupils with SEN support had the highest permanent exclusion rate at 0.35 per cent. This was six times higher than the rate for pupils with no SEN (0.06 per cent).\\n\\n*   Pupils with an Education, Health and Care (EHC) plan or with a statement of SEN had the highest fixed-period exclusion rate at 15.93 per cent - over five times higher than pupils with no SEN (3.06 per cent).\\n\\n**Ethnic group**\\n\\n*   Pupils of Gypsy/Roma and Traveller of Irish Heritage ethnic groups had the highest rates of both permanent and fixed-period exclusions, but as the population is relatively small these figures should be treated with some caution.\\n\\n*   Black Caribbean pupils had a permanent exclusion rate nearly three times higher (0.28 per cent) than the school population as a whole (0.10 per cent). Pupils of Asian ethnic groups had the lowest rates of permanent and fixed-period exclusion.\"}]},{\"Order\":7,\"Heading\":\"Independent exclusion reviews\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Independent review Panel definition: Parents (and pupils if aged over 18) are able to request a review of a permanent exclusion. An independent review panel’s role is to review the decision of the governing body not to reinstate a permanently excluded pupil. The panel must consider the interests and circumstances of the excluded pupil, including the circumstances in which the pupil was excluded and have regard to the interests of other pupils and people working at the school.\\n\\nIn 2016/17 in maintained primary, secondary and special schools and academies there were 560 reviews lodged with independent review panels of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement.\"}]},{\"Order\":8,\"Heading\":\"Exclusions from pupil referral units\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"The rate of permanent exclusion in pupil referral units decreased from 0.14 per cent in 2015/16 to 0.13 in 2016/17. After an increase from 2013/14 to 2014/15, permanent exclusions rates have remained fairly steady. There were 25,815 fixed-period exclusions in pupil referral units in 2016/17, up from 23,400 in 2015/16. The fixed period exclusion rate has been steadily increasing since 2013/14.\\n\\nThe percentage of pupil enrolments in pupil referral units who one or more fixed-period exclusion was 59.17 per cent in 2016/17, up from 58.15 per cent in 2015/16.\"}]},{\"Order\":9,\"Heading\":\"Exclusions by local authority\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"There is considerable variation in the permanent and fixed-period exclusion rate at local authority level (see accompanying maps on the web page).\\n\\nThe regions with the highest overall rates of permanent exclusion across state-funded primary, secondary and special schools are the West Midlands and the North West (at 0.14 per cent). The regions with the lowest rates are the South East (at 0.06 per cent) and Yorkshire and the Humber (at 0.07 per cent).\\n\\nThe region with the highest fixed-period exclusion rate is Yorkshire and the Humber (at 7.22 per cent), whilst the lowest rate was seen in Outer London (3.49 per cent).\\n\\nThese regions also had the highest and lowest rates of exclusion in the previous academic year.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":null,\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"perm_excl_rate\",\"perm_excl\",\"fixed_excl_rate\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * overall rate of permanent exclusions has increased from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17 \\n * number of exclusions has also increased, from 6,685 to 7,720 \\n * overall rate of fixed period exclusions increased, from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17 \\n * number of exclusions has also increased, from 339,360 to 381,865. \\n\"}},\"Tables\":null}", new Guid("2ebe01c0-1a87-43bc-8191-feb1fff3ce1e"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", @"Read national statistical summaries and definitions, view charts and tables and download data files across a range of permanent and fixed-period exclusion subject areas. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-heading-9)", "Permanent and fixed period exclusions" });

            migrationBuilder.InsertData(
                table: "Releases",
                columns: new[] { "Id", "Content", "KeyStatistics", "PublicationId", "Published", "ReleaseName", "Slug", "Summary", "Title" },
                values: new object[] { new Guid("254688cb-ebd8-4b4c-9ec0-8efa67e164ee"), "[{\"Order\":1,\"Heading\":\"About this release\",\"Caption\":\"\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"This release shows results for GCSE and equivalent Key Stage 4 (KS4) qualifications in 2018 across a range of measures, broken down by pupil characteristics and education institutions. Results are also provided on schools below the floor standards and meeting the coasting definition.  \\n\\nThis is an update to Provisional figures released in October 2018. Users should be careful when comparing headline measures to results in previous years given recent methodological changes \\n\\nFigures are available at national, regional, local authority, and school level. Figures held in this release are used for policy development and count towards the secondary performance tables. Schools and local authorities also use the statistics to compare their local performance to regional and national averages for different pupil groups.\"}]},{\"Order\":2,\"Heading\":\"School performance for 2018\",\"Caption\":\"School performance for 2018 shows small increases across all headline measures compared to 2017\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Average headline performance measures over time\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Results for 2018 show an increases across all headline measures compared to 2017. **When drawing comparison over time, however, it is very important to note any changes to methodology or data changes underpinning these measures**. For example, changes in Attainment 8 may have been affected by the introduction of further reformed GCSEs graded on the 9-1 scale which have a higher maximum score than unreformed GCSEs. Similarly, in 2016 there were significant changes to the Attainment in English and Maths measure. \\n\\nThese results cover state-funded schools but results for all schools are available in the supporting tables and show slightly lower performance across all headline measures on average. Differences between the figures for all schools and state-funded schools are primarily due to the impact of unapproved and unregulated qualifications such as international GCSEs taken more commonly in independent schools. These qualification are not included in school performance tables. \\n\\nThere are five primary headline measures used throughout this report: \\n * **Attainment8** - measures the average achievement of pupils in up to 8 qualifications (including English and Maths). \\n * **Attainment in English & Maths (9-5)** - measures the percentage of pupils achieving a grade 5 or above in both English and maths.\\n * **EBacc Entries** – measure the percentage of pupils reaching the English Baccalaureate (EBacc) attainment threshold in core academic subjects at key stage 4. The EBacc is made up of English, maths, science, a language, and history or geography. \\n * **EBacc Average Point Score (APS)** – measures pupils’ point scores across the five pillars of the EBacc, ensuring the attainment of all pupils is recognised. New measure from 2018, replacing the previous threshold EBacc attainment measure. \\n * **Progress** - measures the progress a pupil makes from the end of key stage 2 to the end of key stage 4. It compares pupils’ Attainment 8 score with the average for all pupils nationally who had a similar starting point. Progress 8 is a relative measure, therefore the national average Progress 8 score for mainstream schools is very close to zero. \"}]},{\"Order\":3,\"Heading\":\"Schools meeting the coasting and floor standard\",\"Caption\":\"Over 250 schools failed to support pupils to fulfil their potential in 2018\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"There is wide variation in the percentage of schools meeting the coasting and floor standard by region\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"The floor and coasting standards give measures of whether schools are helping pupils to fulfil their potential based on progress measures. The floor standard is based on results in the most recent year, whereas the Coasting definition looks at slightly different measures over the past three years. Only state-funded mainstream schools are covered by these measures, subject to certain eligibility criteria. \\n* **11.6%** of eligible schools were below the floor standard in 2018. This represents 346 schools\\n* **9.2%** of eligible schools met the coasting definition in 2018. This represents 257 schools \\n* **161** schools were both coating and below the floor standard \\n* due to methodological changes no directly comparable measures exist for previous years \\n\"}]},{\"Order\":4,\"Heading\":\"Pupil characteristics\",\"Caption\":\"Disadvantaged pupils and those with Special Education Needs continue to do less well than their peers\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Average headline scores by pupil characteristics\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Breakdowns by pupil characteristics show that across all headline measures: \\n* girls continue to do better than boys \\n* non-disadvantaged pupils continue to do better than disadvantaged pupils \\n* pupils with no identified Special Educational Needs (SEN) continue to do better perform than SEN pupils \\nIn general the pattern of attainment gaps for Attainment 8 in 2018 remained the same as in 2017 although differences in Attainment 8 scores widened slightly across all groups. This is to be expected due to changes to reformed GCSEs in 2018, meaning more points are available for higher scores.  \\n\\nDue to changes in performance measures over time, comparability over time is complicated. As such, for disadvantaged pupils is recommended to use to disadvantage gap index instead with is more resilient to changes in grading systems over time. The gap between disadvantaged pupils and others, measured using the gap index, has remained broadly stable, widening by 0.6% in 2018, and narrowing by 9.5% since 2011.\"},{\"Type\":\"DataBlock\",\"Heading\":\"Disadvantage attainment gap index\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null}]},{\"Order\":5,\"Heading\":\"Headline performance\",\"Caption\":\"Results across headline performance measures vary by ethnicity\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Average headline scores by pupil ethnicity\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Results across headline measures differ by ethnicity with Chinese pupils in particular achieving scores above the national average. \\n\\nPerformance across headline measures increased for all major ethnic groups from 2017 to 2018, with the exception of EBacc entries for white pupils were there was a small decrease. \\n\\nWithin the more detailed ethnic groupings, pupils from an Indian background are the highest performing group in key stage 4 headline measures other than Chinese pupils. Gypsy/Roma pupils and traveller of Irish heritage pupils are the lowest performing groups. \\n\\nFor context, White pupils made up 75.8% of pupils at the end of key stage 4 in 2018, 10.6% were Asian, 5.5% were black, 4.7% were mixed, 0.4% were Chinese. The remainder are in smaller breakdowns or unclassified.\"}]},{\"Order\":6,\"Heading\":\"Local authority\",\"Caption\":\"Performance by local authority varies considerably \",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Performance varies considerably across the country – for Attainment 8 score per pupil there is nearly a 23 point gap between the poorest and highest performing areas. The highest performing local authorities are concentrated in London and the south with the majority of the lowest performing local authorities are located in the northern and midland regions with average Attainment 8 score per pupil show that. This is similar to patterns seen in recent years and against other performance measures. \"}]},{\"Order\":7,\"Heading\":\"Pupil subject areas\",\"Caption\":\"Pupil subject entries are highest for science and humanities and continue to increase\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Pupil subject entries are highest for science and humanities and continue to increase\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"It is compulsory for pupils to study English and Maths at key stage 4 in state-funded schools.  \\n ### Science\\nIt is compulsory for schools to teach Science at Key Stage 4. For these subjects, the proportion of pupils entering continues to increase.  \\n\\n In 2018, 68.0% of the cohort entered the new combined science pathway rather than the individual science subjects like Chemistry, Biology, Physics or Computer Science. The general pattern is for pupils with higher prior attainment tend to take single sciences; those with lower prior attainment to opt for the combined science pathway; and those with the lowest prior attainment to take no science qualifications. \\n ### Humanities \\nThe proportion of pupils entering EBacc humanities continued to increase in 2018, to 78.3% in state-funded schools, a rise of 1.5 percentage points since 2017. This was driven by small increases in entries across the majority of prior attainment groups for geography, and small increases in entries for pupils with low and average prior attainment for history. In history, the slight increase in entries from pupils with low and average prior attainment groups was counter-balanced by continued decreases in proportion of entries for high prior attainers. This trend has continued since 2016. \\n ### Languages \\n Entries to EBacc languages continued to decrease in 2018 to 46.1%, a fall of 1.3 percentage points compared to 2017. This was the fourth year in a row that entries have fallen. There were decreases across the majority of prior attainment bands but the largest drop occurred for pupils with higher prior attainment.. This decrease in entries for pupils with high prior attainment between 2018 and 2017 is much smaller than the drop that occurred between 2016 and 2017. Some of this drop can be explained by pupils who entered a language qualification early in a subject that was subsequently reformed in 2018. This was the case for over 3,500 pupils, whose language result did not count in 2018 performance tables.  \\n ### Art and design subjects \\n The percentage of pupils entering at least one arts subject decreased in 2018, by 2.2 percentage points compared to equivalent data in 2017. 44.3% of pupils in state-funded schools entered at least one arts subject. This is the third consecutive year that a fall in entries has occurred. \"}]},{\"Order\":8,\"Heading\":\"Schools performance\",\"Caption\":\"Across state-funded schools performance is typically higher in converter academies, the most common school type\",\"Content\":[{\"Type\":\"DataBlock\",\"Heading\":\"Across state-funded schools performance is typically higher in converter academies, the most common school type\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"Schools in England can be divided into state-funded and independent schools (funded by fees paid by attendees). Independent schools are considered separately, because the department holds state-funded schools accountable for their performance.  \\n\\n The vast majority of pupils in state-funded schools are in either academies (68%) or LA maintained schools (29%). *Converter academies* were high performing schools that chose to convert to academies and have on average higher attainment across the headline measures. *Sponsored academies* were schools that were low performing prior to conversion and tend to perform below the average for state-funded schools.  \\n\\n Between 2017 and 2018 EBacc entry remained stable for sponsored academies, with an increase of 0.1 percentage points to 30.1%. EBacc entry fell marginally for converter academies by 0.3 percentage points (from 44.2% to 43.8%). Over the same period, EBacc entry in local authority maintained schools increased by 0.2 percentage points to 37.0%.\"}]},{\"Order\":9,\"Heading\":\"Attainment\",\"Caption\":\"Multi-academy trust schools generally perform below national averages, but typically face greater challenges.\",\"Content\":[{\"Type\":\"MarkDownBlock\",\"Body\":\"Academies are state schools directly funded by the government, each belonging to a trust. Multi-Academy Trusts (MATs) can be responsible for a group of academies and cover around 13.6% of state-funded mainstream pupils. Most MATs are responsible for between 3 and 5 schools but just over 10% cover 11 or more schools.  \\n\\nGenerally speaking MATs are typically more likely to cover previously poor-performing schools and pupils are more likely to have lower prior attainment, be disadvantaged, have special educational needs (SEN) or have English as an additional language (EAL) than the national average. \\n\\nThe number of eligible MATs included in Key Stage 4 measures increased from 62 in 2017 to 85 in 2018. This is an increase from 384 to 494 schools, and from 54,356 to 69,169 pupils. \"},{\"Type\":\"DataBlock\",\"Heading\":\"Performance in MATs compared to national average\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":null,\"Tables\":null},{\"Type\":\"MarkDownBlock\",\"Body\":\"On Progress8 measures, in 2018, 32.9% of MATs were below the national average and 7.1% well below average. 29.4% were not above or below the national average by a statistically significant amount. \\n\\nEntry rate in EBacc is lower in MATs compared to the national average – in 2018 43.5% of MATs had an entry rate higher than the national average of 39.1%. The EBacc average point score is also lower in MATs – 32.9% of MATs had an APS higher than the national average. \\n\\nAnalysis by characteristics shows that in 2018 disadvantaged pupils in MATs made more progress than the national average for disadvantaged. However, non-disadvantaged pupils, SEN and non-SEN pupils, pupils with English as a first language and high prior attainment pupils made less progress than the national average for their respective group.\"}]}]", "{\"Type\":\"DataBlock\",\"Heading\":\"Latest headline facts and figures - 2016 to 2017\",\"DataBlockRequest\":null,\"Charts\":null,\"Summary\":{\"dataKeys\":[\"--\",\"--\",\"--\"],\"description\":{\"Type\":\"MarkDownBlock\",\"Body\":\" * average Attainment8 scores remained stable compared to 2017s \\n * percentage of pupils achieving 5 or above in English and Maths increased \\n * EBacc entry increased slightly \\n * over 250 schools met the coasting definition in 2018\"}},\"Tables\":null}", new Guid("3a9b9782-64e9-4c47-8996-0d6503fac26b"), new DateTime(2018, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "2016 to 2017", "2016-17", @"This statistical first release (SFR) provides information on the achievements in GCSE examinations and other qualifications of young people in academic year 2016 to 2017. This typically covers those starting the academic year aged 15. 

You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-content-6) 

[Find out more about our GCSE and equivalent results methodology and terminology](#extra-information-sections-heading-1)", "GCSE and equivalent results in England, 2016 to 2017" });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("420c8ba4-af05-465c-a8eb-abce0a369fca"), new DateTime(2018, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("74e93f2c-e009-490b-9467-3c48429dab9f") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("18b4a3e4-bacc-4ace-9e43-7caa90b006ea"), new DateTime(2018, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Updated exclusion rates for Gypsy/Roma pupils, to include extended ethnicity categories within the headcount (Gypsy, Roma and other Gypsy/Roma).", new Guid("74e93f2c-e009-490b-9467-3c48429dab9f") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("5614a5ff-2507-4a7d-b99f-3328dd9c4068"), new DateTime(2017, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document.", new Guid("c5ca8fa6-8cbb-4126-ae83-c6e2c1babcfc") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("eea2b9a1-8944-4638-b7d1-4f94d89c8594"), new DateTime(2017, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("c5ca8fa6-8cbb-4126-ae83-c6e2c1babcfc") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("e0231ff8-5160-485d-85e6-9cbd29695327"), new DateTime(2016, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("2bb3a68d-330c-4cc4-a584-797e30f1288f") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("77c3370c-71ef-4b5c-958d-135982eeb5ad"), new DateTime(2018, 6, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Amended title of table 8e in attachment 'Schools pupils and their characteristics 2018 - LA tables'.", new Guid("9c0637ec-db61-46ee-9b63-86349c19a24e") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("5a24198d-ab19-47c6-96f2-6faca416cbb7"), new DateTime(2018, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Removed unrelated extra material from table 7c in attachment 'Schools pupils and their characteristics 2018 - LA tables'.", new Guid("9c0637ec-db61-46ee-9b63-86349c19a24e") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("24b9005b-ed8a-4f48-8a6f-55fa23882832"), new DateTime(2018, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Added cross-border movement local authority level and underlying data tables.", new Guid("9c0637ec-db61-46ee-9b63-86349c19a24e") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("226bac6d-e6ae-4f0b-9dbf-a4d0e97a20fb"), new DateTime(2018, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Added open document version of 'Schools pupils and their characteristics 2018 - Cross-border movement local authority tables'.", new Guid("9c0637ec-db61-46ee-9b63-86349c19a24e") });

            migrationBuilder.InsertData(
                table: "Update",
                columns: new[] { "Id", "On", "Reason", "ReleaseId" },
                values: new object[] { new Guid("ad285753-cdd4-42fc-9b4b-165fd9f07185"), new DateTime(2018, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "First published.", new Guid("254688cb-ebd8-4b4c-9ec0-8efa67e164ee") });
        }
    }
}
