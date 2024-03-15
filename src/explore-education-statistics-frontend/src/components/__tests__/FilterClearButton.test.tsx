import React from 'react';
import { render, screen, within } from '@testing-library/react';
import FilterClearButton from '@frontend/components/FilterClearButton';
import noop from 'lodash/noop';
import userEvent from '@testing-library/user-event';

describe('FilterClearButton', () => {
  test('renders the button with filterType', () => {
    render(
      <FilterClearButton
        filterType="Test type"
        name="Test name"
        onClick={noop}
      />,
    );

    const button = within(screen.getByRole('button'));
    expect(button.getByText('Test type')).toBeInTheDocument();
    expect(button.getByText('Test name')).toBeInTheDocument();
  });

  test('renders the button without filterType', () => {
    render(<FilterClearButton name="Test name" onClick={noop} />);

    const button = within(screen.getByRole('button'));
    expect(button.getByText('Test name')).toBeInTheDocument();
  });

  test('calls the click handler on click', async () => {
    const handleClick = jest.fn();
    render(<FilterClearButton name="Test name" onClick={handleClick} />);

    expect(handleClick).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button'));

    expect(handleClick).toHaveBeenCalled();
  });
});
