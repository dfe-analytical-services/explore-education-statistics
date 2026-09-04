import { AuthContextTestProvider, User } from '@admin/contexts/AuthContext';
import ApiDataSetMappableTable from '@admin/pages/release/data/components/ApiDataSetMappableTable';
import { GlobalPermissions } from '@admin/services/authService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React, { ComponentProps } from 'react';
import { MappableFilterOption } from '../../utils/getApiDataSetFilterMappings';

describe('ApiDataSetMappableTable', () => {
  const testBauUser: User = {
    id: 'user-id-1',
    name: 'BAU user',
    permissions: {
      isBauUser: true,
      canManagePublicApiDataSets: true,
    } as GlobalPermissions,
  };

  const testAnalystUser: User = {
    id: 'user-id-1',
    name: 'Analyst user',
    permissions: {
      isBauUser: false,
      canManagePublicApiDataSets: false,
    } as GlobalPermissions,
  };

  const testFilterOptions: MappableFilterOption[] = [
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

  test('renders correctly', () => {
    renderTable();

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
        name: 'Map filter option for Filter Option 1',
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
        name: 'Map filter option for Filter Option 2',
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
        name: 'Map filter option for Filter Option 3',
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
        name: 'Map filter option for Filter Option 4',
      }),
    ).toBeInTheDocument();
  });

  test('shows the number of mapped and unmapped items in the table caption', () => {
    renderTable();

    expect(
      screen.getByRole('table', {
        name: 'Filter 1 1 unmapped filter option 3 mapped filter options',
      }),
    ).toBeInTheDocument();
  });

  test('hides the buttons if there is a pending update for the mapping', () => {
    renderTable({
      pendingUpdates: [
        {
          previousMapping: testFilterOptions[1].mapping,
          groupKey: 'Filter1Key',
          sourceKey: 'FilterOption2SourceKey',
          type: 'ManualMapped',
        },
      ],
    });

    const rows = within(screen.getByRole('table')).getAllByRole('row');

    const row2Cells = within(rows[2]).getAllByRole('cell');

    expect(
      within(row2Cells[3]).queryByRole('button', {
        name: 'No mapping for Filter Option 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      within(row2Cells[3]).queryByRole('button', {
        name: 'Map filter option for Filter Option 2',
      }),
    ).not.toBeInTheDocument();
  });

  test('does not show the actions column or controls when the user cannot manage public API data sets, even when `readOnly` is false', () => {
    renderTable(
      { readOnly: false },
      {
        user: testAnalystUser,
      },
    );

    const rows = within(screen.getByRole('table')).getAllByRole('row');
    expect(rows).toHaveLength(5);

    expect(
      within(rows[0]).queryByRole('columnheader', { name: 'Actions' }),
    ).not.toBeInTheDocument();

    // Row 1 - AutoNone
    const row1Cells = within(rows[1]).getAllByRole('cell');
    expect(row1Cells).toHaveLength(3);
    expect(
      screen.queryByRole('button', {
        name: 'No mapping for Filter Option 1',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Map filter option for Filter Option 1',
      }),
    ).not.toBeInTheDocument();

    // Row 2 - ManualMapped - Minor
    const row2Cells = within(rows[2]).getAllByRole('cell');
    expect(row2Cells).toHaveLength(3);
    expect(
      screen.queryByRole('button', {
        name: 'No mapping for Filter Option 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', {
        name: 'Map filter option for Filter Option 2',
      }),
    ).not.toBeInTheDocument();
  });

  function renderTable(
    props: Partial<ComponentProps<typeof ApiDataSetMappableTable>> = {},
    options?: { user?: User },
  ) {
    const { user = testBauUser } = options ?? {};

    return render(
      <AuthContextTestProvider user={user}>
        <ApiDataSetMappableTable
          groupKey="Filter1Key"
          tableCaptionText="Filter 1"
          itemLabel="filter option"
          itemPluralLabel="filter options"
          mappableItems={testFilterOptions}
          newItems={[]}
          renderCandidate={candidate => candidate.label}
          renderSource={source => source.label}
          onUpdate={Promise.resolve}
          {...props}
        />
      </AuthContextTestProvider>,
    );
  }
});
