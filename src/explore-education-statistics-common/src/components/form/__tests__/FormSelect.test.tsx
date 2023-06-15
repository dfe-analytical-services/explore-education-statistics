import { render } from '@testing-library/react';
import React from 'react';
import FormSelect from '../FormSelect';

describe('FormSelect', () => {
  test('renders correctly with required props', () => {
    const { container, getByLabelText } = render(
      <FormSelect
        id="test-select"
        label="Test select"
        name="testSelect"
        options={[
          { value: 'option-1', label: 'Option 1' },
          { value: 'option-2', label: 'Option 2' },
          { value: 'option-3', label: 'Option 3' },
        ]}
      />,
    );

    expect(getByLabelText('Test select')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders list of options in alphabetical order by default', () => {
    const { getAllByText } = render(
      <FormSelect
        id="test-select"
        label="Test select"
        name="testSelect"
        options={[
          { value: 'option-2', label: 'Option 2' },
          { value: 'option-3', label: 'Option 3' },
          { value: 'option-1', label: 'Option 1' },
        ]}
      />,
    );

    const options = getAllByText(/Option/);

    expect(options[0]).toHaveTextContent('Option 1');
    expect(options[1]).toHaveTextContent('Option 2');
    expect(options[2]).toHaveTextContent('Option 3');
  });

  test('renders list of options in reverse alphabetical order', () => {
    const { getAllByText } = render(
      <FormSelect
        id="test-select"
        label="Test select"
        name="testSelect"
        orderDirection={['desc']}
        options={[
          { value: 'option-2', label: 'Option 2' },
          { value: 'option-3', label: 'Option 3' },
          { value: 'option-1', label: 'Option 1' },
        ]}
      />,
    );

    const options = getAllByText(/Option/);

    expect(options[0]).toHaveTextContent('Option 3');
    expect(options[1]).toHaveTextContent('Option 2');
    expect(options[2]).toHaveTextContent('Option 1');
  });

  test('renders list of options in custom order', () => {
    const { getAllByText } = render(
      <FormSelect
        id="test-select"
        label="Test select"
        name="testSelect"
        order={['value']}
        options={[
          { value: '1', label: 'Option 2' },
          { value: '2', label: 'Option 3' },
          { value: '3', label: 'Option 1' },
        ]}
      />,
    );

    const options = getAllByText(/Option/);

    expect(options[0]).toHaveTextContent('Option 2');
    expect(options[1]).toHaveTextContent('Option 3');
    expect(options[2]).toHaveTextContent('Option 1');
  });

  test('renders with error message', () => {
    const { container, getByText, getByLabelText } = render(
      <FormSelect
        id="test-select"
        error="Something went wrong"
        label="Test select"
        name="testSelect"
        options={[
          { value: 'option-1', label: 'Option 1' },
          { value: 'option-2', label: 'Option 2' },
          { value: 'option-3', label: 'Option 3' },
        ]}
      />,
    );

    expect(getByText('Something went wrong')).toHaveClass(
      'govuk-error-message',
    );
    expect(getByLabelText('Test select')).toHaveClass('govuk-select--error');
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    const { getByLabelText, getByText } = render(
      <FormSelect
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        options={[{ value: 'option-1', label: 'Option 1' }]}
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
      <FormSelect
        id="test-input"
        label="Test input"
        name="testInput"
        error="Field is required"
        options={[{ value: 'option-1', label: 'Option 1' }]}
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
      <FormSelect
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
        error="Field is required"
        options={[{ value: 'option-1', label: 'Option 1' }]}
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
});
