import { EditableRelease } from '@admin/services/publicationService';
import { GeographicLevel } from '@common/services/dataBlockService';
import { ReleaseType } from '@common/services/publicationService';

const LOREM =
  'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec elementum, mauris eget vulputate iaculis, dui orci efficitur mi, at consectetur metus lorem tempor neque. Etiam in eleifend magna. Sed hendrerit vitae ante at semper. Mauris a erat a ex porta mollis. Aliquam quis justo eu lectus luctus porttitor nec at dolor. Nunc interdum, diam sed lobortis porta, massa arcu volutpat nunc, eget scelerisque arcu neque vel tortor. Fusce sit amet mauris augue. Praesent sed urna vel lacus suscipit mollis id quis nulla. Duis porta sapien et arcu ornare, eget mollis justo finibus. Nunc commodo felis justo, at efficitur purus mattis in. Donec nibh quam, mollis at eros ac, fringilla porta mi.';

export default class PrototypePublicationService {
  public static getNewPublication(): EditableRelease {
    return {
      yearTitle: '',
      coverageTitle: '',
      id: '00000000-0000-0000-0000-000000000000',
      title: 'Pupil absence data and statistics for schools in England',
      releaseName: '2018 to 2019',
      published: '2017-03-22T00:00:00',
      slug: '2018-19',
      summarySection: {
        order: 1,
        heading: '',
        caption: '',
        content: [
          {
            id: '000000',
            type: 'MarkDownBlock',
            body: LOREM,
            comments: [
              {
                comment: LOREM.substring(0, 120),
                name: 'John Smith',
                time: new Date(),
                state: 'open',
              },
            ],
          },
        ],
      },
      headlinesSection: {
        order: 1,
        heading: '',
        caption: '',
        content: [
          {
            id: '000000',
            type: 'MarkDownBlock',
            body: LOREM,
            comments: [
              {
                comment: LOREM.substring(0, 120),
                name: 'John Smith',
                time: new Date(),
                state: 'open',
              },
            ],
          },
        ],
      },
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
      latestRelease: true,
      type: {
        id: '00000000-0000-0000-0000-000000000000',
        title: ReleaseType.OfficialStatistics,
      },
      updates: [],
      content: [
        {
          order: 1,
          heading: 'About this release',
          caption: '',
          content: [
            {
              id: '000000',
              type: 'MarkDownBlock',
              body: LOREM,
              comments: [
                {
                  comment: LOREM.substring(0, 120),
                  name: 'John Smith',
                  time: new Date(),
                  state: 'open',
                },
              ],
            },
          ],
        },
        {
          order: 2,
          heading: 'New content',
          caption: '',
          content: [
            {
              id: '000001',
              type: 'MarkDownBlock',
              body: LOREM,
              comments: [
                {
                  comment: LOREM.substring(0, 120),
                  name: 'John Smith',
                  time: new Date(),
                  state: 'open',
                },
              ],
            },
          ],
        },
      ],
      keyStatisticsSection: {
        order: 1,
        heading: '',
        caption: '',
        content: [
          {
            id: '000002',
            type: 'DataBlock',
            body: '',
            dataBlockRequest: {
              subjectId: '1',
              filters: ['1', '2'],
              indicators: ['23', '26', '28'],
              timePeriod: {
                startYear: 2016,
                startCode: 'HT6',
                endYear: 2017,
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
                    groupBy: 'timePeriod',
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

            summary: {
              dataKeys: [],
              dataSummary: [],
              dataDefinition: [],
              description: {
                type: 'MarkDownBlock',
                body: LOREM,
              },
            },
            comments: [
              {
                comment: LOREM.substring(0, 120),
                name: 'John Smith',
                time: new Date(),
                state: 'open',
              },
            ],
          },
        ],
      },
      publishScheduled: undefined,
      nextReleaseDate: {},
      status: 'Approved',
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
      downloadFiles: [
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
