import SearchForm from '@frontend/components/SearchForm';
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
    render(<SearchForm value="find me" onSubmit={noop} />);
    expect(screen.getByLabelText('Search')).toHaveValue('find me');
  });

  test('calls onSubmit when submitted with a valid length string', async () => {
    const handleSubmit = jest.fn();
    render(<SearchForm onSubmit={handleSubmit} />);
    await userEvent.type(screen.getByLabelText('Search'), 'find me');
    await userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).toHaveBeenCalledWith('find me');
  });

  test('does not call onSubmit and shows and error when submitted with an invalid length string', async () => {
    const handleSubmit = jest.fn();
    render(<SearchForm onSubmit={handleSubmit} />);
    await userEvent.type(screen.getByLabelText('Search'), 'fi');
    await userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).not.toHaveBeenCalled();
    expect(
      screen.getByText('Search must be at least 3 characters'),
    ).toBeInTheDocument();
  });

  test('removes the validation error when the input becomes valid', async () => {
    const handleSubmit = jest.fn();
    render(<SearchForm onSubmit={handleSubmit} />);
    await userEvent.type(screen.getByLabelText('Search'), 'fi');
    await userEvent.click(screen.getByRole('button', { name: 'Search' }));
    expect(handleSubmit).not.toHaveBeenCalled();
    expect(
      screen.getByText('Search must be at least 3 characters'),
    ).toBeInTheDocument();

    await userEvent.type(screen.getByLabelText('Search'), 'fin');
    expect(
      screen.queryByText('Search must be at least 3 characters'),
    ).not.toBeInTheDocument();
  });
});
