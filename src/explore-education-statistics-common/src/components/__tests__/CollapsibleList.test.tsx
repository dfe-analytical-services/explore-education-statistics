import CollapsibleList from '@common/components/CollapsibleList';
import { screen, render, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('CollapsibleList', () => {
  test('renders 3 of 5 items when collapseAfter is 3', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={3}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(3);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');
    expect(listItems[2]).toHaveTextContent('Item 3');
    expect(screen.queryByText('Item 4')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 5')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Show 2 more items' }),
    ).toBeInTheDocument();
  });

  test('renders 4 of 5 items when collapseAfter is 4', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={4}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(4);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');
    expect(listItems[2]).toHaveTextContent('Item 3');
    expect(listItems[3]).toHaveTextContent('Item 4');
    expect(screen.queryByText('Item 5')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Show 1 more item' }),
    ).toBeInTheDocument();
  });

  test('renders all items when collapseAfter is more than the list size', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={8}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(5);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');
    expect(listItems[2]).toHaveTextContent('Item 3');
    expect(listItems[3]).toHaveTextContent('Item 4');
    expect(listItems[4]).toHaveTextContent('Item 5');

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  test('renders all items when collapseAfter is equal to the list size', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={5}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(5);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');
    expect(listItems[2]).toHaveTextContent('Item 3');
    expect(listItems[3]).toHaveTextContent('Item 4');
    expect(listItems[4]).toHaveTextContent('Item 5');

    expect(screen.queryByRole('button')).not.toBeInTheDocument();
  });

  test('renders no items when collapseAfter is negative', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={-1}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(screen.queryByRole('listitem')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 1')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 2')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 3')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 4')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 5')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Show 5 items' }),
    ).toBeInTheDocument();
  });

  test('renders no items when collapseAfter is zero', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={0}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(screen.queryByRole('listitem')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 1')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 2')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 3')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 4')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 5')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Show 5 items' }),
    ).toBeInTheDocument();
  });

  test('clicking the "Show X more items" button reveals all items and changes the button text to "Hide X items"', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={3}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(3);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');
    expect(listItems[2]).toHaveTextContent('Item 3');
    expect(screen.queryByText('Item 4')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 5')).not.toBeInTheDocument();

    userEvent.click(screen.getByRole('button', { name: 'Show 2 more items' }));

    const updatedListItems = screen.getAllByRole('listitem');
    expect(updatedListItems).toHaveLength(5);
    expect(updatedListItems[3]).toHaveTextContent('Item 4');
    expect(updatedListItems[4]).toHaveTextContent('Item 5');

    expect(
      screen.getByRole('button', { name: 'Hide 2 items' }),
    ).toBeInTheDocument();
  });

  test('clicking the "Hide X items" button hides items and changes the button text to "Show X more items"', async () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={3}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(screen.getAllByRole('listitem')).toHaveLength(3);

    userEvent.click(screen.getByRole('button', { name: 'Show 2 more items' }));

    await waitFor(() => {
      expect(screen.getByText('Hide 2 items')).toBeInTheDocument();
    });

    expect(screen.getAllByRole('listitem')).toHaveLength(5);

    userEvent.click(screen.getByRole('button', { name: 'Hide 2 items' }));

    await waitFor(() => {
      expect(screen.getByText('Show 2 more items')).toBeInTheDocument();
    });

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(3);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');
    expect(listItems[2]).toHaveTextContent('Item 3');
    expect(screen.queryByText('Item 4')).not.toBeInTheDocument();
    expect(screen.queryByText('Item 5')).not.toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Show 2 more items' }),
    ).toBeInTheDocument();
  });

  test('renders all items with the hide button when isCollapsed is false', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={3} isCollapsed={false}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const listItems = screen.getAllByRole('listitem');
    expect(listItems).toHaveLength(5);
    expect(listItems[0]).toHaveTextContent('Item 1');
    expect(listItems[1]).toHaveTextContent('Item 2');
    expect(listItems[2]).toHaveTextContent('Item 3');
    expect(listItems[3]).toHaveTextContent('Item 4');
    expect(listItems[4]).toHaveTextContent('Item 5');

    expect(
      screen.getByRole('button', { name: 'Hide 2 items' }),
    ).toBeInTheDocument();
  });

  test('setting itemName and itemNamePlural changes the button text', () => {
    render(
      <CollapsibleList
        id="test-id"
        collapseAfter={3}
        itemName="footnote"
        itemNamePlural="footnotes"
      >
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    userEvent.click(
      screen.getByRole('button', { name: 'Show 2 more footnotes' }),
    );

    expect(
      screen.getByRole('button', { name: 'Hide 2 footnotes' }),
    ).toBeInTheDocument();
  });

  test('uses the singular itemName in the button text when there is only 1 more item', () => {
    render(
      <CollapsibleList
        id="test-id"
        collapseAfter={4}
        itemName="footnote"
        itemNamePlural="footnotes"
      >
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(
      screen.getByRole('button', { name: 'Show 1 more footnote' }),
    ).toBeInTheDocument();
  });

  test('uses the singular default name in the button text when there is only 1 more item', () => {
    render(
      <CollapsibleList id="test-id" collapseAfter={4}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(
      screen.getByRole('button', { name: 'Show 1 more item' }),
    ).toBeInTheDocument();
  });
});
