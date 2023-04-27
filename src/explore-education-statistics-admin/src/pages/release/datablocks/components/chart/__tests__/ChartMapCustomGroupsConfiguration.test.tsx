import ChartMapCustomGroupsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartMapCustomGroupsConfiguration';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ChartMapCustomGroupsConfiguration', () => {
  const testGroups = [
    { min: 0, max: 50 },
    { min: 51, max: 100 },
  ];
  test('renders correctly without initial values', () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={[]}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    expect(screen.getByText('Custom groups')).toBeInTheDocument();
    const table = within(screen.getByRole('table'));
    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(2);
    expect(within(rows[1]).getByLabelText('Min')).toBeInTheDocument();
    expect(within(rows[1]).getByLabelText('Max')).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Add group' }),
    ).toBeInTheDocument();
  });

  test('renders correctly with initial values', () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={testGroups}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    expect(screen.getByText('Custom groups')).toBeInTheDocument();
    const table = within(screen.getByRole('table'));
    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(4);

    expect(within(rows[1]).getByText('0')).toBeInTheDocument();
    expect(within(rows[1]).getByText('50')).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Remove group' }),
    ).toBeInTheDocument();

    expect(within(rows[2]).getByText('51')).toBeInTheDocument();
    expect(within(rows[2]).getByText('100')).toBeInTheDocument();
    expect(
      within(rows[2]).getByRole('button', { name: 'Remove group' }),
    ).toBeInTheDocument();

    expect(within(rows[3]).getByLabelText('Min')).toBeInTheDocument();
    expect(within(rows[3]).getByLabelText('Max')).toBeInTheDocument();
    expect(
      within(rows[3]).getByRole('button', { name: 'Add group' }),
    ).toBeInTheDocument();
  });

  test('shows the unit if available', () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={[]}
        id="testId"
        unit="%"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    expect(screen.getByText('Custom groups')).toBeInTheDocument();
    const table = within(screen.getByRole('table'));
    const rows = table.getAllByRole('row');
    expect(rows).toHaveLength(2);
    expect(within(rows[1]).getByLabelText('Min (%)')).toBeInTheDocument();
    expect(within(rows[1]).getByLabelText('Max (%)')).toBeInTheDocument();
    expect(
      within(rows[1]).getByRole('button', { name: 'Add group' }),
    ).toBeInTheDocument();
  });

  test('calls onAddGroup when add groups', async () => {
    const handleAddGroup = jest.fn();
    render(
      <ChartMapCustomGroupsConfiguration
        groups={[]}
        id="testId"
        onAddGroup={handleAddGroup}
        onRemoveGroup={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Min'), '0');
    userEvent.type(screen.getByLabelText('Max'), '10');

    expect(handleAddGroup).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Add group' }));

    await waitFor(() => {
      expect(handleAddGroup).toHaveBeenCalledWith({ min: 0, max: 10 });
    });

    userEvent.type(screen.getByLabelText('Min'), '11');
    userEvent.type(screen.getByLabelText('Max'), '20');

    userEvent.click(screen.getByRole('button', { name: 'Add group' }));

    await waitFor(() => {
      expect(handleAddGroup).toHaveBeenCalledTimes(2);
      expect(handleAddGroup).toHaveBeenCalledWith({ min: 11, max: 20 });
    });
  });

  test('calls onRemoveGroup when remove groups', async () => {
    const handleRemoveGroup = jest.fn();
    render(
      <ChartMapCustomGroupsConfiguration
        groups={testGroups}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={handleRemoveGroup}
      />,
    );

    const table = within(screen.getByRole('table'));

    const row2 = within(table.getAllByRole('row')[2]);
    expect(row2.getByText('51')).toBeInTheDocument();
    expect(row2.getByText('100')).toBeInTheDocument();

    expect(handleRemoveGroup).not.toHaveBeenCalled();

    userEvent.click(row2.getByRole('button', { name: 'Remove group' }));

    await waitFor(() => {
      expect(handleRemoveGroup).toHaveBeenCalledWith({ min: 51, max: 100 });
    });

    const row1 = within(table.getAllByRole('row')[1]);
    expect(row1.getByText('0')).toBeInTheDocument();
    expect(row1.getByText('50')).toBeInTheDocument();
    userEvent.click(row1.getByRole('button', { name: 'Remove group' }));

    await waitFor(() => {
      expect(handleRemoveGroup).toHaveBeenCalledTimes(2);
      expect(handleRemoveGroup).toHaveBeenCalledWith({ min: 0, max: 50 });
    });
  });

  test('shows a validation errors when submit without min and max values', async () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={[]}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    userEvent.click(screen.getByRole('button', { name: 'Add group' }));

    await waitFor(() => {
      expect(screen.getByText('Enter a minimum value')).toBeInTheDocument();
      expect(screen.getByText('Enter a maximum value')).toBeInTheDocument();
    });
  });

  test('shows a validation error when the max is not greater than the min', async () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={[]}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Min'), '10');
    userEvent.type(screen.getByLabelText('Max'), '9');
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('Must be greater than min')).toBeInTheDocument();
    });
  });

  test('shows a validation error when the minimum value is in an existing group', async () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={testGroups}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Min'), '10');
    userEvent.type(screen.getByLabelText('Max'), '200');
    userEvent.tab();

    const minCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[0];
    const maxCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[1];

    await waitFor(() => {
      expect(
        within(minCell).getByText('Min cannot overlap another group'),
      ).toBeInTheDocument();
      expect(
        within(maxCell).queryByText('Groups cannot overlap'),
      ).not.toBeInTheDocument();
    });
  });

  test('shows a validation error when the maximum value is in an existing group', async () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={testGroups}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Min'), '-10');
    userEvent.type(screen.getByLabelText('Max'), '75');
    userEvent.tab();

    const minCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[0];
    const maxCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[1];

    await waitFor(() => {
      expect(
        within(minCell).queryByText('Min cannot overlap another group'),
      ).not.toBeInTheDocument();

      expect(
        within(maxCell).getByText('Max cannot overlap another group'),
      ).toBeInTheDocument();
    });
  });

  test('shows validation errors when both values are in an existing group', async () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={testGroups}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Min'), '10');
    userEvent.type(screen.getByLabelText('Max'), '75');
    userEvent.tab();

    const minCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[0];
    const maxCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[1];

    await waitFor(() => {
      expect(
        within(minCell).getByText('Min cannot overlap another group'),
      ).toBeInTheDocument();
      expect(
        within(maxCell).getByText('Max cannot overlap another group'),
      ).toBeInTheDocument();
    });
  });

  test('shows validation errors when the new group contains an existing group', async () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={testGroups}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Min'), '-10');
    userEvent.type(screen.getByLabelText('Max'), '150');
    userEvent.tab();

    const minCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[0];
    const maxCell = within(screen.getAllByRole('row')[3]).getAllByRole(
      'cell',
    )[1];

    await waitFor(() => {
      expect(
        within(minCell).getByText('Min cannot overlap another group'),
      ).toBeInTheDocument();
      expect(
        within(maxCell).getByText('Max cannot overlap another group'),
      ).toBeInTheDocument();
    });
  });

  test('shows a validation error when groups with negative values overlap', async () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={[{ min: -2, max: 2 }]}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    userEvent.type(screen.getByLabelText('Min'), '-3');

    const minCell = within(screen.getAllByRole('row')[2]).getAllByRole(
      'cell',
    )[0];

    userEvent.type(screen.getByLabelText('Max'), '3');

    userEvent.tab();

    await waitFor(() => {
      expect(
        within(minCell).getByText('Min cannot overlap another group'),
      ).toBeInTheDocument();
    });
  });

  test('shows the minimum number of groups error when showError is true', () => {
    render(
      <ChartMapCustomGroupsConfiguration
        groups={[]}
        id="testId"
        showError
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

    expect(
      screen.getByText('There must be at least 1 data group'),
    ).toBeInTheDocument();
  });
});
