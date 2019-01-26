import React from 'react';
import { render } from 'react-testing-library';
import { FormTextInput } from '../FormTextInput';

describe('FormTextInput', () => {
  test('renders correctly with required props', () => {
    const { container, getByLabelText } = render(
      <FormTextInput id="test-input" label="Test input" name="testInput" />,
    );

    expect(getByLabelText('Test input')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container, getByText } = render(
      <FormTextInput
        id="test-input"
        label="Test input"
        name="testInput"
        hint="Fill me in"
      />,
    );

    expect(getByText('Fill me in')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with a specific width class', () => {
    const { container } = render(
      <FormTextInput
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
});
