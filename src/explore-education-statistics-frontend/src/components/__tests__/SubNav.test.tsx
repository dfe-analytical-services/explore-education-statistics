import render from '@common-test/render';
import Subnav from '@frontend/components/SubNav';
import { screen, within } from '@testing-library/react';
import React from 'react';

const testItems = [
  {
    href: '/item-1',
    text: 'Item 1',
  },
  {
    href: '/item-2',
    text: 'Item 2',
    isActive: true,
  },
  {
    href: '/item-3',
    text: 'Item 3',
  },
];

describe('SubNav', () => {
  test('renders correctly with default visible heading', () => {
    render(<Subnav items={testItems} />);

    expect(screen.getByRole('heading', { name: 'In this section' }));

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(3);

    expect(
      within(items[0]).getByRole('link', { name: 'Item 1' }),
    ).toHaveAttribute('href', '/item-1');
    expect(
      within(items[1]).getByRole('link', { name: 'Item 2' }),
    ).toHaveAttribute('href', '/item-2');
    expect(
      within(items[2]).getByRole('link', { name: 'Item 3' }),
    ).toHaveAttribute('href', '/item-3');
    expect(
      within(items[0]).getByRole('link', { name: 'Item 1' }),
    ).not.toHaveAttribute('aria-current', 'page');
    expect(
      within(items[1]).getByRole('link', { name: 'Item 2' }),
    ).toHaveAttribute('aria-current', 'page');
  });

  test('renders correctly with custom heading that is not visible', () => {
    render(
      <Subnav
        items={testItems}
        heading="Custom heading title"
        headingVisible={false}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Custom heading title' }),
    ).toHaveAttribute('class', 'govuk-visually-hidden');
  });
});
