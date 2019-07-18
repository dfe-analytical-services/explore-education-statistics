import { GeographicLevel } from '@common/services/dataBlockService';
import { Release } from '@common/services/publicationService';

const LOREM =
  'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec elementum, mauris eget vulputate iaculis, dui orci efficitur mi, at consectetur metus lorem tempor neque. Etiam in eleifend magna. Sed hendrerit vitae ante at semper. Mauris a erat a ex porta mollis. Aliquam quis justo eu lectus luctus porttitor nec at dolor. Nunc interdum, diam sed lobortis porta, massa arcu volutpat nunc, eget scelerisque arcu neque vel tortor. Fusce sit amet mauris augue. Praesent sed urna vel lacus suscipit mollis id quis nulla. Duis porta sapien et arcu ornare, eget mollis justo finibus. Nunc commodo felis justo, at efficitur purus mattis in. Donec nibh quam, mollis at eros ac, fringilla porta mi.';

// const LOREM_SMALL = "Lorem ipsum dolor sit ame";

export default class PrototypePublicationService {
  public static getLatestPublicationRelease(_: string): Promise<Release> {
    // @ts-ignore
    return Promise.resolve({
      id: '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5',
      title: 'Pupil absence data and statistics for schools in England',
      releaseName: '2016 to 2017',
      published: '2017-03-22T00:00:00',
      slug: '2016-17',
      summary:
        'Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas. \n\n',
      publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
      publication: {
        id: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
        slug: 'pupil-absence-in-schools-in-england',
        title: 'Pupil absence in schools in England',
        description: null,
        dataSource:
          '[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)',
        summary:
          'View statistics, create charts and tables and download data files for authorised, overall, persistent and unauthorised absence',
        nextUpdate: '2018-03-22T00:00:00',
        releases: [
          {
            id: '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5',
            title: 'Pupil absence data and statistics for schools in England',
            releaseName: '2016 to 2017',
            published: '2017-03-22T00:00:00',
            slug: '2016-17',
            summary:
              'Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas. \n\n',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
            updates: [
              {
                id: '9c0f0139-7f88-4750-afe0-1c85cdf1d047',
                releaseId: '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5',
                on: '2017-04-19T00:00:00',
                reason:
                  'Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document.',
              },
              {
                id: '18e0d40e-bdf7-4c84-99dd-732e72e9c9a5',
                releaseId: '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5',
                on: '2017-03-22T00:00:00',
                reason: 'First published.',
              },
            ],
            content: null,
            keyStatistics: null,
          },
          {
            id: 'f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d',
            title: 'Pupil absence data and statistics for schools in England',
            releaseName: '2015 to 2016',
            published: '2016-03-25T00:00:00',
            slug: '2015-16',
            summary:
              'Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
            updates: null,
            content: null,
            keyStatistics: null,
          },
        ],
        legacyReleases: [
          {
            id: '45bc02ff-de90-489b-b78e-cdc7db662353',
            description: '2014 to 2015',
            url:
              'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
          },
          {
            id: '82292fe7-1545-44eb-a094-80c5064701a7',
            description: '2013 to 2014',
            url:
              'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
          },
          {
            id: '6907625d-0c2e-4fd8-8e96-aedd85b2ff97',
            description: '2012 to 2013',
            url:
              'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
          },
          {
            id: 'a538e57a-da5e-4a2c-a89e-b74dbae0c30b',
            description: '2011 to 2012',
            url:
              'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
          },
          {
            id: '18b24d60-c56e-44f0-8baa-6db4c6e7deee',
            description: '2010 to 2011',
            url:
              'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
          },
          {
            id: 'c5444f5a-6ba5-4c80-883c-6bca0d8a9eb5',
            description: '2009 to 2010',
            url:
              'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010',
            publicationId: 'cbbd299f-8297-44bc-92ac-558bcf51f8ad',
          },
        ],
        topicId: '1003fa5c-b60a-4036-a178-e3a69a81b852',
        topic: null,
      },
      updates: [
        {
          id: '9c0f0139-7f88-4750-afe0-1c85cdf1d047',
          releaseId: '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5',
          on: '2017-04-19T00:00:00',
          reason:
            'Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document.',
        },
        {
          id: '18e0d40e-bdf7-4c84-99dd-732e72e9c9a5',
          releaseId: '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5',
          on: '2017-03-22T00:00:00',
          reason: 'First published.',
        },
      ],
      content: [
        {
          order: 1,
          heading: 'About this release',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                'This statistical first release (SFR) reports on absence of pupils of compulsory school age in state-funded primary, secondary and special schools during the 2016/17 academic year. Information on absence in pupil referral units, and for pupils aged four, is also included. The Department uses two key measures to monitor pupil absence – overall and persistent absence. Absence by reason and pupils characteristics is also included in this release. Figures are available at national, regional, local authority and school level. Figures held in this release are used for policy development as key indicators in behaviour and school attendance policy. Schools and local authorities also use the statistics to compare their local absence rates to regional and national averages for different pupil groups.',
            },
          ],
        },
        {
          order: 2,
          heading: 'Absence rates',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                'The overall absence rate across state-funded primary, secondary and special schools increased from 4.6 per cent in 2015/16 to 4.7 per cent in 2016/17. In primary schools the overall absence rate stayed the same at 4 per cent and the rate in secondary schools increased from 5.2 per cent to 5.4 per cent. Absence in special schools is much higher at 9.7 per cent in 2016/17\n\nThe increase in overall absence rate has been driven by an increase in the unauthorised absence rate across state-funded primary, secondary and special schools - which increased from 1.1 per cent to 1.3 per cent between 2015/16 and 2016/17.\n\nLooking at longer-term trends, overall and authorised absence rates have been fairly stable over recent years after decreasing gradually between 2006/07 and 2013/14. Unauthorised absence rates have not varied much since 2006/07, however the unauthorised absence rate is now at its highest since records began, at 1.3 per cent.\n\nThis increase in unauthorised absence is due to an increase in absence due to family holidays that were not agreed by the school. The authorised absence rate has not changed since last year, at 3.4 per cent. Though in primary schools authorised absence rates have been decreasing across recent years.\n\nThe total number of days missed due to overall absence across state-funded primary, secondary and special schools has increased since last year, from 54.8 million in 2015/16 to 56.7 million in 2016/17. This partly reflects the rise in the total number of pupil enrolments, the average number of days missed per enrolment has increased very slightly from 8.1 days in 2015/16 to 8.2 days in 2016/17.\n\nIn 2016/17, 91.8 per cent of pupils in state-funded primary, state-funded secondary and special schools missed at least one session during the school year, this is similar to the previous year (91.7 per cent in 2015/16).',
            },
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.DataBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'DataBlock',
              heading: null,
              dataQuery: {
                path: '/api/tablebuilder/characteristics/national',
                method: 'POST',
                body:
                  '{ "indicators": [ "num_schools", "enrolments", "sess_overall_percent", "sess_unauthorised_percent", "sess_authorised_percent" ], "characteristics": [ "Total" ], "endYear": 201617, "publicationId": "cbbd299f-8297-44bc-92ac-558bcf51f8ad", "schoolTypes": [ "Total" ], "startYear": 201213}',
              },
              charts: [
                {
                  $type:
                    'GovUk.Education.ExploreEducationStatistics.Content.Model.LineChart, GovUk.Education.ExploreEducationStatistics.Content.Api',
                  indicators: [
                    'sess_overall_percent',
                    'sess_unauthorised_percent',
                    'sess_authorised_percent',
                  ],
                  xAxis: { title: 'School Year' },
                  yAxis: { title: 'Absence Rate' },
                  type: 'line',
                },
              ],
              summary: null,
            },
          ],
        },
        {
          order: 3,
          heading: 'Persistent absence',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                'The percentage of enrolments in state-funded primary and state-funded secondary schools that were classified as persistent absentees in 2016/17 was 10.8 per cent. This is up from the equivalent figure of 10.5 per cent in 2015/16 (see Figure 2).\n\nIn 2016/17, persistent absentees accounted for 37.6 per cent of all absence compared to 36.6 per cent in 2015/16. Longer term, there has been a decrease in the proportion of absence that persistent absentees account for – down from 43.3 per cent in 2011/12.\n\nThe overall absence rate for persistent absentees across all schools was 18.1 per cent, nearly four times higher than the rate for all pupils. This is a slight increase from 2015/16, when the overall absence rate for persistent absentees was 17.6 per cent.\n\nPersistent absentees account for almost a third, 31.6 per cent, of all authorised absence and more than half, 53.8 per cent of all unauthorised absence. The rate of illness absences is almost four times higher for persistent absentees compared to other pupils, at 7.6 per cent and 2.0 per cent respectively.',
            },
          ],
        },
        {
          order: 4,
          heading: 'Reasons for absence',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.InsetTextBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'InsetTextBlock',
              heading: null,
              body:
                'Within this release absence by reason is broken down in three different ways:\n\nDistribution of absence by reason: The proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences.\n\nRate of absence by reason: The rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions.\n\nOne or more sessions missed due to each reason: The number of pupil enrolments missing at least one session due to each reason.',
            },
          ],
        },
        {
          order: 5,
          heading: 'Distribution of absence',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                'Nearly half of all pupils (48.9 per cent) were absent for five days or fewer across state-funded primary, secondary and special schools in 2016/17, down from 49.1 per cent in 2015/16.\n\n4.3 per cent of pupil enrolments had more than 25 days of absence in 2016/17 (the same as in 2015/16). These pupil enrolments accounted for 23.5 per cent of days missed. 8.2 per cent of pupil enrolments had no absence during 2016/17.\n\nPer pupil enrolment, the average total absence in primary schools was 7.2 days, compared to 16.9 days in special schools and 9.3 days in secondary schools.\n\nWhen looking at absence rates across terms for primary, secondary and special schools, the overall absence rate is lowest in the autumn term and highest in the summer term. The authorised rate is highest in the spring term and lowest in the summer term, and the unauthorised rate is highest in the summer term.',
            },
          ],
        },
        {
          order: 6,
          heading: 'Absence by pupil characteristics',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                'The patterns of absence rates for pupils with different characteristics have been consistent across recent years.\n\n### Gender\n\nThe overall absence rates across state-funded primary, secondary and special schools were very similar for boys and girls, at 4.7 per cent and 4.6 per cent respectively. The persistent absence rates were also similar, at 10.9 per cent for boys and 10.6 per cent for girls.\n\n### Free school meals (FSM) eligibility\n\nAbsence rates are higher for pupils who are known to be eligible for and claiming free school meals. The overall absence rate for these pupils was 7.3 per cent, compared to 4.2 per cent for non FSM pupils. The persistent absence rate for pupils who were eligible for FSM was more than twice the rate for those pupils not eligible for FSM.\n\n### National curriculum year group\n\nPupils in national curriculum year groups 3 and 4 had the lowest overall absence rates at 3.9 and 4 per cent respectively. Pupils in national curriculum year groups 10 and 11 had the highest overall absence rate at 6.1 per cent and 6.2 per cent respectively. This trend is repeated for persistent absence.\n\n### Special educational need (SEN)\n\nPupils with a statement of special educational needs (SEN) or education healthcare plan (EHC) had an overall absence rate of 8.2 per cent compared to 4.3 per cent for those with no identified SEN. The percentage of pupils with a statement of SEN or an EHC plan that are persistent absentees was more than two times higher than the percentage for pupils with no identified SEN.\n\n### Ethnic group\n\nThe highest overall absence rates were for Traveller of Irish Heritage and Gypsy/ Roma pupils at 18.1 per cent and 12.9 per cent respectively. Overall absence rates for pupils of a Chinese and Black African ethnicity were substantially lower than the national average of 4.7 per cent at 2.4 per cent and 2.9 per cent respectively. A similar pattern is seen in persistent absence rates; Traveller of Irish heritage pupils had the highest rate at 64 per cent and Chinese pupils had the lowest rate at 3.1 per cent.',
            },
          ],
        },
        {
          order: 7,
          heading: 'Absence for four year olds',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                "The overall absence rate for four year olds in 2016/17 was 5.1 per cent which is lower than the rate of 5.2 per cent which it has been for the last two years.\n\nAbsence recorded for four year olds is not treated as 'authorised' or 'unauthorised' and is therefore reported as overall absence only.",
            },
          ],
        },
        {
          order: 8,
          heading: 'Pupil referral unit absence',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                'The overall absence rate for pupil referral units in 2016/17 was 33.9 per cent, compared to 32.6 per cent in 2015/16. The percentage of enrolments in pupil referral units who were persistent absentees was 73.9 per cent in 2016/17, compared to 72.5 per cent in 2015/16.',
            },
          ],
        },
        {
          order: 9,
          heading: 'Pupil absence by local authority',
          caption: '',
          content: [
            {
              $type:
                'GovUk.Education.ExploreEducationStatistics.Content.Model.MarkDownBlock, GovUk.Education.ExploreEducationStatistics.Content.Api',
              type: 'MarkDownBlock',
              body:
                'There is variation in overall and persistent absence rates across state-funded primary, secondary and special schools by region and local authority. Similarly to last year, the three regions with the highest overall absence rate across all state-funded primary, secondary and special schools are the North East (4.9 per cent), Yorkshire and the Humber (4.9 per cent) and the South West (4.8 per cent), with Inner and Outer London having the lowest overall absence rate (4.4 per cent). The region with the highest persistent absence rate is Yorkshire and the Humber, where 11.9 per cent of pupil enrolments are persistent absentees, with Outer London having the lowest rate of persistent absence (at 10.0 per cent).\n\nAbsence information at local authority district level is also published within this release, in the accompanying underlying data files.',
            },
          ],
        },
      ],
      keyStatistics: {
        type: 'DataBlock',
        heading: null,
        dataQuery: {
          path: '/api/tablebuilder/characteristics/national',
          method: 'POST',
          body:
            '{ "indicators": ["enrolments","sess_authorised","sess_overall","enrolments_PA_10_exact","sess_unauthorised_percent","enrolments_pa_10_exact_percent","sess_authorised_percent","sess_overall_percent" ], "characteristics": [ "Total" ], "endYear": 201617, "publicationId": "cbbd299f-8297-44bc-92ac-558bcf51f8ad", "schoolTypes": [ "Total" ], "startYear": 201213}',
        },
        charts: [
          {
            $type:
              'GovUk.Education.ExploreEducationStatistics.Content.Model.LineChart, GovUk.Education.ExploreEducationStatistics.Content.Api',
            indicators: [
              'sess_overall_percent',
              'sess_unauthorised_percent',
              'sess_authorised_percent',
            ],
            xAxis: { title: 'School Year' },
            yAxis: { title: 'Absence Rate' },
            type: 'line',
          },
        ],
        summary: {
          dataKeys: [
            'sess_overall_percent',
            'sess_authorised_percent',
            'sess_unauthorised_percent',
          ],
          description: {
            type: 'MarkDownBlock',
            body:
              ' * pupils missed on average 8.2 school days \n  * overall and unauthorised absence rates up on previous year \n * unauthorised rise due to higher rates of unauthorised holidays \n * 10% of pupils persistently absent during 2016/17',
          },
        },
      },
      dataFiles: [
        {
          extension: 'csv',
          name: 'Absence by characteristic',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_by_characteristic.csv',
          size: '58 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence by geographic level',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_by_geographic_level.csv',
          size: '63 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence by term',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_by_term.csv',
          size: '2 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence for four year olds',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_for_four_year_olds.csv',
          size: '13 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence in prus',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_in_prus.csv',
          size: '141 Kb',
        },
        {
          extension: 'csv',
          name: 'Absence number missing at least one session by reason',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_number_missing_at_least_one_session_by_reason.csv',
          size: '19 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence rate percent bands',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_rate_percent_bands.csv',
          size: '198 Kb',
        },
      ],
    });
  }

  public static getNewPublication(): Release {
    return {
      id: '00000000-0000-0000-0000-000000000000',
      title: 'Pupil absence data and statistics for schools in England',
      releaseName: '2018 to 2019',
      published: '2017-03-22T00:00:00',
      slug: '2018-19',
      summary: LOREM,
      publicationId: '00000000-0000-0000-0000-000000000000',
      publication: {
        id: '00000000-0000-0000-0000-000000000000',
        slug: 'pupil-absence-in-schools-in-england',
        title: 'Pupil absence in schools in England',
        description: '',
        dataSource:
          '[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)',
        summary: LOREM,
        nextUpdate: '2018-03-22T00:00:00',
        releases: [],
        legacyReleases: [],
        contact: {
          contactName: 'Mr Smith',
          contactTelNo: '01228 76762',
          teamEmail: 'team@email.com',
          teamName: 'Team A',
        },
        topic: {
          theme: {
            title: 'Pupil absence',
          },
        },
      },
      updates: [],
      content: [
        {
          order: 1,
          heading: 'About this release',
          caption: '',
          content: [
            {
              type: 'MarkDownBlock',
              body: LOREM,
            },
          ],
        },
        {
          order: 2,
          heading: 'New content',
          caption: '',
          content: [
            {
              type: 'MarkDownBlock',
              body: LOREM,
            },
          ],
        },
      ],
      keyStatistics: {
        type: 'DataBlock',
        body: '',
        dataBlockRequest: {
          subjectId: 1,
          filters: ['1', '2'],
          indicators: ['23', '26', '28'],
          timePeriod: {
            startYear: '2016',
            startCode: 'HT6',
            endYear: '2017',
            endCode: 'HT6',
          },
          geographicLevel: GeographicLevel.Country,
        },

        charts: [
          {
            labels: {
              '23_1_2': {
                name: '23_1_2',
                unit: '%',
                value: '23_1_2',
                label: 'Unauthorised absence',
              },
              '26_1_2': {
                name: '26_1_2',
                unit: '%',
                value: '26_1_2',
                label: 'Overall absence',
              },
              '28_1_2': {
                name: '28_1_2',
                unit: '%',
                value: '28_1_2',
                label: 'Authorised absence',
              },
            },
            axes: {
              major: {
                name: 'major',
                type: 'major',
                groupBy: 'timePeriods',
                dataSets: [
                  { indicator: '23', filters: ['1', '2'] },
                  { indicator: '26', filters: ['1', '2'] },
                  { indicator: '28', filters: ['1', '2'] },
                ],
              },
              minor: {
                name: 'minor',
                type: 'minor',
                dataSets: [],
              },
            },
            type: 'line',
          },
        ],
        // @ts-ignore
        summary: {
          dataKeys: [],
          description: {
            type: 'MarkDownBlock',
            body: LOREM,
          },
        },
      },
      dataFiles: [
        {
          extension: 'csv',
          name: 'Absence by characteristic',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_by_characteristic.csv',
          size: '58 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence by geographic level',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_by_geographic_level.csv',
          size: '63 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence by term',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_by_term.csv',
          size: '2 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence for four year olds',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_for_four_year_olds.csv',
          size: '13 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence in prus',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_in_prus.csv',
          size: '141 Kb',
        },
        {
          extension: 'csv',
          name: 'Absence number missing at least one session by reason',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_number_missing_at_least_one_session_by_reason.csv',
          size: '19 Mb',
        },
        {
          extension: 'csv',
          name: 'Absence rate percent bands',
          path:
            'pupil-absence-in-schools-in-england/2016-17/absence_rate_percent_bands.csv',
          size: '198 Kb',
        },
      ],
    };
  }
}
