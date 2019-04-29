import React from 'react';
import { render } from 'react-testing-library';
import FormFieldset from '../FormFieldset';

describe('FormFieldset', () => {
  test('renders correctly with required props', () => {
    const { container } = render(
      <FormFieldset id="test-fieldset" legend="Fill the form">
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    const legend = container.querySelector('legend') as HTMLLegendElement;

    expect(legend.textContent).toBe('Fill the form');
    expect(container.querySelector('#test-fieldset')).not.toBeNull();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container, getByText } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    expect(getByText('All fields are required')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with error', () => {
    const { container, getByText } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        error="There was an error"
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    expect(getByText('There was an error')).toBeDefined();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    const { container } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
      />,
    );

    expect(container.querySelector('fieldset')).toHaveAttribute(
      'aria-describedby',
      'test-fieldset-hint',
    );
  });

  test('aria-describedby is equal to the error id', () => {
    const { container } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        error="There was an error"
      />,
    );

    expect(container.querySelector('fieldset')).toHaveAttribute(
      'aria-describedby',
      'test-fieldset-error',
    );
  });

  test('aria-describedby contains both hint and error ids', () => {
    const { container } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
        error="There was an error"
      />,
    );

    const fieldset = container.querySelector('fieldset');
    const ariaDescribedBy = fieldset
      ? fieldset.getAttribute('aria-describedby')
      : '';

    expect(ariaDescribedBy).toContain('test-fieldset-error');
    expect(ariaDescribedBy).toContain('test-fieldset-hint');
  });

  test('setting `legendSize` to xl applies class correctly', () => {
    const { container } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
        legendSize="xl"
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    expect(container.querySelector('legend')).toHaveClass(
      'govuk-fieldset__legend--xl',
    );
  });

  test('setting `legendHidden` to true hides the legend', () => {
    const { container } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
        legendHidden
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    expect(container.querySelector('legend')).toHaveClass(
      'govuk-visually-hidden',
    );
  });
});
