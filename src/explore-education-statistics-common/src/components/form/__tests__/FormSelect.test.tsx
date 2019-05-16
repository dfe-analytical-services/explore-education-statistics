import React from 'react';
import { render } from 'react-testing-library';
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
});
