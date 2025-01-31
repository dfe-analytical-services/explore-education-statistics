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

    expect(button).toBeDisabled();
    expect(button).toBeAriaDisabled();
  });

  test('aria-disabled if current `onClick` handler is processing', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(<Button onClick={handleClick}>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).not.toBeAriaDisabled();
    expect(button).toBeEnabled();

    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);

    expect(button).toBeAriaDisabled();
    expect(button).toBeEnabled();
  });

  test('not aria-disabled if current `onClick` handler is processing and `preventDoubleClick` is false', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(
      <Button preventDoubleClick={false} onClick={handleClick}>
        Test button
      </Button>,
    );

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).not.toBeAriaDisabled();
    expect(button).toBeEnabled();

    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);

    expect(button).not.toBeAriaDisabled();
    expect(button).toBeEnabled();
  });

  test('not aria-disabled once the current `onClick` handler has finished', async () => {
    const handleClick = jest.fn(async () => delay(100));

    render(<Button onClick={handleClick}>Test button</Button>);

    const button = screen.getByRole('button', { name: 'Test button' });

    expect(button).not.toBeAriaDisabled();
    expect(button).toBeEnabled();

    await userEvent.click(button);

    expect(handleClick).toHaveBeenCalledTimes(1);

    expect(button).toBeAriaDisabled();
    expect(button).toBeEnabled();

    // Task has completed, so button is now enabled
    await waitFor(() => {
      expect(button).not.toBeAriaDisabled();
      expect(button).toBeEnabled();
    });
  });
});
