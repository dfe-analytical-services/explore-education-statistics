import ReorderableList from '@common/components/ReorderableList';
import { ReorderableListItem } from '@common/components/ReorderableItem';
import React from 'react';
import { screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import render from '@common-test/render';

describe('ReorderableList', () => {
  const testList: ReorderableListItem[] = [
    { id: 'item-1', label: 'Item 1' },
    { id: 'item-2', label: 'Item 2' },
    { id: 'item-3', label: 'Item 3' },
  ];

  test('renders the list', () => {
    render(
      <ReorderableList
        id="test"
        list={testList}
        onConfirm={noop}
        onMoveItem={noop}
      />,
    );

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(3);

    const listItem1 = within(listItems[0]);
    expect(
      listItem1.getByRole('button', { name: 'Item 1' }),
    ).toBeInTheDocument();
    expect(
      listItem1.queryByRole('button', { name: 'Move Item 1 up' }),
    ).not.toBeInTheDocument();
    expect(
      listItem1.getByRole('button', { name: 'Move Item 1 down' }),
    ).toBeInTheDocument();

    const listItem2 = within(listItems[1]);
    expect(
      listItem2.getByRole('button', { name: 'Item 2' }),
    ).toBeInTheDocument();
    expect(
      listItem2.getByRole('button', { name: 'Move Item 2 up' }),
    ).toBeInTheDocument();
    expect(
      listItem2.getByRole('button', { name: 'Move Item 2 down' }),
    ).toBeInTheDocument();

    const listItem3 = within(listItems[2]);
    expect(
      listItem3.getByRole('button', { name: 'Item 3' }),
    ).toBeInTheDocument();
    expect(
      listItem3.getByRole('button', { name: 'Move Item 3 up' }),
    ).toBeInTheDocument();
    expect(
      listItem3.queryByRole('button', { name: 'Move Item 3 down' }),
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Confirm order' }),
    ).toBeInTheDocument();
  });

  test('calls onConfirm when the confirm button is clicked', async () => {
    const handleConfirm = jest.fn();
    const { user } = render(
      <ReorderableList
        id="test"
        list={testList}
        onConfirm={handleConfirm}
        onMoveItem={noop}
      />,
    );

    expect(handleConfirm).not.toHaveBeenCalled();
    await user.click(screen.getByRole('button', { name: 'Confirm order' }));
    expect(handleConfirm).toHaveBeenCalledTimes(1);
  });

  test('calls onMoveItem when the move buttons are clicked', async () => {
    const handleMoveItem = jest.fn();
    const { user } = render(
      <ReorderableList
        id="test"
        list={testList}
        onConfirm={noop}
        onMoveItem={handleMoveItem}
      />,
    );

    expect(handleMoveItem).not.toHaveBeenCalled();
    await user.click(screen.getByRole('button', { name: 'Move Item 1 down' }));
    expect(handleMoveItem).toHaveBeenCalledTimes(1);
    expect(handleMoveItem).toHaveBeenCalledWith({ prevIndex: 0, nextIndex: 1 });

    await user.click(screen.getByRole('button', { name: 'Move Item 3 up' }));
    expect(handleMoveItem).toHaveBeenCalledTimes(2);
    expect(handleMoveItem).toHaveBeenCalledWith({ prevIndex: 2, nextIndex: 1 });
  });

  test('calls onReverse when the reverse button is clicked', async () => {
    const handleReverse = jest.fn();
    const { user } = render(
      <ReorderableList
        id="test"
        list={testList}
        onConfirm={noop}
        onMoveItem={noop}
        onReverse={handleReverse}
      />,
    );

    expect(handleReverse).not.toHaveBeenCalled();
    await user.click(screen.getByRole('button', { name: 'Reverse order' }));
    expect(handleReverse).toHaveBeenCalledTimes(1);
  });

  test('calls onCancel when the cancel button is clicked', async () => {
    const handleCancel = jest.fn();
    const { user } = render(
      <ReorderableList
        id="test"
        list={testList}
        onConfirm={noop}
        onMoveItem={noop}
        onCancel={handleCancel}
      />,
    );

    expect(handleCancel).not.toHaveBeenCalled();
    await user.click(screen.getByRole('button', { name: 'Cancel reordering' }));
    expect(handleCancel).toHaveBeenCalledTimes(1);
  });
});
