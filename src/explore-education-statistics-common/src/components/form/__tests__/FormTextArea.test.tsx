import FormTextArea from '@common/components/form/FormTextArea';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('FormTextArea', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <FormTextArea id="test-input" label="Test input" name="testInput" />,
    );

    expect(screen.getByLabelText('Test input')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
      />,
    );

    const hint = screen.getByText('Fill me in');

    expect(hint.id).toBe('test-input-hint');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with error', () => {
    const { container } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
        onChange={noop}
      />,
    );

    const error = screen.getByText('Field is required');

    expect(error.id).toBe('test-input-error');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        onChange={noop}
      />,
    );

    expect(screen.getByText('Fill me in')).toHaveAttribute(
      'id',
      'test-input-hint',
    );
    expect(screen.getByLabelText('Test input')).toHaveAttribute(
      'aria-describedby',
      'test-input-hint',
    );
  });

  test('aria-describedby is equal to the error id', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
        onChange={noop}
      />,
    );

    expect(screen.getByText('Field is required')).toHaveAttribute(
      'id',
      'test-input-error',
    );
    expect(screen.getByLabelText('Test input')).toHaveAttribute(
      'aria-describedby',
      'test-input-error',
    );
  });

  test('aria-describedby contains both hint and error ids', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        error="Field is required"
      />,
    );

    expect(screen.getByText('Fill me in')).toHaveAttribute(
      'id',
      'test-input-hint',
    );
    expect(screen.getByText('Field is required')).toHaveAttribute(
      'id',
      'test-input-error',
    );

    const ariaDescribedBy = screen
      .getByLabelText('Test input')
      .getAttribute('aria-describedby');

    expect(ariaDescribedBy).toContain('test-input-error');
    expect(ariaDescribedBy).toContain('test-input-hint');
  });
});
