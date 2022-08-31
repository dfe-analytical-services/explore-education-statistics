import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
  testTableHeadersConfig,
} from '@common/modules/table-tool/components/__tests__/__data__/tableHeadersConfig.data';
import { waitFor } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('TableHeadersForm', () => {
  test('shows and hides the form', async () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    // Form hidden intially
    expect(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Move and reorder table headers' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Update and view reordered table' }),
    ).not.toBeInTheDocument();

    // Click button to show the form
    userEvent.click(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
    );
    expect(
      screen.queryByRole('button', { name: 'Move and reorder table headers' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Move and reorder table headers' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Update and view reordered table' }),
    ).toBeInTheDocument();

    // Click button to hide the form
    userEvent.click(
      screen.getByRole('button', { name: 'Update and view reordered table' }),
    );
    await waitFor(() => {
      expect(
        screen.queryByText('Update and view reordered table'),
      ).not.toBeInTheDocument();
    });
    expect(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('heading', { name: 'Move and reorder table headers' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Update and view reordered table' }),
    ).not.toBeInTheDocument();
  });

  test('renders the column table headers correctly', () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
    );

    const columnsAxis = within(
      screen.getByRole('group', {
        name: 'Move column headers',
      }),
    );

    const columnGroup1 = within(columnsAxis.getByTestId('columnGroups-0'));
    const columnGroup1Draggable = within(
      columnGroup1.getByTestId('draggable-group-locations'),
    );
    expect(
      columnGroup1Draggable.getByRole('heading', { name: 'Locations' }),
    ).toBeInTheDocument();
    const columnGroup1Items = columnGroup1Draggable.getAllByRole('listitem');
    expect(columnGroup1Items).toHaveLength(2);
    expect(columnGroup1Items[0]).toHaveTextContent('Location 1');
    expect(columnGroup1Items[1]).toHaveTextContent('Location 2');
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

    const columnGroup2 = within(columnsAxis.getByTestId('columnGroups-1'));
    const columnGroup2Draggable = within(
      columnGroup2.getByTestId('draggable-group-category-group'),
    );
    expect(
      columnGroup2Draggable.getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();
    const columnGroup2Items = columnGroup2Draggable.getAllByRole('listitem');
    expect(columnGroup2Items).toHaveLength(2);
    expect(columnGroup2Items[0]).toHaveTextContent('Category 1');
    expect(columnGroup2Items[1]).toHaveTextContent('Category 2');
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
    const columnGroup3Draggable = within(
      columnGroup3.getByTestId('draggable-group-time-periods'),
    );
    expect(
      columnGroup3Draggable.getByRole('heading', { name: 'Time periods' }),
    ).toBeInTheDocument();
    const columnGroup3Items = columnGroup3Draggable.getAllByRole('listitem');
    expect(columnGroup3Items).toHaveLength(2);
    expect(columnGroup3Items[0]).toHaveTextContent('Time period 1');
    expect(columnGroup3Items[1]).toHaveTextContent('Time period 2');
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
    userEvent.click(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
    );

    const rowsAxis = within(
      screen.getByRole('group', { name: 'Move row headers' }),
    );
    const rowGroup1 = within(rowsAxis.getByTestId('rowGroups-0'));
    const rowGroup1Draggable = within(
      rowGroup1.getByTestId('draggable-group-indicators'),
    );
    expect(
      rowGroup1Draggable.getByRole('heading', { name: 'Indicators' }),
    ).toBeInTheDocument();
    const rowGroup1Items = rowGroup1Draggable.getAllByRole('listitem');
    expect(rowGroup1Items).toHaveLength(2);
    expect(rowGroup1Items[0]).toHaveTextContent('Indicator 1');
    expect(rowGroup1Items[1]).toHaveTextContent('Indicator 2');
    expect(
      rowGroup1.getByRole('button', {
        name: 'Show 1 more item in Indicators',
      }),
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
    userEvent.click(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
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
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
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
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
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
    userEvent.click(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
    );

    // readonly
    expect(screen.getByTestId('draggable-group-locations')).toHaveAttribute(
      'role',
      'button',
    );
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
    expect(
      screen.queryByTestId('draggable-group-locations'),
    ).not.toBeInTheDocument();
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
    expect(items[0]).toHaveTextContent('Location 1');
    expect(items[1]).toHaveTextContent('Location 2');
    expect(items[2]).toHaveTextContent('Location 3');
    expect(items[3]).toHaveTextContent('Location 4');
    expect(items[4]).toHaveTextContent('Location 5');

    userEvent.click(screen.getByRole('button', { name: 'Done' }));

    // readonly
    expect(screen.getByTestId('draggable-group-locations')).toHaveAttribute(
      'role',
      'button',
    );
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
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Reorder items in Locations' }),
    );

    const columnGroup2 = within(screen.getByTestId('columnGroups-1'));
    expect(
      columnGroup2.getByTestId('draggable-group-category-group'),
    ).not.toHaveAttribute('role', 'button');
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
      columnGroup3.getByTestId('draggable-group-time-periods'),
    ).not.toHaveAttribute('role', 'button');
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
      rowGroup1.getByTestId('draggable-group-indicators'),
    ).not.toHaveAttribute('role', 'button');
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

  test('toggling showing more and fewer items in a group', async () => {
    render(
      <TableHeadersForm
        initialValues={testTableHeadersConfig}
        onSubmit={noop}
      />,
    );
    userEvent.click(
      screen.getByRole('button', { name: 'Move and reorder table headers' }),
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

    // Waiting to make sure the button has changed,
    // otherwise it's clicked too quickly and thinks it's a double-click.
    await waitFor(() => {
      expect(screen.getByText('Show fewer')).toBeInTheDocument();
    });

    userEvent.click(
      columnGroup1.getByRole('button', {
        name: 'Show fewer items for Locations',
      }),
    );

    expect(columnGroup1.getAllByRole('listitem')).toHaveLength(2);
    expect(
      columnGroup1.queryByRole('button', {
        name: 'Show fewer items for Locations',
      }),
    ).not.toBeInTheDocument();
    expect(
      columnGroup1.getByRole('button', {
        name: 'Show 3 more items in Locations',
      }),
    ).toBeInTheDocument();
  });
});
