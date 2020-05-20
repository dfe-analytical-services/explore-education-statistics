import { fireEvent, render, wait } from '@testing-library/react';
import { Formik } from 'formik';
import React from 'react';
import FormFieldCheckboxGroupsMenu from '../FormFieldCheckboxGroupsMenu';

describe('FormFieldCheckboxGroupsMenu', () => {
  test('renders multiple checkbox groups in correct order with search input', () => {
    const { container, getAllByLabelText, queryByLabelText } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={() => undefined}
      >
        <FormFieldCheckboxGroupsMenu
          id="test"
          name="test"
          legend="Choose options"
          options={[
            {
              legend: 'Group A',
              options: [
                { label: 'Option 1', value: '1' },
                { label: 'Option 2', value: '2' },
              ],
            },
            {
              legend: 'Group B',
              options: [
                { label: 'Option 3', value: '3' },
                { label: 'Option 4', value: '4' },
              ],
            },
          ]}
        />
      </Formik>,
    );

    expect(queryByLabelText('Search options')).not.toBeNull();

    const checkboxes = getAllByLabelText(/Option/);

    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');
    expect(checkboxes[3]).toHaveAttribute('value', '4');

    expect(container.querySelector('#test')).toMatchSnapshot();
  });

  test('renders single checkbox group with search input', () => {
    const { container, getAllByLabelText, queryByLabelText } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={() => undefined}
      >
        <FormFieldCheckboxGroupsMenu
          id="test"
          name="test"
          legend="Choose options"
          options={[
            {
              legend: 'Group A',
              options: [
                { label: 'Option 1', value: '1' },
                { label: 'Option 2', value: '2' },
              ],
            },
          ]}
        />
      </Formik>,
    );

    expect(queryByLabelText('Search options')).not.toBeNull();

    const checkboxes = getAllByLabelText(/Option/);

    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');

    expect(container.querySelector('#test')).toMatchSnapshot();
  });

  test('renders single checkbox group with single checkbox option and no search input', () => {
    const { container, queryByLabelText } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={() => undefined}
      >
        <FormFieldCheckboxGroupsMenu
          id="test"
          name="test"
          legend="Choose options"
          options={[
            {
              legend: 'Group A',
              options: [{ label: 'Option 1', value: '1' }],
            },
          ]}
        />
      </Formik>,
    );

    expect(queryByLabelText('Search options')).toBeNull();

    expect(container.querySelector('#test')).toMatchSnapshot();
  });

  test('menu contents is expanded if there is a field error', async () => {
    const { container } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        initialErrors={{
          test: 'There is an error',
        }}
        initialTouched={{
          test: true,
        }}
        onSubmit={() => undefined}
      >
        <FormFieldCheckboxGroupsMenu
          id="test"
          name="test"
          legend="Choose options"
          options={[
            {
              legend: 'Group A',
              options: [
                { label: 'Option 1', value: '1' },
                { label: 'Option 2', value: '2' },
              ],
            },
          ]}
        />
      </Formik>,
    );

    await wait();

    const summary = container.querySelector('summary') as HTMLElement;

    expect(summary).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking menu does not collapse it if there is a field error', async () => {
    const { container } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        initialErrors={{
          test: 'There is an error',
        }}
        initialTouched={{
          test: true,
        }}
        onSubmit={() => undefined}
      >
        <FormFieldCheckboxGroupsMenu
          id="test"
          name="test"
          legend="Choose options"
          options={[
            {
              legend: 'Group A',
              options: [
                { label: 'Option 1', value: '1' },
                { label: 'Option 2', value: '2' },
              ],
            },
          ]}
        />
      </Formik>,
    );

    await wait();

    const summary = container.querySelector('summary') as HTMLElement;

    expect(summary).toHaveAttribute('aria-expanded', 'true');

    fireEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
  });
});
