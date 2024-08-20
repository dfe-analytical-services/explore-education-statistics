import ApiDataSetMappableFilterColumnsTable from '@admin/pages/release/data/components/ApiDataSetMappableFilterColumnsTable';
import { FilterMapping } from '@admin/services/apiDataSetVersionService';
import { Dictionary } from '@common/types';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetMappableFilterColumnsTable', () => {
  const testFilterColumns: Dictionary<FilterMapping> = {
    Filter1Key: {
      optionMappings: {
        Filter1Option1Key: {
          publicId: 'filter-1-option-1-public-id',
          source: {
            label: 'Filter 1 Option 1',
          },
          type: 'AutoNone',
        },
        Filter1Option2Key: {
          publicId: 'filter-1-option-2-public-id',
          source: {
            label: 'Filter 1 Option 2',
          },
          type: 'AutoNone',
        },
        Filter1Option3Key: {
          publicId: 'filter-1-option-3-public-id',
          source: {
            label: 'Filter 1 Option 3',
          },
          type: 'AutoNone',
        },
      },
      publicId: 'filter-1-public-id',
      source: {
        label: 'Filter 1',
      },
      type: 'AutoNone',
    },
    Filter2Key: {
      optionMappings: {
        Filter2Option1Key: {
          publicId: 'filter-2-option-1-public-id',
          source: {
            label: 'Filter 2 Option 1',
          },
          type: 'AutoNone',
        },
        Filter2Option2Key: {
          publicId: 'filter-2-option-2-public-id',
          source: {
            label: 'Filter 2 Option 2',
          },
          type: 'AutoNone',
        },
      },
      publicId: 'filter-2-public-id',
      source: {
        label: 'Filter 2',
      },
      type: 'AutoNone',
    },
  };

  test('renders correctly', () => {
    render(
      <ApiDataSetMappableFilterColumnsTable
        mappableFilterColumns={testFilterColumns}
      />,
    );
    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(3);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Filter 1 id: Filter1Key');
    expect(
      within(row1Cells[0]).getByRole('button', {
        name: 'View filter options',
      }),
    ).toBeInTheDocument();
    expect(row1Cells[1]).toHaveTextContent('No mapping available');
    expect(row1Cells[2]).toHaveTextContent('Major');

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Filter 2 id: Filter2Key');
    expect(
      within(row2Cells[0]).getByRole('button', {
        name: 'View filter options',
      }),
    ).toBeInTheDocument();
    expect(row2Cells[1]).toHaveTextContent('No mapping available');
    expect(row2Cells[2]).toHaveTextContent('Major');
  });
});
