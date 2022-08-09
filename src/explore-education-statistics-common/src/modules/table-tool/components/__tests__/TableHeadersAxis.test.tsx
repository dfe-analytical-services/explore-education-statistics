import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersAxis from '@common/modules/table-tool/components/TableHeadersAxis';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/TableHeadersConfig.data';
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

    const buttons1 = within(screen.getByTestId('rowGroups-0')).getAllByRole(
      'button',
    );
    // Draggable groups have the role button and are the first button in the container.
    const draggableGroup1 = within(buttons1[0]);
    expect(
      draggableGroup1.getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();
    const draggableGroup1Items = draggableGroup1.getAllByRole('listitem');
    expect(draggableGroup1Items).toHaveLength(2);
    expect(within(draggableGroup1Items[0]).getByText('Category 1'));
    expect(within(draggableGroup1Items[1]).getByText('Category 2'));

    const buttons2 = within(screen.getByTestId('rowGroups-1')).getAllByRole(
      'button',
    );
    // Draggable groups have the role button and are the first button in the container.
    const draggableGroup2 = within(buttons2[0]);
    expect(
      draggableGroup2.getByRole('heading', { name: 'Indicators' }),
    ).toBeInTheDocument();
    const draggableGroup2Items = draggableGroup2.getAllByRole('listitem');
    expect(draggableGroup2Items).toHaveLength(2);
    expect(within(draggableGroup2Items[0]).getByText('Indicator 1'));
    expect(within(draggableGroup2Items[1]).getByText('Indicator 2'));
  });

  test('renders undraggable groups when groupDraggingEnabled is false', () => {
    render({ groupDraggingEnabled: false });

    expect(
      screen.getByRole('heading', { name: 'Test legend' }),
    ).toBeInTheDocument();

    const rowGroup1 = within(screen.getByTestId('rowGroups-0'));
    // No draggable button present
    const buttons1 = rowGroup1.getAllByRole('button');
    expect(
      within(buttons1[0]).queryByText('Category group'),
    ).not.toBeInTheDocument();
    expect(
      rowGroup1.getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();
    const rowGroup1Items = rowGroup1.getAllByRole('listitem');
    expect(rowGroup1Items).toHaveLength(2);
    expect(within(rowGroup1Items[0]).getByText('Category 1'));
    expect(within(rowGroup1Items[1]).getByText('Category 2'));

    const rowGroup2 = within(screen.getByTestId('rowGroups-1'));
    // No draggable button present
    const buttons2 = rowGroup2.getAllByRole('button');
    expect(
      within(buttons2[0]).queryByText('Indicators'),
    ).not.toBeInTheDocument();
    expect(
      rowGroup2.getByRole('heading', { name: 'Indicators' }),
    ).toBeInTheDocument();
    const rowGroup2Items = rowGroup2.getAllByRole('listitem');
    expect(rowGroup2Items).toHaveLength(2);
    expect(within(rowGroup2Items[0]).getByText('Indicator 1'));
    expect(within(rowGroup2Items[1]).getByText('Indicator 2'));
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
