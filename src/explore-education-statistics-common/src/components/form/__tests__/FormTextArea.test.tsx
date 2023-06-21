import { render } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import FormTextArea from '../FormTextArea';

describe('FormTextArea', () => {
  test('renders correctly with required props', () => {
    const { container, getByLabelText } = render(
      <FormTextArea id="test-input" label="Test input" name="testInput" />,
    );

    expect(getByLabelText('Test input')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container, getByText } = render(
      <FormTextArea
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
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
        onChange={noop}
      />,
    );

    const error = getByText('Field is required');

    expect(error.id).toBe('test-input-error');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    const { getByLabelText, getByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        onChange={noop}
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
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
        onChange={noop}
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
      <FormTextArea
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

    const ariaDescribedBy = getByLabelText('Test input').getAttribute(
      'aria-describedby',
    );

    expect(ariaDescribedBy).toContain('test-input-error');
    expect(ariaDescribedBy).toContain('test-input-hint');
  });

  test('shows a character count message when `maxLength` is above 0', () => {
    const { queryByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={10}
      />,
    );

    expect(queryByText('You have 10 characters remaining')).not.toBeNull();
  });

  test('aria-describedby contains the character count message id when `maxLength` is above 0', () => {
    const { getByLabelText, getByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={10}
      />,
    );

    const ariaDescribedBy = getByLabelText('Test input').getAttribute(
      'aria-describedby',
    );

    expect(getByText('You have 10 characters remaining')).toHaveAttribute(
      'id',
      'test-input-info',
    );
    expect(ariaDescribedBy).toContain('test-input-info');
  });

  test('does not show a character count message when `maxLength` is below 0', () => {
    const { queryByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={-1}
      />,
    );

    expect(queryByText(/You have .+ characters remaining/)).toBeNull();
  });

  test('does not show a character count message when `maxLength` is 0', () => {
    const { queryByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={0}
      />,
    );

    expect(queryByText(/You have .+ characters remaining/)).toBeNull();
  });

  test('shows correct character count message when difference to `maxLength` is 1', () => {
    const { queryByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={4}
        value="aaa"
        onChange={noop}
      />,
    );

    expect(queryByText('You have 1 character remaining')).not.toBeNull();
  });

  test('shows correct character count message when difference to `maxLength` is 0', () => {
    const { queryByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={4}
        value="aaaa"
        onChange={noop}
      />,
    );

    expect(queryByText('You have 0 characters remaining')).not.toBeNull();
  });

  test('shows correct character count message when difference to `maxLength` is -1', () => {
    const { queryByText } = render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={4}
        value="aaaaa"
        onChange={noop}
      />,
    );

    expect(queryByText('You have 1 character too many')).not.toBeNull();
  });
});
