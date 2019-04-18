import React from 'react';
import { render } from 'react-testing-library';
import FormCheckbox from '../FormCheckbox';

describe('FormCheckbox', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <FormCheckbox
        name="test"
        id="test-checkbox"
        label="Test checkbox"
        value="true"
      />,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly when `hint` is provided', () => {
    const { container, getByLabelText, getByText } = render(
      <FormCheckbox
        name="test"
        hint="Click me to proceed"
        id="test-checkbox"
        label="Test checkbox"
        value="true"
      />,
    );

    expect(getByText('Click me to proceed')).toHaveAttribute(
      'id',
      'test-checkbox-item-hint',
    );
    expect(getByLabelText('Test checkbox')).toHaveAttribute(
      'aria-describedby',
      'test-checkbox-item-hint',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });
});
