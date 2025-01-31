import ReorderableItem, {
  ReorderableListItem,
} from '@common/components/ReorderableItem';
import React from 'react';
import { screen } from '@testing-library/react';
import noop from 'lodash/noop';
import render from '@common-test/render';
import { DraggableProvided, DraggableStateSnapshot } from '@hello-pangea/dnd';

describe('ReorderableItem', () => {
  const testDraggableProvided: DraggableProvided = {
    innerRef: noop,
    dragHandleProps: {
      'data-rfd-drag-handle-draggable-id': 'test',
      'data-rfd-drag-handle-context-id': 'test',
      role: 'button',
      'aria-describedby': 'test',
      tabIndex: 0,
      draggable: true,
      onDragStart: noop,
    },
    draggableProps: {
      'data-rfd-draggable-context-id': 'test',
      'data-rfd-draggable-id': 'test',
    },
  };

  const testDraggableSnapshot = { isDragging: false } as DraggableStateSnapshot;

  const testItem: ReorderableListItem = {
    id: 'item-1',
    label: 'Item 1',
  };

  test('renders the item', () => {
    render(
      <ReorderableItem
        draggableProvided={testDraggableProvided}
        draggableSnapshot={testDraggableSnapshot}
        dropAreaActive={false}
        index={2}
        item={testItem}
        onMoveItem={noop}
      />,
    );

    expect(screen.getByRole('button', { name: 'Item 1' })).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Item 1 up' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Item 1 down' }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Reorder options for Item 1' }),
    ).not.toBeInTheDocument();
  });

  test('does not render the move up button when it is the first item in a list', () => {
    render(
      <ReorderableItem
        draggableProvided={testDraggableProvided}
        draggableSnapshot={testDraggableSnapshot}
        dropAreaActive={false}
        index={0}
        item={testItem}
        onMoveItem={noop}
      />,
    );

    expect(screen.getByRole('button', { name: 'Item 1' })).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Move Item 1 up' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Item 1 down' }),
    ).toBeInTheDocument();
  });

  test('does not render the move down button when it is the last item in a list', () => {
    render(
      <ReorderableItem
        draggableProvided={testDraggableProvided}
        draggableSnapshot={testDraggableSnapshot}
        dropAreaActive={false}
        index={2}
        isLastItem
        item={testItem}
        onMoveItem={noop}
      />,
    );

    expect(screen.getByRole('button', { name: 'Item 1' })).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Item 1 up' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Move Item 1 down' }),
    ).not.toBeInTheDocument();
  });

  test('renders the reorder options button when the item has multiple child options', () => {
    render(
      <ReorderableItem
        draggableProvided={testDraggableProvided}
        draggableSnapshot={testDraggableSnapshot}
        dropAreaActive={false}
        index={2}
        item={{
          ...testItem,
          childOptions: [
            { id: 'item-1-child-1', label: 'Item 1 child 1' },
            { id: 'item-1-child-2', label: 'Item 1 child 2' },
          ],
        }}
        onMoveItem={noop}
      />,
    );

    expect(screen.getByRole('button', { name: 'Item 1' })).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder options within Item 1' }),
    ).toBeInTheDocument();
  });

  test('renders the reorder options button when the item has only one child option and that has child options', () => {
    render(
      <ReorderableItem
        draggableProvided={testDraggableProvided}
        draggableSnapshot={testDraggableSnapshot}
        dropAreaActive={false}
        index={2}
        item={{
          ...testItem,
          childOptions: [
            {
              id: 'item-1-child-1',
              label: 'Item 1 child 1',
              childOptions: [
                {
                  id: 'item-1-child-1-child-1',
                  label: 'Item 1 child 1 child 1',
                },
                {
                  id: 'item-1-child-1-child-2',
                  label: 'Item 1 child 1 child 2',
                },
              ],
            },
          ],
        }}
        onMoveItem={noop}
      />,
    );

    expect(screen.getByRole('button', { name: 'Item 1' })).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reorder options within Item 1' }),
    ).toBeInTheDocument();
  });

  test('does not render controls when `dropAreaActive` is true', () => {
    render(
      <ReorderableItem
        draggableProvided={testDraggableProvided}
        draggableSnapshot={testDraggableSnapshot}
        dropAreaActive
        index={2}
        item={testItem}
        onMoveItem={noop}
      />,
    );

    expect(
      screen.queryByRole('button', { name: 'Move Item 1 up' }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Move Item 1 down' }),
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole('button', { name: 'Reorder options for Item 1' }),
    ).not.toBeInTheDocument();
  });
});
