import { Formik, FormikProps } from 'formik';
import React from 'react';
import { fireEvent, render, wait } from 'react-testing-library';
import Yup from 'src/lib/validation/yup';
import FormFieldSelect from '../FormFieldSelect';

describe('FormFieldSelect', () => {
  interface FormValues {
    test: string;
  }

  test('changing options changes the select value', async () => {
    const { getByLabelText } = render(
      <Formik
        initialValues={{
          test: '',
        }}
        onSubmit={() => null}
        render={(props: FormikProps<FormValues>) => (
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
            value={props.values.test}
          />
        )}
      />,
    );

    const select = getByLabelText('Test values') as HTMLInputElement;

    expect(select.value).toBe('');

    fireEvent.change(select, {
      target: {
        value: '1',
      },
    });

    await wait();

    expect(select.value).toBe('1');
  });

  describe('error messages', () => {
    test('does not display validation message when select is untouched', async () => {
      const { queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
          render={(props: FormikProps<FormValues>) => (
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
              value={props.values.test}
            />
          )}
        />,
      );

      expect(queryByText('Select an option')).toBeNull();
    });

    test('displays validation message when an invalid option is selected', async () => {
      const { getByLabelText, queryByText } = render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
          render={(props: FormikProps<FormValues>) => {
            // This is super hacky, but `change` event
            // doesn't seem to trigger touched to change
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
                value={props.values.test}
              />
            );
          }}
        />,
      );

      const select = getByLabelText('Test values') as HTMLInputElement;

      expect(select.value).toBe('1');
      expect(queryByText('Select an option')).toBeNull();

      fireEvent.change(select, {
        target: {
          value: '',
        },
      });

      await wait();

      expect(select.value).toBe('');
      expect(queryByText('Select an option')).not.toBeNull();
    });

    test('displays validation message when form is submitted', async () => {
      const { getByText, queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
          render={(props: FormikProps<FormValues>) => (
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
                value={props.values.test}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        />,
      );

      expect(queryByText('Select an option')).toBeNull();

      fireEvent.click(getByText('Submit'));

      await wait();

      expect(queryByText('Select an option')).not.toBeNull();
    });

    test('displays custom error message from `error` prop', async () => {
      const { getByText } = render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={() => null}
          render={(props: FormikProps<FormValues>) => (
            <FormFieldSelect<FormValues>
              name="test"
              id="select"
              label="Test values"
              error="Invalid option"
              options={[
                { value: '1', label: 'Option 1' },
                { value: '2', label: 'Option 2' },
                { value: '3', label: 'Option 3' },
              ]}
              value={props.values.test}
            />
          )}
        />,
      );

      expect(getByText('Invalid option')).toBeDefined();
    });

    test('does not display validation message when `showError` is false and invalid option is selected', async () => {
      const { getByLabelText, queryByText } = render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
          render={(props: FormikProps<FormValues>) => {
            // This is super hacky, but `change` event
            // doesn't seem to trigger touched to change
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
                value={props.values.test}
              />
            );
          }}
        />,
      );

      const select = getByLabelText('Test values') as HTMLInputElement;

      expect(select.value).toBe('1');
      expect(queryByText('Select an option')).toBeNull();

      fireEvent.change(select, {
        target: {
          value: '',
        },
      });

      await wait();

      expect(select.value).toBe('');
      expect(queryByText('Select an option')).toBeNull();
    });

    test('does not display validation message when `showError` is false and form is submitted', async () => {
      const { getByLabelText, getByText, queryByText } = render(
        <Formik
          initialValues={{
            test: '',
          }}
          onSubmit={() => null}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
          render={(props: FormikProps<FormValues>) => (
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
                value={props.values.test}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        />,
      );

      const select = getByLabelText('Test values') as HTMLInputElement;

      expect(select.value).toBe('');
      expect(queryByText('Select an option')).toBeNull();

      fireEvent.click(getByText('Submit'));

      await wait();

      expect(select.value).toBe('');
      expect(queryByText('Select an option')).toBeNull();
    });
  });
});
