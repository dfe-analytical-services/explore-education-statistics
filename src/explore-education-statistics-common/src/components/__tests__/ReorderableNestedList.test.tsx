import ReorderableNestedList from '@common/components/ReorderableNestedList';
import { ReorderableListItem } from '@common/components/ReorderableItem';
import React from 'react';
import { screen, within } from '@testing-library/react';
import noop from 'lodash/noop';
import render from '@common-test/render';

describe('ReorderableNestedList', () => {
  describe('one nested level', () => {
    const testList: ReorderableListItem[] = [
      {
        id: 'item-1',
        label: 'Item 1',
        childOptions: [
          { id: 'item-1-child-1', label: 'Item 1 child 1' },
          { id: 'item-1-child-2', label: 'Item 1 child 2' },
        ],
      },
      {
        id: 'item-2',
        label: 'Item 2',
        childOptions: [{ id: 'item-2-child-1', label: 'Item 2 child 1' }],
      },
      {
        id: 'item-3',
        label: 'Item 3',
        childOptions: [
          { id: 'item-3-child-1', label: 'Item 3 child 1' },
          { id: 'item-3-child-2', label: 'Item 3 child 2' },
          { id: 'item-3-child-3', label: 'Item 3 child 3' },
        ],
      },
      {
        id: 'item-4',
        label: 'Item 4',
      },
    ];
    test('renders the list', () => {
      render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={noop}
        />,
      );

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(4);

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
      expect(
        listItem1.getByRole('button', {
          name: 'Reorder options within Item 1',
        }),
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
      // only one childOption = no reorder options button
      expect(
        listItem2.queryByRole('button', {
          name: 'Reorder options within Item 2',
        }),
      ).not.toBeInTheDocument();

      const listItem3 = within(listItems[2]);
      expect(
        listItem3.getByRole('button', { name: 'Item 3' }),
      ).toBeInTheDocument();
      expect(
        listItem3.getByRole('button', { name: 'Move Item 3 up' }),
      ).toBeInTheDocument();
      expect(
        listItem3.getByRole('button', { name: 'Move Item 3 down' }),
      ).toBeInTheDocument();
      expect(
        listItem3.getByRole('button', {
          name: 'Reorder options within Item 3',
        }),
      ).toBeInTheDocument();

      const listItem4 = within(listItems[3]);
      expect(
        listItem4.getByRole('button', { name: 'Item 4' }),
      ).toBeInTheDocument();
      expect(
        listItem4.getByRole('button', { name: 'Move Item 4 up' }),
      ).toBeInTheDocument();
      expect(
        listItem4.queryByRole('button', { name: 'Move Item 4 down' }),
      ).not.toBeInTheDocument();
      // no childOptions = no reorder options button
      expect(
        listItem4.queryByRole('button', {
          name: 'Reorder options within Item 4',
        }),
      ).not.toBeInTheDocument();
    });

    test('clicking the reorder options button shows the child options for that item', async () => {
      const { user } = render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={noop}
        />,
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 1',
        }),
      );

      expect(
        screen.getByRole('button', { name: 'Close Item 1' }),
      ).toBeInTheDocument();

      const listItem1 = within(screen.getAllByRole('listitem')[0]);
      const childOptions = listItem1.getAllByRole('listitem');
      expect(childOptions).toHaveLength(2);

      const childOption1 = within(childOptions[0]);
      expect(
        childOption1.getByRole('button', { name: 'Item 1 child 1' }),
      ).toBeInTheDocument();
      expect(
        childOption1.queryByRole('button', { name: 'Move Item 1 child 1 up' }),
      ).not.toBeInTheDocument();
      expect(
        childOption1.getByRole('button', { name: 'Move Item 1 child 1 down' }),
      ).toBeInTheDocument();
      expect(
        childOption1.queryByRole('button', {
          name: 'Reorder options within Item 1 child 1',
        }),
      ).not.toBeInTheDocument();

      const childOption2 = within(childOptions[1]);
      expect(
        childOption2.getByRole('button', { name: 'Item 1 child 2' }),
      ).toBeInTheDocument();
      expect(
        childOption2.getByRole('button', { name: 'Move Item 1 child 2 up' }),
      ).toBeInTheDocument();
      expect(
        childOption2.queryByRole('button', {
          name: 'Move Item 1 child 2 down',
        }),
      ).not.toBeInTheDocument();
      expect(
        childOption2.queryByRole('button', {
          name: 'Reorder options within Item 1 child 2',
        }),
      ).not.toBeInTheDocument();
    });

    test('clicking the close button hides the child options for that item', async () => {
      const { user } = render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={noop}
        />,
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 1',
        }),
      );

      expect(
        screen.getByRole('button', { name: 'Close Item 1' }),
      ).toBeInTheDocument();

      const childOptions = within(
        screen.getAllByRole('listitem')[0],
      ).getAllByRole('listitem');
      expect(childOptions).toHaveLength(2);

      await user.click(screen.getByRole('button', { name: 'Close Item 1' }));

      expect(
        within(screen.getAllByRole('listitem')[0]).queryAllByRole('listitem'),
      ).toHaveLength(0);
    });

    test('calls onMoveItem when the move buttons are clicked', async () => {
      const handleMoveItem = jest.fn();

      const { user } = render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={handleMoveItem}
        />,
      );

      expect(handleMoveItem).not.toHaveBeenCalled();

      await user.click(
        screen.getByRole('button', { name: 'Move Item 1 down' }),
      );
      expect(handleMoveItem).toHaveBeenCalledTimes(1);
      expect(handleMoveItem).toHaveBeenCalledWith({
        prevIndex: 0,
        nextIndex: 1,
      });

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 1',
        }),
      );

      await user.click(
        screen.getByRole('button', { name: 'Move Item 1 child 2 up' }),
      );

      expect(handleMoveItem).toHaveBeenCalledTimes(2);
      expect(handleMoveItem).toHaveBeenCalledWith({
        prevIndex: 1,
        nextIndex: 0,
        expandedItemId: 'item-1',
        expandedItemParentId: undefined,
      });
    });
  });

  describe('two nested levels', () => {
    const testList: ReorderableListItem[] = [
      {
        id: 'item-1',
        label: 'Item 1',
        childOptions: [
          { id: 'item-1-child-1', label: 'Item 1 child 1' },
          { id: 'item-1-child-2', label: 'Item 1 child 2' },
        ],
      },
      {
        id: 'item-2',
        label: 'Item 2',
        childOptions: [
          {
            id: 'item-2-child-1',
            label: 'Item 2 child 1',
            parentId: 'item-2',
            childOptions: [
              {
                id: 'item-2-child-1-child-1',
                label: 'Item 2 child 1 child 1',
                parentId: 'item-2-child-1',
              },
              {
                id: 'item-2-child-1-child-2',
                label: 'Item 2 child 1 child 2',
                parentId: 'item-2-child-1',
              },
            ],
          },
        ],
      },
      {
        id: 'item-3',
        label: 'Item 3',
        childOptions: [
          {
            id: 'item-3-child-1',
            label: 'Item 3 child 1',
            parentId: 'item-3',
            childOptions: [
              {
                id: 'item-3-child-1-child-1',
                label: 'Item 3 child 1 child 1',
                parentId: 'item-3-child-1',
              },
            ],
          },
          {
            id: 'item-3-child-2',
            label: 'Item 3 child 2',
            parentId: 'item-3',
            childOptions: [
              {
                id: 'item-3-child-2-child-1',
                label: 'Item 3 child 2 child 1',
                parentId: 'item-3-child-2',
              },
              {
                id: 'item-3-child-2-child-2',
                label: 'Item 3 child 2 child 2',
                parentId: 'item-3-child-2',
              },
            ],
          },
        ],
      },
      {
        id: 'item-4',
        label: 'Item 4',
      },
    ];

    test('renders the list', () => {
      render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={noop}
        />,
      );

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(4);

      // Item 1 - only one nested level
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
      expect(
        listItem1.getByRole('button', {
          name: 'Reorder options within Item 1',
        }),
      ).toBeInTheDocument();

      // Item 2 - only one nested item which has child options
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
      expect(
        listItem2.getByRole('button', {
          name: 'Reorder options within Item 2',
        }),
      ).toBeInTheDocument();

      // Item 3 - two nested items, one with child options
      const listItem3 = within(listItems[2]);
      expect(
        listItem3.getByRole('button', { name: 'Item 3' }),
      ).toBeInTheDocument();
      expect(
        listItem3.getByRole('button', { name: 'Move Item 3 up' }),
      ).toBeInTheDocument();
      expect(
        listItem3.getByRole('button', { name: 'Move Item 3 down' }),
      ).toBeInTheDocument();
      expect(
        listItem3.getByRole('button', {
          name: 'Reorder options within Item 3',
        }),
      ).toBeInTheDocument();

      // Item 4 - no nested items
      const listItem4 = within(listItems[3]);
      expect(
        listItem4.getByRole('button', { name: 'Item 4' }),
      ).toBeInTheDocument();
      expect(
        listItem4.getByRole('button', { name: 'Move Item 4 up' }),
      ).toBeInTheDocument();
      expect(
        listItem4.queryByRole('button', { name: 'Move Item 4 down' }),
      ).not.toBeInTheDocument();
      expect(
        listItem4.queryByRole('button', {
          name: 'Reorder options within Item 4',
        }),
      ).not.toBeInTheDocument();
    });

    test('expanding an item with two nested levels and only one option in the second level shows the third level options', async () => {
      const { user } = render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={noop}
        />,
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 2',
        }),
      );

      expect(
        screen.getByRole('button', { name: 'Close Item 2' }),
      ).toBeInTheDocument();

      const childOptions = within(
        screen.getAllByRole('listitem')[1],
      ).getAllByRole('listitem');
      expect(childOptions).toHaveLength(2);

      const childOption1 = within(childOptions[0]);
      expect(
        childOption1.getByRole('button', { name: 'Item 2 child 1 child 1' }),
      ).toBeInTheDocument();
      expect(
        childOption1.queryByRole('button', {
          name: 'Move Item 2 child 1 child 1 up',
        }),
      ).not.toBeInTheDocument();
      expect(
        childOption1.getByRole('button', {
          name: 'Move Item 2 child 1 child 1 down',
        }),
      ).toBeInTheDocument();
      expect(
        childOption1.queryByRole('button', {
          name: 'Reorder options within Item 2 child 1',
        }),
      ).not.toBeInTheDocument();

      const childOption2 = within(childOptions[1]);
      expect(
        childOption2.getByRole('button', { name: 'Item 2 child 1 child 2' }),
      ).toBeInTheDocument();
      expect(
        childOption2.getByRole('button', {
          name: 'Move Item 2 child 1 child 2 up',
        }),
      ).toBeInTheDocument();
      expect(
        childOption2.queryByRole('button', {
          name: 'Move Item 2 child 1 child 2 down',
        }),
      ).not.toBeInTheDocument();
      expect(
        childOption2.queryByRole('button', {
          name: 'Reorder options within Item 2 child 1 child 2',
        }),
      ).not.toBeInTheDocument();
    });

    test('expanding an item with two nested levels and multiple items in the second level', async () => {
      const { user } = render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={noop}
        />,
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 3',
        }),
      );

      expect(
        screen.getByRole('button', { name: 'Close Item 3' }),
      ).toBeInTheDocument();

      const childOptions = within(
        screen.getAllByRole('listitem')[2],
      ).getAllByRole('listitem');
      expect(childOptions).toHaveLength(2);

      const childOption1 = within(childOptions[0]);
      expect(
        childOption1.getByRole('button', { name: 'Item 3 child 1' }),
      ).toBeInTheDocument();
      expect(
        childOption1.queryByRole('button', {
          name: 'Move Item 3 child 1 up',
        }),
      ).not.toBeInTheDocument();
      expect(
        childOption1.getByRole('button', {
          name: 'Move Item 3 child 1 down',
        }),
      ).toBeInTheDocument();
      expect(
        childOption1.queryByRole('button', {
          name: 'Reorder options within Item 3 child 1',
        }),
      ).not.toBeInTheDocument();

      const childOption2 = within(childOptions[1]);
      expect(
        childOption2.getByRole('button', { name: 'Item 3 child 2' }),
      ).toBeInTheDocument();
      expect(
        childOption2.getByRole('button', {
          name: 'Move Item 3 child 2 up',
        }),
      ).toBeInTheDocument();
      expect(
        childOption2.queryByRole('button', {
          name: 'Move Item 3 child 2 down',
        }),
      ).not.toBeInTheDocument();
      expect(
        childOption2.getByRole('button', {
          name: 'Reorder options within Item 3 child 2',
        }),
      ).toBeInTheDocument();
    });

    test('expanding a second level item', async () => {
      const { user } = render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={noop}
        />,
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 3',
        }),
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 3 child 2',
        }),
      );

      const secondLevelChildOptions = within(
        screen.getAllByRole('listitem')[2],
      ).getAllByRole('listitem');

      const thirdLevelChildOptions = within(
        secondLevelChildOptions[1],
      ).getAllByRole('listitem');
      expect(thirdLevelChildOptions).toHaveLength(2);

      const thirdLevelChildOption1 = within(thirdLevelChildOptions[0]);
      expect(
        thirdLevelChildOption1.getByRole('button', {
          name: 'Item 3 child 2 child 1',
        }),
      ).toBeInTheDocument();
      expect(
        thirdLevelChildOption1.queryByRole('button', {
          name: 'Move Item 3 child 2 child 1 up',
        }),
      ).not.toBeInTheDocument();
      expect(
        thirdLevelChildOption1.getByRole('button', {
          name: 'Move Item 3 child 2 child 1 down',
        }),
      ).toBeInTheDocument();
      expect(
        thirdLevelChildOption1.queryByRole('button', {
          name: 'Reorder options within Item 3 child 2 child 1',
        }),
      ).not.toBeInTheDocument();

      const thirdLevelChildOption2 = within(thirdLevelChildOptions[1]);
      expect(
        thirdLevelChildOption2.getByRole('button', {
          name: 'Item 3 child 2 child 2',
        }),
      ).toBeInTheDocument();
      expect(
        thirdLevelChildOption2.getByRole('button', {
          name: 'Move Item 3 child 2 child 2 up',
        }),
      ).toBeInTheDocument();
      expect(
        thirdLevelChildOption2.queryByRole('button', {
          name: 'Move Item 3 child 2 child 2 down',
        }),
      ).not.toBeInTheDocument();
      expect(
        thirdLevelChildOption2.queryByRole('button', {
          name: 'Reorder options within Item 3 child 2 child 2',
        }),
      ).not.toBeInTheDocument();
    });

    test('calls onMoveItem when the move buttons are clicked on a third level item', async () => {
      const handleMoveItem = jest.fn();

      const { user } = render(
        <ReorderableNestedList
          id="test"
          list={testList}
          onConfirm={noop}
          onMoveItem={handleMoveItem}
        />,
      );

      expect(handleMoveItem).not.toHaveBeenCalled();
      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 3',
        }),
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Reorder options within Item 3 child 2',
        }),
      );

      await user.click(
        screen.getByRole('button', {
          name: 'Move Item 3 child 2 child 1 down',
        }),
      );
      expect(handleMoveItem).toHaveBeenCalledTimes(1);
      expect(handleMoveItem).toHaveBeenCalledWith({
        prevIndex: 0,
        nextIndex: 1,
        expandedItemId: 'item-3-child-2',
        expandedItemParentId: 'item-3',
      });
    });
  });
});
