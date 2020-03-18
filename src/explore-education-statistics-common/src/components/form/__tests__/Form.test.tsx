import Yup from '@common/validation/validation/yup';
import { fireEvent, render, wait } from '@testing-library/react';
import { Field, Formik } from 'formik';
import React from 'react';
import Form from '../Form';

describe('Form', () => {
  test('renders error summary from form errors when form is submitted', async () => {
    const { container, getByText } = render(
      <Formik
        initialValues={{
          firstName: '',
          lastName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required('First name is required'),
          lastName: Yup.string().required('Last name is required'),
        })}
        onSubmit={() => {}}
      >
        {() => <Form id="test-form">The form</Form>}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(getByText('First name is required')).toHaveAttribute(
      'href',
      '#test-form-firstName',
    );
    expect(getByText('Last name is required')).toHaveAttribute(
      'href',
      '#test-form-lastName',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render errors for fields that do not have errors', async () => {
    const { container, queryByText } = render(
      <Formik
        initialValues={{
          firstName: '',
          lastName: 'Lastname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required('First name is required'),
          lastName: Yup.string().required('Last name is required'),
        })}
        onSubmit={() => {}}
      >
        {() => <Form id="test-form">The form</Form>}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(queryByText('First name is required')).not.toBeNull();
    expect(queryByText('Last name is required')).toBeNull();

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders nested error messages', async () => {
    const { container, getByText } = render(
      <Formik
        initialValues={{
          address: {
            line1: '',
          },
        }}
        validationSchema={Yup.object({
          address: Yup.object({
            line1: Yup.string().required('Line 1 of address is required'),
          }),
        })}
        onSubmit={() => {}}
      >
        {() => <Form id="test-form">The form</Form>}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(getByText('Line 1 of address is required')).toHaveAttribute(
      'href',
      '#test-form-addressLine1',
    );
  });

  test('does not render nested error messages for fields that do not have errors', async () => {
    const { container, queryByText } = render(
      <Formik
        initialValues={{
          address: {
            line1: 'Line 1',
            line2: '',
          },
        }}
        validationSchema={Yup.object({
          address: Yup.object({
            line1: Yup.string().required('Line 1 of address is required'),
            line2: Yup.string().required('Line 2 of address is required'),
          }),
        })}
        onSubmit={() => {}}
      >
        {() => <Form id="test-form">The form</Form>}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(queryByText('Line 1 of address is required')).toBeNull();
    expect(queryByText('Line 2 of address is required')).not.toBeNull();
  });

  test('calls `onSubmit` handler when form is submitted successfully', async () => {
    const handleSubmit = jest.fn();

    const { container } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={handleSubmit}
      >
        {() => <Form id="test-form">The form</Form>}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(handleSubmit).toHaveBeenCalled();
  });

  test('renders submit error', async () => {
    const { container, getByText } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw new Error('Something went wrong');
        }}
      >
        {() => (
          <Form id="test-form" showSubmitError>
            The form
          </Form>
        )}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(getByText('Something went wrong')).toHaveAttribute(
      'href',
      '#test-form-submit',
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('removes submit error when form can be submitted successfully', async () => {
    const onSubmit = jest.fn(() => {
      throw new Error('Something went wrong');
    });

    const { container, queryByText } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={onSubmit}
      >
        {() => (
          <Form id="test-form" showSubmitError>
            The form
          </Form>
        )}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(queryByText('Something went wrong')).not.toBeNull();

    // Stop the onSubmit from throwing error
    onSubmit.mockImplementation();

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(queryByText('Something went wrong')).toBeNull();
  });

  test('removes submit error when form is reset', async () => {
    const { container, queryByText, getByText } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw new Error('Something went wrong');
        }}
      >
        {formik => (
          <Form id="test-form" showSubmitError>
            The form
            <button type="button" onClick={formik.handleReset}>
              Reset form
            </button>
          </Form>
        )}
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(queryByText('Something went wrong')).not.toBeNull();

    fireEvent.click(getByText('Reset form'));

    await wait();

    expect(queryByText('Something went wrong')).toBeNull();
  });

  test('removes submit error when form values are changed', async () => {
    const { container, queryByText, getByLabelText } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw new Error('Something went wrong');
        }}
      >
        <Form id="test-form" showSubmitError>
          <label htmlFor="firstName">Firstname</label>
          <Field name="firstName" type="text" id="firstName" />
        </Form>
      </Formik>,
    );

    const form = container.querySelector('#test-form') as HTMLFormElement;

    fireEvent.submit(form, {
      current: form,
      target: form,
    });

    await wait();

    expect(queryByText('Something went wrong')).not.toBeNull();

    fireEvent.change(getByLabelText('Firstname'), {
      target: {
        value: 'Another firstname',
      },
    });

    expect(queryByText('Something went wrong')).toBeNull();
  });
});
