import render from '@common-test/render';
import PageNav from '@common/components/PageNav';
import { screen, within } from '@testing-library/react';
import React from 'react';

describe('PageNav', () => {
  test('renders correctly without sub nav', () => {
    render(
      <PageNav
        items={[
          {
            id: 'item-1',
            text: 'Item 1',
          },
          {
            id: 'item-2',
            text: 'Item 2',
          },
          {
            id: 'item-3',
            text: 'Item 3',
          },
        ]}
      />,
    );

    expect(screen.getByRole('heading', { name: 'On this page' }));

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(3);

    expect(
      within(items[0]).getByRole('link', { name: 'Item 1' }),
    ).toHaveAttribute('href', '#item-1');
    expect(
      within(items[1]).getByRole('link', { name: 'Item 2' }),
    ).toHaveAttribute('href', '#item-2');
    expect(
      within(items[2]).getByRole('link', { name: 'Item 3' }),
    ).toHaveAttribute('href', '#item-3');
  });

  test('renders correctly with sub nav', () => {
    render(
      <PageNav
        items={[
          {
            id: 'item-1',
            text: 'Item 1',
            subNavItems: [{ id: 'item-1-a', text: 'Item 1a' }],
          },
          {
            id: 'item-2',
            text: 'Item 2',
            subNavItems: [
              { id: 'item-2-a', text: 'Item 2a' },
              { id: 'item-2-b', text: 'Item 2b' },
            ],
          },
          {
            id: 'item-3',
            text: 'Item 3',
          },
        ]}
      />,
    );

    expect(screen.getByRole('heading', { name: 'On this page' }));

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(6);

    expect(
      within(items[0]).getByRole('link', { name: 'Item 1' }),
    ).toHaveAttribute('href', '#item-1');

    const item1SubItems = within(items[0]).getAllByRole('listitem');
    expect(item1SubItems).toHaveLength(1);
    expect(
      within(item1SubItems[0]).getByRole('link', { name: 'Item 1a' }),
    ).toHaveAttribute('href', '#item-1-a');

    expect(
      within(items[2]).getByRole('link', { name: 'Item 2' }),
    ).toHaveAttribute('href', '#item-2');
    const item2SubItems = within(items[2]).getAllByRole('listitem');
    expect(item2SubItems).toHaveLength(2);
    expect(
      within(item2SubItems[0]).getByRole('link', { name: 'Item 2a' }),
    ).toHaveAttribute('href', '#item-2-a');
    expect(
      within(item2SubItems[1]).getByRole('link', { name: 'Item 2b' }),
    ).toHaveAttribute('href', '#item-2-b');

    expect(
      within(items[5]).getByRole('link', { name: 'Item 3' }),
    ).toHaveAttribute('href', '#item-3');
  });

  test('renders correctly with a custom heading', () => {
    render(
      <PageNav
        heading="A custom heading"
        items={[
          {
            id: 'item-1',
            text: 'Item 1',
          },
          {
            id: 'item-2',
            text: 'Item 2',
          },
        ]}
      />,
    );

    expect(screen.getByRole('heading', { name: 'A custom heading' }));
    expect(screen.getAllByRole('listitem')).toHaveLength(2);
  });

  test('calls the `onClickItem` method with the item id when clicked', async () => {
    const handleClick = jest.fn();
    const { user } = render(
      <PageNav
        items={[
          {
            id: 'item-1',
            text: 'Item 1',
          },
          {
            id: 'item-2',
            text: 'Item 2',
          },
        ]}
        onClickItem={handleClick}
      />,
    );

    expect(screen.getByRole('heading', { name: 'On this page' }));

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(2);

    expect(handleClick).not.toHaveBeenCalled();

    await user.click(screen.getByRole('link', { name: 'Item 2' }));

    expect(handleClick).toHaveBeenCalledTimes(1);
    expect(handleClick).toHaveBeenCalledWith('item-2');
  });
});
