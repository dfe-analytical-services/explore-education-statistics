import Yup from '@common/validation/yup';
import { fireEvent, render, wait } from '@testing-library/react';
import { Formik, FormikProps } from 'formik';
import React from 'react';
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
        onSubmit={() => undefined}
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
          onSubmit={() => undefined}
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

      expect(queryByText('Select an option')).toBeNull();
    });

    test('displays validation message when an invalid option is selected', async () => {
      const { getByLabelText, queryByText } = render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={() => undefined}
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
          onSubmit={() => undefined}
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

      expect(queryByText('Select an option')).toBeNull();

      fireEvent.click(getByText('Submit'));

      await wait();

      expect(queryByText('Select an option')).not.toBeNull();
    });

    test('does not display validation message when `showError` is false and invalid option is selected', async () => {
      const { getByLabelText, queryByText } = render(
        <Formik
          initialValues={{
            test: '1',
          }}
          onSubmit={() => undefined}
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
          onSubmit={() => undefined}
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
