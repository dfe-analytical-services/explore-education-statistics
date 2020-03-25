import React from 'react';
import { render } from '@testing-library/react';
import Link from '../Link';

describe('Link', () => {
  test('renders correctly without `unvisited` state', () => {
    const { getByText } = render(<Link to="/the-link">Test Link</Link>);

    const link = getByText('Test Link') as HTMLLinkElement;

    expect(link).toMatchSnapshot();
  });

  test('renders correctly with `unvisited` state', () => {
    const { getByText } = render(
      <Link to="/the-link" unvisited>
        Test Link
      </Link>,
    );

    const link = getByText('Test Link');

    expect(link).toMatchSnapshot();
  });

  test('anchor href is correct', () => {
    const { getByText } = render(<Link to="/the-link">Test Link</Link>);

    const link = getByText('Test Link') as HTMLLinkElement;

    expect(link.href).toBe('http://localhost/the-link');
  });
});
