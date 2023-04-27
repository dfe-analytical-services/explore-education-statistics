import PreReleaseTableToolPage from '@admin/pages/release/pre-release/PreReleaseTableToolPage';
import {
  preReleaseTableToolRoute,
  PreReleaseTableToolRouteParams,
} from '@admin/routes/preReleaseRoutes';
import _dataBlockService, {
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import _publicationService, {
  Publication,
} from '@admin/services/publicationService';
import _tableBuilderService, {
  SubjectMeta,
  TableDataResponse,
  FeaturedTable,
  Subject,
} from '@common/services/tableBuilderService';
import { render, screen, waitFor, within } from '@testing-library/react';
import { MemoryRouter, Route } from 'react-router';
import { generatePath } from 'react-router-dom';
import React from 'react';

jest.mock('@admin/services/dataBlockService');
jest.mock('@admin/services/publicationService');
jest.mock('@common/services/tableBuilderService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('PreReleaseTableToolPage', () => {
  const testSubjectMeta: SubjectMeta = {
    filters: {
      SchoolType: {
        id: 'school-type',
        totalValue: '',
        hint: 'Filter by school type',
        legend: 'School type',
        name: 'school_type',
        options: {
          Default: {
            id: 'default',
            label: 'Default',
            options: [
              {
                label: 'State-funded primary',
                value: 'state-funded-primary',
              },
              {
                label: 'State-funded secondary',
                value: 'state-funded-secondary',
              },
            ],
            order: 0,
          },
        },
        order: 0,
      },
    },
    indicators: {
      AbsenceFields: {
        id: 'absence-fields',
        label: 'Absence fields',
        options: [
          {
            value: 'authorised-absence-sessions',
            label: 'Number of authorised absence sessions',
            unit: '',
            name: 'sess_authorised',
            decimalPlaces: 2,
          },
        ],
        order: 0,
      },
    },
    locations: {
      localAuthority: {
        legend: 'Local authority',
        options: [
          { value: 'barnet', label: 'Barnet' },
          { value: 'barnsley', label: 'Barnsley' },
        ],
      },
    },
    timePeriod: {
      legend: 'Time period',
      options: [{ label: '2014/15', code: 'AY', year: 2014 }],
    },
  };

  const testTableData: TableDataResponse = {
    subjectMeta: {
      filters: {
        SchoolType: {
          totalValue: '',
          hint: 'Filter by school type',
          legend: 'School type',
          options: {
            Default: {
              id: 'default',
              label: 'Default',
              options: [
                {
                  label: 'State-funded primary',
                  value: 'state-funded-primary',
                },
                {
                  label: 'State-funded secondary',
                  value: 'state-funded-secondary',
                },
              ],
              order: 0,
            },
          },
          order: 0,
          name: 'school_type',
        },
      },
      footnotes: [],
      indicators: [
        {
          label: 'Number of authorised absence sessions',
          unit: '',
          value: 'authorised-absence-sessions',
          name: 'sess_authorised',
        },
      ],
      locations: {
        localAuthority: [
          { id: 'barnet', label: 'Barnet', value: 'barnet' },
          { id: 'barnsley', label: 'Barnsley', value: 'barnsley' },
        ],
      },
      boundaryLevels: [],
      publicationName: 'Pupil absence',
      subjectName: 'Absence by characteristic',
      timePeriodRange: [{ code: 'AY', label: '2014/15', year: 2014 }],
      geoJsonAvailable: false,
    },
    results: [
      {
        filters: ['state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '2613',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': 'x',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['state-funded-secondary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnet',
        measures: {
          'authorised-absence-sessions': '1939',
        },
        timePeriod: '2014_AY',
      },
      {
        filters: ['state-funded-primary'],
        geographicLevel: 'localAuthority',
        locationId: 'barnsley',
        measures: {
          'authorised-absence-sessions': '39',
        },
        timePeriod: '2014_AY',
      },
    ],
  };

  const testPublication: Publication = {
    id: 'publication-1',
    title: 'Pupil absence',
    summary: 'Pupil absence summary',
    slug: 'pupil-absence',
    theme: { id: 'theme-1', title: 'Test theme' },
    topic: { id: 'topic-1', title: 'Test topic' },
  };

  const testFeaturedTables: FeaturedTable[] = [
    {
      id: 'block-1',
      name: 'Test highlight',
      description: 'Test highlight description',
    },
  ];
  const testSubjects: Subject[] = [
    {
      id: 'subject-1',
      name: 'Test subject',
      content: '<p>Test content</p>',
      timePeriods: {
        from: '2018',
        to: '2020',
      },
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

  const testDataBlock: ReleaseDataBlock = {
    id: 'block-1',
    name: 'Test block',
    highlightName: 'Test highlight name',
    source: '',
    heading: '',
    table: {
      tableHeaders: {
        columnGroups: [
          [
            { value: 'barnet', type: 'Location', level: 'localAuthority' },
            { value: 'barnsley', type: 'Location', level: 'localAuthority' },
          ],
        ],
        rowGroups: [
          [
            { value: 'state-funded-primary', type: 'Filter' },
            { value: 'state-funded-secondary', type: 'Filter' },
          ],
        ],
        columns: [{ value: '2014_AY', type: 'TimePeriod' }],
        rows: [{ value: 'authorised-absence-sessions', type: 'Indicator' }],
      },
      indicators: [],
    },
    charts: [],
    query: {
      subjectId: 'subject-1',
      indicators: ['authorised-absence-sessions'],
      filters: ['state-funded-primary', 'state-funded-secondary'],
      timePeriod: {
        startYear: 2014,
        startCode: 'AY',
        endYear: 2014,
        endCode: 'AY',
      },
      locationIds: ['barnet', 'barnsley'],
    },
  };

  test('renders correctly on step 1 with subjects and featured tables', async () => {
    publicationService.getPublication.mockResolvedValue(testPublication);
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
    tableBuilderService.listReleaseFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
        'aria-current',
        'step',
      );
    });
    const step1 = within(screen.getByTestId('wizardStep-1'));
    const tabs = step1.getAllByRole('tabpanel', { hidden: true });

    expect(tabs).toHaveLength(2);

    expect(
      within(tabs[0]).getByRole('heading', { name: 'Choose a table' }),
    ).toBeInTheDocument();
    expect(
      within(tabs[0]).getByRole('link', { name: 'Test highlight' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/prerelease/table-tool/block-1',
    );

    expect(within(tabs[1]).getByLabelText('Test subject')).toBeInTheDocument();

    expect(screen.queryByTestId('dataTableCaption')).not.toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('renders correctly on step 1 without featured tables', async () => {
    publicationService.getPublication.mockResolvedValue(testPublication);
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
    tableBuilderService.listReleaseFeaturedTables.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-1')).toHaveAttribute(
        'aria-current',
        'step',
      );
    });

    const step1 = within(screen.getByTestId('wizardStep-1'));

    expect(step1.getByLabelText('Test subject')).toBeInTheDocument();
    expect(
      step1.queryByRole('heading', { name: 'Choose a table' }),
    ).not.toBeInTheDocument();

    expect(screen.queryByTestId('dataTableCaption')).not.toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('renders correctly on step 5 with `dataBlockId` route param', async () => {
    publicationService.getPublication.mockResolvedValue(testPublication);
    dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);

    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
    tableBuilderService.listReleaseFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    tableBuilderService.getTableData.mockResolvedValue(testTableData);
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);

    renderPage([
      generatePath<PreReleaseTableToolRouteParams>(
        preReleaseTableToolRoute.path,
        {
          publicationId: 'publication-1',
          releaseId: 'release-1',
          dataBlockId: 'block-1',
        },
      ),
    ]);

    await waitFor(() => {
      expect(screen.getByTestId('wizardStep-5')).toHaveAttribute(
        'aria-current',
        'step',
      );
    });

    expect(screen.getByTestId('dataTableCaption')).toHaveTextContent(
      /Number of authorised absence sessions for 'Absence by characteristic'/,
    );

    expect(screen.getByRole('table')).toBeInTheDocument();
    expect(screen.getAllByRole('row')).toHaveLength(3);
    expect(screen.getAllByRole('cell')).toHaveLength(5);

    expect(
      screen.queryByRole('radio', {
        name: 'Table in ODS format (spreadsheet, with title and footnotes)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('radio', {
        name: 'Table in CSV format (flat file, with location codes)',
      }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Download table',
      }),
    ).toBeInTheDocument();
  });

  const renderPage = (
    initialEntries: string[] = [
      generatePath<PreReleaseTableToolRouteParams>(
        preReleaseTableToolRoute.path,
        {
          publicationId: 'publication-1',
          releaseId: 'release-1',
        },
      ),
    ],
  ) => {
    return render(
      <MemoryRouter initialEntries={initialEntries}>
        <Route
          component={PreReleaseTableToolPage}
          path={preReleaseTableToolRoute.path}
        />
      </MemoryRouter>,
    );
  };
});
