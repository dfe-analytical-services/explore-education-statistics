import React from 'react';
import { render } from '@testing-library/react';
import FormRadio from '../FormRadio';

describe('FormRadio', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <FormRadio name="test" id="test-radio" label="Test radio" value="true" />,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly when `hint` is provided', () => {
    const { container, getByLabelText, getByText } = render(
      <FormRadio
        name="test"
        hint="Click me to proceed"
        id="test-radio"
        label="Test radio"
        value="true"
      />,
    );

    expect(getByText('Click me to proceed')).toHaveAttribute(
      'id',
      'test-radio-item-hint',
    );
    expect(getByLabelText('Test radio')).toHaveAttribute(
      'aria-describedby',
      'test-radio-item-hint',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders conditional content when checked', () => {
    const { container, getByText } = render(
      <FormRadio
        name="test"
        id="test-radio"
        label="Test radio"
        value="true"
        checked
        onChange={() => {}}
        conditional={<p>The conditional content</p>}
      />,
    );

    expect(getByText('The conditional content').parentElement).not.toHaveClass(
      'govuk-radios__conditional--hidden',
    );
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render conditional content when not checked', () => {
    const { container, getByText } = render(
      <FormRadio
        name="test"
        id="test-radio"
        label="Test radio"
        value="true"
        conditional={<p>The conditional content</p>}
      />,
    );

    expect(getByText('The conditional content').parentElement).toHaveClass(
      'govuk-radios__conditional--hidden',
    );
    expect(container.innerHTML).toMatchSnapshot();
  });
});
