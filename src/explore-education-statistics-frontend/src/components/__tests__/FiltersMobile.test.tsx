import FiltersMobile from '@frontend/components/FiltersMobile';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FiltersMobile', () => {
  test('renders the modal trigger button', () => {
    render(
      <FiltersMobile title="Test title" totalResults={10}>
        <p>The filters</p>
      </FiltersMobile>,
    );

    expect(
      screen.getByRole('button', { name: 'Filter results' }),
    ).toBeInTheDocument();

    expect(screen.queryByText('10 results')).not.toBeInTheDocument();
  });

  test('renders the modal trigger button', () => {
    render(
      <FiltersMobile title="Test title" totalResults={10}>
        <p>The filters</p>
      </FiltersMobile>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Filter results' }));

    expect(screen.getByText('10 results')).toBeInTheDocument();
    expect(screen.getByText('The filters')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Back to results' }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Show 10 results' }),
    ).toBeInTheDocument();
  });
});
