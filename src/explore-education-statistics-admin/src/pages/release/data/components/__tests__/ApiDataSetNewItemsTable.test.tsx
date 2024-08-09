import ApiDataSetNewItemsTable from '@admin/pages/release/data/components/ApiDataSetNewItemsTable';
import { LocationCandidateWithKey } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { FilterCandidateWithKey } from '../../utils/getApiDataSetFilterMappings';

describe('ApiDataSetNewItemsTable', () => {
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

  const testFilterOptions: FilterCandidateWithKey[] = [
    {
      key: 'FilterOption1Key',
      label: 'Filter Option 1',
    },
    {
      key: 'FilterOption2Key',
      label: 'Filter Option 2',
    },
  ];

  test('renders correctly with locations', () => {
    render(
      <ApiDataSetNewItemsTable
        groupKey="localAuthority"
        groupLabel="Local Authorities"
        label="location"
        newItems={testLocations}
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

  test('renders correctly with filter options', () => {
    render(
      <ApiDataSetNewItemsTable
        groupKey="Filter1Key"
        groupLabel="Filter 1"
        label="filter option"
        newItems={testFilterOptions}
      />,
    );
    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(3);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Not applicable');
    expect(row1Cells[1]).toHaveTextContent('Filter Option 1');
    expect(row1Cells[2]).toHaveTextContent('Minor');

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Not applicable');
    expect(row2Cells[1]).toHaveTextContent('Filter Option 2');
    expect(row2Cells[2]).toHaveTextContent('Minor');
  });
});
