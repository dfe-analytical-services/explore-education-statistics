import { render } from '@testing-library/react';
import React from 'react';
import FormBaseInput from '../FormBaseInput';

describe('FormTextInput', () => {
  test('renders correctly with required props', () => {
    const { container, getByLabelText } = render(
      <FormBaseInput id="test-input" label="Test input" name="testInput" />,
    );

    expect(getByLabelText('Test input')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container, getByText } = render(
      <FormBaseInput
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
      <FormBaseInput
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
    const { getByLabelText, getByText } = render(
      <FormBaseInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
      />,
    );

    expect(getByText('Fill me in')).toHaveAttribute('id', 'test-input-hint');
    expect(getByLabelText('Test input')).toHaveAttribute(
      'aria-describedby',
      'test-input-hint',
    );
  });

  test('aria-describedby is equal to the error id', () => {
    const { getByLabelText, getByText } = render(
      <FormBaseInput
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
      />,
    );

    expect(getByText('Field is required')).toHaveAttribute(
      'id',
      'test-input-error',
    );
    expect(getByLabelText('Test input')).toHaveAttribute(
      'aria-describedby',
      'test-input-error',
    );
  });

  test('aria-describedby contains both hint and error ids', () => {
    const { getByLabelText, getByText } = render(
      <FormBaseInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        error="Field is required"
      />,
    );

    expect(getByText('Fill me in')).toHaveAttribute('id', 'test-input-hint');
    expect(getByText('Field is required')).toHaveAttribute(
      'id',
      'test-input-error',
    );

    const ariaDescribedBy =
      getByLabelText('Test input').getAttribute('aria-describedby');

    expect(ariaDescribedBy).toContain('test-input-error');
    expect(ariaDescribedBy).toContain('test-input-hint');
  });

  test('renders with a specific width class', () => {
    const { container } = render(
      <FormBaseInput
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

  test('renders with an add on', () => {
    const { container, getByRole } = render(
      <FormBaseInput
        addOn={<button type="button">Click me!</button>}
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        width={20}
      />,
    );

    expect(getByRole('button', { name: 'Click me!' })).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });
});
