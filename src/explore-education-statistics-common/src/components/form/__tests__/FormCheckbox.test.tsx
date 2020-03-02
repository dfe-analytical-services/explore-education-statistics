import React from 'react';
import { render } from '@testing-library/react';
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

  test('renders conditional content when checked', () => {
    const { container, getByText } = render(
      <FormCheckbox
        name="test"
        id="test-checkbox"
        label="Test checkbox"
        value="true"
        checked
        onChange={() => {}}
        conditional={<p>The conditional content</p>}
      />,
    );

    expect(getByText('The conditional content').parentElement).not.toHaveClass(
      'govuk-checkboxes__conditional--hidden',
    );
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render conditional content when not checked', () => {
    const { container, getByText } = render(
      <FormCheckbox
        name="test"
        id="test-checkbox"
        label="Test checkbox"
        value="true"
        conditional={<p>The conditional content</p>}
      />,
    );

    expect(getByText('The conditional content').parentElement).toHaveClass(
      'govuk-checkboxes__conditional--hidden',
    );
    expect(container.innerHTML).toMatchSnapshot();
  });
});
