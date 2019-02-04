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
          { value: 'option-1', text: 'Option 1' },
          { value: 'option-2', text: 'Option 2' },
          { value: 'option-3', text: 'Option 3' },
        ]}
      />,
    );

    expect(getByLabelText('Test select')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with error message', () => {
    const { container, getByText, getByLabelText } = render(
      <FormSelect
        id="test-select"
        error="Something went wrong"
        label="Test select"
        name="testSelect"
        options={[
          { value: 'option-1', text: 'Option 1' },
          { value: 'option-2', text: 'Option 2' },
          { value: 'option-3', text: 'Option 3' },
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
