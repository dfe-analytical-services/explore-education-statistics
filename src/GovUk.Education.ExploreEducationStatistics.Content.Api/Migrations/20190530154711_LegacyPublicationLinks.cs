using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class LegacyPublicationLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegacyPublicationUrl",
                table: "Publications",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("0c67bbdb-4eb0-41cf-a62e-2589cee58538"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-on-higher-education-initial-participation-rates#participation-rates-in-higher-education-for-england");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-survey-of-parents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("123461ab-50be-45d9-8523-c5241a2c9c5b"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-admission-appeals#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#fe-and-skills---older-data");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("14953fda-02ff-45ed-9573-3a7a0ad8cb10"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-pupil-absence#combined-autumn--and-spring-term-release");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("14cfd218-5480-4ba1-a051-5b1e6be14b46"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/official-statistics-releases#higher-education-enrolments-and-qualifications");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1b2fb05c-eb2c-486b-80be-ebd772eda4f1"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#multi-academy-trust-performance-measures");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2434335f-f8e1-41fb-8d6e-4a11bc62b14a"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-key-stage-2#primary-school-performance-tables");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("263e10d2-b9c3-4e90-a6aa-b52b86de1f5f"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("28aabfd4-a3fb-45e1-bb34-21ca3b7d1aec"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-neet#neet:-2016-to-2017-data-");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2e95f880-629c-417b-981f-0901e97776ff"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("2ffbc8d3-eb53-4c4b-a6fb-219a5b95ebc8"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-education-and-training#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("30874b87-483a-427e-8916-43cf9020d9a1"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3260801d-601a-48c6-93b7-cf51680323d1"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-looked-after-children#looked-after-children");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3ceb43d0-e705-4cb9-aeb9-cb8638fcbf3d"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("3f3a66ec-5777-42ee-b427-8102a14ce0c5"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/graduate-labour-market-quarterly-statistics#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("441a13f6-877c-4f18-828f-119dbd401a5b"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-key-stage-1#phonics-screening-check-and-key-stage-1-assessment");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("657b1484-0369-4a0e-873a-367b79a48c35"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/fe-choices#learner-satisfaction-survey-data");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-school-applications#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("6c25a3e9-fc96-472f-895c-9ae4492dd2a4"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("6c388293-d027-4f74-8d74-29a42e02231c"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-pupil-absence#autumn-term-release");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("75568912-25ba-499a-8a96-6161b54994db"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/further-education#advanced-learner-loans-applications-2017-to-2018");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("79a08466-dace-4ff0-94b6-59c5528c9262"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/sfa-national-success-rates-tables#national-achievement-rates-tables");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("7ecea655-7b22-4832-b697-26e86769399a"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-key-stage-2#key-stage-2-national-curriculum-tests:-review-outcomes");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("86af24dc-67c4-47f0-a849-e94c7a1cfe9b"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8a92c6a5-8110-4c9c-87b1-e15f1c80c66a"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-destinations#destinations-after-key-stage-4-and-5");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8ab47806-e36f-4226-9988-1efe23156872"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#academy-spending");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("8b12776b-3d36-4475-8115-00974d7de1d0"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-outcome-based-success-measures#statistics");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("94d16c6e-1e5f-48d5-8195-8ea770f1b0d4"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#planned-local-authority-and-school-spending-");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9cc08298-7370-499f-919a-7d203ba21415"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-teacher-training#census-data");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("9e7e9d5c-b761-43a4-9685-4892392200b7"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#secondary-school-performance-tables");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-neet#participation-in-education,-employment-or-training");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-school-and-pupil-numbers#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("aa545525-9ffe-496c-a5b3-974ace56746e"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-pupil-projections#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("b318967f-2931-472a-93f2-fbed1e181e6a"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-school-workforce#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("b83f55db-73fc-46fc-9fda-9b59f5896e9d"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("bddcd4b8-db0d-446c-b6e9-03d4230c6927"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("c28f7aca-f1e8-4916-8ce3-fc177b140695"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/widening-participation-in-higher-education#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("c8756008-ed50-4632-9b96-01b5ca002a43"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#gcse-and-equivalent-results,-including-pupil-characteristics");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("ce6098a6-27b6-44b5-8e63-36df3a659e69"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/further-education-for-benefit-claimants#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-traineeships---older-data");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d0b47c96-d7de-4d80-9ff7-3bff135d2636"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/teacher-workforce-statistics-and-analysis#documents");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d24783b6-24a7-4ef3-8304-fd07eeedff92"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d34978d5-0317-46bc-9258-13412270ac4d"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-teacher-training#performance-data");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d4b9551b-d92c-4f98-8731-847780d3c9fa"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/official-statistics-releases#destinations-of-higher-education-leavers");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d63daa75-5c3e-48bf-a232-f232e0d13898"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d7bd5d9d-dc65-4b1d-99b1-4d815b7369a3"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-secure-children-s-homes");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("d8baee79-3c88-45f4-b12a-07b91e9b5c11"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-childrens-social-care-workforce#statutory-collection");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("dcb8b32b-4e50-4fe2-a539-58f9b6b3a366"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#local-authority-and-school-finance");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-key-stage-2#pupil-attainment-at-key-stage-2");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f00a784b-52e8-475b-b8ee-dbe730382ba8"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/fe-choices#employer-satisfaction-survey-data");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f51895df-c682-45e6-b23e-3138ddbfdaeb"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-looked-after-children#outcomes-for-looked-after-children");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("f657afb4-8f4a-427d-a683-15f11a2aefb5"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fa591a15-ae37-41b5-98f6-4ce06e5225f4"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-school-capacity#school-capacity-data:-by-academic-year");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-early-years-foundation-stage-profile#results-at-national-and-local-authority-level");

            migrationBuilder.UpdateData(
                table: "Publications",
                keyColumn: "Id",
                keyValue: new Guid("fd68e147-b7ee-464f-8b02-dcd917dc362d"),
                column: "LegacyPublicationUrl",
                value: "https://www.gov.uk/government/collections/statistics-student-loan-forecasts#documents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyPublicationUrl",
                table: "Publications");
        }
    }
}
