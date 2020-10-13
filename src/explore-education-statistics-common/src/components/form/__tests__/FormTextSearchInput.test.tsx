import React from 'react';
import { fireEvent, render } from '@testing-library/react';
import FormTextSearchInput from '../FormTextSearchInput';

jest.mock('lodash/debounce');

describe('FormTextSearchInput', () => {
  test('renders correctly with required props', () => {
    const { container, getByLabelText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
      />,
    );

    expect(getByLabelText('Test input')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container, getByText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
      />,
    );

    const hint = getByText('Fill me in');

    expect(hint.id).toBe('test-input-hint');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with error', () => {
    const { container, getByText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
      />,
    );

    const error = getByText('Field is required');

    expect(error.id).toBe('test-input-error');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    const { getByLabelText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
      />,
    );

    expect(getByLabelText('Test input')).toHaveAttribute(
      'aria-describedby',
      'test-input-hint',
    );
  });

  test('aria-describedby is equal to the error id', () => {
    const { getByLabelText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
      />,
    );

    expect(getByLabelText('Test input')).toHaveAttribute(
      'aria-describedby',
      'test-input-error',
    );
  });

  test('aria-describedby contains both hint and error ids', () => {
    const { getByLabelText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        error="Field is required"
      />,
    );

    const ariaDescribedBy = getByLabelText('Test input').getAttribute(
      'aria-describedby',
    );

    expect(ariaDescribedBy).toContain('test-input-error');
    expect(ariaDescribedBy).toContain('test-input-hint');
  });

  test('renders with a specific width class', () => {
    const { container } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        width={20}
      />,
    );

    expect(container.querySelector('.govuk-input--width-20')).not.toBeNull();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('setting `hideLabel` visually hides the label', () => {
    const { getByText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        hideLabel
        name="testInput"
        hint="Fill me in"
        width={20}
      />,
    );

    expect(getByText('Test input')).toHaveClass('govuk-visually-hidden');
  });

  test('automatically debounces the `onChange` handler', () => {
    jest.useFakeTimers();

    const handleChange = jest.fn();

    const { getByLabelText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        onChange={handleChange}
      />,
    );

    const input = getByLabelText('Test input');

    fireEvent.change(input, {
      target: {
        value: 'Value changed',
      },
    });

    expect(handleChange).not.toHaveBeenCalled();

    jest.runOnlyPendingTimers();

    expect(handleChange).toHaveBeenCalled();
  });

  test('changing `debounce` prop changes debounce time', () => {
    jest.useFakeTimers();

    const handleChange = jest.fn();

    const { getByLabelText } = render(
      <FormTextSearchInput
        id="test-input"
        label="Test input"
        name="testInput"
        onChange={handleChange}
        debounce={400}
      />,
    );

    const input = getByLabelText('Test input');

    fireEvent.change(input, {
      target: {
        value: 'Value changed',
      },
    });

    expect(handleChange).not.toHaveBeenCalled();

    jest.advanceTimersByTime(399);

    expect(handleChange).not.toHaveBeenCalled();

    jest.advanceTimersByTime(1);

    expect(handleChange).toHaveBeenCalled();
  });
});
