import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersAxis from '@common/modules/table-tool/components/TableHeadersAxis';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/tableHeadersConfig.data';
import { Formik } from 'formik';
import { DragDropContext } from 'react-beautiful-dnd';
import { render as baseRender, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('TableHeadersAxis', () => {
  test('renders draggable groups when groupDraggingEnabled is true', () => {
    render({ groupDraggingEnabled: true });

    expect(
      screen.getByRole('group', { name: 'Test legend' }),
    ).toBeInTheDocument();

    const group1 = screen.getByTestId('draggable-group-category-group');
    // Draggable groups have button role
    expect(group1).toHaveAttribute('role', 'button');
    expect(
      within(group1).getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();
    const group1Items = within(group1).getAllByRole('listitem');
    expect(group1Items).toHaveLength(2);
    expect(within(group1Items[0]).getByText('Category 1'));
    expect(within(group1Items[1]).getByText('Category 2'));

    const group2 = screen.getByTestId('draggable-group-indicators');
    // Draggable groups have button role
    expect(group2).toHaveAttribute('role', 'button');
    expect(
      within(group2).getByRole('heading', { name: 'Indicators' }),
    ).toBeInTheDocument();
    const group2Items = within(group2).getAllByRole('listitem');
    expect(group2Items).toHaveLength(2);
    expect(within(group2Items[0]).getByText('Indicator 1'));
    expect(within(group2Items[1]).getByText('Indicator 2'));
  });

  test('renders undraggable groups when groupDraggingEnabled is false', () => {
    render({ groupDraggingEnabled: false });

    expect(
      screen.getByRole('heading', { name: 'Test legend' }),
    ).toBeInTheDocument();

    const group1 = screen.getByTestId('draggable-group-category-group');
    // Non-draggable groups do not have button role
    expect(group1).not.toHaveAttribute('role', 'button');

    expect(
      within(group1).getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();
    const group1Items = within(group1).getAllByRole('listitem');
    expect(group1Items).toHaveLength(2);
    expect(within(group1Items[0]).getByText('Category 1'));
    expect(within(group1Items[1]).getByText('Category 2'));

    const group2 = screen.getByTestId('draggable-group-indicators');
    // Non-draggable groups do not have button role
    expect(group2).not.toHaveAttribute('role', 'button');
    expect(
      within(group2).getByRole('heading', { name: 'Indicators' }),
    ).toBeInTheDocument();
    const group2Items = within(group2).getAllByRole('listitem');
    expect(group2Items).toHaveLength(2);
    expect(within(group2Items[0]).getByText('Indicator 1'));
    expect(within(group2Items[1]).getByText('Indicator 2'));
  });
});

function render({ groupDraggingEnabled }: { groupDraggingEnabled: boolean }) {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider groupDraggingEnabled={groupDraggingEnabled}>
      <Formik onSubmit={noop} initialValues={testInitialFormValues}>
        <DragDropContext onDragEnd={noop}>
          <TableHeadersAxis
            id="test-id"
            legend="Test legend"
            name="rowGroups"
            onMoveGroupToOtherAxis={noop}
          />
        </DragDropContext>
      </Formik>
    </TableHeadersContextProvider>,
  );
}
