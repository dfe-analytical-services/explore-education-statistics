import RHFFormTextArea from '@common/components/form/rhf/RHFFormTextArea';
import FormProvider from '@common/components/form/rhf/FormProvider';
import { render, screen } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('RHFFormTextArea', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <FormProvider>
        <RHFFormTextArea id="test-input" label="Test input" name="testInput" />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test input')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container } = render(
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          hint="Fill me in"
        />
      </FormProvider>,
    );

    const hint = screen.getByText('Fill me in');

    expect(hint.id).toBe('test-input-hint');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with error', () => {
    const { container } = render(
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          error="Field is required"
          onChange={noop}
        />
      </FormProvider>,
    );

    const error = screen.getByText('Field is required');

    expect(error.id).toBe('test-input-error');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    render(
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          hint="Fill me in"
          onChange={noop}
        />
      </FormProvider>,
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
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          error="Field is required"
          onChange={noop}
        />
      </FormProvider>,
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
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          hint="Fill me in"
          error="Field is required"
        />
      </FormProvider>,
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
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          maxLength={10}
        />
      </FormProvider>,
    );

    expect(
      screen.getByText('You have 10 characters remaining'),
    ).toBeInTheDocument();
  });

  test('aria-describedby contains the character count message id when `maxLength` is above 0', () => {
    render(
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          maxLength={10}
        />
      </FormProvider>,
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
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          maxLength={-1}
        />
      </FormProvider>,
    );

    expect(
      screen.queryByText(/You have .+ characters remaining/),
    ).not.toBeInTheDocument();
  });

  test('does not show a character count message when `maxLength` is 0', () => {
    render(
      <FormProvider>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          maxLength={0}
        />
      </FormProvider>,
    );

    expect(
      screen.queryByText(/You have .+ characters remaining/),
    ).not.toBeInTheDocument();
  });

  test('shows correct character count message when difference to `maxLength` is 1', () => {
    render(
      <FormProvider initialValues={{ testInput: 'aaa' }}>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          maxLength={4}
          onChange={noop}
        />
      </FormProvider>,
    );

    expect(
      screen.getByText('You have 1 character remaining'),
    ).toBeInTheDocument();
  });

  test('shows correct character count message when difference to `maxLength` is 0', () => {
    render(
      <FormProvider initialValues={{ testInput: 'aaaa' }}>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          maxLength={4}
          onChange={noop}
        />
      </FormProvider>,
    );

    expect(
      screen.getByText('You have 0 characters remaining'),
    ).toBeInTheDocument();
  });

  test('shows correct character count message when difference to `maxLength` is -1', () => {
    render(
      <FormProvider initialValues={{ testInput: 'aaaaa' }}>
        <RHFFormTextArea
          id="test-input"
          label="Test input"
          name="testInput"
          maxLength={4}
          onChange={noop}
        />
      </FormProvider>,
    );

    expect(
      screen.getByText('You have 1 character too many'),
    ).toBeInTheDocument();
  });
});
