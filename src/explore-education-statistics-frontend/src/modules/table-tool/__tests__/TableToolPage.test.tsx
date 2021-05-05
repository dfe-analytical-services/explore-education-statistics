import { FastTrackTable } from '@common/services/fastTrackService';
import {
  Publication,
  SubjectMeta,
  Theme,
} from '@common/services/tableBuilderService';
import { render, screen } from '@testing-library/react';
import React from 'react';
// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore
import preloadAll from 'jest-next-dynamic';
import TableToolPage from '@frontend/modules/table-tool/TableToolPage';

describe('TableToolPage', () => {
  const testFastTrack: FastTrackTable = {
    id: 'a08bd50a-7814-4e43-b152-2f7464d566ff',
    created: '2021-04-27T15:17:05.7530941Z',
    configuration: {
      tableHeaders: {
        columnGroups: [],
        columns: [{ value: '2018_TY', type: 'TimePeriod' }],
        rowGroups: [
          [
            { value: '72303947-27a4-4ea2-a708-dfeb7bb31efb', type: 'Filter' },
            { value: '0164a10e-c041-49f8-b44d-daa6f0c1fbfd', type: 'Filter' },
          ],
        ],
        rows: [
          { value: 'e5174cb8-02f6-48d3-662e-08d9097c4255', type: 'Indicator' },
          { value: '0e8f9198-ce65-40ac-662a-08d9097c4255', type: 'Indicator' },
        ],
      },
    },
    fullTable: {
      subjectMeta: {
        filters: {
          SubjectStudied: {
            totalValue: '',
            hint: 'Filter by Subject studied',
            legend: 'Subject studied',
            options: {
              Default: {
                label: 'Default',
                options: [
                  {
                    label: '1 .  PGCE',
                    value: '72303947-27a4-4ea2-a708-dfeb7bb31efb',
                  },
                  {
                    label: '2 .  MBA',
                    value: '0164a10e-c041-49f8-b44d-daa6f0c1fbfd',
                  },
                ],
              },
            },
            name: 'subject_name',
          },
        },
        footnotes: [],
        indicators: [
          {
            label: 'Gender gap',
            unit: '%',
            value: 'e5174cb8-02f6-48d3-662e-08d9097c4255',
            name: 'gender_gap',
            decimalPlaces: 1,
          },
          {
            label: 'Graduates included in earnings figures',
            unit: '',
            value: '0e8f9198-ce65-40ac-662a-08d9097c4255',
            name: 'earnings_include',
            decimalPlaces: 0,
          },
        ],
        locations: [
          { level: 'country', label: 'Great Britain', value: 'K03000001' },
        ],
        boundaryLevels: [
          {
            id: 1,
            label:
              'Countries December 2017 Ultra Generalised Clipped Boundaries in UK',
          },
        ],
        publicationName: 'Test publication',
        subjectName: 'data 1',
        timePeriodRange: [{ code: 'TY', label: '2018-19', year: 2018 }],
        geoJsonAvailable: true,
      },
      results: [
        {
          filters: ['72303947-27a4-4ea2-a708-dfeb7bb31efb'],
          geographicLevel: 'country',
          location: {
            country: { code: 'K03000001', name: 'Great Britain' },
          },
          measures: {
            '0e8f9198-ce65-40ac-662a-08d9097c4255': '2',
            'e5174cb8-02f6-48d3-662e-08d9097c4255': '20.00%',
          },
          timePeriod: '2018_TY',
        },
        {
          filters: ['0164a10e-c041-49f8-b44d-daa6f0c1fbfd'],
          geographicLevel: 'country',
          location: {
            country: { code: 'K03000001', name: 'Great Britain' },
          },
          measures: {
            '0e8f9198-ce65-40ac-662a-08d9097c4255': '2',
            'e5174cb8-02f6-48d3-662e-08d9097c4255': '60.00%',
          },
          timePeriod: '2018_TY',
        },
      ],
    },
    query: {
      publicationId: '536154f5-7f82-4dc7-060a-08d9097c1945',
      subjectId: 'ef95f53e-2b91-417e-b32f-08d9097c40f5',
      timePeriod: {
        startYear: 2018,
        startCode: 'TY',
        endYear: 2018,
        endCode: 'TY',
      },
      filters: [
        '72303947-27a4-4ea2-a708-dfeb7bb31efb',
        '0164a10e-c041-49f8-b44d-daa6f0c1fbfd',
      ],
      indicators: [
        'e5174cb8-02f6-48d3-662e-08d9097c4255',
        '0e8f9198-ce65-40ac-662a-08d9097c4255',
      ],
      locations: { country: ['K03000001'] },
      includeGeoJson: false,
    },
    releaseId: '47a39fb5-1fed-4820-8906-5b54192524d7',
    releaseSlug: '2000-01',
  };

  const testPublication: Publication = {
    id: '536154f5-7f82-4dc7-060a-08d9097c1945',
    highlights: [],
    subjects: [
      {
        id: '1f1b1780-a607-454e-b331-08d9097c40f5',
        name: 'dates',
        content: '<p>ryt</p>',
        timePeriods: { from: '2020 Week 13', to: '2021 Week 24' },
        geographicLevels: ['National'],
      },
    ],
  };

  const testSubjectMeta: SubjectMeta = {
    filters: {
      SubjectStudied: {
        totalValue: '',
        name: 'Subject studied',
        hint: 'Filter by Subject studied',
        legend: 'Subject studied',
        options: {
          Default: {
            label: 'Default',
            options: [
              {
                label: '1 .  PGCE',
                value: '72303947-27a4-4ea2-a708-dfeb7bb31efb',
              },
              {
                label: '10 .  Psychology',
                value: '0164a10e-c041-49f8-b44d-daa6f0c1fbfd',
              },
              {
                label: '11 .  Veterinary sciences',
                value: '89cf21bd-2d1c-479b-9bde-550fc1ade623',
              },
            ],
          },
        },
      },
    },
    indicators: {
      Earnings: {
        label: 'Earnings',
        options: [
          {
            label: 'Gender gap',
            unit: '%',
            value: 'e5174cb8-02f6-48d3-662e-08d9097c4255',
            name: 'gender_gap',
            decimalPlaces: 1,
          },
          {
            label: 'Graduates included in earnings figures',
            unit: '',
            value: '0e8f9198-ce65-40ac-662a-08d9097c4255',
            name: 'earnings_include',
            decimalPlaces: 0,
          },
          {
            label: 'Lower quartile of earnings of graduates',
            unit: '£',
            value: '6029838f-720c-4aee-662b-08d9097c4255',
            name: 'earnings_LQ',
            decimalPlaces: 0,
          },
        ],
      },
      Outcomes: {
        label: 'Outcomes',
        options: [
          {
            label: 'Activity not captured',
            unit: '%',
            value: '987216a6-ca30-4577-6624-08d9097c4255',
            name: 'activity_not_captured',
            decimalPlaces: 1,
          },
          {
            label: 'Further study with or without sustained employment',
            unit: '%',
            value: '6a94a8da-d405-4df8-6629-08d9097c4255',
            name: 'fs_with_or_without_sust_emp',
            decimalPlaces: 1,
          },
          {
            label: 'No sustained destination',
            unit: '%',
            value: '978ff59a-aaca-4846-6625-08d9097c4255',
            name: 'no_sust_dest',
            decimalPlaces: 1,
          },
        ],
      },
      Populations: {
        label: 'Populations',
        options: [
          {
            label: 'Graduates',
            unit: '',
            value: 'bf63d6db-4312-4cba-661f-08d9097c4255',
            name: 'grads',
            decimalPlaces: 0,
          },
          {
            label: 'Graduates included in outcomes calculation',
            unit: '',
            value: '1debc286-5608-4a8b-6620-08d9097c4255',
            name: 'grads_uk',
            decimalPlaces: 0,
          },
          {
            label: 'Matched graduates',
            unit: '',
            value: '7c5a3b0c-03f0-45ce-6623-08d9097c4255',
            name: 'matched',
            decimalPlaces: 0,
          },
        ],
      },
    },
    locations: {
      country: {
        hint: '',
        legend: 'National',
        options: [{ label: 'Great Britain', value: 'K03000001' }],
      },
    },
    timePeriod: {
      hint: 'Filter statistics by a given start and end date',
      legend: '',
      options: [{ code: 'TY', label: '2018-19', year: 2018 }],
    },
  };

  const testThemeMeta: Theme[] = [
    {
      id: 'ee1855ca-d1e1-4f04-a795-cbd61d326a1f',
      title: 'Pupils and schools',
      slug: 'pupils-and-schools',
      topics: [
        {
          id: 'c9f0b897-d58a-42b0-9d12-ca874cc7c810',
          title: 'Admission appeals',
          slug: 'admission-appeals',
          publications: [
            {
              id: '536154f5-7f82-4dc7-060a-08d9097c1945',
              title: 'Test publication',
              slug: 'test-publication',
            },
          ],
        },
      ],
    },
  ];

  beforeEach(preloadAll);

  test('renders the Table Tool page correctly when a Theme is chosen, giving the user a choice of Publications', async () => {
    render(<TableToolPage themeMeta={testThemeMeta} />);

    expect(screen.getByRole('main')).toMatchSnapshot();
  });

  test('renders the Table Tool page correctly when Publication is chosen, giving the user a choice of Subjects', async () => {
    await preloadAll();

    render(
      <TableToolPage publication={testPublication} themeMeta={testThemeMeta} />,
    );

    expect(screen.getByRole('main')).toMatchSnapshot();
  });

  test('renders the Table Tool page correctly when a Fast Track is provided, rendering a previously configured table', async () => {
    await preloadAll();

    render(
      <TableToolPage
        fastTrack={testFastTrack}
        publication={testPublication}
        themeMeta={testThemeMeta}
        subjectMeta={testSubjectMeta}
      />,
    );

    expect(screen.getByRole('main')).toMatchSnapshot();
  });
});
