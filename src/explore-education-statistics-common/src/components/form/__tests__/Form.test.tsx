import { FormFieldTextInput } from '@common/components/form';
import Yup from '@common/validation/yup';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Field, Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import Form from '../Form';

describe('Form', () => {
  test('renders error summary from form errors when form is submitted', async () => {
    const { container } = render(
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
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('First name is required')).toHaveAttribute(
        'href',
        '#test-form-firstName',
      );
      expect(screen.getByText('Last name is required')).toHaveAttribute(
        'href',
        '#test-form-lastName',
      );
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('does not render errors for fields that do not have errors', async () => {
    const { container } = render(
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
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.queryByText('First name is required')).toBeInTheDocument();
      expect(
        screen.queryByText('Last name is required'),
      ).not.toBeInTheDocument();
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders nested error messages', async () => {
    render(
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
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Line 1 of address is required')).toHaveAttribute(
        'href',
        '#test-form-addressLine1',
      );
    });
  });

  test('does not render nested error messages for fields that do not have errors', async () => {
    render(
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
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.queryByText('Line 1 of address is required'),
      ).not.toBeInTheDocument();
      expect(
        screen.queryByText('Line 2 of address is required'),
      ).toBeInTheDocument();
    });
  });

  test('calls `onSubmit` handler when form is submitted successfully', async () => {
    const handleSubmit = jest.fn();

    render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={handleSubmit}
      >
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
    });
  });

  test('renders submit error', async () => {
    const { container } = render(
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
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Something went wrong')).toHaveAttribute(
        'href',
        '#test-form-submit',
      );
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('removes submit error when form can be submitted successfully', async () => {
    const onSubmit = jest.fn(() => {
      throw new Error('Something went wrong');
    });

    render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={onSubmit}
      >
        <Form id="test-form" showSubmitError>
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.queryByText('Something went wrong')).toBeInTheDocument();
    });

    // Stop the onSubmit from throwing error
    onSubmit.mockImplementation();

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong'),
      ).not.toBeInTheDocument();
    });
  });

  test('removes submit error when form is reset', async () => {
    render(
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
            <button type="submit">Submit</button>
            <button type="button" onClick={formik.handleReset}>
              Reset form
            </button>
          </Form>
        )}
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.queryByText('Something went wrong')).toBeInTheDocument();
    });

    fireEvent.click(screen.getByText('Reset form'));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong'),
      ).not.toBeInTheDocument();
    });
  });

  test('removes submit error when form values are changed', async () => {
    render(
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

          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.queryByText('Something went wrong')).toBeInTheDocument();
    });

    fireEvent.change(screen.getByLabelText('Firstname'), {
      target: {
        value: 'Another firstname',
      },
    });

    expect(screen.queryByText('Something went wrong')).not.toBeInTheDocument();
  });

  test('focuses error summary on submit', async () => {
    render(
      <Formik
        initialValues={{
          firstName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={noop}
      >
        <Form id="test-form" showSubmitError>
          <FormFieldTextInput
            id="test-form-firstName"
            label="First name"
            name="firstName"
          />

          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveFocus();
    });
  });

  test('does not re-focus error summary when changing input', async () => {
    render(
      <Formik
        initialValues={{
          firstName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={noop}
      >
        <Form id="test-form" showSubmitError>
          <FormFieldTextInput
            id="test-form-firstName"
            label="First name"
            name="firstName"
          />

          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveFocus();
    });

    const input = screen.getByLabelText('First name');

    userEvent.type(input, 'a first name');

    await waitFor(() => {
      expect(screen.queryByText('There is a problem')).not.toBeInTheDocument();
    });

    userEvent.clear(input);

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(screen.getByRole('alert')).not.toHaveFocus();
  });

  test('re-focuses error summary on re-submit', async () => {
    render(
      <Formik
        initialValues={{
          firstName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={noop}
      >
        {() => (
          <Form id="test-form" showSubmitError>
            <FormFieldTextInput
              id="test-form-firstName"
              label="First name"
              name="firstName"
            />

            <button type="submit">Submit</button>
          </Form>
        )}
      </Formik>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveFocus();
    });

    screen.getByLabelText('First name').focus();
    expect(screen.getByRole('alert')).not.toHaveFocus();

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveFocus();
    });
  });
});
