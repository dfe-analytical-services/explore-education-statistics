import ApiDataSetNewLocationsTable from '@admin/pages/release/data/components/ApiDataSetNewLocationsTable';
import { LocationCandidateWithKey } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetNewLocationsTable', () => {
  const testLocations: LocationCandidateWithKey[] = [
    {
      key: 'Location1Key',
      label: 'Location 1',
      code: 'location-1-code',
    },
    {
      key: 'Location2Key',
      label: 'Location 2',
      code: 'location-2-code',
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
    expect(row1Cells[0]).toHaveTextContent('Not applicable');
    expect(row1Cells[1]).toHaveTextContent('Location 1');
    expect(row1Cells[2]).toHaveTextContent('Minor');

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Not applicable');
    expect(row2Cells[1]).toHaveTextContent('Location 2');
    expect(row2Cells[1]).toHaveTextContent('location-2-code');
    expect(row2Cells[2]).toHaveTextContent('Minor');
  });
});
