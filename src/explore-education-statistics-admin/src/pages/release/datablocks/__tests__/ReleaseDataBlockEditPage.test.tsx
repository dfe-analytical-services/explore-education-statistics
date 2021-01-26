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
} from '@admin/services/dataBlockService';
import _tableBuilderService from '@common/services/tableBuilderService';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { MemoryRouter } from 'react-router';
import { generatePath, Route } from 'react-router-dom';

jest.mock('@admin/services/dataBlockService');
jest.mock('@admin/services/permissionService');
jest.mock('@common/services/tableBuilderService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const tableBuilderService = _tableBuilderService as jest.Mocked<
  typeof _tableBuilderService
>;

describe('ReleaseDataBlockEditPage', () => {
  const testDataBlock: ReleaseDataBlock = {
    id: 'block-1',
    name: 'Test name 1',
    heading: 'Test title 1',
    highlightName: 'Test highlight name 1',
    source: 'Test source 1',
    query: {
      subjectId: 'subject-1',
      locations: {},
      filters: [],
      indicators: [],
    },
    table: {
      indicators: [],
      tableHeaders: {
        columnGroups: [],
        columns: [],
        rowGroups: [],
        rows: [],
      },
    },
    charts: [],
  };

  beforeEach(() => {
    tableBuilderService.getSubjectMeta.mockResolvedValue(testSubjectMeta);
    tableBuilderService.getTableData.mockResolvedValue(testTableData);
  });

  test('renders with correct data block details', async () => {
    dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
    dataBlockService.listDataBlocks.mockResolvedValue([]);

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('heading', { name: 'Test name 1' }),
      ).toBeInTheDocument();
      expect(screen.getByTestId('fastTrackUrl')).toHaveTextContent(
        'http://localhost/data-tables/fast-track/block-1',
      );
    });
  });

  test('renders selector with list of data blocks', async () => {
    dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
    dataBlockService.listDataBlocks.mockResolvedValue([
      {
        id: 'block-2',
        name: 'Test name 2',
        heading: 'Test title 2',
        highlightName: 'Test highlight name 2',
        source: 'Test source 2',
        chartsCount: 0,
      },
      {
        id: testDataBlock.id,
        name: testDataBlock.name,
        heading: testDataBlock.heading,
        highlightName: testDataBlock.highlightName,
        source: testDataBlock.source,
        chartsCount: 0,
      },
    ]);

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

  test('clicking `Delete this data block` button shows modal', async () => {
    dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
    dataBlockService.listDataBlocks.mockResolvedValue([]);
    dataBlockService.getDeleteBlockPlan.mockResolvedValue({
      dependentDataBlocks: [
        {
          name: 'Test name',
          contentSectionHeading: 'Test section',
          infographicFilesInfo: [],
        },
      ],
    });

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Delete this data block' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Delete this data block' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
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
    dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
    dataBlockService.listDataBlocks.mockResolvedValue([]);
    dataBlockService.getDeleteBlockPlan.mockResolvedValue({
      dependentDataBlocks: [
        {
          name: 'Test name',
          contentSectionHeading: 'Test section',
          infographicFilesInfo: [],
        },
      ],
    });

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Delete this data block' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Delete this data block' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    expect(dataBlockService.deleteDataBlock).not.toHaveBeenCalled();

    userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

    expect(dataBlockService.deleteDataBlock).toHaveBeenCalledTimes(1);
    expect(dataBlockService.deleteDataBlock).toHaveBeenCalledWith(
      'release-1',
      'block-1',
    );
  });

  test('clicking `Cancel` on modal hides it', async () => {
    dataBlockService.getDataBlock.mockResolvedValue(testDataBlock);
    dataBlockService.listDataBlocks.mockResolvedValue([]);
    dataBlockService.getDeleteBlockPlan.mockResolvedValue({
      dependentDataBlocks: [
        {
          name: 'Test name',
          contentSectionHeading: 'Test section',
          infographicFilesInfo: [],
        },
      ],
    });

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByRole('button', { name: 'Delete this data block' }),
      ).toBeInTheDocument();
    });

    userEvent.click(
      screen.getByRole('button', { name: 'Delete this data block' }),
    );

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    expect(dataBlockService.deleteDataBlock).not.toHaveBeenCalled();

    userEvent.click(modal.getByRole('button', { name: 'Cancel' }));

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
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
                releaseId: 'release-1',
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
