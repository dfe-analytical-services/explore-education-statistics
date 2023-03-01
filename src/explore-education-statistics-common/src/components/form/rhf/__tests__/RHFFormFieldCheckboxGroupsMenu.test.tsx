import RHFFormFieldCheckboxGroupsMenu from '@common/components/form/rhf/RHFFormFieldCheckboxGroupsMenu';
import RHFForm from '@common/components/form/rhf/RHFForm';
import FormProvider from '@common/components/form/rhf/FormProvider';
import Yup from '@common/validation/yup';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FormFieldCheckboxGroupsMenu', () => {
  test('renders multiple checkbox groups in correct order with search input', () => {
    const { container } = render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFFormFieldCheckboxGroupsMenu
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
      </FormProvider>,
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
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFFormFieldCheckboxGroupsMenu
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
      </FormProvider>,
    );

    expect(screen.queryByLabelText('Search options')).not.toBeNull();

    const checkboxes = screen.getAllByLabelText(/Option/);

    expect(checkboxes[0]).toHaveAttribute('value', '1');
    expect(checkboxes[1]).toHaveAttribute('value', '2');

    expect(container.querySelector('#test')).toMatchSnapshot();
  });

  test('renders single checkbox group with single checkbox option and no search input', () => {
    const { container, queryByLabelText } = render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFFormFieldCheckboxGroupsMenu
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
      </FormProvider>,
    );

    expect(queryByLabelText('Search options')).toBeNull();

    expect(container.querySelector('#test')).toMatchSnapshot();
  });

  test('menu contents is expanded if there is a field error', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
        validationSchema={Yup.object({
          test: Yup.array().required('Select at least one option'),
        })}
      >
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldCheckboxGroupsMenu
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
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    expect(
      screen.getByRole('button', { name: 'Choose options' }),
    ).not.toHaveAttribute('aria-expanded', 'true');

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Option 1')).toBeVisible();
    });

    expect(
      screen.getByRole('button', { name: 'Choose options' }),
    ).toHaveAttribute('aria-expanded', 'true');

    expect(screen.getByRole('group', { name: 'Choose options' })).toBeVisible();
  });

  test('clicking menu does not collapse it if there is a field error', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
        validationSchema={Yup.object({
          test: Yup.array().required('Select at least one option'),
        })}
      >
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldCheckboxGroupsMenu
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
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    const summary = screen.getByRole('button', { name: 'Choose options' });

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Option 1')).toBeVisible();
    });

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('group', { name: 'Choose options' })).toBeVisible();

    userEvent.click(summary);

    expect(summary).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('group', { name: 'Choose options' })).toBeVisible();
  });
});
