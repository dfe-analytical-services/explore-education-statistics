import publicationService, {
  PublicationMethodologiesList,
  PublicationReleaseSeriesItem,
  PublicationSummary,
  PublicationTreeOptions,
  RelatedInformationItem,
  ReleaseSummary,
  ReleaseVersionDataContent,
  ReleaseVersionHomeContent,
  ReleaseVersionSummary,
  Theme,
} from '@common/services/publicationService';
import {
  PaginatedList,
  PaginationRequestParams,
} from '@common/services/types/pagination';
import { UseQueryOptions } from '@tanstack/react-query';

const publicationQueries = {
  getPublicationSummary(
    publicationSlug: string,
  ): UseQueryOptions<PublicationSummary> {
    return {
      queryKey: ['publicationSummary', publicationSlug],
      queryFn: () => publicationService.getPublicationSummary(publicationSlug),
    };
  },
  getReleaseVersionSummary(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<ReleaseVersionSummary> {
    return {
      queryKey: ['releaseVersionSummary', publicationSlug, releaseSlug],
      queryFn: () =>
        publicationService.getReleaseVersionSummary(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getReleaseVersionHomeContent(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<ReleaseVersionHomeContent> {
    return {
      queryKey: ['releaseVersionHomeContent', publicationSlug, releaseSlug],
      queryFn: () =>
        publicationService.getReleaseVersionHomeContent(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getReleaseVersionDataContent(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<ReleaseVersionDataContent> {
    return {
      queryKey: ['releaseVersionDataContent', publicationSlug, releaseSlug],
      queryFn: () =>
        publicationService.getReleaseVersionDataContent(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getReleaseVersionRelatedInformation(
    publicationSlug: string,
    releaseSlug: string,
  ): UseQueryOptions<RelatedInformationItem[]> {
    return {
      queryKey: [
        'releaseVersionRelatedInformation',
        publicationSlug,
        releaseSlug,
      ],
      queryFn: () =>
        publicationService.getReleaseVersionRelatedInformation(
          publicationSlug,
          releaseSlug,
        ),
    };
  },
  getPublicationMethodologies(
    publicationSlug: string,
  ): UseQueryOptions<PublicationMethodologiesList> {
    return {
      queryKey: ['publicationMethodologies', publicationSlug],
      queryFn: () =>
        publicationService.getPublicationMethodologies(publicationSlug),
    };
  },
  getPublicationReleaseList(
    publicationSlug: string,
    params?: PaginationRequestParams,
  ): UseQueryOptions<PaginatedList<PublicationReleaseSeriesItem>> {
    return {
      queryKey: ['publicationReleaseList', publicationSlug, params ?? null],
      queryFn: () =>
        publicationService.getPublicationReleaseList(publicationSlug, params),
    };
  },
  getPublicationTree(query: PublicationTreeOptions): UseQueryOptions<Theme[]> {
    return {
      queryKey: ['publicationTree', query],
      queryFn: () => publicationService.getPublicationTree(query),
    };
  },
  listReleases(publicationSlug: string): UseQueryOptions<ReleaseSummary[]> {
    return {
      queryKey: ['listReleases', publicationSlug],
      queryFn: () => publicationService.listReleases(publicationSlug),
    };
  },
  // TODO remove this once prototype is complete - currently used for demo-ing find data from azure,
  // as that uses data sets from prod
  getPrototypePublicationTree(
    query: PublicationTreeOptions,
  ): UseQueryOptions<Theme[]> {
    return {
      queryKey: ['publicationTreePrototype', query],
      queryFn: () =>
        new Promise(resolve =>
          resolve([
            {
              id: 'cc8e02fd-5599-41aa-940d-26bca68eab53',
              summary:
                'Including children in need and child protection, children looked after and social work workforce statistics',
              title: "Children's social care",
              publications: [
                {
                  id: 'd7bd5d9d-dc65-4b1d-99b1-4d815b7369a3',
                  slug: 'children-accommodated-in-secure-childrens-homes',
                  title: "Children accommodated in secure children's homes",
                  isSuperseded: false,
                },
                {
                  id: '89869bba-0c00-40f7-b7d6-e28cb904ad37',
                  slug: 'children-in-need',
                  title: 'Children in need',
                  isSuperseded: false,
                },
                {
                  id: '7f32ebcd-8f36-4b9b-fdab-08ddd41933e2',
                  slug: 'children-in-need-a-focus-on-re-referrals',
                  title: 'Children in need: A focus on re-referrals',
                  isSuperseded: false,
                },
                {
                  id: '07f9ca49-e12e-4c06-fdac-08ddd41933e2',
                  slug: 'children-in-need-a-focus-on-sexual-abuse-and-exploitation',
                  title:
                    'Children in need: A focus on sexual abuse and exploitation',
                  isSuperseded: false,
                },
                {
                  id: '3260801d-601a-48c6-93b7-cf51680323d1',
                  slug: 'children-looked-after-in-england-including-adoptions',
                  title: 'Children looked after in England including adoptions',
                  isSuperseded: false,
                },
                {
                  id: '25216e7a-729d-4f80-c22c-08ddbe116041',
                  slug: 'children-looked-after-a-focus-on-placement-location',
                  title: 'Children looked after: A focus on placement location',
                  isSuperseded: false,
                },
                {
                  id: 'd8baee79-3c88-45f4-b12a-07b91e9b5c11',
                  slug: 'children-s-social-work-workforce',
                  title: "Children's social work workforce",
                  isSuperseded: false,
                },
                {
                  id: '18576c79-e938-4154-9f12-08da13d30bd1',
                  slug: 'children-s-social-work-workforce-attrition-caseload-and-agency-workforce',
                  title:
                    "Children's social work workforce: attrition, caseload, and agency workforce",
                  isSuperseded: false,
                },
                {
                  id: '98a7cf7e-30d6-4b76-03f5-08da5b671103',
                  slug: 'looked-after-children-aged-16-to-17-in-independent-or-semi-independent-placements',
                  title:
                    'Looked after children aged 16 to 17 in independent or semi-independent placements',
                  isSuperseded: false,
                },
                {
                  id: 'f51895df-c682-45e6-b23e-3138ddbfdaeb',
                  slug: 'outcomes-for-children-in-need-including-children-looked-after-by-local-authorities-in-england',
                  title:
                    'Outcomes for children in need, including children looked after by local authorities in England',
                  isSuperseded: false,
                },
                {
                  id: 'ea42de23-80c0-4609-89fe-91df00c4f249',
                  slug: 'outcomes-of-children-in-need-including-looked-after-children',
                  title:
                    'Outcomes of children in need, including looked after children',
                  supersededBy: {
                    id: 'f51895df-c682-45e6-b23e-3138ddbfdaeb',
                    slug: 'outcomes-for-children-in-need-including-children-looked-after-by-local-authorities-in-england',
                    title:
                      'Outcomes for children in need, including children looked after by local authorities in England',
                  },
                  isSuperseded: true,
                },
                {
                  id: '0c2da30b-0210-49d4-cac0-08d87696e25e',
                  slug: 'serious-incident-notifications',
                  title: 'Serious incident notifications',
                  isSuperseded: false,
                },
                {
                  id: '27c27d79-290d-4cef-e5ec-08dd6e04ec1b',
                  slug: 'stability-measures-for-children-looked-after-in-england',
                  title:
                    'Stability measures for children looked after in England',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: '2eee78b2-e4d5-4046-9866-c6c5b717a96c',
              summary:
                'Including Attendance in education and early years settings during the coronavirus (COVID-19) outbreak',
              title: 'COVID-19',
              publications: [
                {
                  id: '036e2c36-7c48-4a29-8419-a3939be9e173',
                  slug: 'attendance-in-education-and-early-years-settings-during-covid-19',
                  title:
                    'Attendance in education and early years settings during COVID-19',
                  supersededBy: {
                    id: '9676af6b-d563-41f4-d071-08da8f468680',
                    slug: 'pupil-attendance-in-schools',
                    title: 'Pupil attendance in schools',
                  },
                  isSuperseded: true,
                },
                {
                  id: '4b2e6dfd-69c8-42d7-8fac-08d999fbf656',
                  slug: 'co2-monitors-cumulative-delivery-statistics',
                  title: 'CO2 monitors: cumulative delivery statistics',
                  isSuperseded: false,
                },
                {
                  id: '359958a5-b28a-4ab3-508a-08d88f97dffc',
                  slug: 'coronavirus-covid-19-reporting-in-higher-education-providers',
                  title:
                    'Coronavirus (COVID-19) Reporting in Higher Education Providers',
                  isSuperseded: false,
                },
                {
                  id: '6d2ff07c-7ea9-43c0-6937-08d8cddbb024',
                  slug: 'covid-mass-testing-data-in-education',
                  title: 'COVID mass testing data in education',
                  isSuperseded: false,
                },
                {
                  id: '1c45fcd7-e1ca-44d2-b4c8-08d9ec8bbabe',
                  slug: 'delivery-of-air-cleaning-units',
                  title: 'Delivery of air cleaning units',
                  isSuperseded: false,
                },
                {
                  id: '4d747ecd-10cc-4660-bcdc-08d8b9339e60',
                  slug: 'laptops-and-tablets-data',
                  title: 'Laptops and tablets data',
                  isSuperseded: false,
                },
                {
                  id: '7718a5b8-da1c-466f-d350-08daa14d393a',
                  slug: 'vulnerable-children-and-young-people-survey',
                  title: 'Vulnerable children and young people survey',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: '6412a76c-cf15-424f-8ebc-3a530132b1b3',
              summary:
                'Including not in education, employment or training (NEET) statistics',
              title: 'Destination of pupils and students',
              publications: [
                {
                  id: '657a20f6-13ef-494f-c9c0-08d82a49a1d0',
                  slug: '16-18-destination-measures',
                  title: '16-18 destination measures',
                  isSuperseded: false,
                },
                {
                  id: '8b12776b-3d36-4475-8115-00974d7de1d0',
                  slug: 'further-education-outcomes',
                  title: 'Further education outcomes',
                  isSuperseded: false,
                },
                {
                  id: 'b70e71fa-5767-4fb5-c9bf-08d82a49a1d0',
                  slug: 'key-stage-4-destination-measures',
                  title: 'Key stage 4 destination measures',
                  isSuperseded: false,
                },
                {
                  id: '2ee2b32a-3fa0-42bb-c9c2-08d82a49a1d0',
                  slug: 'longer-term-destinations',
                  title: 'Longer term destinations',
                  isSuperseded: false,
                },
                {
                  id: '2e510281-ca8c-41bf-bbe0-fd15fcc81aae',
                  slug: 'neet-statistics-annual-brief',
                  title: 'NEET age 16 to 24',
                  isSuperseded: false,
                },
                {
                  id: 'a0eb117e-44a8-4732-adf1-8fbc890cbb62',
                  slug: 'participation-in-education-and-training-and-employment',
                  title:
                    'Participation in education, training and employment age 16 to 18',
                  isSuperseded: false,
                },
                {
                  id: '9f5d0e21-45d3-41e5-1b71-08da5e798404',
                  slug: 'participation-in-education-training-and-neet-age-16-to-17-by-local-authority',
                  title:
                    'Participation in education, training and NEET age 16 to 17 by local authority',
                  isSuperseded: false,
                },
                {
                  id: '61784b00-d1e7-4dbd-c9c1-08d82a49a1d0',
                  slug: 'progression-to-higher-education-or-training',
                  title: 'Progression to higher education or training',
                  isSuperseded: false,
                },
                {
                  id: 'cb160073-139b-4069-3478-08dad90fbd2f',
                  slug: 'september-guarantee-offers-of-education-and-training-for-young-people-age-16-and-17',
                  title:
                    'September Guarantee: offers of education and training for young people age 16 and 17',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: 'e6e31160-fe79-4556-f3a9-08d86094b9e8',
              summary:
                'Including early years foundation stage profile and early years surveys statistics',
              title: 'Early years',
              publications: [
                {
                  id: '79a08466-dace-4ff0-94b6-59c5528c9262',
                  slug: 'childcare-and-early-years-provider-survey',
                  title: 'Childcare and early years provider survey',
                  isSuperseded: false,
                },
                {
                  id: '060c5376-35d8-420b-8266-517a9339b7bc',
                  slug: 'childcare-and-early-years-survey-of-parents',
                  title: 'Childcare and early years survey of parents',
                  isSuperseded: false,
                },
                {
                  id: '2b4fdd0f-e58f-4368-8d66-08dc0dee4273',
                  slug: 'early-years-education-recovery',
                  title: 'Early years education recovery',
                  isSuperseded: false,
                },
                {
                  id: 'fcda2962-82a6-4052-afa2-ea398c53c85f',
                  slug: 'early-years-foundation-stage-profile-results',
                  title: 'Early years foundation stage profile results',
                  isSuperseded: false,
                },
                {
                  id: 'f2e7f229-f7b0-45ab-624f-08dd83281976',
                  slug: 'expansion-to-early-childcare-entitlements-childcare-experiences-survey',
                  title:
                    'Expansion to early childcare entitlements: Childcare Experiences Survey',
                  isSuperseded: false,
                },
                {
                  id: '8042c52d-16d4-4b7b-66f5-08dc1b6b9886',
                  slug: 'expansion-to-early-childcare-entitlements-eligibility-codes-issued-and-validated',
                  title:
                    'Expansion to early childcare entitlements: eligibility codes issued and validated',
                  isSuperseded: false,
                },
                {
                  id: '0ce6a6c6-5451-4967-8dd4-2f4fa8131982',
                  slug: 'funded-early-education-and-childcare',
                  title: 'Funded early education and childcare',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: 'bc08839f-2970-4f34-af2d-29608a48082f',
              summary:
                'Including local authority (LA) and student loan statistics',
              title: 'Finance and funding',
              publications: [
                {
                  id: 'dcb8b32b-4e50-4fe2-a539-58f9b6b3a366',
                  slug: 'la-and-school-expenditure',
                  title: 'LA and school expenditure',
                  isSuperseded: false,
                },
                {
                  id: '94d16c6e-1e5f-48d5-8195-8ea770f1b0d4',
                  slug: 'planned-la-and-school-expenditure',
                  title: 'Planned LA and school expenditure',
                  isSuperseded: false,
                },
                {
                  id: 'bdbab446-c985-4b44-d072-08da8f468680',
                  slug: 'school-finances-during-the-covid-19-pandemic',
                  title: 'School finances during the Covid-19 pandemic',
                  isSuperseded: false,
                },
                {
                  id: '39dce2e3-4976-41ac-cb95-08d8bdf1a994',
                  slug: 'school-funding-statistics',
                  title: 'School funding statistics',
                  isSuperseded: false,
                },
                {
                  id: 'fd68e147-b7ee-464f-8b02-dcd917dc362d',
                  slug: 'student-loan-forecasts-for-england',
                  title: 'Student loan forecasts for England',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: '92c5df93-c4da-4629-ab25-51bd2920cdca',
              summary:
                'Including advanced learner loan, benefit claimant and apprenticeship and traineeship statistics',
              title: 'Further education',
              publications: [
                {
                  id: '412d8090-ab45-455a-c176-08dbf5ab522b',
                  slug: 'apprenticeships',
                  title: 'Apprenticeships',
                  isSuperseded: false,
                },
                {
                  id: 'b2eed9e7-3845-47ab-e9a9-08d9a8ec2f9e',
                  slug: 'apprenticeships-and-19-plus-further-education-skills-index',
                  title:
                    'Apprenticeships and 19-plus Further Education Skills Index',
                  isSuperseded: false,
                },
                {
                  id: 'cf0ec981-3583-42a5-b21b-3f2f32008f1b',
                  slug: 'apprenticeships-and-traineeships',
                  title: 'Apprenticeships and traineeships',
                  supersededBy: {
                    id: '412d8090-ab45-455a-c176-08dbf5ab522b',
                    slug: 'apprenticeships',
                    title: 'Apprenticeships',
                  },
                  isSuperseded: true,
                },
                {
                  id: 'c858107c-1b98-4818-2731-08d8a11fd8ef',
                  slug: 'apprenticeships-in-england-by-industry-characteristics',
                  title:
                    'Apprenticeships in England by industry characteristics ',
                  isSuperseded: false,
                },
                {
                  id: '9c2209b2-a702-439b-bece-08da1714e874',
                  slug: 'career-pathways-post-16-qualifications-held-by-employees',
                  title:
                    'Career pathways: post-16 qualifications held by employees',
                  isSuperseded: false,
                },
                {
                  id: 'a2778204-c541-404a-85aa-08da115e8407',
                  slug: 'detailed-destinations-of-16-to-18-year-olds-in-further-education',
                  title:
                    'Detailed destinations of 16 to 18 year olds in Further Education',
                  isSuperseded: false,
                },
                {
                  id: '1aa10012-ee64-4ece-f4b1-08d8b7d3c33b',
                  slug: 'fe-learners-going-into-employment-and-learning-destinations-by-local-authority-district',
                  title:
                    'FE learners going into employment and learning destinations by local authority district',
                  isSuperseded: false,
                },
                {
                  id: '13b81bcb-e8cd-4431-9807-ca588fd1d02a',
                  slug: 'further-education-and-skills',
                  title: 'Further education and skills',
                  isSuperseded: false,
                },
                {
                  id: '9ddd7d28-a0cd-40e9-74a9-08de751c53dd',
                  slug: 'further-education-college-workforce-using-teacher-pension-data',
                  title:
                    'Further education college workforce using teacher pension data',
                  isSuperseded: false,
                },
                {
                  id: '11d3385f-8d48-4e07-1255-08db886b552e',
                  slug: 'further-education-workforce',
                  title: 'Further education workforce',
                  isSuperseded: false,
                },
                {
                  id: 'ffc387d9-0836-435b-30d6-08dacbc13895',
                  slug: 'skills-bootcamps-starts',
                  title: 'Skills Bootcamps starts',
                  supersededBy: {
                    id: 'ce8ef30f-1b14-45d4-4854-08d99eeed35d',
                    slug: 'skills-bootcamps-starts-completions-and-outcomes',
                    title: 'Skills bootcamps starts, completions and outcomes',
                  },
                  isSuperseded: true,
                },
                {
                  id: 'ce8ef30f-1b14-45d4-4854-08d99eeed35d',
                  slug: 'skills-bootcamps-starts-completions-and-outcomes',
                  title: 'Skills bootcamps starts, completions and outcomes',
                  isSuperseded: false,
                },
                {
                  id: '1d44a317-2128-4359-84c3-08dc5dea63ea',
                  slug: 'supply-of-skills-for-jobs-in-science-and-technology',
                  title: 'Supply of skills for jobs in science and technology',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: '2ca22e34-b87a-4281-a0eb-b80f4f8dd374',
              summary:
                'Including university graduate employment, graduate labour market and participation statistics',
              title: 'Higher education',
              publications: [
                {
                  id: '8008b2e4-a23f-459b-1ec9-08dad132b8b4',
                  slug: 'foundation-years-statistics',
                  title:
                    'Foundation year participation, provision and outcomes at HE providers',
                  isSuperseded: false,
                },
                {
                  id: '42a888c4-9ee7-40fd-9128-f5de546780b3',
                  slug: 'graduate-labour-markets',
                  title: 'Graduate labour market statistics',
                  isSuperseded: false,
                },
                {
                  id: '2d629fad-d66d-46d8-849d-08d8b249ed05',
                  slug: 'graduate-outcomes-leo',
                  title: 'Graduate outcomes (LEO)',
                  supersededBy: {
                    id: 'b329f8e4-4191-4f0c-66a8-08d9daa3b093',
                    slug: 'leo-graduate-and-postgraduate-outcomes',
                    title: 'LEO Graduate and Postgraduate Outcomes',
                  },
                  isSuperseded: true,
                },
                {
                  id: '4d29c28c-efd1-4245-a80c-b55c6a50e3f7',
                  slug: 'graduate-outcomes-leo-postgraduate-outcomes',
                  title: 'Graduate outcomes (LEO): postgraduate outcomes',
                  supersededBy: {
                    id: 'b329f8e4-4191-4f0c-66a8-08d9daa3b093',
                    slug: 'leo-graduate-and-postgraduate-outcomes',
                    title: 'LEO Graduate and Postgraduate Outcomes',
                  },
                  isSuperseded: true,
                },
                {
                  id: '4fc3a5e3-7f33-49a7-028e-08dc39e23fcc',
                  slug: 'higher-education-entrants-and-qualifiers-by-their-level-2-and-3-attainment',
                  title:
                    'Higher Education Entrants and Qualifiers by their Level 2 and 3 Attainment',
                  isSuperseded: false,
                },
                {
                  id: 'e926dcde-e610-4721-ea39-08d968ac9105',
                  slug: 'higher-level-learners-in-england',
                  title: 'Higher Level Learners in England',
                  isSuperseded: false,
                },
                {
                  id: 'b329f8e4-4191-4f0c-66a8-08d9daa3b093',
                  slug: 'leo-graduate-and-postgraduate-outcomes',
                  title: 'LEO Graduate and Postgraduate Outcomes',
                  isSuperseded: false,
                },
                {
                  id: 'f27b380e-98a6-4b1a-9d98-f7b7a5392032',
                  slug: 'graduate-outcomes-leo-provider-level-data',
                  title: 'LEO Graduate outcomes provider level data',
                  isSuperseded: false,
                },
                {
                  id: '0c67bbdb-4eb0-41cf-a62e-2589cee58538',
                  slug: 'participation-measures-in-higher-education',
                  title: 'Participation measures in higher education',
                  isSuperseded: false,
                },
                {
                  id: '04b59c44-bfb8-4d7d-841d-08d9a365444f',
                  slug: 'uk-revenue-from-education-related-exports-and-transnational-education-activity',
                  title:
                    'UK revenue from education related exports and transnational education activity',
                  isSuperseded: false,
                },
                {
                  id: 'c28f7aca-f1e8-4916-8ce3-fc177b140695',
                  slug: 'widening-participation-in-higher-education',
                  title: 'Widening participation in higher education',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: '291cb3a2-4a29-4ff6-db25-08dcc348573e',
              summary: 'Including occupations in demand',
              title: 'Labour market and skills',
              publications: [
                {
                  id: '36097731-f107-4e6c-e2e7-08dcc3489646',
                  slug: 'occupations-in-demand',
                  title: 'Occupations in demand',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: 'ee1855ca-d1e1-4f04-a795-cbd61d326a1f',
              summary:
                'Including absence, application and offers, capacity, exclusion and special educational needs (SEN) statistics',
              title: 'Pupils and schools',
              publications: [
                {
                  id: '4e9eb1f5-8440-4e39-2d3e-08d93a1d8d28',
                  slug: 'academy-transfers-and-funding',
                  title: 'Academy transfers and funding',
                  isSuperseded: false,
                },
                {
                  id: '123461ab-50be-45d9-8523-c5241a2c9c5b',
                  slug: 'admission-appeals-in-england',
                  title: 'Admission appeals in England',
                  isSuperseded: false,
                },
                {
                  id: '34a2c514-d603-4246-f47b-08db2568bd10',
                  slug: 'children-missing-education',
                  title: 'Children missing education',
                  isSuperseded: false,
                },
                {
                  id: '8a3aafe8-4191-4a62-f479-08db2568bd10',
                  slug: 'education-children-s-social-care-and-offending-local-authority-level-dashboard',
                  title:
                    'Education, children’s social care and offending: local authority level dashboard',
                  isSuperseded: false,
                },
                {
                  id: '88312cc0-fe1d-4ab5-81df-33fd708185cb',
                  slug: 'education-health-and-care-plans',
                  title: 'Education, health and care plans',
                  isSuperseded: false,
                },
                {
                  id: 'a5b2d325-d8a2-4cad-f47a-08db2568bd10',
                  slug: 'elective-home-education',
                  title: 'Elective home education',
                  isSuperseded: false,
                },
                {
                  id: '23a9962f-37ef-4b1c-8077-08dda2b70e1a',
                  slug: 'estimate-of-additional-children-claiming-free-school-meals-following-expansion-of-eligibility',
                  title:
                    'Estimate of additional children claiming Free School Meals following expansion of eligibility',
                  isSuperseded: false,
                },
                {
                  id: '346a6978-9ee5-4b63-0ce1-08d88fd5ace1',
                  slug: 'free-school-meals-autumn-term',
                  title: 'Free school meals: Autumn term',
                  supersededBy: {
                    id: 'a91d9e05-be82-474c-85ae-4913158406d0',
                    slug: 'school-pupils-and-their-characteristics',
                    title: 'Schools, pupils and their characteristics',
                  },
                  isSuperseded: true,
                },
                {
                  id: '5c066362-9e14-4688-4f8a-08d83303033f',
                  slug: 'local-authority-school-places-scorecards',
                  title: 'Local authority school places scorecards',
                  isSuperseded: false,
                },
                {
                  id: 'aa545525-9ffe-496c-a5b3-974ace56746e',
                  slug: 'national-pupil-projections',
                  title: 'National pupil projections',
                  isSuperseded: false,
                },
                {
                  id: '71cfcea3-359c-4089-722e-08da07fcb6c9',
                  slug: 'national-tutoring-programme',
                  title: 'National Tutoring Programme',
                  isSuperseded: false,
                },
                {
                  id: '86af24dc-67c4-47f0-a849-e94c7a1cfe9b',
                  slug: 'parental-responsibility-measures',
                  title: 'Parental responsibility measures',
                  isSuperseded: false,
                },
                {
                  id: '66c8e9db-8bf2-4b0b-b094-cfab25c20b05',
                  slug: 'primary-and-secondary-school-applications-and-offers',
                  title: 'Primary and secondary school applications and offers',
                  isSuperseded: false,
                },
                {
                  id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
                  slug: 'pupil-absence-in-schools-in-england',
                  title: 'Pupil absence in schools in England',
                  isSuperseded: false,
                },
                {
                  id: '14953fda-02ff-45ed-9573-3a7a0ad8cb10',
                  slug: 'pupil-absence-in-schools-in-england-autumn-and-spring-terms',
                  title:
                    'Pupil absence in schools in England: autumn and spring terms',
                  supersededBy: {
                    id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
                    slug: 'pupil-absence-in-schools-in-england',
                    title: 'Pupil absence in schools in England',
                  },
                  isSuperseded: true,
                },
                {
                  id: '6c388293-d027-4f74-8d74-29a42e02231c',
                  slug: 'pupil-absence-in-schools-in-england-autumn-term',
                  title: 'Pupil absence in schools in England: autumn term',
                  supersededBy: {
                    id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
                    slug: 'pupil-absence-in-schools-in-england',
                    title: 'Pupil absence in schools in England',
                  },
                  isSuperseded: true,
                },
                {
                  id: '9676af6b-d563-41f4-d071-08da8f468680',
                  slug: 'pupil-attendance-in-schools',
                  title: 'Pupil attendance in schools',
                  isSuperseded: false,
                },
                {
                  id: 'f022fa36-ebb9-4e88-acd9-08d870f85341',
                  slug: 'pupil-yield-from-housing-developments',
                  title: 'Pupil yield from housing developments',
                  isSuperseded: false,
                },
                {
                  id: 'fa591a15-ae37-41b5-98f6-4ce06e5225f4',
                  slug: 'school-capacity',
                  title: 'School capacity',
                  isSuperseded: false,
                },
                {
                  id: '26d77b31-e11a-4ef1-866d-08da3d724347',
                  slug: 'school-placements-for-children-from-outside-of-the-uk',
                  title:
                    'School placements for children from outside of the UK',
                  isSuperseded: false,
                },
                {
                  id: '4cd161ca-468f-4023-9b91-08d8d4cdc4e0',
                  slug: 'school-places-sufficiency-survey',
                  title: 'School places sufficiency survey',
                  isSuperseded: false,
                },
                {
                  id: '95b66a3e-6b1c-43f0-25a6-08dd868ff134',
                  slug: 'schools-eligible-for-rise-intervention',
                  title: 'Schools eligible for RISE intervention',
                  isSuperseded: false,
                },
                {
                  id: 'a91d9e05-be82-474c-85ae-4913158406d0',
                  slug: 'school-pupils-and-their-characteristics',
                  title: 'Schools, pupils and their characteristics',
                  isSuperseded: false,
                },
                {
                  id: 'f657afb4-8f4a-427d-a683-15f11a2aefb5',
                  slug: 'special-educational-needs-in-england',
                  title: 'Special educational needs in England',
                  isSuperseded: false,
                },
                {
                  id: 'bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9',
                  slug: 'suspensions-and-permanent-exclusions-in-england',
                  title: 'Suspensions and permanent exclusions in England',
                  isSuperseded: false,
                },
                {
                  id: '729bac29-9204-4b9f-2426-08d9fdd01d19',
                  slug: 'the-link-between-absence-and-attainment-at-ks2-and-ks4',
                  title:
                    'The link between absence and attainment at KS2 and KS4',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: '74648781-85a9-4233-8be3-fe6f137165f4',
              summary: 'Including GCSE and key stage statistics',
              title: 'School and college outcomes and performance',
              publications: [
                {
                  id: '3f3a66ec-5777-42ee-b427-8102a14ce0c5',
                  slug: 'a-level-and-other-16-to-18-results',
                  title: 'A level and other 16 to 18 results',
                  isSuperseded: false,
                },
                {
                  id: 'bc4815bf-f3dc-477b-7559-08da5f64dcf1',
                  slug: 'key-stage-1-and-phonics-screening-check-attainment',
                  title: 'Key stage 1 and phonics screening check attainment',
                  supersededBy: {
                    id: '5becb18e-852b-4cdf-e2e8-08dcc3489646',
                    slug: 'phonics-screening-check-attainment',
                    title: 'Phonics screening check attainment',
                  },
                  isSuperseded: true,
                },
                {
                  id: '8b7474f9-5870-4ecc-7557-08da5f64dcf1',
                  slug: 'key-stage-2-attainment',
                  title: 'Key stage 2 attainment',
                  isSuperseded: false,
                },
                {
                  id: '10370062-93b0-4dde-9097-5a56bf5b3064',
                  slug: 'key-stage-2-attainment-national-headlines',
                  title: 'Key stage 2 attainment: National headlines',
                  isSuperseded: false,
                },
                {
                  id: 'c8756008-ed50-4632-9b96-01b5ca002a43',
                  slug: 'key-stage-4-performance',
                  title: 'Key stage 4 performance',
                  isSuperseded: false,
                },
                {
                  id: '2e95f880-629c-417b-981f-0901e97776ff',
                  slug: 'level-2-and-3-attainment-by-young-people-aged-19',
                  title: 'Level 2 and 3 attainment age 16 to 25',
                  isSuperseded: false,
                },
                {
                  id: 'd61988ae-f325-45df-8246-08dac639349a',
                  slug: 'multi-academy-trust-performance-measures-key-stages-2-4-and-5',
                  title:
                    'Multi-academy trust performance measures (Key stages 2, 4 and 5)',
                  supersededBy: {
                    id: '8b7474f9-5870-4ecc-7557-08da5f64dcf1',
                    slug: 'key-stage-2-attainment',
                    title: 'Key stage 2 attainment',
                  },
                  isSuperseded: true,
                },
                {
                  id: 'eab51107-4ef0-4926-8f8b-c8bd7f5a21d5',
                  slug: 'multi-academy-trust-performance-measures-at-key-stage-2',
                  title:
                    'Multi-academy trust performance measures at key stage 2',
                  supersededBy: {
                    id: 'd61988ae-f325-45df-8246-08dac639349a',
                    slug: 'multi-academy-trust-performance-measures-key-stages-2-4-and-5',
                    title:
                      'Multi-academy trust performance measures (Key stages 2, 4 and 5)',
                  },
                  isSuperseded: true,
                },
                {
                  id: '13484f09-40e7-4b8b-7558-08da5f64dcf1',
                  slug: 'multiplication-tables-check-attainment',
                  title: 'Multiplication tables check attainment',
                  isSuperseded: false,
                },
                {
                  id: '5becb18e-852b-4cdf-e2e8-08dcc3489646',
                  slug: 'phonics-screening-check-attainment',
                  title: 'Phonics screening check attainment',
                  isSuperseded: false,
                },
                {
                  id: 'd6d6ef58-2fad-4ef4-1c62-08da6faca262',
                  slug: 'provisional-t-level-results',
                  title: 'Provisional T Level results',
                  isSuperseded: false,
                },
                {
                  id: 'b99702f4-a277-4f1b-eba8-08dd9f501d22',
                  slug: 'school-counts-by-average-progress-8-scores-for-disadvantaged-white-british-pupils',
                  title:
                    'School counts by average Progress 8 scores for disadvantaged White British pupils',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: 'b601b9ea-b1c7-4970-b354-d1f695c446f1',
              summary: 'Including initial teacher training (ITT) statistics',
              title: 'Teachers and school workforce',
              publications: [
                {
                  id: '4946c7e4-5ab4-48d4-5932-08db04473582',
                  slug: 'ecf-and-npq-starts',
                  title: 'ECF and NPQ starts',
                  isSuperseded: false,
                },
                {
                  id: '465d5a87-dba8-49ee-b7e3-08d86b7beb8f',
                  slug: 'initial-teacher-training-census',
                  title: 'Initial Teacher Training Census',
                  isSuperseded: false,
                },
                {
                  id: 'd34978d5-0317-46bc-9258-13412270ac4d',
                  slug: 'initial-teacher-training-performance-profiles',
                  title: 'Initial teacher training performance profiles',
                  isSuperseded: false,
                },
                {
                  id: '5dbac9b4-522f-46fa-20fd-08dc73fd4523',
                  slug: 'median-teacher-pay-using-teacher-pension-scheme-data',
                  title: 'Median teacher pay using teacher pension scheme data',
                  supersededBy: {
                    id: 'b318967f-2931-472a-93f2-fbed1e181e6a',
                    slug: 'school-workforce-in-england',
                    title: 'School workforce in England',
                  },
                  isSuperseded: true,
                },
                {
                  id: '02e502cd-6333-4c28-a535-08da004ba44f',
                  slug: 'postgraduate-initial-teacher-training-targets',
                  title: 'Postgraduate initial teacher training targets',
                  isSuperseded: false,
                },
                {
                  id: '8132cffb-8ecb-4f7c-d1a0-08ddec782b96',
                  slug: 'school-leadership-retention',
                  title: 'School Leadership retention',
                  isSuperseded: false,
                },
                {
                  id: 'b318967f-2931-472a-93f2-fbed1e181e6a',
                  slug: 'school-workforce-in-england',
                  title: 'School workforce in England',
                  isSuperseded: false,
                },
                {
                  id: 'b3134103-1297-4758-4654-08da2e6fc320',
                  slug: 'teacher-and-leader-development-ecf-and-npqs',
                  title: 'Teacher and leader development: ECF and NPQs',
                  isSuperseded: false,
                },
                {
                  id: 'baaa0baa-a1cc-4400-87be-08de79e5cea5',
                  slug: 'teacher-demand-and-postgraduate-trainee-need',
                  title: 'Teacher demand and postgraduate trainee need',
                  isSuperseded: false,
                },
              ],
            },
            {
              id: 'a95d2ca2-a969-4320-b1e9-e4781112574a',
              summary:
                'Including summarised expenditure, post-compulsory education, qualification and school statistics',
              title: 'UK education and training statistics',
              publications: [
                {
                  id: '2ffbc8d3-eb53-4c4b-a6fb-219a5b95ebc8',
                  slug: 'education-and-training-statistics-for-the-uk',
                  title: 'Education and training statistics for the UK',
                  isSuperseded: false,
                },
                {
                  id: '3c1ba528-aa57-4b9c-347a-08dad90fbd2f',
                  slug: 'employer-skills-survey',
                  title: 'Employer Skills Survey ',
                  isSuperseded: false,
                },
              ],
            },
          ]),
        ),
    };
  },
} as const;

export default publicationQueries;
