import ApiDataSetDeletedLocationGroupsTable from '@admin/pages/release/data/components/ApiDataSetDeletedLocationGroupsTable';
import { LocationOptionSource } from '@admin/services/apiDataSetVersionService';
import render from '@common-test/render';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('ApiDataSetDeletedLocationGroupsTable', () => {
  test('renders correctly', () => {
    const testLocationGroups: Partial<
      Record<LocationLevelKey, LocationOptionSource[]>
    > = {
      localAuthorityDistrict: [
        {
          label: 'Location option 1',
          code: 'location-opt-1-code',
        },
      ],
      multiAcademyTrust: [
        {
          label: 'Location option 2',
          code: 'location-opt-2-code',
        },
      ],
    };

    render(
      <ApiDataSetDeletedLocationGroupsTable
        locationGroups={testLocationGroups}
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(3);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Local Authority Districts');
    expect(
      within(row1Cells[0]).getByRole('button', {
        name: 'View group options for Local Authority Districts',
      }),
    ).toBeInTheDocument();
    expect(row1Cells[1]).toHaveTextContent('No mapping available');
    expect(row1Cells[2]).toHaveTextContent('Major');

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Multi Academy Trusts');
    expect(
      within(row2Cells[0]).getByRole('button', {
        name: 'View group options for Multi Academy Trusts',
      }),
    ).toBeInTheDocument();
    expect(row2Cells[1]).toHaveTextContent('No mapping available');
    expect(row2Cells[2]).toHaveTextContent('Major');
  });

  test('renders correctly without option buttons', () => {
    const testLocationGroups: Partial<
      Record<LocationLevelKey, LocationOptionSource[]>
    > = {
      localAuthorityDistrict: [],
      multiAcademyTrust: [],
    };

    render(
      <ApiDataSetDeletedLocationGroupsTable
        locationGroups={testLocationGroups}
      />,
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    expect(rows).toHaveLength(3);

    // Row 1
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells[0]).toHaveTextContent('Local Authority Districts');
    expect(
      within(row1Cells[0]).queryByRole('button', {
        name: /View group options/,
      }),
    ).not.toBeInTheDocument();

    // Row 2
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells[0]).toHaveTextContent('Multi Academy Trusts');
    expect(
      within(row2Cells[0]).queryByRole('button', {
        name: /View group options/,
      }),
    ).not.toBeInTheDocument();
  });

  test('clicking options buttons shows modals with correct options', async () => {
    const testLocationGroups: Partial<
      Record<LocationLevelKey, LocationOptionSource[]>
    > = {
      localAuthorityDistrict: [
        {
          label: 'Location option 1',
          code: 'location-opt-1-code',
        },
        {
          label: 'Location option 2',
          code: 'location-opt-2-code',
        },
        {
          label: 'Location option 3',
          code: 'location-opt-3-code',
        },
      ],
      multiAcademyTrust: [
        {
          label: 'Location option 4',
          code: 'location-opt-4-code',
        },
        {
          label: 'Location option 5',
          code: 'location-opt-5-code',
        },
      ],
    };

    const { user } = render(
      <ApiDataSetDeletedLocationGroupsTable
        locationGroups={testLocationGroups}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'View group options for Local Authority Districts',
      }),
    );

    let modal = within(screen.getByRole('dialog'));

    let optionItems = modal.getAllByRole('listitem');

    expect(optionItems).toHaveLength(3);

    expect(optionItems[0]).toHaveTextContent('Location option 1');
    expect(optionItems[0]).toHaveTextContent('Code: location-opt-1-code');

    expect(optionItems[1]).toHaveTextContent('Location option 2');
    expect(optionItems[1]).toHaveTextContent('Code: location-opt-2-code');

    expect(optionItems[2]).toHaveTextContent('Location option 3');
    expect(optionItems[2]).toHaveTextContent('Code: location-opt-3-code');

    await user.click(screen.getByRole('button', { name: 'Close modal' }));

    await user.click(
      screen.getByRole('button', {
        name: 'View group options for Multi Academy Trusts',
      }),
    );

    modal = within(screen.getByRole('dialog'));

    optionItems = modal.getAllByRole('listitem');

    expect(optionItems).toHaveLength(2);

    expect(optionItems[0]).toHaveTextContent('Location option 4');
    expect(optionItems[0]).toHaveTextContent('Code: location-opt-4-code');

    expect(optionItems[1]).toHaveTextContent('Location option 5');
    expect(optionItems[1]).toHaveTextContent('Code: location-opt-5-code');
  });
});
