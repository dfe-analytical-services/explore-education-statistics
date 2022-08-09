import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
  testTableHeadersConfig,
} from '@common/modules/table-tool/components/__tests__/__data__/TableHeadersConfig.data';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('TableHeadersForm', () => {
  test('renders the form correctly', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    expect(
      screen.getByText('Move and reorder table headers'),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('group', { name: 'Move column headers' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('group', { name: 'Move row headers' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Update and view reordered table' }),
    ).toBeInTheDocument();
  });

  test('renders the column table headers correctly', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    const columnGroup1 = within(screen.getByTestId('columnGroups-0'));
    // the draggable element
    expect(
      columnGroup1.getByRole('button', {
        name: 'Locations Location 1 Location 2',
      }),
    ).toBeInTheDocument();
    expect(
      columnGroup1.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();
    const columnGroup1Items = columnGroup1.getAllByRole('listitem');
    expect(columnGroup1Items).toHaveLength(2);
    expect(
      within(columnGroup1Items[0]).getByText('Location 1'),
    ).toBeInTheDocument();
    expect(
      within(columnGroup1Items[1]).getByText('Location 2'),
    ).toBeInTheDocument();
    expect(
      columnGroup1.getByRole('button', {
        name: 'Show 3 more items in Locations',
      }),
    ).toBeInTheDocument();
    expect(
      columnGroup1.getByRole('button', {
        name: 'Reorder items in Locations',
      }),
    ).toBeInTheDocument();
    expect(
      columnGroup1.getByRole('button', {
        name: 'Move Locations to rows',
      }),
    ).toBeInTheDocument();

    const columnGroup2 = within(screen.getByTestId('columnGroups-1'));
    // the draggable element
    expect(
      columnGroup2.getByRole('button', {
        name: 'Category group Category 1 Category 2',
      }),
    ).toBeInTheDocument();
    expect(
      columnGroup2.getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();
    const columnGroup2Items = columnGroup2.getAllByRole('listitem');
    expect(columnGroup2Items).toHaveLength(2);
    expect(
      within(columnGroup2Items[0]).getByText('Category 1'),
    ).toBeInTheDocument();
    expect(
      within(columnGroup2Items[1]).getByText('Category 2'),
    ).toBeInTheDocument();
    expect(
      columnGroup2.getByRole('button', {
        name: 'Reorder items in Category group',
      }),
    ).toBeInTheDocument();
    expect(
      columnGroup2.getByRole('button', {
        name: 'Move Category group to rows',
      }),
    ).toBeInTheDocument();

    const columnGroup3 = within(screen.getByTestId('columnGroups-2'));
    // the draggable element
    expect(
      columnGroup3.getByRole('button', {
        name: 'Time periods Time period 1 Time period 2',
      }),
    ).toBeInTheDocument();
    expect(
      columnGroup3.getByRole('heading', { name: 'Time periods' }),
    ).toBeInTheDocument();
    const columnGroup3Items = columnGroup3.getAllByRole('listitem');
    expect(columnGroup3Items).toHaveLength(2);
    expect(
      within(columnGroup3Items[0]).getByText('Time period 1'),
    ).toBeInTheDocument();
    expect(
      within(columnGroup3Items[1]).getByText('Time period 2'),
    ).toBeInTheDocument();
    expect(
      columnGroup3.getByRole('button', {
        name: 'Reorder items in Time periods',
      }),
    ).toBeInTheDocument();
    expect(
      columnGroup3.getByRole('button', {
        name: 'Move Time periods to rows',
      }),
    ).toBeInTheDocument();
  });

  test('renders the row table headers correctly', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    const rowGroup1 = within(screen.getByTestId('rowGroups-0'));
    // the draggable element
    expect(
      rowGroup1.getByRole('button', {
        name: 'Indicators Indicator 1 Indicator 2',
      }),
    ).toBeInTheDocument();
    expect(
      rowGroup1.getByRole('heading', { name: 'Indicators' }),
    ).toBeInTheDocument();
    const rowGroup1Items = rowGroup1.getAllByRole('listitem');
    expect(rowGroup1Items).toHaveLength(2);
    expect(within(rowGroup1Items[0]).getByText('Indicator 1'));
    expect(within(rowGroup1Items[1]).getByText('Indicator 2'));

    expect(
      rowGroup1.getByRole('button', { name: 'Show 1 more item in Indicators' }),
    ).toBeInTheDocument();
    expect(
      rowGroup1.getByRole('button', {
        name: 'Reorder items in Indicators',
      }),
    ).toBeInTheDocument();
    expect(
      rowGroup1.getByRole('button', {
        name: 'Move Indicators to columns',
      }),
    ).toBeInTheDocument();
  });

  test('handles moving a group to the other axis', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    const columnAxis = within(
      screen.getByRole('group', { name: 'Move column headers' }),
    );
    expect(
      columnAxis.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();

    userEvent.click(
      screen.getByRole('button', { name: 'Move Locations to rows' }),
    );

    const rowAxis = within(
      screen.getByRole('group', { name: 'Move row headers' }),
    );

    expect(
      rowAxis.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();
    expect(
      columnAxis.queryByRole('heading', { name: 'Locations' }),
    ).not.toBeInTheDocument();
  });

  test('handles submitting the form with no reordering', async () => {
    const handleSubmit = jest.fn();
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Update and view reordered table' }),
    );
    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(testTableHeadersConfig);
    });
  });

  test('handles submitting the form with reordering', async () => {
    const handleSubmit = jest.fn();
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={handleSubmit}
      />,
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Move Locations to rows' }),
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Update and view reordered table' }),
    );

    const updatedTableHeadersConfig: TableHeadersConfig = {
      columnGroups: [testCategoryFilters],
      columns: testTimePeriodFilters,
      rowGroups: [testIndicators],
      rows: testLocationFilters,
    };

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith(updatedTableHeadersConfig);
    });
  });

  test('toggling a group between readonly and reorderable mode', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    // readonly
    expect(
      screen.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: 'Locations' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Done' }),
    ).not.toBeInTheDocument();

    userEvent.click(
      screen.getByRole('button', { name: 'Reorder items in Locations' }),
    );

    // reorderable
    expect(screen.getByText('Done')).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Locations' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Reorder items in Locations' }),
    ).not.toBeInTheDocument();

    const reorderableLocations = screen.getByRole('group', {
      name: 'Locations',
    });
    const items = within(reorderableLocations).getAllByRole('button');
    expect(items).toHaveLength(5);
    expect(within(items[0]).getByText('Location 1')).toBeInTheDocument();
    expect(within(items[1]).getByText('Location 2')).toBeInTheDocument();
    expect(within(items[2]).getByText('Location 3')).toBeInTheDocument();
    expect(within(items[3]).getByText('Location 4')).toBeInTheDocument();
    expect(within(items[4]).getByText('Location 5')).toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Done' }));

    // readonly
    expect(screen.queryByText('Done')).not.toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('group', { name: 'Locations' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Done' }),
    ).not.toBeInTheDocument();
  });

  test('when one group is reorderable the others cannot be moved or reordered', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Reorder items in Locations' }),
    );

    const columnGroup2 = within(screen.getByTestId('columnGroups-1'));
    expect(
      columnGroup2.queryByRole('button', {
        name: 'Category group Category 1 Category 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      columnGroup2.getByRole('button', {
        name: 'Reorder items in Category group',
      }),
    ).toBeDisabled();
    expect(
      columnGroup2.getByRole('button', {
        name: 'Move Category group to rows',
      }),
    ).toBeDisabled();

    const columnGroup3 = within(screen.getByTestId('columnGroups-2'));
    expect(
      columnGroup3.queryByRole('button', {
        name: 'Time periods Time period 1 Time period 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      columnGroup3.getByRole('button', {
        name: 'Reorder items in Time periods',
      }),
    ).toBeDisabled();
    expect(
      columnGroup3.getByRole('button', {
        name: 'Move Time periods to rows',
      }),
    ).toBeDisabled();

    const rowGroup1 = within(screen.getByTestId('rowGroups-0'));
    expect(
      rowGroup1.queryByRole('button', {
        name: 'Indicators Indicators 1 Indicators 2',
      }),
    ).not.toBeInTheDocument();
    expect(
      rowGroup1.getByRole('button', {
        name: 'Reorder items in Indicators',
      }),
    ).toBeDisabled();
    expect(
      rowGroup1.getByRole('button', {
        name: 'Move Indicators to columns',
      }),
    ).toBeDisabled();
  });

  test('toggling showing more and fewer items in a group', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    const columnGroup1 = within(screen.getByTestId('columnGroups-0'));
    const columnGroup1Items = columnGroup1.getAllByRole('listitem');
    expect(columnGroup1Items).toHaveLength(2);
    expect(
      within(columnGroup1Items[0]).getByText('Location 1'),
    ).toBeInTheDocument();
    expect(
      within(columnGroup1Items[1]).getByText('Location 2'),
    ).toBeInTheDocument();

    userEvent.click(
      columnGroup1.getByRole('button', {
        name: 'Show 3 more items in Locations',
      }),
    );

    const expandedColumnGroup1Items = columnGroup1.getAllByRole('listitem');
    expect(expandedColumnGroup1Items).toHaveLength(5);
    expect(
      within(expandedColumnGroup1Items[0]).getByText('Location 1'),
    ).toBeInTheDocument();
    expect(
      within(expandedColumnGroup1Items[1]).getByText('Location 2'),
    ).toBeInTheDocument();
    expect(
      within(expandedColumnGroup1Items[2]).getByText('Location 3'),
    ).toBeInTheDocument();
    expect(
      within(expandedColumnGroup1Items[3]).getByText('Location 4'),
    ).toBeInTheDocument();
    expect(
      within(expandedColumnGroup1Items[4]).getByText('Location 5'),
    ).toBeInTheDocument();
    expect(
      columnGroup1.queryByRole('button', {
        name: 'Show 3 more items in Locations',
      }),
    ).not.toBeInTheDocument();

    userEvent.click(
      columnGroup1.getByRole('button', { name: 'Show fewer Locations items' }),
    );

    expect(columnGroup1.getAllByRole('listitem')).toHaveLength(2);
    expect(
      columnGroup1.queryByRole('button', {
        name: 'Show fewer Locations items',
      }),
    ).not.toBeInTheDocument();
    expect(
      columnGroup1.getByRole('button', {
        name: 'Show 3 more items in Locations',
      }),
    ).toBeInTheDocument();
  });
});
