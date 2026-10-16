import CollapsibleList from '@common/components/CollapsibleList';
import { screen, render } from '@testing-library/react';
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

    // Without `renderAllItems` the button stays visible without JS, as it
    // is the only sign that there are more items.
    expect(
      screen.getByRole('button', { name: 'Show 2 more items' }),
    ).not.toHaveClass('dfe-js-only');
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

  test('clicking the "Show X more items" button reveals all items and changes the button text to "Hide X items"', async () => {
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

    await userEvent.click(
      screen.getByRole('button', { name: 'Show 2 more items' }),
    );

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
      <CollapsibleList id="test-id" collapseAfter={3} isCollapsed={false}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(screen.getAllByRole('listitem')).toHaveLength(5);

    await userEvent.click(screen.getByRole('button', { name: 'Hide 2 items' }));

    expect(await screen.findByText('Show 2 more items')).toBeInTheDocument();

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

  test('setting itemName and itemNamePlural changes the button text', async () => {
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

    await userEvent.click(
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

  // Note: jsdom does not load the `js-enabled` stylesheet that hides
  // `dfe-js-hidden`, so these assert on the class rather than on visibility.
  describe('renderAllItems', () => {
    test('renders every item, marking the collapsed overflow as js-hidden', () => {
      render(
        <CollapsibleList id="test-id" collapseAfter={3} renderAllItems>
          <li>Item 1</li>
          <li>Item 2</li>
          <li>Item 3</li>
          <li>Item 4</li>
          <li>Item 5</li>
        </CollapsibleList>,
      );

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(5);
      expect(listItems[4]).toHaveTextContent('Item 5');

      expect(listItems[0]).not.toHaveClass('dfe-js-hidden');
      expect(listItems[1]).not.toHaveClass('dfe-js-hidden');
      expect(listItems[2]).not.toHaveClass('dfe-js-hidden');
      expect(listItems[3]).toHaveClass('dfe-js-hidden');
      expect(listItems[4]).toHaveClass('dfe-js-hidden');

      // Every item is already shown without JS, so the button is hidden then.
      expect(
        screen.getByRole('button', { name: 'Show 2 more items' }),
      ).toHaveClass('dfe-js-only');
    });

    test('clears the js-hidden class once expanded', async () => {
      render(
        <CollapsibleList id="test-id" collapseAfter={3} renderAllItems>
          <li>Item 1</li>
          <li>Item 2</li>
          <li>Item 3</li>
          <li>Item 4</li>
          <li>Item 5</li>
        </CollapsibleList>,
      );

      await userEvent.click(
        screen.getByRole('button', { name: 'Show 2 more items' }),
      );

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(5);
      listItems.forEach(item => {
        expect(item).not.toHaveClass('dfe-js-hidden');
      });

      expect(
        screen.getByRole('button', { name: 'Hide 2 items' }),
      ).toBeInTheDocument();
    });

    test('renders every item as js-hidden when collapseAfter is zero', () => {
      render(
        <CollapsibleList id="test-id" collapseAfter={0} renderAllItems>
          <li>Item 1</li>
          <li>Item 2</li>
          <li>Item 3</li>
        </CollapsibleList>,
      );

      const listItems = screen.getAllByRole('listitem');
      expect(listItems).toHaveLength(3);
      listItems.forEach(item => {
        expect(item).toHaveClass('dfe-js-hidden');
      });

      expect(
        screen.getByRole('button', { name: 'Show 3 items' }),
      ).toBeInTheDocument();
    });
  });
});
