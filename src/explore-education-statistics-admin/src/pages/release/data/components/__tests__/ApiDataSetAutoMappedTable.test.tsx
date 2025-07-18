import ApiDataSetAutoMappedTable from '@admin/pages/release/data/components/ApiDataSetAutoMappedTable';
import { AutoMappedLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetAutoMappedTable', () => {
  const testLocations: AutoMappedLocation[] = [
    {
      candidate: {
        label: 'Location 1',
        code: 'location-1-code',
        key: 'Location1Key',
      },
      mapping: {
        candidateKey: 'Location1Key',
        publicId: 'location-1-public-id',
        source: {
          label: 'Location 1',
          code: 'location-1-code',
        },
        sourceKey: 'Location1Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 2',
        code: 'location-2-code',
        key: 'Location2Key',
      },
      mapping: {
        candidateKey: 'Location2Key',
        publicId: 'location-2-public-id',
        source: {
          label: 'Location 2',
          code: 'location-2-code',
        },
        sourceKey: 'Location2Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 3',
        code: 'location-3-code',
        key: 'Location3Key',
      },
      mapping: {
        candidateKey: 'Location3Key',
        publicId: 'location-3-public-id',
        source: {
          label: 'Location 3',
          code: 'location-3-code',
        },
        sourceKey: 'Location3Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 4',
        code: 'location-4-code',
        key: 'Location4Key',
      },
      mapping: {
        candidateKey: 'Location4Key',
        publicId: 'location-4-public-id',
        source: {
          label: 'Location 4',
          code: 'location-4-code',
        },
        sourceKey: 'Location4Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 5',
        code: 'location-5-code',
        key: 'Location5Key',
      },
      mapping: {
        candidateKey: 'Location5Key',
        publicId: 'location-5-public-id',
        source: {
          label: 'Location 5',
          code: 'location-5-code',
        },
        sourceKey: 'Location5Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 6',
        code: 'location-6-code',
        key: 'Location6Key',
      },
      mapping: {
        candidateKey: 'Location6Key',
        publicId: 'location-6-public-id',
        source: {
          label: 'Location 6',
          code: 'location-6-code',
        },
        sourceKey: 'Location6Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 7',
        code: 'location-7-code',
        key: 'Location7Key',
      },
      mapping: {
        candidateKey: 'Location7Key',
        publicId: 'location-7-public-id',
        source: {
          label: 'Location 7',
          code: 'location-7-code',
        },
        sourceKey: 'Location7Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 8',
        code: 'location-8-code',
        key: 'Location8Key',
      },
      mapping: {
        candidateKey: 'Location8Key',
        publicId: 'location-8-public-id',
        source: {
          label: 'Location 8',
          code: 'location-8-code',
        },
        sourceKey: 'Location8Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 9',
        code: 'location-9-code',
        key: 'Location9Key',
      },
      mapping: {
        candidateKey: 'Location9Key',
        publicId: 'location-9-public-id',
        source: {
          label: 'Location 9',
          code: 'location-9-code',
        },
        sourceKey: 'Location9Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 10',
        code: 'location-10-code',
        key: 'Location10Key',
      },
      mapping: {
        candidateKey: 'Location10Key',
        publicId: 'location-10-public-id',
        source: {
          label: 'Location 10',
          code: 'location-10-code',
        },
        sourceKey: 'Location10Key',
        type: 'AutoMapped',
      },
    },
    {
      candidate: {
        label: 'Location 11',
        code: 'location-11-code',
        key: 'Location11Key',
      },
      mapping: {
        candidateKey: 'Location11Key',
        publicId: 'location-11-public-id',
        source: {
          label: 'Location 11',
          code: 'location-11-code',
        },
        sourceKey: 'Location11Key',
        type: 'AutoMapped',
      },
    },
  ];

  test('renders correctly', () => {
    render(
      <ApiDataSetAutoMappedTable
        autoMappedItems={testLocations}
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        itemLabel="location"
        newItems={[]}
        renderCandidate={candidate => candidate.label}
        renderSource={source => source.label}
        searchFilter={() => []}
        onUpdate={Promise.resolve}
      />,
    );

    // Search
    expect(
      screen.getByLabelText(/Search auto mapped options/),
    ).toBeInTheDocument();

    // Table
    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(11);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Location 1');
    expect(row1Cells[1]).toHaveTextContent('Location 1');
    expect(row1Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'Map option for Location 1',
      }),
    ).toBeInTheDocument();

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Location 2');
    expect(row2Cells[1]).toHaveTextContent('Location 2');
    expect(row2Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'Map option for Location 2',
      }),
    ).toBeInTheDocument();

    // Row 3
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Location 3');
    expect(row3Cells[1]).toHaveTextContent('Location 3');
    expect(row3Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'Map option for Location 3',
      }),
    ).toBeInTheDocument();

    // Row 4
    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('Location 4');
    expect(row4Cells[1]).toHaveTextContent('Location 4');
    expect(row4Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Map option for Location 4',
      }),
    ).toBeInTheDocument();

    // Row 5
    const row5Cells = within(rows[5]).getAllByRole('cell');
    expect(row5Cells[0]).toHaveTextContent('Location 5');
    expect(row5Cells[1]).toHaveTextContent('Location 5');
    expect(row5Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row5Cells[3]).getByRole('button', {
        name: 'Map option for Location 5',
      }),
    ).toBeInTheDocument();

    // Row 6
    const row6Cells = within(rows[6]).getAllByRole('cell');
    expect(row6Cells[0]).toHaveTextContent('Location 6');
    expect(row6Cells[1]).toHaveTextContent('Location 6');
    expect(row6Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row6Cells[3]).getByRole('button', {
        name: 'Map option for Location 6',
      }),
    ).toBeInTheDocument();

    // Row 7
    const row7Cells = within(rows[7]).getAllByRole('cell');
    expect(row7Cells[0]).toHaveTextContent('Location 7');
    expect(row7Cells[1]).toHaveTextContent('Location 7');
    expect(row7Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row7Cells[3]).getByRole('button', {
        name: 'Map option for Location 7',
      }),
    ).toBeInTheDocument();

    // Row 8
    const row8Cells = within(rows[8]).getAllByRole('cell');
    expect(row8Cells[0]).toHaveTextContent('Location 8');
    expect(row8Cells[1]).toHaveTextContent('Location 8');
    expect(row8Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row8Cells[3]).getByRole('button', {
        name: 'Map option for Location 8',
      }),
    ).toBeInTheDocument();

    // Row 9
    const row9Cells = within(rows[9]).getAllByRole('cell');
    expect(row9Cells[0]).toHaveTextContent('Location 9');
    expect(row9Cells[1]).toHaveTextContent('Location 9');
    expect(row9Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row9Cells[3]).getByRole('button', {
        name: 'Map option for Location 9',
      }),
    ).toBeInTheDocument();

    // Row 10
    const row10Cells = within(rows[10]).getAllByRole('cell');
    expect(row10Cells[0]).toHaveTextContent('Location 10');
    expect(row10Cells[1]).toHaveTextContent('Location 10');
    expect(row10Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row10Cells[3]).getByRole('button', {
        name: 'Map option for Location 10',
      }),
    ).toBeInTheDocument();

    // Pagination
    const pagination = within(
      screen.getByRole('navigation', { name: 'Pagination' }),
    );
    expect(
      pagination.getByRole('button', { name: 'Page 1' }),
    ).toBeInTheDocument();
    expect(
      pagination.getByRole('button', { name: 'Page 2' }),
    ).toBeInTheDocument();

    expect(
      pagination.getByRole('button', { name: 'Next page' }),
    ).toBeInTheDocument();
  });

  test('searching', async () => {
    const { user } = render(
      <ApiDataSetAutoMappedTable
        autoMappedItems={testLocations}
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        itemLabel="location"
        newItems={[]}
        renderCandidate={candidate => candidate.label}
        renderSource={source => source.label}
        searchFilter={() => [
          {
            candidate: {
              label: 'Location 3',
              code: 'location-3-code',
              key: 'Location3Key',
            },
            mapping: {
              candidateKey: 'Location3Key',
              publicId: 'location-3-public-id',
              source: {
                label: 'Location 3',
                code: 'location-3-code',
              },
              sourceKey: 'Location3Key',
              type: 'AutoMapped',
            },
          },
        ]}
        onUpdate={Promise.resolve}
      />,
    );

    expect(within(screen.getByRole('table')).getAllByRole('row')).toHaveLength(
      11,
    );

    expect(
      screen.getByRole('navigation', { name: 'Pagination' }),
    ).toBeInTheDocument();

    await user.type(
      screen.getByLabelText(/Search auto mapped options/),
      'location 3',
    );

    await waitFor(() =>
      expect(screen.queryByText('Location 1')).not.toBeInTheDocument(),
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(2);
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Location 3');
    expect(row1Cells[1]).toHaveTextContent('Location 3');
    expect(row1Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'Map option for Location 3',
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('navigation', { name: 'Pagination' }),
    ).not.toBeInTheDocument();
  });

  test('pagination', async () => {
    const { user } = render(
      <ApiDataSetAutoMappedTable
        autoMappedItems={testLocations}
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        itemLabel="location"
        newItems={[]}
        renderCandidate={candidate => candidate.label}
        renderSource={source => source.label}
        searchFilter={() => []}
        onUpdate={Promise.resolve}
      />,
    );

    expect(within(screen.getByRole('table')).getAllByRole('row')).toHaveLength(
      11,
    );

    expect(
      screen.getByRole('navigation', { name: 'Pagination' }),
    ).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Page 2' }));

    await waitFor(() =>
      expect(screen.queryByText('Location 1')).not.toBeInTheDocument(),
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(2);
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Location 11');
    expect(row1Cells[1]).toHaveTextContent('Location 11');
    expect(row1Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'Map option for Location 11',
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Previous page' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Next page' }),
    ).not.toBeInTheDocument();
  });

  test('hides the edit button if there is a pending update for the mapping', () => {
    render(
      <ApiDataSetAutoMappedTable
        autoMappedItems={testLocations}
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        itemLabel="location"
        newItems={[]}
        pendingUpdates={[
          {
            previousMapping: testLocations[1].mapping,
            groupKey: 'localAuthority',
            sourceKey: 'Location2Key',
            type: 'ManualMapped',
          },
        ]}
        renderCandidate={candidate => candidate.label}
        renderSource={source => source.label}
        searchFilter={() => []}
        onUpdate={Promise.resolve}
      />,
    );
    const rows = within(screen.getByRole('table')).getAllByRole('row');

    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(
      within(row2Cells[3]).queryByRole('button', {
        name: 'Map option for Location 2',
      }),
    ).not.toBeInTheDocument();
  });
});
