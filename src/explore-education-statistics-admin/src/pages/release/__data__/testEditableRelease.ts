import { EditableRelease } from '@admin/services/releaseContentService';
import { Table } from '@common/services/types/blocks';

const emptyTable: Table = {
  indicators: [],
  tableHeaders: {
    columnGroups: [],
    columns: [],
    rowGroups: [],
    rows: [],
  },
};

// eslint-disable-next-line import/prefer-default-export
export const testEditableRelease: EditableRelease = {
  id: '6a97c9b6-eaa2-4d22-7ba9-08d7bec1ba1a',
  title: 'Academic year 2020/21',
  yearTitle: '2020/21',
  coverageTitle: 'Academic year',
  releaseName: '2020',
  slug: '2020-21',
  publicationId: '8586c490-7027-4a8b-9df5-cf105c3e88ec',
  approvalStatus: 'Draft',
  published: '',
  publication: {
    id: '8586c490-7027-4a8b-9df5-cf105c3e88ec',
    title: 'My Pub',
    slug: 'my-pub',
    releases: [],
    legacyReleases: [],
    topic: { theme: { title: 'Children, early years and social care' } },
    contact: {
      contactName: 'John Smith',
      contactTelNo: '0777777777',
      teamEmail: 'john.smith@test.com',
      teamName: 'Team Smith',
    },
    methodologies: [
      {
        id: '8807af2b-d9a7-4efa-a376-08d7b451a98e',
        title: 'First methodology',
        slug: 'my-pub-methodology',
      },
    ],
  },
  latestRelease: false,
  type: 'OfficialStatistics',
  updates: [
    {
      id: '262cf6c8-db96-40d8-8fb1-b55028a9f55b',
      reason: 'First published',
      on: new Date(),
    },
  ],
  keyStatistics: [
    {
      type: 'KeyStatisticDataBlock',
      id: 'keyStat-1',
      dataBlockId: 'dataBlock-1',
      trend: 'keyStat-1 trend',
      order: 0,
      created: '2023-01-01',
    },
    {
      type: 'KeyStatisticText',
      id: 'keyStat-2',
      title: 'KeyStat-2 title',
      statistic: 'KeyStat-2 value',
      trend: 'KeyStat-2 trend',
      guidanceTitle: 'keyStat-2 guidanceTitle',
      guidanceText: 'keyStat-2 guidanceText',
      order: 1,
      created: '2023-01-02',
    },
    {
      type: 'KeyStatisticDataBlock',
      id: 'keyStat-3',
      dataBlockId: 'dataBlock-2',
      guidanceText: 'KeyStat-3 guidanceText',
      order: 2,
      created: '2023-01-03',
    },
  ],
  content: [
    {
      id: '30e57106-66e2-43a3-fc07-08d7bf65f293',
      order: 0,
      heading: 'New section 3',
      content: [
        {
          id: 'eb809a91-af3d-438d-632c-08d7c408aca5',
          body: '',
          type: 'MarkDownBlock',
          order: 0,
          comments: [
            {
              id: 'comment-1',
              content: 'A comment',
              createdBy: {
                id: 'user-1',
                firstName: 'Bau1',
                lastName: '',
                email: 'bau1@test.com',
              },
              created: '2020-03-09T09:39:53.736',
            },
            {
              id: 'comment-2',
              content: 'another comment',
              createdBy: {
                id: 'user-1',
                firstName: 'Bau1',
                lastName: '',
                email: 'bau1@test.com',
              },
              created: '2020-03-09T09:40:16.534',
            },
          ],
        },
        {
          heading: "'prma' from 'My Pub' in England for 2017/18",
          name: 'DataBlock 1',
          source: '',
          query: {
            subjectId: '36aa28ce-83ca-49b5-8c27-b34e77b062c9',
            timePeriod: {
              startYear: 2017,
              startCode: 'AY',
              endYear: 2017,
              endCode: 'AY',
            },
            filters: ['5a1b6c93-c9c3-4710-f71f-08d7bec1ddb3'],
            indicators: [
              '272f4b12-9dfe-45ef-fdf9-08d7bec1dd71',
              'd314e2a9-5069-4d79-fdfa-08d7bec1dd71',
              '214f5fb8-c12b-411e-fdfb-08d7bec1dd71',
              '99f5bbc3-de9a-4831-fdf8-08d7bec1dd71',
            ],
            locationIds: ['dd590fcf-b0c1-4fa3-8599-d13c0f540793'],
            includeGeoJson: true,
          },
          charts: [],
          table: emptyTable,
          type: 'DataBlock',
          id: '69a9522d-501d-441a-9ee5-260ede5cd85c',
          order: 1,
          comments: [],
        },
      ],
    },
    {
      id: '47b7cabf-4855-44b8-5fdc-08d7c1e45526',
      order: 1,
      heading: 'New section',
      content: [],
    },
  ],
  summarySection: {
    id: '6662107c-5c58-4ace-d1b1-08d7bec1ba1b',
    order: 0,
    content: [],
    heading: '',
  },
  headlinesSection: {
    id: '25a6cfbc-2cb5-43da-d1b7-08d7bec1ba1b',
    heading: '',
    order: 0,
    content: [
      {
        id: 'f27bf214-a244-41a7-b09b-08d7c1e10b96',
        type: 'MarkDownBlock',
        body: '',
        order: 0,
        comments: [
          {
            id: 'comment-3',
            content: 'Test comment 3',
            createdBy: {
              id: 'user-1',
              firstName: 'Bau1',
              lastName: '',
              email: 'bau1@test.com',
            },
            created: '2020-03-09T12:00:00.000',
          },
        ],
      },
    ],
  },
  keyStatisticsSecondarySection: {
    heading: '',
    id: '5509024d-9389-40d1-d1b5-08d7bec1ba1b',
    order: 0,
    content: [
      {
        heading: "'prma' from 'My Pub' in England for 2017/18",
        name: 'Aggregate table (Secondary)',
        source: '',
        query: {
          subjectId: '36aa28ce-83ca-49b5-8c27-b34e77b062c9',
          timePeriod: {
            startYear: 2017,
            startCode: 'AY',
            endYear: 2017,
            endCode: 'AY',
          },
          filters: ['5a1b6c93-c9c3-4710-f71f-08d7bec1ddb3'],
          indicators: [
            '272f4b12-9dfe-45ef-fdf9-08d7bec1dd71',
            'd314e2a9-5069-4d79-fdfa-08d7bec1dd71',
            '214f5fb8-c12b-411e-fdfb-08d7bec1dd71',
          ],
          locationIds: ['dd590fcf-b0c1-4fa3-8599-d13c0f540793'],
          includeGeoJson: true,
        },
        charts: [],
        table: emptyTable,
        type: 'DataBlock',
        id: 'a3197018-66b6-4ce5-97fa-da2355270c40',
        order: 0,
        comments: [],
      },
    ],
  },
  relatedDashboardsSection: {
    id: 'related-dashboards-section-id',
    order: 0,
    content: [],
    heading: '',
  },
  downloadFiles: [
    {
      id: 'download-1',
      extension: 'csv',
      fileName: 'prma.csv',
      name: 'prma',
      size: '268 Kb',
      type: 'Data',
    },
  ],
  hasPreReleaseAccessList: false,
  hasDataGuidance: true,
  publishScheduled: '2020-03-03T00:00:00',
  nextReleaseDate: { year: 2020, month: 3, day: 4 },
  relatedInformation: [],
};
