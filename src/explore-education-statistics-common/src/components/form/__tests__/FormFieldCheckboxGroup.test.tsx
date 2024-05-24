import Yup from '@common/validation/yup';
import FormFieldCheckboxGroup from '@common/components/form/FormFieldCheckboxGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FormFieldCheckboxGroup', () => {
  test('renders with correct default ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxGroup
          name="test"
          legend="Test checkboxes"
          selectAll
          options={[
            { label: 'Checkbox 1', value: '1' },
            { label: 'Checkbox 2', value: '2' },
            { label: 'Checkbox 3', value: '3' },
          ]}
        />
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'test');
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('id', 'test-1');
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute('id', 'test-2');
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute('id', 'test-3');
  });

  test('renders with correct default ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxGroup
            name="test"
            legend="Test checkboxes"
            selectAll
            options={[
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
              { label: 'Checkbox 3', value: '3' },
            ]}
          />
        </Form>
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'testForm-test');
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'testForm-test-1',
    );
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'testForm-test-2',
    );
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'testForm-test-3',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxGroup
            id="customId"
            name="test"
            legend="Test checkboxes"
            selectAll
            options={[
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
              { id: 'customOption', label: 'Checkbox 3', value: '3' },
            ]}
          />
        </Form>
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'id',
      'testForm-customId',
    );
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'testForm-customId-1',
    );
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'testForm-customId-2',
    );
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'testForm-customId-customOption',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxGroup
          id="customId"
          name="test"
          legend="Test checkboxes"
          selectAll
          options={[
            { label: 'Checkbox 1', value: '1' },
            { label: 'Checkbox 2', value: '2' },
            { id: 'customOption', label: 'Checkbox 3', value: '3' },
          ]}
        />
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'customId');
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'customId-1',
    );
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'customId-2',
    );
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'customId-customOption',
    );
  });

  test('checking option checks it', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1');

    expect(checkbox).not.toBeChecked();

    await userEvent.click(checkbox);

    expect(checkbox).toBeChecked();
  });

  test('un-checking option un-checks it', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1'],
        }}
      >
        <FormFieldCheckboxGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1');

    expect(checkbox).toBeChecked();

    await userEvent.click(checkbox);

    expect(checkbox).not.toBeChecked();
  });

  test('clicking `Select all 3 options` button checks all values', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          selectAll
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1');
    const checkbox2 = screen.getByLabelText('Checkbox 2');
    const checkbox3 = screen.getByLabelText('Checkbox 3');

    expect(checkbox1).not.toBeChecked();
    expect(checkbox2).not.toBeChecked();
    expect(checkbox3).not.toBeChecked();

    await userEvent.click(screen.getByText('Select all 3 options'));

    expect(checkbox1).toBeChecked();
    expect(checkbox2).toBeChecked();
    expect(checkbox3).toBeChecked();
  });

  test('clicking `Unselect all 3 options` button un-checks all values', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1', '2', '3'],
        }}
      >
        <FormFieldCheckboxGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          selectAll
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1');
    const checkbox2 = screen.getByLabelText('Checkbox 2');
    const checkbox3 = screen.getByLabelText('Checkbox 3');

    expect(checkbox1).toBeChecked();
    expect(checkbox2).toBeChecked();
    expect(checkbox3).toBeChecked();

    await userEvent.click(
      screen.getByRole('button', { name: 'Unselect all 3 options' }),
    );

    expect(checkbox1).not.toBeChecked();
    expect(checkbox2).not.toBeChecked();
    expect(checkbox3).not.toBeChecked();
  });

  test('checking all options renders the `Unselect all 3 options` button', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          selectAll
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1');
    const checkbox2 = screen.getByLabelText('Checkbox 2');
    const checkbox3 = screen.getByLabelText('Checkbox 3');

    expect(checkbox1).not.toBeChecked();
    expect(
      screen.getByRole('button', { name: 'Select all 3 options' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Unselect all 3 options' }),
    ).not.toBeInTheDocument();

    await userEvent.click(checkbox1);
    await userEvent.click(checkbox2);
    await userEvent.click(checkbox3);

    expect(checkbox1).toBeChecked();
    expect(checkbox2).toBeChecked();
    expect(checkbox3).toBeChecked();
    expect(
      screen.queryByRole('button', { name: 'Select all 3 options' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Unselect all 3 options' }),
    ).toBeInTheDocument();
  });

  test('un-checking any options renders the `Select all 3 options` button', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1', '2', '3'],
        }}
      >
        <FormFieldCheckboxGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          selectAll
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1');

    expect(checkbox).toBeChecked();
    expect(
      screen.queryByRole('button', { name: 'Select all 3 options' }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Unselect all 3 options' }),
    ).toBeInTheDocument();

    await userEvent.click(checkbox);

    expect(checkbox).not.toBeChecked();
    expect(
      screen.getByRole('button', { name: 'Select all 3 options' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Unselect all 3 options' }),
    ).not.toBeInTheDocument();
  });

  describe('error messages', () => {
    test('does not display validation message when checkboxes are untouched', async () => {
      render(
        <FormProvider
          initialValues={{
            test: [],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <FormFieldCheckboxGroup
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        </FormProvider>,
      );

      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();
    });

    test('displays validation message when form is submitted', async () => {
      render(
        <FormProvider
          initialValues={{
            test: [],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <Form id="testId" showErrorSummary={false} onSubmit={Promise.resolve}>
            <FormFieldCheckboxGroup
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              selectAll
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
            <button type="submit">Submit</button>
          </Form>
        </FormProvider>,
      );

      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('displays validation message when checkboxes have been touched', async () => {
      render(
        <FormProvider
          initialValues={{
            test: [],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <FormFieldCheckboxGroup
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        </FormProvider>,
      );

      await userEvent.tab();
      await userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('displays validation message when no checkboxes are checked', async () => {
      render(
        <FormProvider
          initialValues={{
            test: ['1'],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <FormFieldCheckboxGroup
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        </FormProvider>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1');

      expect(checkbox).toBeChecked();
      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      await userEvent.click(checkbox);

      expect(checkbox).not.toBeChecked();

      await userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false', async () => {
      render(
        <FormProvider
          initialValues={{
            test: ['1'],
          }}
          validationSchema={Yup.object({
            test: Yup.array().min(1, 'Select at least one option'),
          })}
        >
          <FormFieldCheckboxGroup
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            showError={false}
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        </FormProvider>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1');

      expect(checkbox).toBeChecked();
      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      await userEvent.click(checkbox);

      expect(checkbox).not.toBeChecked();

      await userEvent.tab();

      await waitFor(() => {
        expect(
          screen.queryByText('Select at least one option'),
        ).not.toBeInTheDocument();
      });
    });
  });
});
