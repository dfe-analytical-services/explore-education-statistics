import { render, fireEvent } from '@testing-library/react';
import CollapsibleList from '@common/components/CollapsibleList';
import React from 'react';

describe('CollapsibleList', () => {
  test('renders 5 items with 2 visually hidden', () => {
    const { getByText, queryByText } = render(
      <CollapsibleList collapseAfter={3}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(getByText('Item 1')).not.toHaveClass('govuk-visually-hidden');
    expect(getByText('Item 2')).not.toHaveClass('govuk-visually-hidden');
    expect(getByText('Item 3')).not.toHaveClass('govuk-visually-hidden');
    expect(getByText('Item 4')).toHaveClass('govuk-visually-hidden');
    expect(getByText('Item 5')).toHaveClass('govuk-visually-hidden');

    expect(queryByText('Show 2 more items'));
  });

  test('renders no visually hidden items when `collapseAfter` is more than the list size', () => {
    const { container } = render(
      <CollapsibleList collapseAfter={8}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(container.querySelectorAll('li.govuk-visually-hidden')).toHaveLength(
      0,
    );
  });

  test('renders all items as visually hidden when `collapseAfter` is negative', () => {
    const { container, queryByText } = render(
      <CollapsibleList collapseAfter={-1}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(container.querySelectorAll('li.govuk-visually-hidden')).toHaveLength(
      5,
    );

    expect(queryByText('Show 5 items')).not.toBeNull();
  });

  test('renders all items as visually hidden when `collapseAfter` is zero', () => {
    const { container, queryByText } = render(
      <CollapsibleList collapseAfter={0}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(container.querySelectorAll('li.govuk-visually-hidden')).toHaveLength(
      5,
    );

    expect(queryByText('Show 5 items')).not.toBeNull();
  });

  test("clicking on 'Show 2 more items' reveals all list items when some are hidden", () => {
    const { container, getByText } = render(
      <CollapsibleList collapseAfter={3}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(container.querySelectorAll('li.govuk-visually-hidden')).toHaveLength(
      2,
    );

    fireEvent.click(getByText('Show 2 more items'));

    expect(container.querySelectorAll('li.govuk-visually-hidden')).toHaveLength(
      0,
    );
  });

  test("clicking on 'Show all' toggles the button text to 'Collapse list'", () => {
    const { getByText, container } = render(
      <CollapsibleList collapseAfter={3}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const button = container.querySelector('button');

    expect(button).toHaveTextContent('Show 2 more items');

    fireEvent.click(getByText('Show 2 more items'));

    expect(button).toHaveTextContent('Collapse list');
  });

  test("clicking on 'Collapse list' hides list items", () => {
    const { container, getByText } = render(
      <CollapsibleList collapseAfter={3} isCollapsed={false}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    expect(container.querySelectorAll('li.govuk-visually-hidden')).toHaveLength(
      0,
    );
    fireEvent.click(getByText('Collapse list'));

    expect(container.querySelectorAll('li.govuk-visually-hidden')).toHaveLength(
      2,
    );
  });

  test("clicking on 'Collapse list' toggles the button text to 'Show 2 more items'", () => {
    const { getByText, container } = render(
      <CollapsibleList collapseAfter={3} isCollapsed={false}>
        <li>Item 1</li>
        <li>Item 2</li>
        <li>Item 3</li>
        <li>Item 4</li>
        <li>Item 5</li>
      </CollapsibleList>,
    );

    const button = container.querySelector('button');

    expect(button).toHaveTextContent('Collapse list');

    fireEvent.click(getByText('Collapse list'));

    expect(button).toHaveTextContent('Show 2 more items');
  });
});
