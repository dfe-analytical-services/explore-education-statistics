import ReleaseDataBlocksPage from '@admin/pages/release/datablocks/ReleaseDataBlocksPage';
import {
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import _dataBlockService, {
  DeleteDataBlockPlan,
  ReleaseDataBlockSummary,
} from '@admin/services/dataBlockService';
import _featuredTableService, {
  FeaturedTable,
} from '@admin/services/featuredTableService';
import _permissionService from '@admin/services/permissionService';
import render from '@common-test/render';
import { waitFor } from '@testing-library/dom';
import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { generatePath, MemoryRouter } from 'react-router';
import { Route } from 'react-router-dom';

jest.mock('@admin/services/dataBlockService');
jest.mock('@admin/services/featuredTableService');
jest.mock('@admin/services/permissionService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;
const featuredTableService = _featuredTableService as jest.Mocked<
  typeof _featuredTableService
>;
const permissionService = _permissionService as jest.Mocked<
  typeof _permissionService
>;

describe('ReleaseDataBlocksPage', () => {
  const testDataBlocks: ReleaseDataBlockSummary[] = [
    {
      id: 'block-1',
      name: 'Block 1',
      created: undefined,
      heading: 'Block 1 heading',
      source: 'Block 1 source',
      inContent: true,
      chartsCount: 1,
    },
    {
      id: 'block-2',
      name: 'Block 2',
      created: '2021-01-01T15:00:00.0000000',
      heading: 'Block 2 heading',
      source: 'Block 2 source',
      inContent: false,
      chartsCount: 0,
    },
    {
      id: 'block-3',
      name: 'Block 3',
      created: '2021-01-01T15:00:00.0000000',
      heading: 'Block 3 heading',
      source: 'Block 3 source',
      inContent: false,
      chartsCount: 0,
    },
    {
      id: 'block-4',
      name: 'Block 4',
      created: '2021-02-01T15:00:00.0000000',
      heading: 'Block 4 heading',
      source: 'Block 4 source',
      inContent: false,
      chartsCount: 0,
    },
  ];

  const testFeaturedTables: FeaturedTable[] = [
    {
      id: 'featured-1',
      dataBlockId: 'block-1',
      description: 'Featured 1 description',
      name: 'Featured 1',
      order: 0,
    },
    {
      id: 'featured-2',
      dataBlockId: 'block-3',
      description: 'Featured 2 description',
      name: 'Featured 3',
      order: 1,
    },
  ];

  const testBlock1DeletePlan: DeleteDataBlockPlan = {
    dependentDataBlocks: [
      {
        name: 'Block 1',
        contentSectionHeading: 'Section 1',
        infographicFilesInfo: [],
        isKeyStatistic: true,
        featuredTable: {
          name: 'Featured 1',
          description: 'Featured 1 description',
        },
      },
    ],
  };

  beforeEach(() => {
    permissionService.canUpdateRelease.mockResolvedValue(true);
  });

  test('renders featured tables and data blocks correctly', async () => {
    dataBlockService.listDataBlocks.mockResolvedValue(testDataBlocks);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('dataBlocks')).toBeInTheDocument();
    });

    const featuredTablesTable = within(screen.getByTestId('featuredTables'));
    const featuredTablesRows = featuredTablesTable.getAllByRole('row');
    expect(featuredTablesRows).toHaveLength(3);

    const featuredTablesRow1Cells = within(featuredTablesRows[1]).getAllByRole(
      'cell',
    );
    expect(featuredTablesRow1Cells).toHaveLength(6);
    expect(featuredTablesRow1Cells[0]).toHaveTextContent('Block 1');
    expect(featuredTablesRow1Cells[1]).toHaveTextContent('Yes');
    expect(featuredTablesRow1Cells[2]).toHaveTextContent('Yes');
    expect(featuredTablesRow1Cells[3]).toHaveTextContent('Featured 1');
    expect(featuredTablesRow1Cells[4]).toHaveTextContent('Not available');
    expect(
      within(featuredTablesRow1Cells[5]).getByRole('link', {
        name: 'Edit block',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data-blocks/block-1',
    );
    expect(
      within(featuredTablesRow1Cells[5]).getByRole('button', {
        name: 'Delete block',
      }),
    ).toBeInTheDocument();

    const featuredTablesRow2Cells = within(featuredTablesRows[2]).getAllByRole(
      'cell',
    );
    expect(featuredTablesRow2Cells).toHaveLength(6);
    expect(featuredTablesRow2Cells[0]).toHaveTextContent('Block 3');
    expect(featuredTablesRow2Cells[1]).toHaveTextContent('No');
    expect(featuredTablesRow2Cells[2]).toHaveTextContent('No');
    expect(featuredTablesRow2Cells[3]).toHaveTextContent('Featured 3');
    expect(featuredTablesRow2Cells[4]).toHaveTextContent(
      '1 January 2021 15:00',
    );
    expect(
      within(featuredTablesRow2Cells[5]).getByRole('link', {
        name: 'Edit block',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data-blocks/block-3',
    );
    expect(
      within(featuredTablesRow2Cells[5]).getByRole('button', {
        name: 'Delete block',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reorder featured tables' }),
    ).toBeInTheDocument();

    const dataBlocksTable = within(screen.getByTestId('dataBlocks'));
    const dataBlocksRows = dataBlocksTable.getAllByRole('row');
    expect(dataBlocksRows).toHaveLength(3);

    const dataBlocksRow1Cells = within(dataBlocksRows[1]).getAllByRole('cell');
    expect(dataBlocksRow1Cells).toHaveLength(5);
    expect(dataBlocksRow1Cells[0]).toHaveTextContent('Block 2');
    expect(dataBlocksRow1Cells[1]).toHaveTextContent('No');
    expect(dataBlocksRow1Cells[2]).toHaveTextContent('No');
    expect(dataBlocksRow1Cells[3]).toHaveTextContent('1 January 2021 15:00');
    expect(
      within(dataBlocksRow1Cells[4]).getByRole('link', { name: 'Edit block' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data-blocks/block-2',
    );
    expect(
      within(dataBlocksRow1Cells[4]).getByRole('button', {
        name: 'Delete block',
      }),
    ).toBeInTheDocument();

    const dataBlocksRow2Cells = within(dataBlocksRows[2]).getAllByRole('cell');
    expect(dataBlocksRow2Cells).toHaveLength(5);
    expect(dataBlocksRow2Cells[0]).toHaveTextContent('Block 4');
    expect(dataBlocksRow2Cells[1]).toHaveTextContent('No');
    expect(dataBlocksRow2Cells[2]).toHaveTextContent('No');
    expect(dataBlocksRow2Cells[3]).toHaveTextContent('1 February 2021 15:00');
    expect(
      within(dataBlocksRow2Cells[4]).getByRole('link', { name: 'Edit block' }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data-blocks/block-4',
    );
    expect(
      within(dataBlocksRow2Cells[4]).getByRole('button', {
        name: 'Delete block',
      }),
    ).toBeInTheDocument();
  });

  test('renders page correctly when release cannot be updated', async () => {
    permissionService.canUpdateRelease.mockResolvedValue(false);
    dataBlockService.listDataBlocks.mockResolvedValue(testDataBlocks);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(
        screen.getByText(
          /This release has been approved, and can no longer be updated/,
        ),
      ).toBeInTheDocument();
    });

    const featuredTablesTable = within(screen.getByTestId('featuredTables'));
    const featuredTablesRows = featuredTablesTable.getAllByRole('row');
    expect(featuredTablesRows).toHaveLength(3);

    const featuredTablesRow1Cells = within(featuredTablesRows[1]).getAllByRole(
      'cell',
    );
    expect(featuredTablesRow1Cells).toHaveLength(6);
    expect(featuredTablesRow1Cells[0]).toHaveTextContent('Block 1');
    expect(featuredTablesRow1Cells[1]).toHaveTextContent('Yes');
    expect(featuredTablesRow1Cells[2]).toHaveTextContent('Yes');
    expect(featuredTablesRow1Cells[3]).toHaveTextContent('Featured 1');
    expect(featuredTablesRow1Cells[4]).toHaveTextContent('Not available');
    expect(
      within(featuredTablesRow1Cells[5]).queryByRole('link', {
        name: 'Edit block',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(featuredTablesRow1Cells[5]).queryByRole('button', {
        name: 'Delete block',
      }),
    ).not.toBeInTheDocument();

    const featuredTablesRow2Cells = within(featuredTablesRows[2]).getAllByRole(
      'cell',
    );
    expect(featuredTablesRow2Cells).toHaveLength(6);
    expect(featuredTablesRow2Cells[0]).toHaveTextContent('Block 3');
    expect(featuredTablesRow2Cells[1]).toHaveTextContent('No');
    expect(featuredTablesRow2Cells[2]).toHaveTextContent('No');
    expect(featuredTablesRow2Cells[3]).toHaveTextContent('Featured 3');
    expect(featuredTablesRow2Cells[4]).toHaveTextContent(
      '1 January 2021 15:00',
    );
    expect(
      within(featuredTablesRow2Cells[5]).queryByRole('link', {
        name: 'Edit block',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(featuredTablesRow2Cells[5]).queryByRole('button', {
        name: 'Delete block',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Reorder featured tables' }),
    ).not.toBeInTheDocument();

    const dataBlocksTable = within(screen.getByTestId('dataBlocks'));
    const dataBlocksRows = dataBlocksTable.getAllByRole('row');
    expect(dataBlocksRows).toHaveLength(3);

    const dataBlocksRow1Cells = within(dataBlocksRows[1]).getAllByRole('cell');
    expect(dataBlocksRow1Cells).toHaveLength(5);
    expect(dataBlocksRow1Cells[0]).toHaveTextContent('Block 2');
    expect(dataBlocksRow1Cells[1]).toHaveTextContent('No');
    expect(dataBlocksRow1Cells[2]).toHaveTextContent('No');
    expect(dataBlocksRow1Cells[3]).toHaveTextContent('1 January 2021 15:00');
    expect(
      within(dataBlocksRow1Cells[4]).queryByRole('link', {
        name: 'Edit block',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(dataBlocksRow1Cells[4]).queryByRole('button', {
        name: 'Delete block',
      }),
    ).not.toBeInTheDocument();

    const dataBlocksRow2Cells = within(dataBlocksRows[2]).getAllByRole('cell');
    expect(dataBlocksRow2Cells).toHaveLength(5);
    expect(dataBlocksRow2Cells[0]).toHaveTextContent('Block 4');
    expect(dataBlocksRow2Cells[1]).toHaveTextContent('No');
    expect(dataBlocksRow2Cells[2]).toHaveTextContent('No');
    expect(dataBlocksRow2Cells[3]).toHaveTextContent('1 February 2021 15:00');
    expect(
      within(dataBlocksRow2Cells[4]).queryByRole('link', {
        name: 'Edit block',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(dataBlocksRow2Cells[4]).queryByRole('button', {
        name: 'Delete block',
      }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', { name: 'Create data block' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('link', { name: 'Go to table tool' }),
    ).toBeInTheDocument();
  });

  test('clicking `Delete block` button shows modal', async () => {
    dataBlockService.listDataBlocks.mockResolvedValue(testDataBlocks);
    dataBlockService.getDeleteBlockPlan.mockResolvedValue(testBlock1DeletePlan);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('dataBlocks')).toBeInTheDocument();
    });

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(dataBlockService.getDeleteBlockPlan).toHaveBeenCalledTimes(0);

    const buttons = screen.getAllByRole('button', { name: 'Delete block' });

    userEvent.click(buttons[0]);

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    expect(dataBlockService.getDeleteBlockPlan).toHaveBeenCalledTimes(1);

    const modal = within(screen.getByRole('dialog'));

    expect(modal.getByTestId('deleteDataBlock-name')).toHaveTextContent(
      'Block 1',
    );
    expect(
      modal.getByTestId('deleteDataBlock-contentSectionHeading'),
    ).toHaveTextContent('Section 1');
  });

  test('clicking `Cancel` button hides modal', async () => {
    dataBlockService.listDataBlocks.mockResolvedValue(testDataBlocks);
    dataBlockService.getDeleteBlockPlan.mockResolvedValue(testBlock1DeletePlan);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('dataBlocks')).toBeInTheDocument();
    });

    const buttons = screen.getAllByRole('button', { name: 'Delete block' });

    userEvent.click(buttons[0]);

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    userEvent.click(modal.getByRole('button', { name: 'Cancel' }));

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  test('clicking `Confirm` button hides modal and deletes data block', async () => {
    dataBlockService.listDataBlocks.mockResolvedValue(testDataBlocks);
    dataBlockService.getDeleteBlockPlan.mockResolvedValue(testBlock1DeletePlan);
    featuredTableService.listFeaturedTables.mockResolvedValue(
      testFeaturedTables,
    );

    renderPage();

    await waitFor(() => {
      expect(screen.getByTestId('dataBlocks')).toBeInTheDocument();
    });

    const buttons = screen.getAllByRole('button', { name: 'Delete block' });

    userEvent.click(buttons[0]);

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    const modal = within(screen.getByRole('dialog'));

    userEvent.click(modal.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });

    expect(dataBlockService.deleteDataBlock).toHaveBeenCalledTimes(1);
    expect(dataBlockService.deleteDataBlock).toHaveBeenCalledWith(
      'release-1',
      'block-1',
    );

    const featuredTablesTable = within(screen.getByTestId('featuredTables'));
    const featuredTablesRows = featuredTablesTable.getAllByRole('row');
    expect(featuredTablesRows).toHaveLength(2);

    const featuredTablesRow1Cells = within(featuredTablesRows[1]).getAllByRole(
      'cell',
    );
    expect(featuredTablesRow1Cells).toHaveLength(6);
    expect(featuredTablesRow1Cells[0]).toHaveTextContent('Block 3');
    expect(featuredTablesRow1Cells[1]).toHaveTextContent('No');
    expect(featuredTablesRow1Cells[2]).toHaveTextContent('No');
    expect(featuredTablesRow1Cells[3]).toHaveTextContent('Featured 3');
    expect(featuredTablesRow1Cells[4]).toHaveTextContent(
      '1 January 2021 15:00',
    );
    expect(
      within(featuredTablesRow1Cells[5]).getByRole('link', {
        name: 'Edit block',
      }),
    ).toHaveAttribute(
      'href',
      '/publication/publication-1/release/release-1/data-blocks/block-3',
    );
    expect(
      within(featuredTablesRow1Cells[5]).getByRole('button', {
        name: 'Delete block',
      }),
    ).toBeInTheDocument();
  });

  const renderPage = () => {
    return render(
      <MemoryRouter
        initialEntries={[
          generatePath<ReleaseRouteParams>(releaseDataBlocksRoute.path, {
            releaseId: 'release-1',
            publicationId: 'publication-1',
          }),
        ]}
      >
        <Route
          path={releaseDataBlocksRoute.path}
          component={ReleaseDataBlocksPage}
        />
      </MemoryRouter>,
    );
  };
});
