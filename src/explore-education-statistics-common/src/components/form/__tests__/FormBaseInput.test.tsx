import FormBaseInput from '@common/components/form/FormBaseInput';
import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

describe('FormTextInput', () => {
  test('renders correctly with required props', () => {
    const { container, getByLabelText } = render(
      <FormBaseInput id="test-input" label="Test input" name="testInput" />,
    );

    expect(getByLabelText('Test input')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container } = render(
      <FormBaseInput
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
      <FormBaseInput
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
      />,
    );

    const error = screen.getByText('Field is required');

    expect(error.id).toBe('test-input-error');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    render(
      <FormBaseInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
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
      <FormBaseInput
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
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
      <FormBaseInput
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
    const { container } = render(
      <FormBaseInput
        addOn={<button type="button">Click me!</button>}
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        width={20}
      />,
    );

    expect(
      screen.getByRole('button', { name: 'Click me!' }),
    ).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('trims on blur by default', async () => {
    render(
      <FormBaseInput id="test-input" label="Test input" name="testInput" />,
    );

    await userEvent.type(screen.getByLabelText('Test input'), '  trim me  ');
    await userEvent.tab();
    expect(screen.getByLabelText('Test input')).toHaveValue('trim me');
  });

  test('does not trim on blur when `trimValue` is false', async () => {
    render(
      <FormBaseInput
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

  test('does not trim when enter is pressed and `trimValue` is false', async () => {
    render(
      <FormBaseInput id="test-input" label="Test input" name="testInput" />,
    );

    await userEvent.type(
      screen.getByLabelText('Test input'),
      '  trim me  {enter}',
    );
    expect(screen.getByLabelText('Test input')).toHaveValue('trim me');
  });
});
