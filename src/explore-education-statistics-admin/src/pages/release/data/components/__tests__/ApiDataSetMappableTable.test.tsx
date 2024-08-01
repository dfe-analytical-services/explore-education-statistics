import ApiDataSetMappableTable from '@admin/pages/release/data/components/ApiDataSetMappableTable';
import { MappableLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MappableFilter } from '../../utils/getApiDataSetFilterMappings';

describe('ApiDataSetMappableTable', () => {
  const testLocations: MappableLocation[] = [
    {
      mapping: {
        publicId: 'location-1-public-id',
        source: {
          label: 'Location 1',
          code: 'location-1-code',
        },
        sourceKey: 'Location1SourceKey',
        type: 'AutoNone',
      },
    },
    {
      candidate: {
        label: 'Location 2 updated',
        code: 'location-2-code',
        key: 'Location2CandidateKey',
      },
      mapping: {
        publicId: 'location-2-public-id',
        candidateKey: 'Location2CandidateKey',
        source: {
          label: 'Location 2',
          code: 'location-2-code',
        },
        sourceKey: 'Location2SourceKey',
        type: 'ManualMapped',
      },
    },
    {
      candidate: {
        label: 'Location 3',
        code: 'location-3-updated-code',
        key: 'Location3CandidateKey',
      },
      mapping: {
        publicId: 'location-3-public-id',
        candidateKey: 'Location3CandidateKey',
        source: {
          label: 'Location 3',
          code: 'location-3-code',
        },
        sourceKey: 'Location3SourceKey',
        type: 'ManualMapped',
      },
    },
    {
      mapping: {
        publicId: 'location-4-public-id',
        source: {
          label: 'Location 4',
          code: 'location-4-code',
        },
        sourceKey: 'Location4SourceKey',
        type: 'ManualNone',
      },
    },
  ];

  const testFilterOptions: MappableFilter[] = [
    {
      mapping: {
        publicId: 'filter-option-1-public-id',
        source: {
          label: 'Filter Option 1',
        },
        sourceKey: 'FilterOption1SourceKey',
        type: 'AutoNone',
      },
    },
    {
      candidate: {
        label: 'Filter Option 2 updated',
        key: 'FilterOption2CandidateKey',
      },
      mapping: {
        publicId: 'filter-option-2-public-id',
        candidateKey: 'FilterOption2CandidateKey',
        source: {
          label: 'Filter Option 2',
        },
        sourceKey: 'FilterOption2SourceKey',
        type: 'ManualMapped',
      },
    },
    {
      candidate: {
        label: 'Filter Option 3',
        key: 'FilterOption3CandidateKey',
      },
      mapping: {
        publicId: 'filter-option-3-public-id',
        candidateKey: 'FilterOption3CandidateKey',
        source: {
          label: 'Filter Option 3',
        },
        sourceKey: 'FilterOption3SourceKey',
        type: 'ManualMapped',
      },
    },
    {
      mapping: {
        publicId: 'filter-option-4-public-id',
        source: {
          label: 'Filter Option 4',
        },
        sourceKey: 'FilterOption4SourceKey',
        type: 'ManualNone',
      },
    },
  ];

  test('renders correctly with locations', () => {
    render(
      <ApiDataSetMappableTable
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        label="location"
        mappableItems={testLocations}
        newItems={[]}
        onUpdate={Promise.resolve}
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(5);

    // Row 1 - AutoNone
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Location 1');
    expect(row1Cells[0]).toHaveTextContent('location-1-code');
    expect(row1Cells[1]).toHaveTextContent('Unmapped');
    expect(row1Cells[2]).toHaveTextContent('N/A');
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'No mapping for Location 1',
      }),
    );
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 1',
      }),
    ).toBeInTheDocument();

    // Row 2 - ManualMapped - Minor
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Location 2');
    expect(row2Cells[0]).toHaveTextContent('location-2-code');
    expect(row2Cells[1]).toHaveTextContent('Location 2 updated');
    expect(row2Cells[1]).toHaveTextContent('location-2-code');
    expect(row2Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'No mapping for Location 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 2',
      }),
    ).toBeInTheDocument();

    // Row 3 - ManualMapped - Major
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Location 3');
    expect(row3Cells[0]).toHaveTextContent('location-3-code');
    expect(row3Cells[1]).toHaveTextContent('Location 3');
    expect(row3Cells[1]).toHaveTextContent('location-3-updated-code');
    expect(row3Cells[2]).toHaveTextContent('Major');
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'No mapping for Location 3',
      }),
    ).toBeInTheDocument();
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 3',
      }),
    ).toBeInTheDocument();

    // Row 3 - ManualNone
    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('Location 4');
    expect(row4Cells[0]).toHaveTextContent('location-4-code');
    expect(row4Cells[1]).toHaveTextContent('No mapping available');
    expect(row4Cells[2]).toHaveTextContent('Major');
    expect(
      within(row4Cells[3]).queryByRole('button', {
        name: 'No mapping for Location 4',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Edit mapping for Location 4',
      }),
    ).toBeInTheDocument();
  });

  test('renders correctly with filter options', () => {
    render(
      <ApiDataSetMappableTable
        groupKey="Filter1Key"
        groupLabel="Filter 1"
        label="filter option"
        mappableItems={testFilterOptions}
        newItems={[]}
        onUpdate={Promise.resolve}
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(5);

    // Row 1 - AutoNone
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Filter Option 1');
    expect(row1Cells[1]).toHaveTextContent('Unmapped');
    expect(row1Cells[2]).toHaveTextContent('N/A');
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'No mapping for Filter Option 1',
      }),
    );
    expect(
      within(row1Cells[3]).getByRole('button', {
        name: 'Edit mapping for Filter Option 1',
      }),
    ).toBeInTheDocument();

    // Row 2 - ManualMapped - Minor
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Filter Option 2');
    expect(row2Cells[1]).toHaveTextContent('Filter Option 2 updated');
    expect(row2Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'No mapping for Filter Option 2',
      }),
    ).toBeInTheDocument();
    expect(
      within(row2Cells[3]).getByRole('button', {
        name: 'Edit mapping for Filter Option 2',
      }),
    ).toBeInTheDocument();

    // Row 3 - ManualMapped - Major
    const row3Cells = within(rows[3]).getAllByRole('cell');
    expect(row3Cells[0]).toHaveTextContent('Filter Option 3');
    expect(row3Cells[1]).toHaveTextContent('Filter Option 3');
    expect(row3Cells[2]).toHaveTextContent('Minor');
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'No mapping for Filter Option 3',
      }),
    ).toBeInTheDocument();
    expect(
      within(row3Cells[3]).getByRole('button', {
        name: 'Edit mapping for Filter Option 3',
      }),
    ).toBeInTheDocument();

    // Row 3 - ManualNone
    const row4Cells = within(rows[4]).getAllByRole('cell');
    expect(row4Cells[0]).toHaveTextContent('Filter Option 4');
    expect(row4Cells[1]).toHaveTextContent('No mapping available');
    expect(row4Cells[2]).toHaveTextContent('Major');
    expect(
      within(row4Cells[3]).queryByRole('button', {
        name: 'No mapping for Filter Option 4',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row4Cells[3]).getByRole('button', {
        name: 'Edit mapping for Filter Option 4',
      }),
    ).toBeInTheDocument();
  });

  test('shows the number of mapped and unmapped items in the table caption', () => {
    render(
      <ApiDataSetMappableTable
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        label="location"
        mappableItems={testLocations}
        newItems={[]}
        onUpdate={Promise.resolve}
      />,
    );

    expect(
      screen.getByRole('table', {
        name: 'Local Authorities 1 unmapped location 3 mapped locations',
      }),
    ).toBeInTheDocument();
  });

  test('hides the buttons if there is a pending update for the mapping', () => {
    render(
      <ApiDataSetMappableTable
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        label="location"
        mappableItems={testLocations}
        newItems={[]}
        pendingUpdates={[
          {
            previousMapping: testLocations[1].mapping,
            groupKey: 'localAuthority',
            sourceKey: 'Location2SourceKey',
            type: 'ManualMapped',
          },
        ]}
        onUpdate={Promise.resolve}
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    const row2Cells = within(rows[2]).getAllByRole('cell');

    expect(
      within(row2Cells[3]).queryByRole('button', {
        name: 'No mapping for Location 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row2Cells[3]).queryByRole('button', {
        name: 'Edit mapping for Location 2',
      }),
    ).not.toBeInTheDocument();
  });
});
