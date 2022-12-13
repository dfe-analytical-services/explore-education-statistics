import SearchForm from '@frontend/modules/find-statistics/components/SearchForm';
import { render, screen } from '@testing-library/react';
import React from 'react';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

describe('SearchForm', () => {
  test('renders correctly', () => {
    render(<SearchForm onSubmit={noop} />);
    expect(screen.getByLabelText('Search')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Search' })).toBeInTheDocument();
  });

  test('renders correctly with an initial search term', () => {
    render(<SearchForm searchTerm="find me" onSubmit={noop} />);
    expect(screen.getByLabelText('Search')).toHaveValue('find me');
  });

  test('calls onSubmit when submitted with a valid length string', () => {
    const handleSubmit = jest.fn();
    render(<SearchForm onSubmit={handleSubmit} />);
    userEvent.type(screen.getByLabelText('Search'), 'find me');
    userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).toHaveBeenCalledWith('find me');
  });

  test('does not call onSubmit and shows and error when submitted with an invalid length string', () => {
    const handleSubmit = jest.fn();
    render(<SearchForm onSubmit={handleSubmit} />);
    userEvent.type(screen.getByLabelText('Search'), 'fi');
    userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).not.toHaveBeenCalled();
    expect(
      screen.getByText('Search must be at least 3 characters'),
    ).toBeInTheDocument();
  });

  test('removes the validation error when the input becomes valid', () => {
    const handleSubmit = jest.fn();
    render(<SearchForm onSubmit={handleSubmit} />);
    userEvent.type(screen.getByLabelText('Search'), 'fi');
    userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).not.toHaveBeenCalled();
    expect(
      screen.getByText('Search must be at least 3 characters'),
    ).toBeInTheDocument();

    userEvent.type(screen.getByLabelText('Search'), 'fin');
    expect(
      screen.queryByText('Search must be at least 3 characters'),
    ).not.toBeInTheDocument();
  });
});
