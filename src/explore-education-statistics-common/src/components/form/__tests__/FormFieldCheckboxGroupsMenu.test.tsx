import FormFieldCheckboxGroupsMenu from '@common/components/form/FormFieldCheckboxGroupsMenu';
import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import Yup from '@common/validation/yup';
import { screen } from '@testing-library/react';
import React from 'react';
import render from '@common-test/render';

describe('FormFieldCheckboxGroupsMenu', () => {
  test('renders multiple checkbox groups in correct order with search input', () => {
    const { container } = render(
      <FormProvider
        initialValues={{
          test: '',
        }}
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
      </FormProvider>,
    );

    expect(queryByLabelText('Search options')).toBeNull();

    expect(container.querySelector('#test')).toMatchSnapshot();
  });

  test('menu contents is expanded if there is a field error', async () => {
    const { user } = render(
      <FormProvider
        initialValues={{
          test: [],
        }}
        validationSchema={Yup.object({
          test: Yup.array().min(1, 'Select at least one option'),
        })}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
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
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    expect(
      screen.getByRole('button', { name: 'Choose options - show options' }),
    ).toHaveAttribute('aria-expanded', 'false');

    await user.click(screen.getByRole('button', { name: 'Submit' }));

    expect(await screen.findByText('Option 1')).toBeVisible();

    expect(
      await screen.findByRole('button', {
        name: 'Choose options - hide options',
      }),
    ).toHaveAttribute('aria-expanded', 'true');
  });

  test('clicking menu does not collapse it if there is a field error', async () => {
    const { user } = render(
      <FormProvider
        initialValues={{
          test: [],
        }}
        validationSchema={Yup.object({
          test: Yup.array().min(1, 'Select at least one option'),
        })}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
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
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    await user.click(screen.getByRole('button', { name: 'Submit' }));

    expect(await screen.findByText('Option 1')).toBeVisible();

    const button = await screen.findByRole('button', {
      name: 'Choose options - hide options',
    });

    expect(button).toHaveAttribute('aria-expanded', 'true');

    await user.click(button);

    expect(button).toHaveAttribute('aria-expanded', 'true');
  });
});
