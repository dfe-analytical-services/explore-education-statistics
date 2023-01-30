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
  title: 'Academic Year 2020/21',
  yearTitle: '2020/21',
  coverageTitle: 'Academic Year',
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
        comments: [],
      },
    ],
  },
  keyStatisticsSection: {
    id: '10e39b93-0304-43fb-d1b3-08d7bec1ba1b',
    order: 0,
    heading: '',
    content: [
      {
        heading:
          "Main reason for issue: Unauthorised family holiday absence for 'prma' from 'My Pub' in England for 2017/18",
        name: 'Holiday (Key Stat)',
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
          indicators: ['272f4b12-9dfe-45ef-fdf9-08d7bec1dd71'],
          locationIds: ['dd590fcf-b0c1-4fa3-8599-d13c0f540793'],
          includeGeoJson: true,
        },
        charts: [],
        table: emptyTable,
        type: 'DataBlock',
        id: 'e2db1389-8220-41f0-a29a-bb8dd1ccfc9c',
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
      {
        heading:
          "Main reason for issue: Arriving late for 'prma' from 'My Pub' in England for 2017/18",
        name: 'Main Reason (Key Stat)',
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
          indicators: ['d314e2a9-5069-4d79-fdfa-08d7bec1dd71'],
          locationIds: ['dd590fcf-b0c1-4fa3-8599-d13c0f540793'],
          includeGeoJson: true,
        },
        charts: [],
        table: emptyTable,
        type: 'DataBlock',
        id: 'aa9f1b63-abb3-41a2-bd57-fcf24dfd71ed',
        order: 1,
        comments: [],
      },
      {
        heading:
          "Main reason for issue: Absence due to other unauthorised circumstances for 'prma' from 'My Pub' in England for 2017/18",
        name: "Unauth'd (Key Stat)",
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
          indicators: ['214f5fb8-c12b-411e-fdfb-08d7bec1dd71'],
          locationIds: ['dd590fcf-b0c1-4fa3-8599-d13c0f540793'],
          includeGeoJson: true,
        },
        charts: [],
        table: emptyTable,
        type: 'DataBlock',
        id: 'ba1ff405-1f44-4509-9dcf-5822df5b6f8c',
        order: 2,
        comments: [],
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
