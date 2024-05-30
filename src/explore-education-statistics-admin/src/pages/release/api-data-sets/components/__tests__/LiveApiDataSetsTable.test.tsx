import LiveApiDataSetsTable, {
  LiveApiDataSetSummary,
} from '@admin/pages/release/api-data-sets/components/LiveApiDataSetsTable';
import { render as baseRender, screen, within } from '@testing-library/react';
import { ReactNode } from 'react';
import { MemoryRouter } from 'react-router-dom';

describe('LiveApiDataSetsTable', () => {
  const testDataSets: LiveApiDataSetSummary[] = [
    {
      id: 'data-set-2',
      title: 'Data set 2 title',
      summary: 'Data set 2 summary',
      status: 'Published',
      latestLiveVersion: {
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-2',
        version: '1.0',
        status: 'Published',
        type: 'Major',
      },
    },
    {
      id: 'data-set-1',
      title: 'Data set 1 title',
      summary: 'Data set 1 summary',
      status: 'Published',
      latestLiveVersion: {
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-1',
        version: '2.0',
        status: 'Published',
        type: 'Major',
      },
    },
    {
      id: 'data-set-3',
      title: 'Data set 3 title',
      summary: 'Data set 3 summary',
      status: 'Published',
      latestLiveVersion: {
        published: '2024-02-01T09:30:00+00:00',
        id: 'version-3',
        version: '1.2',
        status: 'Published',
        type: 'Minor',
      },
    },
  ];

  test('renders live data set rows correctly', () => {
    render(
      <LiveApiDataSetsTable
        canCreateNewVersions
        canUpdateRelease
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseId="release-1"
      />,
    );

    const baseDataSetUrl =
      '/publication/publication-1/release/release-1/api-data-sets';

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(4);

    // Row 1

    const row1Cells = within(rows[1]).getAllByRole('cell');

    expect(row1Cells[0]).toHaveTextContent('v2.0');
    expect(row1Cells[1]).toHaveTextContent('Data set 1 title');

    expect(
      within(row1Cells[2]).getByRole('link', {
        name: 'View details for Data set 1 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-1`);

    expect(
      within(row1Cells[2]).getByRole('button', {
        name: 'Create new version for Data set 1 title',
      }),
    ).toBeInTheDocument();

    // Row 2

    const row2Cells = within(rows[2]).getAllByRole('cell');

    expect(row2Cells[0]).toHaveTextContent('v1.0');
    expect(row2Cells[1]).toHaveTextContent('Data set 2 title');

    expect(
      within(row2Cells[2]).getByRole('link', {
        name: 'View details for Data set 2 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-2`);

    expect(
      within(row2Cells[2]).getByRole('button', {
        name: 'Create new version for Data set 2 title',
      }),
    ).toBeInTheDocument();

    // Row 3

    const row3Cells = within(rows[3]).getAllByRole('cell');

    expect(row3Cells[0]).toHaveTextContent('v1.2');
    expect(row3Cells[1]).toHaveTextContent('Data set 3 title');

    expect(
      within(row3Cells[2]).getByRole('link', {
        name: 'View details for Data set 3 title',
      }),
    ).toHaveAttribute('href', `${baseDataSetUrl}/data-set-3`);

    expect(
      within(row3Cells[2]).getByRole('button', {
        name: 'Create new version for Data set 3 title',
      }),
    ).toBeInTheDocument();
  });

  test("does not render 'Create new version' buttons when release cannot be updated", () => {
    render(
      <LiveApiDataSetsTable
        canCreateNewVersions
        canUpdateRelease={false}
        dataSets={testDataSets}
        publicationId="publication-1"
        releaseId="release-1"
      />,
    );

    expect(
      screen.queryAllByRole('button', { name: /Create new version/ }),
    ).toHaveLength(0);
  });

  test('renders message when no data sets', () => {
    render(
      <LiveApiDataSetsTable
        dataSets={[]}
        publicationId="publication-1"
        releaseId="release-1"
      />,
    );

    expect(screen.getByText(/No live API data sets/)).toBeInTheDocument();
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  function render(element: ReactNode) {
    return baseRender(<MemoryRouter>{element}</MemoryRouter>);
  }
});
