import { Formik, FormikProps } from 'formik';
import React from 'react';
import { fireEvent, render, wait } from 'react-testing-library';
import Yup from 'src/lib/validation/yup';
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
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldRadioGroup<FormValues>
            name="test"
            id="radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
            ]}
            value={props.values.test}
          />
        )}
      />,
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
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
          <FormFieldRadioGroup<FormValues>
            name="test"
            id="radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
            ]}
            value={props.values.test}
          />
        )}
      />,
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
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
          render={(props: FormikProps<FormValues>) => (
            <FormFieldRadioGroup<FormValues>
              name="test"
              id="radios"
              options={[
                { id: 'radio-1', value: '1', label: 'Radio 1' },
                { id: 'radio-2', value: '2', label: 'Radio 2' },
                { id: 'radio-3', value: '3', label: 'Radio 3' },
              ]}
              value={props.values.test}
            />
          )}
        />,
      );

      expect(queryByText('Select at least one option')).toBeNull();
    });

    test('displays validation message when form is submitted', async () => {
      const { getByText, queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
          render={(props: FormikProps<FormValues>) => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldRadioGroup<FormValues>
                name="test"
                id="radios"
                options={[
                  { id: 'radio-1', value: '1', label: 'Radio 1' },
                  { id: 'radio-2', value: '2', label: 'Radio 2' },
                  { id: 'radio-3', value: '3', label: 'Radio 3' },
                ]}
                value={props.values.test}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        />,
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
          onSubmit={() => null}
          render={(props: FormikProps<FormValues>) => (
            <FormFieldRadioGroup<FormValues>
              name="test"
              id="radios"
              error="Invalid radio selection"
              options={[
                { id: 'radio-1', value: '1', label: 'Radio 1' },
                { id: 'radio-2', value: '2', label: 'Radio 2' },
                { id: 'radio-3', value: '3', label: 'Radio 3' },
              ]}
              value={props.values.test}
            />
          )}
        />,
      );

      expect(getByText('Invalid radio selection')).toBeDefined();
    });

    test('does not display validation message when `showError` is false', async () => {
      const { getByLabelText, getByText, queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
          render={(props: FormikProps<FormValues>) => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldRadioGroup<FormValues>
                name="test"
                id="radios"
                showError={false}
                options={[
                  { id: 'radio-1', value: '1', label: 'Radio 1' },
                  { id: 'radio-2', value: '2', label: 'Radio 2' },
                  { id: 'radio-3', value: '3', label: 'Radio 3' },
                ]}
                value={props.values.test}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        />,
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
