import { render } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';
import Link from '../Link';

describe('Link', () => {
  test('renders correctly without `unvisited` state', () => {
    const { getByText } = render(
      <MemoryRouter>
        <Link to="/the-link">Test Link</Link>
      </MemoryRouter>,
    );

    const link = getByText('Test Link') as HTMLLinkElement;

    expect(link).toMatchSnapshot();
  });

  test('renders correctly with `unvisited` state', () => {
    const { getByText } = render(
      <MemoryRouter>
        <Link to="/the-link" unvisited>
          Test Link
        </Link>
      </MemoryRouter>,
    );

    const link = getByText('Test Link');

    expect(link).toMatchSnapshot();
  });

  test('links to relative URL correctly', () => {
    const { getByText } = render(
      <MemoryRouter>
        <Link to="/the-link">Test Link</Link>
      </MemoryRouter>,
    );

    const link = getByText('Test Link') as HTMLLinkElement;

    expect(link).toHaveAttribute('href', '/the-link');
  });

  test('links to absolute URL correctly', () => {
    const { getByText } = render(
      <MemoryRouter>
        <Link to="https://www.gov.uk">Test Link</Link>
      </MemoryRouter>,
    );

    const link = getByText('Test Link');

    expect(link).toHaveAttribute('href', 'https://www.gov.uk');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer');
    expect(link).toHaveAttribute('target', '_blank');
  });

  test('links to hash URL correctly', () => {
    const { getByText } = render(
      <MemoryRouter>
        <Link to="#test">Test Link</Link>
      </MemoryRouter>,
    );

    const link = getByText('Test Link');

    expect(link).toHaveAttribute('href', '#test');
    expect(link).not.toHaveAttribute('rel', 'noopener noreferrer');
    expect(link).not.toHaveAttribute('target', '_blank');
  });
});
