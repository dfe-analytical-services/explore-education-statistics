import render from '@common-test/render';
import FilterResetButton from '@frontend/components/FilterResetButton';
import noop from 'lodash/noop';
import React from 'react';
import { screen, within } from '@testing-library/react';

describe('FilterResetButton', () => {
  test('renders the button', () => {
    render(
      <FilterResetButton
        filterType="Test type"
        name="Test name"
        onClick={noop}
      />,
    );

    const button = within(screen.getByRole('button'));
    expect(button.getByText('Test type:')).toBeInTheDocument();
    expect(button.getByText('Test name')).toBeInTheDocument();
  });

  test('calls the click handler on click', async () => {
    const handleClick = jest.fn();
    const { user } = render(
      <FilterResetButton
        filterType="Test type"
        name="Test name"
        onClick={handleClick}
      />,
    );

    expect(handleClick).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button'));

    expect(handleClick).toHaveBeenCalled();
  });
});
