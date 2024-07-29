import ApiDataSetUnmappedAndManuallyMappedLocationsTable from '@admin/pages/release/data/components/ApiDataSetUnmappedAndManuallyMappedLocationsTable';
import { UnmappedAndManuallyMappedLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetUnmappedAndManuallyMappedLocationsTable', () => {
  const testLocations: UnmappedAndManuallyMappedLocation[] = [
    {
      mapping: {
        type: 'AutoNone',
        source: {
          label: 'Location 1',
          code: 'location-1-code',
        },
      },
    },
    {
      candidate: {
        label: 'Location 2 updated',
        code: 'location-2-code',
      },
      mapping: {
        candidateKey: 'Location2Key',
        type: 'ManualMapped',
        source: {
          label: 'Location 2',
          code: 'location-2-code',
        },
      },
    },
    {
      mapping: {
        type: 'ManualNone',
        source: {
          label: 'Location 3',
          code: 'location-3-code',
        },
      },
    },
  ];

  test('renders the table correctly', () => {
    render(
      <ApiDataSetUnmappedAndManuallyMappedLocationsTable
        level="localAuthority"
        locations={testLocations}
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(4);

    // Row 1 - AutoNone
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Location 1');
    expect(row1Cells[0]).toHaveTextContent('location-1-code');
    expect(row1Cells[1]).toHaveTextContent('Unmapped');
    expect(
      within(row1Cells[2]).getByRole('button', {
        name: 'No mapping for Location 1',
      }),
    );
    expect(
      within(row1Cells[2]).getByRole('button', {
        name: 'Edit mapping for Location 1',
      }),
    ).toBeInTheDocument();
    expect(row1Cells[3]).toHaveTextContent('N/A');

    // Row 2 - ManualMapped
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Location 2');
    expect(row2Cells[0]).toHaveTextContent('location-2-code');
    expect(row2Cells[1]).toHaveTextContent('Location 2 updated');
    expect(row2Cells[1]).toHaveTextContent('location-2-code');
    expect(
      within(row2Cells[2]).getByRole('button', {
        name: 'No mapping for Location 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[2]).getByRole('button', {
        name: 'Edit mapping for Location 2',
      }),
    ).toBeInTheDocument();
    expect(row2Cells[3]).toHaveTextContent('Minor');

    // Row 3 - ManualNone
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Location 3');
    expect(row3Cells[0]).toHaveTextContent('location-3-code');
    expect(row3Cells[1]).toHaveTextContent('No mapping available');
    expect(
      within(row3Cells[2]).queryByRole('button', {
        name: 'No mapping for Location 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row3Cells[2]).getByRole('button', {
        name: 'Edit mapping for Location 3',
      }),
    ).toBeInTheDocument();
    expect(row3Cells[3]).toHaveTextContent('Major');
  });

  test('shows the number of mapped and unmapped locations in the table caption', () => {
    render(
      <ApiDataSetUnmappedAndManuallyMappedLocationsTable
        level="localAuthority"
        locations={testLocations}
      />,
    );

    expect(
      screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 2 mapped locations',
      }),
    ).toBeInTheDocument();
  });
});
