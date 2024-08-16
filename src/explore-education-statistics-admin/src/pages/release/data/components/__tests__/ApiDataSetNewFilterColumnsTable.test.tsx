import ApiDataSetNewFilterColumnsTable from '@admin/pages/release/data/components/ApiDataSetNewFilterColumnsTable';
import { FilterCandidate } from '@admin/services/apiDataSetVersionService';
import { Dictionary } from '@common/types';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetNewFilterColumnsTable', () => {
  const testFilterColumns: Dictionary<FilterCandidate> = {
    Filter1Key: {
      label: 'Filter 1',
      options: {
        Filter1Option1Key: {
          label: 'Filter 1 Option 1',
        },
        Filter1Option2Key: {
          label: 'Filter 1 Option 2',
        },
        Filter1Option3Key: {
          label: 'Filter 1 Option 3',
        },
      },
    },
    Filter2Key: {
      label: 'Filter 2',
      options: {
        Filter2Option1Key: {
          label: 'Filter 2 Option 1',
        },
        Filter2Option2Key: {
          label: 'Filter 2 Option 2',
        },
      },
    },
  };

  test('renders correctly', () => {
    render(
      <ApiDataSetNewFilterColumnsTable newFilterColumns={testFilterColumns} />,
    );
    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(3);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('No mapping available');
    expect(row1Cells[1]).toHaveTextContent('Filter 1 id: Filter1Key');
    expect(
      within(row1Cells[1]).getByRole('button', {
        name: 'View filter options',
      }),
    ).toBeInTheDocument();
    expect(row1Cells[2]).toHaveTextContent('Minor');

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('No mapping available');
    expect(row2Cells[1]).toHaveTextContent('Filter 2 id: Filter2Key');
    expect(
      within(row2Cells[1]).getByRole('button', {
        name: 'View filter options',
      }),
    ).toBeInTheDocument();
    expect(row2Cells[2]).toHaveTextContent('Minor');
  });
});
