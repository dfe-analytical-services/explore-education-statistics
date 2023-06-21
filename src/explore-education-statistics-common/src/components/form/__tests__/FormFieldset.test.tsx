import { getAllDescribedBy } from '@common-test/queries';
import { FormIdContextProvider } from '@common/components/form/contexts/FormIdContext';
import React from 'react';
import { render, screen } from '@testing-library/react';
import FormFieldset from '../FormFieldset';

describe('FormFieldset', () => {
  test('renders correctly with required props', () => {
    render(
      <FormFieldset id="test-fieldset" legend="Fill the form">
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    const legend = screen.getByRole('group', { name: 'Fill the form' });
    expect(legend).toBeInTheDocument();
    expect(legend.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with hint', () => {
    const { container } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    expect(screen.getByText('All fields are required')).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders correctly with error', () => {
    const { container } = render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        error="There was an error"
      >
        <input type="text" name="test-input-1" />
        <input type="text" name="test-input-2" />
      </FormFieldset>,
    );

    expect(screen.getByText('There was an error')).toBeInTheDocument();
    expect(container.innerHTML).toMatchSnapshot();
  });

  test('aria-describedby is equal to the hint id', () => {
    render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
      />,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'aria-describedby',
      'test-fieldset-hint',
    );
  });

  test('aria-describedby is equal to the error id', () => {
    render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        error="There was an error"
      />,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'aria-describedby',
      'test-fieldset-error',
    );
  });

  test('aria-describedby contains both hint and error ids', () => {
    render(
      <FormFieldset
        id="test-fieldset"
        legend="Fill the form"
        hint="All fields are required"
        error="There was an error"
      />,
    );

    const group = screen.getByRole('group');
    expect(group).toHaveAttribute('id', 'test-fieldset');

    const describingElements = getAllDescribedBy(group);

    expect(describingElements).toHaveLength(2);
    expect(describingElements[0]).toHaveAttribute('id', 'test-fieldset-error');
    expect(describingElements[0]).toHaveTextContent('There was an error');

    expect(describingElements[1]).toHaveAttribute('id', 'test-fieldset-hint');
    expect(describingElements[1]).toHaveTextContent('All fields are required');
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

  test('renders with correct ids with form context', () => {
    render(
      <FormIdContextProvider id="testForm">
        <FormFieldset
          id="test-fieldset"
          legend="Fill the form"
          hint="All fields are required"
          error="There was an error"
          legendHidden
        />
      </FormIdContextProvider>,
    );

    const group = screen.getByRole('group');
    expect(group).toHaveAttribute('id', 'testForm-test-fieldset');

    const describingElements = getAllDescribedBy(group);

    expect(describingElements).toHaveLength(2);
    expect(describingElements[0]).toHaveAttribute(
      'id',
      'testForm-test-fieldset-error',
    );
    expect(describingElements[0]).toHaveTextContent('There was an error');

    expect(describingElements[1]).toHaveAttribute(
      'id',
      'testForm-test-fieldset-hint',
    );
    expect(describingElements[1]).toHaveTextContent('All fields are required');
  });
});
