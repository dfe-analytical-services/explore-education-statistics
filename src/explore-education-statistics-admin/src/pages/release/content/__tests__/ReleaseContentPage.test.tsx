import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import ReleaseContentPage from '@admin/pages/release/content/ReleaseContentPage';
import {
  releaseContentRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import _releaseContentService, {
  EditableRelease,
  ReleaseContent,
} from '@admin/services/releaseContentService';
import _featuredTableService, {
  FeaturedTable,
} from '@admin/services/featuredTableService';
import _permissionService from '@admin/services/permissionService';
import _dataBlockService, {
  ReleaseDataBlock,
} from '@admin/services/dataBlockService';
import _publicationService from '@admin/services/publicationService';
import _methodologyService from '@admin/services/methodologyService';
import _tableBuilderService, {
  SubjectMeta,
  TableDataResponse,
  FeaturedTable as TableToolFeaturedTable,
  Subject,
} from '@common/services/tableBuilderService';
import connectionMock from '@admin/services/hubs/utils/__mocks__/connectionMock';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter, Route } from 'react-router';
import { generatePath } from 'react-router-dom';
import { HubConnectionState } from '@microsoft/signalr';
import userEvent from '@testing-library/user-event';

jest.mock('@admin/services/featuredTableService');
jest.mock('@admin/services/releaseContentService');
jest.mock('@admin/services/permissionService');
jest.mock('@admin/services/publicationService');
jest.mock('@admin/services/dataBlockService');
jest.mock('@admin/services/methodologyService');
jest.mock('@admin/services/hubs/utils/createConnection');
jest.mock('@common/services/tableBuilderService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const releaseContentService = _releaseContentService as jest.Mocked<
  typeof _releaseContentService
>;
const featuredTableService = _featuredTableService as jest.Mocked<
  typeof _featuredTableService
>;
const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ReleaseContentPage', () => {
  const testEditableRelease: EditableRelease = {
    approvalStatus: 'Draft',
    content: [
      {
        id: 'section-1',
        order: 0,
        heading: 'Section 1',
        content: [
          {
            id: 'block-1',
            order: 1,
            body: '<p>Section 1 content</p><p><a href="/data-table/fast-track/data-block-parent-1?featuredTable=true" data-featured-table>featured table link</a></p>',
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
      {
        id: 'section-2',
        order: 0,
        heading: 'Section 2',
        content: [
          {
            id: 'block-2',
            order: 0,
            body: '<p>Section 2 content</p>',
            type: 'HtmlBlock',
            comments: [],
          },
        ],
      },
    ],
    coverageTitle: 'Academic year',
    downloadFiles: [],
    hasDataGuidance: true,
    hasPreReleaseAccessList: false,
    headlinesSection: {
      id: 'headlines-id',
      heading: '',
      order: 0,
      content: [
        {
          id: 'headllines-id',
          order: 0,
          body: '<p>Headlines content</p>',
          type: 'HtmlBlock',
          comments: [],
        },
      ],
    },

    id: 'release-id',
    keyStatistics: [],
    keyStatisticsSecondarySection: {
      id: '',
      order: 0,
      heading: '',
      content: [],
    },
    latestRelease: false,
    publicationId: 'publication-id',
    publication: {
      id: 'publication-id',
      title: 'Publication 1',
      slug: 'publication-1',
      releaseSeries: [],
      theme: { id: 'theme-1', title: 'Theme 1' },
      contact: {
        contactName: 'John Smith',
        contactTelNo: '0777777777',
        teamEmail: 'john.smith@test.com',
        teamName: 'Team Smith',
      },
      methodologies: [],
    },
    publishingOrganisations: [
      {
        id: 'org-id-1',
        title: 'Department for Education',
        url: 'https://www.gov.uk/government/organisations/department-for-education',
      },
      {
        id: 'org-id-2',
        title: 'Other Organisation',
        url: 'https://example.com',
      },
    ],
    relatedInformation: [],
    slug: '2020-21',
    summarySection: {
      id: 'summary-id',
      order: 0,
      content: [],
      heading: '',
    },
    title: 'Academic year 2020/21',
    type: 'OfficialStatistics',
    updates: [],
    yearTitle: '2020/21',
  };

  const testReleaseContent: ReleaseContent = {
    release: testEditableRelease,
    unattachedDataBlocks: [],
  };

  const testFeaturedTables: FeaturedTable[] = [
    {
      id: 'featured-table-1',
      dataBlockId: 'data-block-1',
      dataBlockParentId: 'data-block-parent-1',
      description: '',
      name: 'Featured table 1',
      order: 0,
    },
    {
      id: 'featured-table-2',
      dataBlockId: 'data-block-2',
      dataBlockParentId: 'data-block-parent-2',
      description: '',
      name: 'Featured table 2',
      order: 0,
    },
  ];

  const testSubjectMeta: SubjectMeta = {
    filters: {
      SchoolType: {
        id: 'school-type',
        autoSelectFilterItemId: '',
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
          autoSelectFilterItemId: '',
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

  const testTableToolFeaturedTables: TableToolFeaturedTable[] = [
    {
      id: 'featured-table-1',
      dataBlockId: 'data-block-1',
      dataBlockParentId: 'data-block-parent-1',
      description: '',
      name: 'Featured table 1',
      order: 0,
      subjectId: 'subject-1',
    },
    {
      id: 'featured-table-2',
      dataBlockId: 'data-block-2',
      dataBlockParentId: 'data-block-parent-2',
      description: '',
      name: 'Featured table 2',
      order: 0,
      subjectId: 'subject-1',
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
      filters: ['Filter 1'],
      indicators: ['Indicator 1'],
      lastUpdated: '2023-12-01',
    },
  ];

  const testDataBlock: ReleaseDataBlock = {
    id: 'data-block-1',
    dataBlockParentId: 'data-block-parent-1',
    dataSetId: 'data-set-1',
    dataSetName: 'Test data set',
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

  beforeEach(() => {
    jest
      .spyOn(connectionMock, 'state', 'get')
      .mockReturnValue(HubConnectionState.Connected);
  });

  test('renders the editing mode panel', async () => {
    releaseContentService.getContent.mockResolvedValue(testReleaseContent);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );
    permissionService.canUpdateRelease.mockResolvedValue(true);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('button', { name: 'Change page view' }),
    ).toBeInTheDocument();

    const radios = within(
      screen.getByRole('group', { name: 'Change page view' }),
    ).getAllByRole('radio');
    expect(radios).toHaveLength(3);
    expect(radios[0]).toEqual(screen.getByLabelText('Edit content'));
    expect(radios[0]).toBeChecked();
    expect(radios[1]).toEqual(screen.getByLabelText('Preview release page'));
    expect(radios[2]).toEqual(screen.getByLabelText('Preview table tool'));
  });

  test('maintains the open state of accordions when switching between edit and preview mode', async () => {
    releaseContentService.getContent.mockResolvedValue(testReleaseContent);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );
    permissionService.canUpdateRelease.mockResolvedValue(true);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
    });

    const contentAccordion = screen.getAllByTestId('accordion')[0];
    const contentAccordionSections =
      within(contentAccordion).getAllByTestId('accordionSection');

    const section1Button = await within(contentAccordionSections[0]).findByRole(
      'button',
      {
        name: /Section 1/,
      },
    );

    expect(section1Button).toHaveAttribute('aria-expanded', 'false');

    await userEvent.click(section1Button);

    expect(section1Button).toHaveAttribute('aria-expanded', 'true');

    await userEvent.click(screen.getByLabelText('Preview release page'));

    await waitFor(() =>
      expect(screen.queryByText('Add note')).not.toBeInTheDocument(),
    );

    expect(section1Button).toHaveAttribute('aria-expanded', 'true');

    await userEvent.click(section1Button);

    expect(section1Button).toHaveAttribute('aria-expanded', 'false');

    await userEvent.click(screen.getByLabelText('Edit content'));

    await waitFor(() =>
      expect(screen.getByText('Add note')).toBeInTheDocument(),
    );

    expect(section1Button).toHaveAttribute('aria-expanded', 'false');
  });

  describe('edit content mode', () => {
    test('renders the release content in edit mode', async () => {
      releaseContentService.getContent.mockResolvedValue(testReleaseContent);
      featuredTableService.listFeaturedTables.mockResolvedValue(
        testFeaturedTables,
      );
      permissionService.canUpdateRelease.mockResolvedValue(true);

      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('heading', { name: 'Publication 1' }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('Published-value')).toHaveTextContent('TBA');

      expect(screen.getByTestId('Release type-value')).toHaveTextContent(
        'Official statistics',
      );

      expect(
        screen.getByRole('button', { name: 'Add secondary stats' }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', {
          name: 'Add key statistic from data block',
        }),
      ).toBeInTheDocument();
      expect(
        screen.getByRole('button', { name: 'Add free text key statistic' }),
      ).toBeInTheDocument();

      const contentAccordion = screen.getAllByTestId('accordion')[0];
      const contentAccordionSections =
        within(contentAccordion).getAllByTestId('accordionSection');

      expect(contentAccordionSections).toHaveLength(2);

      expect(await screen.findByText('Show all sections')).toBeInTheDocument();

      expect(
        within(contentAccordionSections[0]).getByRole('button', {
          name: /Section 1/,
        }),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).getByText('Section 1 content'),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).getByRole('button', {
          name: 'Edit section title',
        }),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).getByRole('button', {
          name: 'Reorder this section',
        }),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).getByRole('button', {
          name: 'Remove this section',
        }),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).getByRole('button', {
          name: 'Edit block',
        }),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).getByRole('button', {
          name: 'Remove block',
        }),
      ).toBeInTheDocument();

      expect(
        screen.getByRole('button', {
          name: 'Add new section',
        }),
      ).toBeInTheDocument();
    });
  });

  describe('release preview mode', () => {
    test('renders the release content in preview mode', async () => {
      releaseContentService.getContent.mockResolvedValue(testReleaseContent);
      featuredTableService.listFeaturedTables.mockResolvedValue(
        testFeaturedTables,
      );
      permissionService.canUpdateRelease.mockResolvedValue(true);

      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
      });

      await userEvent.click(screen.getByLabelText('Preview release page'));

      expect(
        screen.getByRole('heading', { name: 'Publication 1' }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('Published-value')).toHaveTextContent('TBA');

      expect(screen.getByTestId('Release type-value')).toHaveTextContent(
        'Official statistics',
      );

      expect(screen.getByTestId('Produced by-value')).toHaveTextContent(
        'Department for Education and Other Organisation',
      );

      expect(
        screen.queryByRole('button', { name: 'Add secondary stats' }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', {
          name: 'Add key statistic from data block',
        }),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByRole('button', { name: 'Add free text key statistic' }),
      ).not.toBeInTheDocument();

      const contentAccordion = screen.getAllByTestId('accordion')[0];
      const contentAccordionSections =
        within(contentAccordion).getAllByTestId('accordionSection');

      expect(contentAccordionSections).toHaveLength(2);

      expect(
        within(contentAccordionSections[0]).getByRole('button', {
          name: /Section 1/,
        }),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).getByText('Section 1 content'),
      ).toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).queryByRole('button', {
          name: 'Edit section title',
        }),
      ).not.toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).queryByRole('button', {
          name: 'Reorder this section',
        }),
      ).not.toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).queryByRole('button', {
          name: 'Remove this section',
        }),
      ).not.toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).queryByRole('button', {
          name: 'Edit block',
        }),
      ).not.toBeInTheDocument();
      expect(
        within(contentAccordionSections[0]).queryByRole('button', {
          name: 'Remove block',
        }),
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole('button', {
          name: 'Add new section',
        }),
      ).not.toBeInTheDocument();
    });

    test('transforms featured table links to buttons which open the table tool preview on draft releases', async () => {
      releaseContentService.getContent.mockResolvedValue(testReleaseContent);
      featuredTableService.listFeaturedTables.mockResolvedValue(
        testFeaturedTables,
      );
      permissionService.canUpdateRelease.mockResolvedValue(true);
      dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
      tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
      tableBuilderService.listReleaseFeaturedTables.mockResolvedValue(
        testTableToolFeaturedTables,
      );
      tableBuilderService.getTableData.mockResolvedValue(testTableData);
      tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
      methodologyService.listLatestMethodologyVersions.mockResolvedValue([]);
      publicationService.getExternalMethodology.mockResolvedValue(undefined);
      publicationService.getContact.mockResolvedValue({
        contactName: 'Contact name',
        teamEmail: 'team@test.co.uk',
        teamName: 'Team name',
        contactTelNo: '01234567',
      });

      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
      });

      await userEvent.click(screen.getByLabelText('Preview release page'));

      expect(
        screen.queryByRole('link', {
          name: 'featured table link',
        }),
      ).not.toBeInTheDocument();

      await userEvent.click(
        screen.getByRole('button', { name: 'featured table link' }),
      );

      await waitFor(() => {
        expect(screen.getByText('Table tool')).toBeInTheDocument();
      });

      expect(screen.getByText('Explore data')).toBeInTheDocument();
      expect(screen.getByTestId('dataTableCaption-table')).toBeInTheDocument();
    });

    test('transforms featured table links to buttons which open the table tool preview on scheduled releases', async () => {
      const testScheduledRelease: EditableRelease = {
        ...testReleaseContent.release,
        approvalStatus: 'Approved',
        publishScheduled: '3000-01-01',
      };
      releaseContentService.getContent.mockResolvedValue({
        ...testReleaseContent,
        release: testScheduledRelease,
      });
      featuredTableService.listFeaturedTables.mockResolvedValue(
        testFeaturedTables,
      );
      permissionService.canUpdateRelease.mockResolvedValue(true);
      dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
      tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
      tableBuilderService.listReleaseFeaturedTables.mockResolvedValue(
        testTableToolFeaturedTables,
      );
      tableBuilderService.getTableData.mockResolvedValue(testTableData);
      tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
      methodologyService.listLatestMethodologyVersions.mockResolvedValue([]);
      publicationService.getExternalMethodology.mockResolvedValue(undefined);
      publicationService.getContact.mockResolvedValue({
        contactName: 'Contact name',
        teamEmail: 'team@test.co.uk',
        teamName: 'Team name',
        contactTelNo: '01234567',
      });

      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
      });

      await userEvent.click(screen.getByLabelText('Preview release page'));

      expect(
        screen.queryByRole('link', {
          name: 'featured table link',
        }),
      ).not.toBeInTheDocument();

      await userEvent.click(
        screen.getByRole('button', { name: 'featured table link' }),
      );

      await waitFor(() => {
        expect(screen.getByText('Table tool')).toBeInTheDocument();
      });

      expect(screen.getByText('Explore data')).toBeInTheDocument();
      expect(screen.getByTestId('dataTableCaption-table')).toBeInTheDocument();
    });

    test('does not transform featured table links on published releases', async () => {
      const testPublishedRelease: EditableRelease = {
        ...testReleaseContent.release,
        approvalStatus: 'Approved',
        published: '2023-01-01',
      };
      releaseContentService.getContent.mockResolvedValue({
        ...testReleaseContent,
        release: testPublishedRelease,
      });
      featuredTableService.listFeaturedTables.mockResolvedValue(
        testFeaturedTables,
      );
      permissionService.canUpdateRelease.mockResolvedValue(false);

      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
      });

      expect(
        screen.queryByRole('button', { name: 'featured table link' }),
      ).not.toBeInTheDocument();

      expect(
        screen.getByRole('link', {
          name: 'featured table link',
        }),
      ).toHaveAttribute(
        'href',
        '/data-table/fast-track/data-block-parent-1?featuredTable=true',
      );
    });
  });

  describe('table tool preview mode', () => {
    test('renders the table tool preview', async () => {
      releaseContentService.getContent.mockResolvedValue(testReleaseContent);
      featuredTableService.listFeaturedTables.mockResolvedValue(
        testFeaturedTables,
      );
      permissionService.canUpdateRelease.mockResolvedValue(true);
      dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
      tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);
      tableBuilderService.listReleaseFeaturedTables.mockResolvedValue(
        testTableToolFeaturedTables,
      );
      tableBuilderService.getTableData.mockResolvedValue(testTableData);
      tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);

      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Academic year 2020/21')).toBeInTheDocument();
      });

      await userEvent.click(screen.getByLabelText('Preview table tool'));

      await waitFor(() => {
        expect(screen.getByText('Table tool')).toBeInTheDocument();
      });
    });
  });

  const renderPage = (
    initialEntries: string[] = [
      generatePath<ReleaseRouteParams>(releaseContentRoute.path, {
        publicationId: 'publication-1',
        releaseVersionId: 'release-1',
      }),
    ],
  ) => {
    return render(
      <MemoryRouter initialEntries={initialEntries}>
        <TestConfigContextProvider>
          <Route
            component={ReleaseContentPage}
            path={releaseContentRoute.path}
          />
        </TestConfigContextProvider>
      </MemoryRouter>,
    );
  };
});
