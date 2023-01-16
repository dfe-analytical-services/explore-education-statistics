import { Theme } from '@common/services/publicationService';
import {
  FastTrackTable,
  SelectedPublication,
  Subject,
  SubjectMeta,
} from '@common/services/tableBuilderService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import preloadAll from 'jest-next-dynamic';
import TableToolPage from '@frontend/modules/table-tool/TableToolPage';
import React from 'react';

describe('TableToolPage', () => {
  const testPublicationId = '536154f5-7f82-4dc7-060a-08d9097c1945';

  const testFastTrack: FastTrackTable = {
    id: 'a08bd50a-7814-4e43-b152-2f7464d566ff',
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
                id: 'default',
                label: 'Default',
                options: [
                  {
                    label: 'PGCE',
                    value: '72303947-27a4-4ea2-a708-dfeb7bb31efb',
                  },
                  {
                    label: 'MBA',
                    value: '0164a10e-c041-49f8-b44d-daa6f0c1fbfd',
                  },
                ],
                order: 0,
              },
            },
            order: 0,
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
        locations: {
          country: [
            {
              id: '26100e90-c8c5-43a5-9999-296d402f02fb',
              label: 'Great Britain',
              value: 'K03000001',
            },
          ],
        },
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
          locationId: '26100e90-c8c5-43a5-9999-296d402f02fb',
          measures: {
            '0e8f9198-ce65-40ac-662a-08d9097c4255': '2',
            'e5174cb8-02f6-48d3-662e-08d9097c4255': '20.00%',
          },
          timePeriod: '2018_TY',
        },
        {
          filters: ['0164a10e-c041-49f8-b44d-daa6f0c1fbfd'],
          geographicLevel: 'country',
          locationId: '26100e90-c8c5-43a5-9999-296d402f02fb',
          measures: {
            '0e8f9198-ce65-40ac-662a-08d9097c4255': '2',
            'e5174cb8-02f6-48d3-662e-08d9097c4255': '60.00%',
          },
          timePeriod: '2018_TY',
        },
      ],
    },
    query: {
      publicationId: testPublicationId,
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
      locationIds: ['26100e90-c8c5-43a5-9999-296d402f02fb'],
      includeGeoJson: false,
    },
  };

  const testSubjects: Subject[] = [
    {
      id: '1f1b1780-a607-454e-b331-08d9097c40f5',
      name: 'Test subject',
      content: '<p>ryt</p>',
      timePeriods: { from: '2020 Week 13', to: '2021 Week 24' },
      geographicLevels: ['National'],
      file: {
        id: 'file-1',
        name: 'Test subject',
        fileName: 'file-1.csv',
        extension: 'csv',
        size: '10 Mb',
        type: 'Data',
      },
    },
  ];

  const testSubjectMeta: SubjectMeta = {
    filters: {
      SubjectStudied: {
        id: 'subject-studied',
        totalValue: '',
        name: 'Subject studied',
        hint: 'Filter by Subject studied',
        legend: 'Subject studied',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'PGCE',
                value: '72303947-27a4-4ea2-a708-dfeb7bb31efb',
              },
              {
                label: 'Psychology',
                value: '0164a10e-c041-49f8-b44d-daa6f0c1fbfd',
              },
              {
                label: 'Veterinary sciences',
                value: '89cf21bd-2d1c-479b-9bde-550fc1ade623',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
    },
    indicators: {
      Earnings: {
        id: 'earnings',
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
        order: 0,
      },
      Outcomes: {
        id: 'outcomes',
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
        order: 1,
      },
      Populations: {
        id: 'populations',
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
        order: 2,
      },
    },
    locations: {
      country: {
        legend: 'National',
        options: [
          {
            id: '26100e90-c8c5-43a5-9999-296d402f02fb',
            label: 'Great Britain',
            value: 'K03000001',
          },
        ],
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
      summary: '',
      topics: [
        {
          id: 'c9f0b897-d58a-42b0-9d12-ca874cc7c810',
          title: 'Admission appeals',
          summary: '',
          publications: [
            {
              id: testPublicationId,
              title: 'Test publication',
              slug: 'test-publication',
              isSuperseded: false,
            },
          ],
        },
      ],
    },
  ];

  const testSelectedPublicationWithLatestRelease: SelectedPublication = {
    id: testPublicationId,
    title: 'Test Publication',
    slug: 'test-publication',
    selectedRelease: {
      id: 'latest-release-id',
      latestData: true,
      slug: 'latest-release-slug',
      title: 'Latest Release Title',
    },
    latestRelease: {
      title: 'Latest Release Title',
    },
  };

  const testSelectedPublicationWithNonLatestRelease: SelectedPublication = {
    id: testPublicationId,
    title: 'Test Publication',
    slug: 'test-publication',
    selectedRelease: {
      id: 'selected-release-id',
      latestData: false,
      slug: 'selected-release-slug',
      title: 'Selected Release Title',
    },
    latestRelease: {
      title: 'Latest Release Title',
    },
  };

  beforeAll(preloadAll);

  test('renders the page correctly with themes and publications', async () => {
    render(<TableToolPage themeMeta={testThemeMeta} />);

    expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
      'aria-current',
      'step',
    );
    const themeRadios = within(
      screen.getByRole('group', { name: 'Select a theme' }),
    ).getAllByRole('radio');
    expect(themeRadios).toHaveLength(1);
    expect(themeRadios[0]).toEqual(
      screen.getByRole('radio', { name: 'Pupils and schools' }),
    );

    expect(
      screen.queryByRole('radio', {
        name: 'Test publication',
      }),
    ).not.toBeInTheDocument();

    userEvent.click(screen.getByRole('radio', { name: 'Pupils and schools' }));

    // Check there is only one radio for the publication;
    const publicationRadios = within(
      screen.getByRole('group', {
        name: /Select a publication/,
      }),
    ).queryAllByRole('radio');
    expect(publicationRadios).toHaveLength(1);
    expect(publicationRadios[0]).toEqual(
      screen.getByRole('radio', {
        name: 'Test publication',
      }),
    );
  });

  test('renders the page correctly with pre-selected publication', async () => {
    render(
      <TableToolPage
        selectedPublication={testSelectedPublicationWithLatestRelease}
        subjects={testSubjects}
        themeMeta={testThemeMeta}
      />,
    );

    // Check we are on step 2, not 1
    expect(screen.getByTestId('wizardStep-1')).not.toHaveAttribute(
      'aria-current',
      'step',
    );
    expect(screen.getByTestId('wizardStep-2')).toHaveAttribute(
      'aria-current',
      'step',
    );

    expect(screen.getByTestId('Publication')).toHaveTextContent(
      'Test publication',
    );

    // Check there is only one radio for the subject
    expect(screen.getAllByRole('radio', { hidden: true })).toHaveLength(1);
    expect(screen.getByLabelText('Test subject')).toBeInTheDocument();
  });

  test('renders the page correctly with pre-built table when a fast track is provided', async () => {
    render(
      <TableToolPage
        selectedPublication={testSelectedPublicationWithLatestRelease}
        subjects={testSubjects}
        themeMeta={testThemeMeta}
        subjectMeta={testSubjectMeta}
        fastTrack={testFastTrack}
      />,
    );

    expect(screen.getByRole('table')).toMatchSnapshot();
  });

  test('renders the page correctly when a fast track is provided and this is the latest data', async () => {
    render(
      <TableToolPage
        selectedPublication={testSelectedPublicationWithLatestRelease}
        subjects={testSubjects}
        themeMeta={testThemeMeta}
        subjectMeta={testSubjectMeta}
        fastTrack={testFastTrack}
      />,
    );

    expect(screen.getByText('This is the latest data')).toBeInTheDocument();
    expect(
      screen.queryByText('This data is not from the latest release'),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByTestId('View latest data link'),
    ).not.toBeInTheDocument();
  });

  test('renders the page correctly when a fast track is provided and this is not the latest data', async () => {
    render(
      <TableToolPage
        selectedPublication={testSelectedPublicationWithNonLatestRelease}
        subjects={testSubjects}
        themeMeta={testThemeMeta}
        subjectMeta={testSubjectMeta}
        fastTrack={testFastTrack}
      />,
    );

    expect(
      screen.queryByText('This is the latest data'),
    ).not.toBeInTheDocument();
    expect(
      screen.getByText('This data is not from the latest release'),
    ).toBeInTheDocument();

    const latestDataLink = screen.getByTestId(
      'View latest data link',
    ) as HTMLAnchorElement;

    expect(latestDataLink).toBeInTheDocument();
    expect(latestDataLink.href).toEqual(
      'http://localhost/find-statistics/test-publication',
    );
    expect(latestDataLink.text).toContain('View latest data');
    expect(latestDataLink.text).toContain('Latest Release Title');
  });
});
