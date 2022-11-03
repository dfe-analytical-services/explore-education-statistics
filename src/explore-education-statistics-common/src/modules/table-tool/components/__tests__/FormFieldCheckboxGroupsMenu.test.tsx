import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import FormFieldCheckboxGroupsMenu from '../FormFieldCheckboxGroupsMenu';

describe('FormFieldCheckboxGroupsMenu', () => {
  test('renders multiple checkbox groups in correct order with search input', () => {
    const { container } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
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

    expect(screen.queryByLabelText('Search options')).not.toBeNull();

    const checkboxes = screen.getAllByLabelText(/Option/);

    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');
    expect(checkboxes[2]).toHaveAttribute('value', '3');
    expect(checkboxes[3]).toHaveAttribute('value', '4');

    expect(container.querySelector('#test')).toMatchSnapshot();
  });

  test('renders single checkbox group with search input', () => {
    const { container } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
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

    expect(screen.queryByLabelText('Search options')).not.toBeNull();

    const checkboxes = screen.getAllByLabelText(/Option/);

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
        onSubmit={noop}
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
    render(
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
        onSubmit={noop}
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

    expect(
      screen.getByRole('button', { name: 'Choose options' }),
    ).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('group', { name: 'Choose options' })).toBeVisible();
  });

  test('clicking menu does not collapse it if there is a field error', async () => {
    render(
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
        onSubmit={noop}
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

    const summary = screen.getByRole('button', { name: 'Choose options' });

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('group', { name: 'Choose options' })).toBeVisible();

    userEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('group', { name: 'Choose options' })).toBeVisible();
  });
});
