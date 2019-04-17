import React from 'react';
import { render } from 'react-testing-library';
import FormTextSearchInput from '../FormTextInput';

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
});
