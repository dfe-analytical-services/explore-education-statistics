import ChartCustomDataGroupingsConfiguration from '@admin/pages/release/datablocks/components/chart/ChartCustomDataGroupingsConfiguration';
import FormProvider from '@common/components/form/rhf/FormProvider';
import baseRender from '@common-test/render';
import { screen, waitFor, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React, { ReactNode } from 'react';

describe('ChartCustomDataGroupingsConfiguration', () => {
  const testGroups = [
    { min: 0, max: 50 },
    { min: 51, max: 100 },
  ];

  test('renders correctly without initial values', () => {
    render(
      <ChartCustomDataGroupingsConfiguration
        groups={[]}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

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
      <ChartCustomDataGroupingsConfiguration
        groups={testGroups}
        id="testId"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

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
      <ChartCustomDataGroupingsConfiguration
        groups={[]}
        id="testId"
        unit="%"
        onAddGroup={noop}
        onRemoveGroup={noop}
      />,
    );

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
    const { user } = render(
      <ChartCustomDataGroupingsConfiguration
        groups={[]}
        id="testId"
        onAddGroup={handleAddGroup}
        onRemoveGroup={noop}
      />,
    );

    await user.type(screen.getByLabelText('Min'), '0');
    await user.type(screen.getByLabelText('Max'), '10');

    expect(handleAddGroup).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Add group' }));

    await waitFor(() => {
      expect(handleAddGroup).toHaveBeenCalled();
    });

    await user.type(screen.getByLabelText('Min'), '11');
    await user.type(screen.getByLabelText('Max'), '20');

    await user.click(screen.getByRole('button', { name: 'Add group' }));

    await waitFor(() => {
      expect(handleAddGroup).toHaveBeenCalledTimes(2);
    });
  });

  test('calls onRemoveGroup when remove groups', async () => {
    const handleRemoveGroup = jest.fn();
    const { user } = render(
      <ChartCustomDataGroupingsConfiguration
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

    await user.click(row2.getByRole('button', { name: 'Remove group' }));

    await waitFor(() => {
      expect(handleRemoveGroup).toHaveBeenCalledWith({ min: 51, max: 100 });
    });

    const row1 = within(table.getAllByRole('row')[1]);
    expect(row1.getByText('0')).toBeInTheDocument();
    expect(row1.getByText('50')).toBeInTheDocument();
    await user.click(row1.getByRole('button', { name: 'Remove group' }));

    await waitFor(() => {
      expect(handleRemoveGroup).toHaveBeenCalledTimes(2);
      expect(handleRemoveGroup).toHaveBeenCalledWith({ min: 0, max: 50 });
    });
  });

  function render(children: ReactNode) {
    return baseRender(<FormProvider>{children}</FormProvider>);
  }
});
