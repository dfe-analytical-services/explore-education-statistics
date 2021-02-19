import { Form } from '@common/components/form';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import FormFieldCheckboxGroup from '../FormFieldCheckboxGroup';

describe('FormFieldCheckboxGroup', () => {
  interface FormValues {
    test: string[];
  }

  test('renders with correct default ids without form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <FormFieldCheckboxGroup<FormValues>
          name="test"
          legend="Test checkboxes"
          selectAll
          options={[
            { label: 'Checkbox 1', value: '1' },
            { label: 'Checkbox 2', value: '2' },
            { label: 'Checkbox 3', value: '3' },
          ]}
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'test');
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('id', 'test-1');
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute('id', 'test-2');
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute('id', 'test-3');
  });

  test('renders with correct default ids with form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldCheckboxGroup<FormValues>
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
      </Formik>,
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
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldCheckboxGroup<FormValues>
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
      </Formik>,
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
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <FormFieldCheckboxGroup<FormValues>
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
      </Formik>,
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

  test('checking option checks it', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    userEvent.click(checkbox);

    expect(checkbox.checked).toBe(true);
  });

  test('un-checking option un-checks it', () => {
    render(
      <Formik
        initialValues={{
          test: ['1'],
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    userEvent.click(checkbox);

    expect(checkbox.checked).toBe(false);
  });

  test('clicking `Select all 3 options` button checks all values', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldCheckboxGroup<FormValues>
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
        )}
      </Formik>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);

    userEvent.click(screen.getByText('Select all 3 options'));

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);
  });

  test('clicking `Unselect all 3 options` button un-checks all values', () => {
    render(
      <Formik
        initialValues={{
          test: ['1', '2', '3'],
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldCheckboxGroup<FormValues>
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
        )}
      </Formik>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    userEvent.click(
      screen.getByRole('button', { name: 'Unselect all 3 options' }),
    );

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
  });

  test('checking all options renders the `Unselect all 3 options` button', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              { id: 'checkbox-1', label: 'Checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Checkbox 3', value: '3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(
      screen.getByRole('button', { name: 'Select all 3 options' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Unselect all 3 options' }),
    ).toBeNull();

    userEvent.click(checkbox1);
    userEvent.click(checkbox2);
    userEvent.click(checkbox3);

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);
    expect(
      screen.queryByRole('button', { name: 'Select all 3 options' }),
    ).toBeNull();
    expect(
      screen.getByRole('button', { name: 'Unselect all 3 options' }),
    ).toBeInTheDocument();
  });

  test('un-checking any options renders the `Select all 3 options` button', () => {
    render(
      <Formik
        initialValues={{
          test: ['1', '2', '3'],
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldCheckboxGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              { id: 'checkbox-1', label: 'Checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Checkbox 3', value: '3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);
    expect(
      screen.queryByRole('button', { name: 'Select all 3 options' }),
    ).toBeNull();
    expect(
      screen.getByRole('button', { name: 'Unselect all 3 options' }),
    ).toBeInTheDocument();

    userEvent.click(checkbox);

    expect(checkbox.checked).toBe(false);
    expect(
      screen.getByRole('button', { name: 'Select all 3 options' }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: 'Unselect all 3 options' }),
    ).toBeNull();
  });

  describe('error messages', () => {
    test('does not display validation message when checkboxes are untouched', async () => {
      render(
        <Formik
          initialValues={{
            test: [],
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      expect(screen.queryByText('Select at least one option')).toBeNull();
    });

    test('displays validation message when form is submitted', async () => {
      render(
        <Formik
          initialValues={{
            test: [],
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {props => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldCheckboxGroup<FormValues>
                name="test"
                id="radios"
                legend="Test radios"
                options={[
                  { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                  { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                  { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
                ]}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        </Formik>,
      );

      expect(screen.queryByText('Select at least one option')).toBeNull();

      userEvent.click(screen.getByRole('button', { name: 'Submit' }));

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('displays validation message when checkboxes have been touched', async () => {
      render(
        <Formik
          initialValues={{
            test: [],
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      userEvent.tab();
      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('displays validation message when checkbox has been unchecked', async () => {
      render(
        <Formik
          initialValues={{
            test: [],
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      userEvent.tab();

      userEvent.click(checkbox);
      userEvent.click(checkbox);

      userEvent.tab();

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('displays validation message when no checkboxes are checked', async () => {
      render(
        <Formik
          initialValues={{
            test: ['1'],
          }}
          initialTouched={{
            test: true,
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(screen.queryByText('Select at least one option')).toBeNull();

      userEvent.click(checkbox);

      expect(checkbox.checked).toBe(false);

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false', async () => {
      render(
        <Formik
          initialValues={{
            test: ['1'],
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              showError={false}
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(screen.queryByText('Select at least one option')).toBeNull();

      userEvent.click(checkbox);

      expect(checkbox.checked).toBe(false);

      await waitFor(() => {
        expect(screen.queryByText('Select at least one option')).toBeNull();
      });
    });
  });
});
