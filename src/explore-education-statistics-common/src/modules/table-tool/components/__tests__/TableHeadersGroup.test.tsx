import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import TableHeadersGroup from '@common/modules/table-tool/components/TableHeadersGroup';
import { TableHeadersContextProvider } from '@common/modules/table-tool/contexts/TableHeadersContext';
import {
  testCategoryFilters,
  testIndicators,
  testLocationFilters,
  testTimePeriodFilters,
} from '@common/modules/table-tool/components/__tests__/__data__/tableHeadersConfig.data';
import { Formik } from 'formik';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';
import { render as baseRender, screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('TableHeadersGroup', () => {
  test('is draggable when groupDraggingEnabled is true', () => {
    render({ groupDraggingEnabled: true });

    const group = screen.getByTestId('draggable-group-category-group');
    // Draggable group has button role
    expect(group).toHaveAttribute('role', 'button');
    expect(
      within(group).getByRole('heading', { name: 'Category group' }),
    ).toBeInTheDocument();

    // Non-reorderable items are in a list
    const groupItems = within(group).getAllByRole('listitem');
    expect(groupItems).toHaveLength(2);
    expect(groupItems[0]).toHaveTextContent('Category 1');
    expect(groupItems[1]).toHaveTextContent('Category 2');

    expect(
      screen.getByRole('button', { name: 'Reorder items in Category group' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Category group to columns' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Done' }),
    ).not.toBeInTheDocument();
  });

  test('is reorderable and not draggable  when groupDraggingEnabled is false and the group is active', () => {
    render({ activeGroup: 'group-category-group' });

    // Does not have the drag handle div when reorderable
    expect(
      screen.queryByTestId('draggable-group-category-group'),
    ).not.toBeInTheDocument();

    // Reorderable items are buttons
    const groupButtons = screen.getAllByRole('button');
    expect(groupButtons).toHaveLength(3);
    expect(groupButtons[0]).toHaveTextContent('Category 1');
    expect(groupButtons[1]).toHaveTextContent('Category 2');
    expect(groupButtons[2]).toHaveTextContent('Done');

    expect(
      screen.queryByRole('button', { name: 'Reorder items in Category group' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Move Category group to columns' }),
    ).not.toBeInTheDocument();
  });

  test('is not draggable or reorderable when groupDraggingEnabled is false and the group is not active', () => {
    render({ activeGroup: 'anotherGroup' });

    const group = screen.getByTestId('draggable-group-category-group');

    // Does not have button role when not draggable
    expect(group).not.toHaveAttribute('role', 'button');

    // Non-reorderable items are in a list
    const groupItems = within(group).getAllByRole('listitem');
    expect(groupItems).toHaveLength(2);
    expect(groupItems[0]).toHaveTextContent('Category 1');
    expect(groupItems[1]).toHaveTextContent('Category 2');

    expect(
      screen.queryByRole('button', { name: 'Done' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Reorder items in Category group' }),
    ).toBeDisabled();
    expect(
      screen.getByRole('button', { name: 'Move Category group to columns' }),
    ).toBeDisabled();
  });
});

function render({
  activeGroup,
  groupDraggingActive = false,
  groupDraggingEnabled = false,
}: {
  activeGroup?: string;
  groupDraggingActive?: boolean;
  groupDraggingEnabled?: boolean;
}) {
  const testInitialFormValues: TableHeadersFormValues = {
    columnGroups: [testTimePeriodFilters, testLocationFilters],
    rowGroups: [testCategoryFilters, testIndicators],
  };

  baseRender(
    <TableHeadersContextProvider
      activeGroup={activeGroup}
      groupDraggingActive={groupDraggingActive}
      groupDraggingEnabled={groupDraggingEnabled}
    >
      <Formik onSubmit={noop} initialValues={testInitialFormValues}>
        <DragDropContext onDragEnd={noop}>
          <Droppable droppableId="test">
            {droppableProvided => (
              <div ref={droppableProvided.innerRef}>
                <TableHeadersGroup
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
