import Yup from '@common/validation/yup';
import { fireEvent, render, wait } from '@testing-library/react';
import { Formik, FormikProps } from 'formik';
import React from 'react';
import FormFieldRadioGroup from '../FormFieldRadioGroup';

describe('FormFieldRadioGroup', () => {
  interface FormValues {
    test: string;
  }

  test('checking an option checks it', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={() => undefined}
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

    const radio = getByLabelText('Radio 1') as HTMLInputElement;

    expect(radio.checked).toBe(false);

    fireEvent.click(radio);

    await wait();

    expect(radio.checked).toBe(true);
  });

  test('checking another option un-checks the currently checked option', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: '1',
        }}
        onSubmit={() => undefined}
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

    const radio1 = getByLabelText('Radio 1') as HTMLInputElement;
    const radio2 = getByLabelText('Radio 2') as HTMLInputElement;

    expect(radio1.checked).toBe(true);
    expect(radio2.checked).toBe(false);

    fireEvent.click(radio2);

    await wait();

    expect(radio1.checked).toBe(false);
    expect(radio2.checked).toBe(true);
  });

  describe('error messages', () => {
    test('does not display validation message when radios are untouched', async () => {
      const { queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => undefined}
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

      expect(queryByText('Select at least one option')).toBeNull();
    });

    test('displays validation message when form is submitted', async () => {
      const { getByText, queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => undefined}
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

      expect(queryByText('Select at least one option')).toBeNull();

      fireEvent.click(getByText('Submit'));

      await wait();

      expect(queryByText('Select at least one option')).not.toBeNull();
    });

    test('displays custom error message from `error` prop', async () => {
      const { getByText } = render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={() => undefined}
        >
          {() => (
            <FormFieldRadioGroup<FormValues>
              name="test"
              id="radios"
              legend="Test radios"
              error="Invalid radio selection"
              options={[
                { id: 'radio-1', value: '1', label: 'Radio 1' },
                { id: 'radio-2', value: '2', label: 'Radio 2' },
                { id: 'radio-3', value: '3', label: 'Radio 3' },
              ]}
            />
          )}
        </Formik>,
      );

      expect(getByText('Invalid radio selection')).toBeDefined();
    });

    test('does not display validation message when `showError` is false', async () => {
      const { getByLabelText, getByText, queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => undefined}
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

      const radio = getByLabelText('Radio 1') as HTMLInputElement;

      expect(radio.checked).toBe(false);
      expect(queryByText('Select at least one option')).toBeNull();

      fireEvent.click(getByText('Submit'));

      await wait();

      expect(radio.checked).toBe(false);
      expect(queryByText('Select at least one option')).toBeNull();
    });
  });
});
