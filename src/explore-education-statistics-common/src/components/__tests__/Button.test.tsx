import flushPromises from '@common-test/flushPromises';
import userEvent from '@testing-library/user-event';
import React from 'react';
import { render, screen } from '@testing-library/react';
import Button from '../Button';

describe('Button', () => {
  test('renders correctly with required props', () => {
    const { container } = render(<Button>Test button</Button>);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('correctly sets disabled attributes', () => {
    render(<Button disabled>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toHaveAttribute('disabled');
    expect(button).toHaveAttribute('aria-disabled', 'true');
  });

  test('disabled if current `onClick` handler is processing', async () => {
    jest.useFakeTimers();

    const handleClick = jest.fn(
      () => new Promise(resolve => setTimeout(resolve, 2000)),
    );

    render(<Button onClick={handleClick}>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toBeEnabled();

    userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);
    expect(button).toBeDisabled();

    jest.advanceTimersByTime(1000);
    // Flushes promise queue so any state change is triggered
    await flushPromises();

    // Button is still disabled
    expect(button).toBeDisabled();

    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });

  test('enabled if current `onClick` handler is processing and `disableDoubleClick` is false', async () => {
    jest.useFakeTimers();

    const handleClick = jest.fn(
      () => new Promise(resolve => setTimeout(resolve, 2000)),
    );

    render(
      <Button disableDoubleClick={false} onClick={handleClick}>
        Test button
      </Button>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).toBeEnabled();

    userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);
    expect(button).toBeEnabled();

    jest.advanceTimersByTime(1000);
    // Flushes promise queue so any state change is triggered
    await flushPromises();

    // Button is still enabled
    expect(button).toBeEnabled();

    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });

  test('enabled once the current `onClick` handler has finished', async () => {
    jest.useFakeTimers();

    const handleClick = jest.fn(
      () => new Promise(resolve => setTimeout(resolve, 2000)),
    );

    render(<Button onClick={handleClick}>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);
    expect(button).toBeDisabled();

    jest.advanceTimersByTime(2000);
    // Flushes promise queue so any state change is triggered
    await flushPromises();

    // Task has completed, so button is now enabled
    expect(button).toBeEnabled();

    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });
});
