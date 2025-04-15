import delay from '@common/utils/delay';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { render, screen } from '@testing-library/react';
import ButtonText from '../ButtonText';

describe('ButtonText', () => {
  test('renders correctly with required props', () => {
    const { container } = render(<ButtonText>Test button</ButtonText>);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('correctly sets attributes when `disabled = true`', () => {
    render(<ButtonText disabled>Test button</ButtonText>);

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toBeDisabled();
    expect(button).toBeAriaDisabled();
    expect(button).not.toHaveAttribute('aria-describedby');
  });

  test('correctly sets attributes when `ariaDisabled = true`', () => {
    render(
      <ButtonText ariaDisabled id="test-button">
        Test button
      </ButtonText>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).not.toBeDisabled();
    expect(button).toBeAriaDisabled();
  });

  test('correctly sets attributes when using both `disabled` and `ariaDisabled`', () => {
    render(
      <ButtonText disabled ariaDisabled id="test-button">
        Test button
      </ButtonText>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toBeDisabled();
    expect(button).toBeAriaDisabled();
  });

  test('calls `onClick` handler once when clicking twice, if `preventDoubleClick` is true', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(<ButtonText onClick={handleClick}>Test button</ButtonText>);

    const button = screen.getByRole('button', { name: 'Test button' });

    await userEvent.click(button);
    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  test('calls `onClick` handler twice when clicking twice, if `preventDoubleClick` is false', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(
      <ButtonText preventDoubleClick={false} onClick={handleClick}>
        Test button
      </ButtonText>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    await userEvent.click(button);
    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(2);
  });
});
