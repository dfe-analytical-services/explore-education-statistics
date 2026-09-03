import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import {
  testSubjectMeta,
  testTableData,
} from '@admin/pages/release/datablocks/__data__/tableToolServiceData';
import ReleaseDataBlockEditPage from '@admin/pages/release/datablocks/ReleaseDataBlockEditPage';
import {
  releaseDataBlockEditRoute,
  ReleaseDataBlockRouteParams,
} from '@admin/routes/releaseRoutes';
import _dataBlockService, {
  ReleaseDataBlock,
  ReleaseDataBlockSummary,
} from '@admin/services/dataBlockService';
import _permissionService from '@admin/services/permissionService';
import render from '@common-test/render';
import { Chart } from '@common/modules/charts/types/chart';
import _tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { produce } from 'immer';
import { ReactNode } from 'react';
import { MemoryRouter } from 'react-router';
import { generatePath, Route } from 'react-router-dom';

jest.mock('@admin/services/dataBlockService');
jest.mock('@admin/services/permissionService');
jest.mock('@common/services/tableBuilderService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

// Mock useMedia as tests for tabs can be flaky due to the
// role being added only on desktop.
jest.mock('@common/hooks/useMedia', () => ({
  useDesktopMedia: () => {
    return {
      onMedia: (value: string) => value,
    };
  },
}));

// Responsive charts don't render in tests so have do
// mock the responsive container and apply a fixed width.
jest.mock('recharts', () => {
  const OriginalModule = jest.requireActual('recharts');
  return {
    ...OriginalModule,
    ResponsiveContainer: ({
      children,
      height,
    }: {
      children: ReactNode;
      height: string;
    }) => (
      <OriginalModule.ResponsiveContainer width={800} height={height}>
        {children}
      </OriginalModule.ResponsiveContainer>
    ),
  };
});

describe('ReleaseDataBlockEditPage', () => {
  const chart: Chart = {
    title: 'Test chart title',
    alt: 'Test chart alt',
    type: 'verticalbar',
    height: 300,
    axes: {
      major: {
        type: 'major',
        visible: true,
        groupBy: 'timePeriod',
        min: 0,
        sortAsc: true,
        size: 50,
        showGrid: true,
        tickConfig: 'default',
        referenceLines: [],
        dataSets: [
          {
            filters: ['gender-female'],
            indicator: 'authorised-absence-sessions',
          },
        ],
      },
      minor: {
        type: 'minor',
        visible: true,
        min: 0,
        sortAsc: true,
        size: 50,
        showGrid: true,
        tickConfig: 'default',
        referenceLines: [],
        dataSets: [],
      },
    },
    legend: {
      position: 'top',
      items: [],
    },
  };

  const testDataBlock: ReleaseDataBlock = {
    id: 'block-1',
    dataBlockParentId: 'block-1-parent',
    dataSetId: 'data-set-1',
    dataSetName: 'Test data set',
    name: 'Test name 1',
    heading: 'Test title 1',
    highlightName: 'Test highlight name 1',
    highlightDescription: 'Test highlight description 1',
    source: 'Test source 1',
    query: {
      subjectId: 'subject-1',
      locationIds: ['barnet'],
      timePeriod: {
        startYear: 2020,
        startCode: 'AY',
        endYear: 2020,
        endCode: 'AY',
      },
      filters: ['gender-female'],
      indicators: ['authorised-absence-sessions'],
    },
    table: {
      tableHeaders: {
        columnGroups: [[{ type: 'TimePeriod', value: '2020_AY' }]],
        columns: [{ type: 'Filter', value: 'gender-female' }],
        rowGroups: [
          [{ type: 'Location', value: 'barnet', level: 'localAuthority' }],
        ],
        rows: [{ type: 'Indicator', value: 'authorised-absence-sessions' }],
      },
    },
    charts: [chart],
  };

  const testDataBlockSummaries: ReleaseDataBlockSummary[] = [
    {
      id: 'block-2',
      name: 'Test name 2',
      heading: 'Test title 2',
      highlightName: 'Test highlight name 2',
      highlightDescription: 'Test highlight description 2',
      source: 'Test source 2',
      chartsCount: 0,
      inContent: false,
    },
    {
      id: testDataBlock.id,
      name: testDataBlock.name,
      heading: testDataBlock.heading,
      highlightName: testDataBlock.highlightName,
      highlightDescription: testDataBlock.highlightDescription,
      source: testDataBlock.source,
      chartsCount: 0,
      inContent: true,
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

  beforeEach(() => {
    tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);
    tableBuilderService.getDataBlockTableData.mockResolvedValue(testTableData);

    dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
    dataBlockService.listDataBlocks.mockResolvedValue(testDataBlockSummaries);

    permissionService.canUpdateRelease.mockResolvedValue(true);
  });

  test('renders page elements correctly', async () => {
    renderPage();

    expect(
      await screen.findByRole('heading', { name: 'Edit data block' }),
    ).toBeInTheDocument();

    expect(
      await screen.findByRole('heading', { name: 'Test name 1' }),
    ).toBeInTheDocument();

    const tabs = screen.getAllByRole('tab');

    expect(tabs).toHaveLength(3);
    expect(tabs[0]).toHaveTextContent('Data source');
    expect(tabs[1]).toHaveTextContent('Table');
    expect(tabs[2]).toHaveTextContent('Chart');
  });

  test('renders with correct data block details', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Test name 1')).toBeInTheDocument();
    });

    expect(
      screen.getByRole('heading', { name: 'Test name 1' }),
    ).toBeInTheDocument();

    expect(screen.queryByTestId('Highlight name')).not.toBeInTheDocument();
    expect(screen.getByLabelText('URL')).toHaveValue(
      'http://localhost/data-tables/fast-track/block-1-parent',
    );
  });

  test('renders page selector with list of data blocks', async () => {
    renderPage();

    await waitFor(() => {
      expect(
        screen.getByLabelText('Select a data block to edit'),
      ).toBeInTheDocument();
    });

    const selector = screen.getByLabelText('Select a data block to edit');

    expect(selector).toHaveValue('block-1');

    const options = within(selector).getAllByRole('option');

    expect(options[0]).toHaveTextContent('Test name 2');
    expect(options[1]).toHaveTextContent('Test name 1');
  });

  test('renders with correct data block details with data set name', async () => {
    renderPage();

    await waitFor(() => {
      expect(
        screen.getByLabelText('Select a data block to edit'),
      ).toBeInTheDocument();
    });

    expect(screen.queryByText('Data set name')).toBeInTheDocument();
  });

  test('renders with correct data block details without data set name', async () => {
    dataBlockService.getDataBlock.mockResolvedValue(
      produce(testDataBlock, draft => {
        draft.dataSetName = undefined;
      }),
    );
    renderPage();

    await waitFor(() => {
      expect(
        screen.getByLabelText('Select a data block to edit'),
      ).toBeInTheDocument();
    });

    expect(screen.queryByText('Data set name')).not.toBeInTheDocument();
  });

  test('clicking `Delete this data block` button shows modal', async () => {
    dataBlockService.getDeleteBlockPlan.mockResolvedValue({
      dependentDataBlocks: [
        {
          name: 'Test name',
          contentSectionHeading: 'Test section',
          infographicFilesInfo: [],
          isKeyStatistic: false,
        },
      ],
    });

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Delete this data block')).toBeInTheDocument();
    });

    await userEvent.click(
      screen.getByRole('button', { name: 'Delete this data block' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Delete data block')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByRole('heading')).toHaveTextContent('Delete data block');
    expect(modal.getByTestId('deleteDataBlock-name')).toHaveTextContent(
      'Test name',
    );
    expect(
      modal.getByTestId('deleteDataBlock-contentSectionHeading'),
    ).toHaveTextContent('Test section');
  });

  test('clicking `Confirm` on modal deletes data block', async () => {
    dataBlockService.getDeleteBlockPlan.mockResolvedValue({
      dependentDataBlocks: [
        {
          name: 'Test name',
          contentSectionHeading: 'Test section',
          infographicFilesInfo: [],
          isKeyStatistic: false,
        },
      ],
    });

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Delete this data block')).toBeInTheDocument();
    });

    await userEvent.click(
      screen.getByRole('button', { name: 'Delete this data block' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Delete data block')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByRole('heading')).toHaveTextContent('Delete data block');

    expect(dataBlockService.deleteDataBlock).not.toHaveBeenCalled();

    await userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

    expect(dataBlockService.deleteDataBlock).toHaveBeenCalledTimes(1);
    expect(dataBlockService.deleteDataBlock).toHaveBeenCalledWith(
      'release-1',
      'block-1',
    );
  });

  test('clicking `Cancel` on modal hides it', async () => {
    dataBlockService.getDeleteBlockPlan.mockResolvedValue({
      dependentDataBlocks: [
        {
          name: 'Test name',
          contentSectionHeading: 'Test section',
          infographicFilesInfo: [],
          isKeyStatistic: false,
        },
      ],
    });

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Delete this data block')).toBeInTheDocument();
    });

    await userEvent.click(
      screen.getByRole('button', { name: 'Delete this data block' }),
    );

    await waitFor(() => {
      expect(screen.getByText('Delete data block')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByRole('heading')).toHaveTextContent('Delete data block');

    expect(dataBlockService.deleteDataBlock).not.toHaveBeenCalled();

    await userEvent.click(modal.getByRole('button', { name: 'Cancel' }));

    await waitFor(() => {
      expect(screen.queryByText('Delete data block')).not.toBeInTheDocument();
    });

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  test.each([
    { containsFileId: false, backButtonLabel: 'Back' },
    { containsFileId: true, backButtonLabel: 'Back to data replacement page' },
  ])(
    "page contains ($backButtonLabel) button depending on if the page has file ID in it's query parameters",
    async ({ containsFileId, backButtonLabel }) => {
      renderPage();

      if (containsFileId) {
        window.history.pushState(
          {},
          '',
          '?fromFileReplacementId=123e4567-e89b-12d3-a456-426614174000',
        );
      }
      const backButton = await screen.findByText(backButtonLabel);

      if (containsFileId) {
        expect(backButton).toHaveAttribute(
          'href',
          '/publication/publication-1/release/release-1/data/123e4567-e89b-12d3-a456-426614174000/replace',
        );
      } else {
        expect(backButton).toHaveAttribute(
          'href',
          '/publication/publication-1/release/release-1/data-blocks',
        );
      }
    },
  );

  describe('read-only view', () => {
    beforeEach(() => {
      permissionService.canUpdateRelease.mockResolvedValue(false);
    });

    test('renders page elements correctly', async () => {
      tableBuilderService.listReleaseSubjects.mockResolvedValue(testSubjects);

      renderPage();

      expect(
        await screen.findByRole('heading', { name: 'View data block' }),
      ).toBeInTheDocument();

      expect(
        await screen.findByRole('heading', { name: 'Test name 1' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByRole('button', { name: 'Delete this data block' }),
      ).not.toBeInTheDocument();

      expect(await screen.findByText('Test title 1')).toBeInTheDocument();

      const tabs = screen.getAllByRole('tab');

      expect(tabs).toHaveLength(2);
      expect(tabs[0]).toHaveTextContent('Table');
      expect(tabs[1]).toHaveTextContent('Chart');
    });

    test('renders with correct data block details with featured table name and description', async () => {
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Test name 1')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('heading', { name: 'Test name 1' }),
      ).toBeInTheDocument();

      expect(screen.getByTestId('Featured table name')).toHaveTextContent(
        'Test highlight name 1',
      );
      expect(
        screen.getByTestId('Featured table description'),
      ).toHaveTextContent('Test highlight description 1');
      expect(screen.getByLabelText('URL')).toHaveValue(
        'http://localhost/data-tables/fast-track/block-1-parent',
      );
    });

    test('renders with correct data block details without Featured table name and description', async () => {
      dataBlockService.getDataBlock.mockResolvedValue({
        ...testDataBlock,
        highlightName: '',
        highlightDescription: '',
      });

      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Test name 1')).toBeInTheDocument();
      });

      expect(
        screen.getByRole('heading', { name: 'Test name 1' }),
      ).toBeInTheDocument();

      expect(
        screen.queryByTestId('Featured table name'),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByTestId('Featured table description'),
      ).not.toBeInTheDocument();

      expect(screen.getByLabelText('URL')).toHaveValue(
        'http://localhost/data-tables/fast-track/block-1-parent',
      );
    });

    test('renders page selector with list of data blocks', async () => {
      renderPage();

      await waitFor(() => {
        expect(
          screen.getByLabelText('Select a data block to view'),
        ).toBeInTheDocument();
      });

      const selector = screen.getByLabelText('Select a data block to view');

      expect(selector).toHaveValue('block-1');

      const options = within(selector).getAllByRole('option');

      expect(options[0]).toHaveTextContent('Test name 2');
      expect(options[1]).toHaveTextContent('Test name 1');
    });

    test('renders table in `Table` tab', async () => {
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Table')).toBeInTheDocument();
      });

      expect(screen.getAllByRole('tab')).toHaveLength(2);

      const tabPanel = within(screen.getByRole('tabpanel'));
      expect(tabPanel.getByRole('table')).toBeInTheDocument();

      const rows = screen.getAllByRole('row');
      expect(rows).toHaveLength(2);

      expect(within(rows[0]).getByRole('columnheader')).toHaveTextContent(
        '2020/21',
      );
      expect(within(rows[1]).getByRole('rowheader')).toHaveTextContent(
        'Barnet',
      );
      expect(within(rows[1]).getByRole('cell')).toHaveTextContent('123.00');
    });

    test('renders chart in `Chart` tab', async () => {
      renderPage();

      await waitFor(() => {
        expect(screen.getByText('Chart')).toBeInTheDocument();
      });

      expect(screen.getAllByRole('tab')).toHaveLength(2);
      await userEvent.click(screen.getByRole('tab', { name: 'Chart' }));

      await waitFor(() => {
        const tabPanel = screen.getByRole('tabpanel');

        expect(
          tabPanel.querySelector('.recharts-cartesian-axis.xAxis'),
        ).toBeInTheDocument();
        expect(
          tabPanel.querySelector('.recharts-cartesian-axis.yAxis'),
        ).toBeInTheDocument();
      });
    });
  });

  const renderPage = () => {
    return render(
      <TestConfigContextProvider>
        <MemoryRouter
          initialEntries={[
            generatePath<ReleaseDataBlockRouteParams>(
              releaseDataBlockEditRoute.path,
              {
                publicationId: 'publication-1',
                releaseVersionId: 'release-1',
                dataBlockId: 'block-1',
              },
            ),
          ]}
        >
          <Route
            path={releaseDataBlockEditRoute.path}
            component={ReleaseDataBlockEditPage}
          />
        </MemoryRouter>
      </TestConfigContextProvider>,
    );
  };
});
