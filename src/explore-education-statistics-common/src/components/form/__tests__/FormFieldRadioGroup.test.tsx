import { Form } from '@common/components/form';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import FormFieldRadioGroup from '../FormFieldRadioGroup';

describe('FormFieldRadioGroup', () => {
  interface FormValues {
    test: string;
  }

  test('renders with correct default ids without form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <FormFieldRadioGroup<FormValues>
          name="test"
          legend="Test radios"
          options={[
            { value: '1', label: 'Radio 1' },
            { value: '2', label: 'Radio 2' },
            { value: '3', label: 'Radio 3' },
          ]}
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'test');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute('id', 'test-1');
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute('id', 'test-2');
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute('id', 'test-3');
  });

  test('renders with correct default ids with form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldRadioGroup<FormValues>
            name="test"
            legend="Test radios"
            options={[
              { value: '1', label: 'Radio 1' },
              { value: '2', label: 'Radio 2' },
              { value: '3', label: 'Radio 3' },
            ]}
          />
        </Form>
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'testForm-test');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'testForm-test-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'testForm-test-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'testForm-test-3',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldRadioGroup<FormValues>
            name="test"
            id="customId"
            legend="Test radios"
            options={[
              { value: '1', label: 'Radio 1' },
              { value: '2', label: 'Radio 2' },
              { id: 'customOption', value: '3', label: 'Radio 3' },
            ]}
          />
        </Form>
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'id',
      'testForm-customId',
    );
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'testForm-customId-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'testForm-customId-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'testForm-customId-customOption',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <FormFieldRadioGroup<FormValues>
          name="test"
          id="customId"
          legend="Test radios"
          options={[
            { value: '1', label: 'Radio 1' },
            { value: '2', label: 'Radio 2' },
            { id: 'customOption', value: '3', label: 'Radio 3' },
          ]}
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'customId');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'customId-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'customId-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'customId-customOption',
    );
  });

  test('checking an option checks it', async () => {
    render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldRadioGroup<FormValues>
            name="test"
            id="radios"
            legend="Test radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const radio = screen.getByLabelText('Radio 1') as HTMLInputElement;

    expect(radio.checked).toBe(false);

    fireEvent.click(radio);

    expect(radio.checked).toBe(true);
  });

  test('checking another option un-checks the currently checked option', async () => {
    render(
      <Formik
        initialValues={{
          test: '1',
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldRadioGroup<FormValues>
            name="test"
            id="radios"
            legend="Test radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const radio1 = screen.getByLabelText('Radio 1') as HTMLInputElement;
    const radio2 = screen.getByLabelText('Radio 2') as HTMLInputElement;

    expect(radio1.checked).toBe(true);
    expect(radio2.checked).toBe(false);

    fireEvent.click(radio2);

    expect(radio1.checked).toBe(false);
    expect(radio2.checked).toBe(true);
  });

  describe('error messages', () => {
    test('displays validation message when form is submitted', async () => {
      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {props => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldRadioGroup<FormValues>
                name="test"
                id="radios"
                legend="Test radios"
                options={[
                  { id: 'radio-1', value: '1', label: 'Radio 1' },
                  { id: 'radio-2', value: '2', label: 'Radio 2' },
                  { id: 'radio-3', value: '3', label: 'Radio 3' },
                ]}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        </Formik>,
      );

      expect(screen.queryByText('Select at least one option')).toBeNull();

      fireEvent.click(screen.getByRole('button', { name: 'Submit' }));

      await waitFor(() => {
        expect(
          screen.getByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('displays validation message when radios has been touched', async () => {
      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldRadioGroup<FormValues>
              name="test"
              id="radios"
              legend="Test radios"
              options={[
                { id: 'radio-1', value: '1', label: 'Radio 1' },
                { id: 'radio-2', value: '2', label: 'Radio 2' },
                { id: 'radio-3', value: '3', label: 'Radio 3' },
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

    test('does not display validation message when radios are untouched', async () => {
      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldRadioGroup<FormValues>
              name="test"
              id="radios"
              legend="Test radios"
              options={[
                { id: 'radio-1', value: '1', label: 'Radio 1' },
                { id: 'radio-2', value: '2', label: 'Radio 2' },
                { id: 'radio-3', value: '3', label: 'Radio 3' },
              ]}
            />
          )}
        </Formik>,
      );

      expect(screen.queryByText('Select at least one option')).toBeNull();
    });

    test('does not display validation message when `showError` is false', async () => {
      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {props => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldRadioGroup<FormValues>
                name="test"
                id="radios"
                legend="Test radios"
                showError={false}
                options={[
                  { id: 'radio-1', value: '1', label: 'Radio 1' },
                  { id: 'radio-2', value: '2', label: 'Radio 2' },
                  { id: 'radio-3', value: '3', label: 'Radio 3' },
                ]}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        </Formik>,
      );

      const radio = screen.getByLabelText('Radio 1') as HTMLInputElement;

      expect(radio.checked).toBe(false);
      expect(screen.queryByText('Select at least one option')).toBeNull();

      fireEvent.click(screen.getByRole('button', { name: 'Submit' }));

      expect(radio.checked).toBe(false);
      expect(screen.queryByText('Select at least one option')).toBeNull();
    });
  });
});
