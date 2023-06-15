import { Form } from '@common/components/form';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import React from 'react';

jest.mock('lodash/debounce');

describe('FormFieldRadioSearchGroup', () => {
  interface FormValues {
    test: string;
  }

  test('renders with correctly without form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <FormFieldRadioSearchGroup
          id="test-group"
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

    expect(screen.getByRole('group')).toHaveAttribute('id', 'test-group');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'test-group-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'test-group-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'test-group-3',
    );
  });

  test('renders with correctly with form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: '',
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldRadioSearchGroup
            id="test-group"
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

    expect(screen.getByRole('group')).toHaveAttribute(
      'id',
      'testForm-test-group',
    );
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'testForm-test-group-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'testForm-test-group-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'testForm-test-group-3',
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
          <FormFieldRadioSearchGroup
            id="test-group"
            name="test"
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
          <FormFieldRadioSearchGroup
            id="test-group"
            name="test"
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
    const radio3 = screen.getByLabelText('Radio 3') as HTMLInputElement;

    expect(radio1.checked).toBe(true);
    expect(radio2.checked).toBe(false);
    expect(radio3.checked).toBe(false);

    fireEvent.click(radio2);

    expect(radio1.checked).toBe(false);
    expect(radio2.checked).toBe(true);
    expect(radio3.checked).toBe(false);
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
              <FormFieldRadioSearchGroup
                id="test-group"
                name="test"
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
            <FormFieldRadioSearchGroup
              id="test-group"
              name="test"
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
            <FormFieldRadioSearchGroup
              id="test-group"
              name="test"
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
              <FormFieldRadioSearchGroup
                id="test-group"
                name="test"
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

  describe('search', () => {
    test('providing a search term filters the radios', async () => {
      jest.useFakeTimers();

      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => undefined}
        >
          {() => (
            <FormFieldRadioSearchGroup
              name="test"
              id="test-group"
              legend="Test radios"
              options={[
                { id: 'radio-1', value: '1', label: 'Radio 1' },
                { id: 'radio-2', value: '2', label: 'Radio 2' },
                { id: 'radio-3', value: '3', label: 'Radio 3' },
                { id: 'radio-22', value: '22', label: 'Radio 22' },
              ]}
            />
          )}
        </Formik>,
      );

      const searchInput = screen.getByLabelText('Search') as HTMLInputElement;

      userEvent.type(searchInput, '2');

      jest.runAllTimers();
      const radios = screen.getAllByRole('radio');
      expect(radios).toHaveLength(2);
      expect(radios[0]).toHaveAttribute('value', '2');
      expect(radios[1]).toHaveAttribute('value', '22');
    });

    test('providing a search term does not remove a radio that has already been checked', async () => {
      jest.useFakeTimers();

      render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => undefined}
        >
          {() => (
            <FormFieldRadioSearchGroup
              name="test"
              id="test-group"
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

      const searchInput = screen.getByLabelText('Search') as HTMLInputElement;

      const radio1 = screen.getByLabelText('Radio 1') as HTMLInputElement;
      const radio2 = screen.getByLabelText('Radio 2') as HTMLInputElement;

      fireEvent.click(radio1);
      expect(radio1.checked).toBe(true);

      userEvent.type(searchInput, '2');

      jest.runAllTimers();
      expect(screen.getAllByRole('radio')).toHaveLength(2);
      expect(radio1.checked).toBe(true);
      expect(radio2.checked).toBe(false);
    });
  });
});
