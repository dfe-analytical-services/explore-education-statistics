import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersDraggableGroup from '@common/modules/table-tool/components/TableHeadersDraggableGroup';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/TableHeadersConfig.data';
import { Formik } from 'formik';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';
import { render as baseRender, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('TableHeadersDraggableGroup', () => {
  test('renders the draggable group', () => {
    render({ groupDraggingActive: true });

    const buttons = screen.getAllByRole('button');
    // Draggable groups have the role button and are the first button in the container.
    const draggableGroup = within(buttons[0]);
    expect(
      draggableGroup.getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();
    const draggableGroupItems = draggableGroup.getAllByRole('listitem');
    expect(draggableGroupItems).toHaveLength(2);
    expect(within(draggableGroupItems[0]).getByText('Category 1'));
    expect(within(draggableGroupItems[1]).getByText('Category 2'));
  });

  test('renders the group controls when groupDraggingActive is false', () => {
    render({ groupDraggingActive: false });

    expect(
      screen.getByRole('button', { name: 'Reorder items in Category group' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Category group to columns' }),
    ).toBeInTheDocument();
  });

  test('does not render the group controls when groupDraggingActive is true', () => {
    render({ groupDraggingActive: true });

    expect(
      screen.queryByRole('button', { name: 'Reorder items in Category group' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Move Category group to columns' }),
    ).not.toBeInTheDocument();
  });
});

function render({ groupDraggingActive }: { groupDraggingActive: boolean }) {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider groupDraggingActive={groupDraggingActive}>
      <Formik onSubmit={noop} initialValues={testInitialFormValues}>
        <DragDropContext onDragEnd={noop}>
          <Droppable droppableId="test">
            {droppableProvided => (
              <div ref={droppableProvided.innerRef}>
                <TableHeadersDraggableGroup
                  index={0}
                  legend="Category group"
                  name="rowGroups[0]"
                  totalItems={2}
                  onMoveGroupToOtherAxis={noop}
                />
              </div>
            )}
          </Droppable>
        </DragDropContext>
      </Formik>
    </TableHeadersContextProvider>,
  );
}
