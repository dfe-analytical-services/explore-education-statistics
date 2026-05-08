import themeService, { ThemeSummary } from '@common/services/themeService';
import { UseQueryOptions } from '@tanstack/react-query';

const themeQueries = {
  list(): UseQueryOptions<ThemeSummary[]> {
    return {
      queryKey: ['listThemes'],
      queryFn: () => themeService.listThemes(),
    };
  },
  // TODO remove this once prototype is complete - currently used for demo-ing find data from azure,
  // as that uses data sets from prod
  listProdThemes(): UseQueryOptions<ThemeSummary[]> {
    return {
      queryKey: ['listThemesProd'],
      queryFn: () =>
        new Promise(resolve =>
          resolve([
            {
              id: 'cc8e02fd-5599-41aa-940d-26bca68eab53',
              slug: 'children-s-social-care',
              title: "Children's social care",
              summary:
                'Including children in need and child protection, children looked after and social work workforce statistics',
            },
            {
              id: '2eee78b2-e4d5-4046-9866-c6c5b717a96c',
              slug: 'covid-19',
              title: 'COVID-19',
              summary:
                'Including Attendance in education and early years settings during the coronavirus (COVID-19) outbreak',
            },
            {
              id: '6412a76c-cf15-424f-8ebc-3a530132b1b3',
              slug: 'destination-of-pupils-and-students',
              title: 'Destination of pupils and students',
              summary:
                'Including not in education, employment or training (NEET) statistics',
            },
            {
              id: 'e6e31160-fe79-4556-f3a9-08d86094b9e8',
              slug: 'early-years',
              title: 'Early years',
              summary:
                'Including early years foundation stage profile and early years surveys statistics',
            },
            {
              id: 'bc08839f-2970-4f34-af2d-29608a48082f',
              slug: 'finance-and-funding',
              title: 'Finance and funding',
              summary:
                'Including local authority (LA) and student loan statistics',
            },
            {
              id: '92c5df93-c4da-4629-ab25-51bd2920cdca',
              slug: 'further-education',
              title: 'Further education',
              summary:
                'Including advanced learner loan, benefit claimant and apprenticeship and traineeship statistics',
            },
            {
              id: '2ca22e34-b87a-4281-a0eb-b80f4f8dd374',
              slug: 'higher-education',
              title: 'Higher education',
              summary:
                'Including university graduate employment, graduate labour market and participation statistics',
            },
            {
              id: '291cb3a2-4a29-4ff6-db25-08dcc348573e',
              slug: 'labour-market-and-skills',
              title: 'Labour market and skills',
              summary: 'Including occupations in demand',
            },
            {
              id: 'ee1855ca-d1e1-4f04-a795-cbd61d326a1f',
              slug: 'pupils-and-schools',
              title: 'Pupils and schools',
              summary:
                'Including absence, application and offers, capacity, exclusion and special educational needs (SEN) statistics',
            },
            {
              id: '74648781-85a9-4233-8be3-fe6f137165f4',
              slug: 'school-and-college-performance',
              title: 'School and college outcomes and performance',
              summary: 'Including GCSE and key stage statistics',
            },
            {
              id: 'b601b9ea-b1c7-4970-b354-d1f695c446f1',
              slug: 'teachers-and-school-workforce',
              title: 'Teachers and school workforce',
              summary: 'Including initial teacher training (ITT) statistics',
            },
            {
              id: 'a95d2ca2-a969-4320-b1e9-e4781112574a',
              slug: 'uk-education-and-training-statistics',
              title: 'UK education and training statistics',
              summary:
                'Including summarised expenditure, post-compulsory education, qualification and school statistics',
            },
          ]),
        ),
    };
  },
} as const;

export default themeQueries;
