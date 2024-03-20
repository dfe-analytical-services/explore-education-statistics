import delay from '@common/utils/delay';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import Button from '../Button';

describe('Button', () => {
  test('renders correctly with required props', () => {
    const { container } = render(<Button>Test button</Button>);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('correctly sets attributes when `disabled = true`', () => {
    render(<Button disabled>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toBeDisabled();
    expect(button).toBeAriaDisabled();
    expect(button).not.toHaveAttribute('aria-describedby');
  });

  test('correctly sets attributes when `ariaDisabled = true`', () => {
    render(
      <Button ariaDisabled id="test-button">
        Test button
      </Button>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).not.toBeDisabled();
    expect(button).toBeAriaDisabled();
  });

  test('correctly sets attributes when using both `disabled` and `ariaDisabled`', () => {
    render(
      <Button disabled ariaDisabled id="test-button">
        Test button
      </Button>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).not.toBeDisabled();
    expect(button).toBeAriaDisabled();
  });

  test('disabled if current `onClick` handler is processing', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(<Button onClick={handleClick}>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toBeEnabled();

    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);
    expect(button).toBeDisabled();

    // Button is still disabled
    expect(button).toBeDisabled();
  });

  test('enabled if current `onClick` handler is processing and `disableDoubleClick` is false', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(
      <Button disableDoubleClick={false} onClick={handleClick}>
        Test button
      </Button>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toBeEnabled();

    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);
    expect(button).toBeEnabled();

    // Button is still enabled
    expect(button).toBeEnabled();
  });

  test('enabled once the current `onClick` handler has finished', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(<Button onClick={handleClick}>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);
    expect(button).toBeDisabled();

    // Task has completed, so button is now enabled
    await waitFor(() => {
      expect(button).toBeEnabled();
    });
  });
});
