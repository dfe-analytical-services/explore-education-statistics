import { Form } from '@common/components/form';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik, FormikProps } from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import FormFieldSelect from '../FormFieldSelect';

describe('FormFieldSelect', () => {
  interface FormValues {
    test: string;
  }

  test('renders with correct defaults ids with form', () => {
    render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldSelect<FormValues>
            name="test"
            label="Test values"
            hint="Test hint"
            options={[
              { value: '', label: '' },
              { value: '1', label: 'Option 1' },
              { value: '2', label: 'Option 2' },
              { value: '3', label: 'Option 3' },
            ]}
          />
        </Form>
      </Formik>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-test-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'testForm-test',
    );
  });

  test('renders with correct defaults ids without form', () => {
    render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <FormFieldSelect<FormValues>
          name="test"
          label="Test values"
          hint="Test hint"
          options={[
            { value: '', label: '' },
            { value: '1', label: 'Option 1' },
            { value: '2', label: 'Option 2' },
            { value: '3', label: 'Option 3' },
          ]}
        />
      </Formik>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute('id', 'test-hint');
    expect(screen.getByLabelText('Test values')).toHaveAttribute('id', 'test');
  });

  test('renders with correct custom ids with form', () => {
    render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldSelect<FormValues>
            name="test"
            id="customId"
            label="Test values"
            hint="Test hint"
            options={[
              { value: '', label: '' },
              { value: '1', label: 'Option 1' },
              { value: '2', label: 'Option 2' },
              { value: '3', label: 'Option 3' },
            ]}
          />
        </Form>
      </Formik>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-customId-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'testForm-customId',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <FormFieldSelect<FormValues>
          name="test"
          id="customId"
          label="Test values"
          hint="Test hint"
          options={[
            { value: '', label: '' },
            { value: '1', label: 'Option 1' },
            { value: '2', label: 'Option 2' },
            { value: '3', label: 'Option 3' },
          ]}
        />
      </Formik>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'customId-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'customId',
    );
  });

  test('changing options changes the select value', async () => {
    render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldSelect<FormValues>
            name="test"
            id="select"
            label="Test values"
            options={[
              { value: '', label: '' },
              { value: '1', label: 'Option 1' },
              { value: '2', label: 'Option 2' },
              { value: '3', label: 'Option 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const select = screen.getByLabelText('Test values') as HTMLInputElement;

    expect(select.value).toBe('');

    userEvent.selectOptions(select, '1');

    await waitFor(() => {
      expect(select.value).toBe('1');
    });
  });

  describe('error messages', () => {
    test('does not display validation message when select is untouched', async () => {
      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          {() => (
            <FormFieldSelect<FormValues>
              name="test"
              id="select"
              label="Test values"
              options={[
                { value: '', label: '' },
                { value: '1', label: 'Option 1' },
                { value: '2', label: 'Option 2' },
                { value: '3', label: 'Option 3' },
              ]}
            />
          )}
        </Formik>,
      );

      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();
    });

    test('displays validation message when an invalid option is selected', async () => {
      render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          {(props: FormikProps<FormValues>) => {
            // This is super hacky, but `change` event
            // doesn't seem to trigger touched to change
            // eslint-disable-next-line no-param-reassign
            props.touched.test = true;

            return (
              <FormFieldSelect<FormValues>
                name="test"
                id="select"
                label="Test values"
                options={[
                  { value: '', label: '' },
                  { value: '1', label: 'Option 1' },
                  { value: '2', label: 'Option 2' },
                  { value: '3', label: 'Option 3' },
                ]}
              />
            );
          }}
        </Formik>,
      );

      const select = screen.getByLabelText('Test values') as HTMLInputElement;

      expect(select.value).toBe('1');
      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      userEvent.selectOptions(select, '');

      await waitFor(() => {
        expect(select.value).toBe('');
        expect(screen.queryByText('Select an option')).toBeInTheDocument();
      });
    });

    test('displays validation message when form is submitted', async () => {
      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          {props => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldSelect<FormValues>
                name="test"
                id="select"
                label="Test values"
                options={[
                  { value: '', label: '' },
                  { value: '1', label: 'Option 1' },
                  { value: '2', label: 'Option 2' },
                  { value: '3', label: 'Option 3' },
                ]}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        </Formik>,
      );

      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      userEvent.click(screen.getByText('Submit'));

      await waitFor(() => {
        expect(screen.queryByText('Select an option')).toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false and invalid option is selected', async () => {
      render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          {props => {
            // This is super hacky, but `change` event
            // doesn't seem to trigger touched to change
            // eslint-disable-next-line no-param-reassign
            props.touched.test = true;

            return (
              <FormFieldSelect<FormValues>
                name="test"
                id="select"
                label="Test values"
                showError={false}
                options={[
                  { value: '', label: '' },
                  { value: '1', label: 'Option 1' },
                  { value: '2', label: 'Option 2' },
                  { value: '3', label: 'Option 3' },
                ]}
              />
            );
          }}
        </Formik>,
      );

      const select = screen.getByLabelText('Test values') as HTMLInputElement;

      expect(select.value).toBe('1');
      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      userEvent.selectOptions(select, '');

      await waitFor(() => {
        expect(select.value).toBe('');
        expect(screen.queryByText('Select an option')).not.toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false and form is submitted', async () => {
      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          {props => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldSelect<FormValues>
                name="test"
                id="select"
                label="Test values"
                showError={false}
                options={[
                  { value: '', label: '' },
                  { value: '1', label: 'Option 1' },
                  { value: '2', label: 'Option 2' },
                  { value: '3', label: 'Option 3' },
                ]}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        </Formik>,
      );

      const select = screen.getByLabelText('Test values') as HTMLInputElement;

      expect(select.value).toBe('');
      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      userEvent.click(screen.getByText('Submit'));

      await waitFor(() => {
        expect(select.value).toBe('');
        expect(screen.queryByText('Select an option')).not.toBeInTheDocument();
      });
    });
  });
});
