import ApiDataSetNewLocationsTable from '@admin/pages/release/data/components/ApiDataSetNewLocationsTable';
import { NewLocationCandidate } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetNewLocationsTable', () => {
  const testLocations: NewLocationCandidate[] = [
    {
      candidate: {
        label: 'Location 1',
        code: 'location-1-code',
      },
    },
    {
      candidate: {
        label: 'Location 2',
        code: 'location-2-code',
      },
    },
  ];

  test('renders the table correctly', () => {
    render(
      <ApiDataSetNewLocationsTable
        level="localAuthority"
        locations={testLocations}
      />,
    );
    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(3);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent(
      'Not applicable to current data set',
    );
    expect(row1Cells[1]).toHaveTextContent('Location 1');

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent(
      'Not applicable to current data set',
    );
    expect(row2Cells[1]).toHaveTextContent('Location 2');
    expect(row2Cells[1]).toHaveTextContent('location-2-code');
  });
});
