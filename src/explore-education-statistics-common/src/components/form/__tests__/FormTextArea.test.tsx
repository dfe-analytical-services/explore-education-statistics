import FormTextArea from '@common/components/form/FormTextArea';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';
import userEvent from '@testing-library/user-event';

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

  test('shows a character count message when `maxLength` is above 0', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={10}
      />,
    );

    expect(
      screen.getByText('You have 10 characters remaining'),
    ).toBeInTheDocument();
  });

  test('aria-describedby contains the character count message id when `maxLength` is above 0', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={10}
      />,
    );

    const ariaDescribedBy = screen
      .getByLabelText('Test input')
      .getAttribute('aria-describedby');

    expect(
      screen.getByText('You have 10 characters remaining'),
    ).toHaveAttribute('id', 'test-input-info');
    expect(ariaDescribedBy).toContain('test-input-info');
  });

  test('does not show a character count message when `maxLength` is below 0', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={-1}
      />,
    );

    expect(
      screen.queryByText(/You have .+ characters remaining/),
    ).not.toBeInTheDocument();
  });

  test('does not show a character count message when `maxLength` is 0', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={0}
      />,
    );

    expect(
      screen.queryByText(/You have .+ characters remaining/),
    ).not.toBeInTheDocument();
  });

  test('shows correct character count message when difference to `maxLength` is 1', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={4}
        value="aaa"
        onChange={noop}
      />,
    );

    expect(
      screen.getByText('You have 1 character remaining'),
    ).toBeInTheDocument();
  });

  test('shows correct character count message when difference to `maxLength` is 0', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={4}
        value="aaaa"
        onChange={noop}
      />,
    );

    expect(
      screen.getByText('You have 0 characters remaining'),
    ).toBeInTheDocument();
  });

  test('shows correct character count message when difference to `maxLength` is -1', () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        maxLength={4}
        value="aaaaa"
        onChange={noop}
      />,
    );

    expect(
      screen.getByText('You have 1 character too many'),
    ).toBeInTheDocument();
  });

  test('trims on blur by default', async () => {
    render(
      <FormTextArea id="test-input" label="Test input" name="testInput" />,
    );

    await userEvent.type(screen.getByLabelText('Test input'), '  trim me  ');
    await userEvent.tab();
    expect(screen.getByLabelText('Test input')).toHaveValue('trim me');
  });

  test('does not trim on blur when `trimValue` is false', async () => {
    render(
      <FormTextArea
        id="test-input"
        label="Test input"
        name="testInput"
        trimValue={false}
      />,
    );

    await userEvent.type(
      screen.getByLabelText('Test input'),
      '   do not trim me  ',
    );
    await userEvent.tab();
    expect(screen.getByLabelText('Test input')).toHaveValue(
      '   do not trim me  ',
    );
  });
});
