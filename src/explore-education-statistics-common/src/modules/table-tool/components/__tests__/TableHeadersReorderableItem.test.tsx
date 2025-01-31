import TableHeadersReorderableItem from '@common/modules/table-tool/components/TableHeadersReorderableItem';
import { render, screen } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import {
  DraggableProvided,
  DraggableProvidedDraggableProps,
  DraggableStateSnapshot,
} from '@hello-pangea/dnd';
import { Filter } from '../../types/filters';

describe('TableHeadersReorderableItem', () => {
  const testOption: Filter = {
    group: 'test-group',
    id: 'test-id',
    label: 'Test label',
    value: 'test-value',
  };
  test('renders the reorderable item', () => {
    render(
      <TableHeadersReorderableItem
        activeItem=""
        draggableProvided={
          {
            innerRef: noop,
            dragHandleProps: {},
            draggableProps: {} as DraggableProvidedDraggableProps,
          } as DraggableProvided
        }
        draggableSnapshot={{ isDragging: false } as DraggableStateSnapshot}
        index={1}
        isGhosted={false}
        isLastItem={false}
        isSelected={false}
        option={testOption}
        selectedIndicesLength={1}
        onClick={noop}
        onClickMoveDown={noop}
        onClickMoveUp={noop}
        onKeyDown={noop}
        onSetActive={noop}
        onTouchEnd={noop}
      />,
    );

    const button = screen.getByTestId('reorderable-item');
    expect(button).toHaveTextContent('Test label');

    expect(
      screen.getByRole('button', { name: 'Move Test label' }),
    ).toBeInTheDocument();
  });

  test('renders the move controls when active', () => {
    render(
      <TableHeadersReorderableItem
        activeItem={testOption.id}
        draggableProvided={
          {
            innerRef: noop,
            dragHandleProps: {},
            draggableProps: {} as DraggableProvidedDraggableProps,
          } as DraggableProvided
        }
        draggableSnapshot={{ isDragging: false } as DraggableStateSnapshot}
        index={1}
        isGhosted={false}
        isLastItem={false}
        isSelected={false}
        option={testOption}
        selectedIndicesLength={1}
        onClick={noop}
        onClickMoveDown={noop}
        onClickMoveUp={noop}
        onKeyDown={noop}
        onSetActive={noop}
        onTouchEnd={noop}
      />,
    );

    const button = screen.getByTestId('reorderable-item');
    expect(button).toHaveTextContent('Test label');

    expect(
      screen.queryByRole('button', { name: 'Move' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Move Test label up' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Move Test label down' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Done' })).toBeInTheDocument();
  });
});
