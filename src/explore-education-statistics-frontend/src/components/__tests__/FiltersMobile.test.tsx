import render from '@common-test/render';
import FiltersMobile from '@frontend/components/FiltersMobile';
import { screen } from '@testing-library/react';
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

  test('renders the modal when click the trigger button', async () => {
    const { user } = render(
      <FiltersMobile title="Test title" totalResults={10}>
        <p>The filters</p>
      </FiltersMobile>,
    );

    await user.click(screen.getByRole('button', { name: 'Filter results' }));

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
