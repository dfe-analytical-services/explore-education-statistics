import React from 'react';
import { render } from 'react-testing-library';
import FormFieldSet from '../FormFieldSet';

describe('FormFieldSet', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <FormFieldSet legend="Fill the form">
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldSet>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container, getByText } = render(
      <FormFieldSet
        legend="Fill the form"
        hint="All fields are required"
        hintId="the-hint"
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldSet>,
    );

    expect(getByText('All fields are required')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('auto-generates `hintId` when none is set', () => {
    const { getByText } = render(
      <FormFieldSet legend="Fill the form" hint="All fields are required">
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldSet>,
    );

    expect(getByText('All fields are required').id).toContain(
      'formFieldSetHint',
    );
  });

  test('setting `legendSize` to xl applies class correctly', () => {
    const { container } = render(
      <FormFieldSet
        legend="Fill the form"
        hint="All fields are required"
        legendSize="xl"
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldSet>,
    );

    expect(container.querySelector('legend')).toHaveClass(
      'govuk-fieldset__legend--xl',
    );
  });
});
