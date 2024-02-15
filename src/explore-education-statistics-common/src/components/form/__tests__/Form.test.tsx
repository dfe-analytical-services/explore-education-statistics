import { createServerValidationErrorMock } from '@common-test/createAxiosErrorMock';
import { FormFieldTextInput } from '@common/components/form';
import SubmitError from '@common/components/form/util/SubmitError';
import useFormSubmit from '@common/hooks/useFormSubmit';
import Yup from '@common/validation/yup';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import {
  Field,
  Formik as BaseFormik,
  FormikConfig,
  FormikValues,
} from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import Form from '../Form';

describe('Form', () => {
  // Wrapper around Formik so we can enable `useFormSubmit` hook.
  function Formik<FormValues extends FormikValues>({
    onSubmit,
    ...props
  }: FormikConfig<FormValues>) {
    const handleSubmit = useFormSubmit(onSubmit);

    return <BaseFormik {...props} onSubmit={handleSubmit} />;
  }

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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
    });
  });

  test('renders submit error with default message when error thrown', async () => {
    const { container } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw new Error();
        }}
      >
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.getByText('Something went wrong whilst submitting the form'),
      ).toHaveAttribute('href', '#test-form-submit');
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders submit error with custom message when `SubmitError` thrown', async () => {
    const { container } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw new SubmitError('Custom submit error message');
        }}
      >
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Custom submit error message')).toHaveAttribute(
        'href',
        '#test-form-submit',
      );
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders submit error with custom field when `SubmitError` thrown', async () => {
    const { container } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw new SubmitError('Custom submit error message', {
            field: 'firstName',
          });
        }}
      >
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Custom submit error message')).toHaveAttribute(
        'href',
        '#test-form-firstName',
      );
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders submit error with default href when `SubmitError` thrown with invalid field', async () => {
    const { container } = render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw new SubmitError('Custom submit error message', {
            field: 'invalidField',
          });
        }}
      >
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Custom submit error message')).toHaveAttribute(
        'href',
        '#test-form-submit',
      );
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('removes submit error when form can be submitted successfully', async () => {
    const onSubmit = jest.fn(() => {
      throw new Error();
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
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong whilst submitting the form'),
      ).toBeInTheDocument();
    });

    // Stop the onSubmit from throwing error
    onSubmit.mockImplementation();

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong whilst submitting the form'),
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
          throw new Error();
        }}
      >
        {formik => (
          <Form id="test-form">
            The form
            <button type="submit">Submit</button>
            <button type="button" onClick={formik.handleReset}>
              Reset form
            </button>
          </Form>
        )}
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong whilst submitting the form'),
      ).toBeInTheDocument();
    });

    fireEvent.click(screen.getByText('Reset form'));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong whilst submitting the form'),
      ).not.toBeInTheDocument();
    });
  });

  test('renders mapped server validation submit errors', async () => {
    render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
        onSubmit={() => {
          throw createServerValidationErrorMock([
            { path: 'firstName', message: 'Invalid first name' },
          ]);
        }}
      >
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toHaveAttribute(
        'href',
        '#test-form-firstName',
      );
    });
  });

  test('removes mapped server validation errors when form can be submitted successfully', async () => {
    const onSubmit = jest.fn(() => {
      throw createServerValidationErrorMock([
        { path: 'firstName', message: 'Invalid first name' },
      ]);
    });

    render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
        onSubmit={onSubmit}
      >
        <Form id="test-form">
          The form
          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toBeInTheDocument();
    });

    // Stop the onSubmit from throwing error
    onSubmit.mockImplementation();

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.queryByText('Invalid first name')).not.toBeInTheDocument();
    });
  });

  test('removes mapped server validation errors when form is reset', async () => {
    render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
        onSubmit={() => {
          throw createServerValidationErrorMock([
            { path: 'firstName', message: 'Invalid first name' },
          ]);
        }}
      >
        {formik => (
          <Form id="test-form">
            The form
            <button type="submit">Submit</button>
            <button type="button" onClick={formik.handleReset}>
              Reset form
            </button>
          </Form>
        )}
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toBeInTheDocument();
    });

    await userEvent.click(screen.getByText('Reset form'));

    await waitFor(() => {
      expect(screen.queryByText('Invalid first name')).not.toBeInTheDocument();
    });
  });

  test('removes mapped server validation errors when field values are changed', async () => {
    render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
        onSubmit={() => {
          throw createServerValidationErrorMock([
            { path: 'firstName', message: 'Invalid first name' },
          ]);
        }}
      >
        <Form id="test-form">
          <label htmlFor="firstName">Firstname</label>
          <Field name="firstName" type="text" id="firstName" />

          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toBeInTheDocument();
    });

    await userEvent.type(
      screen.getByLabelText('Firstname'),
      'Another firstname',
    );

    await waitFor(() => {
      expect(screen.queryByText('Invalid first name')).not.toBeInTheDocument();
    });
  });

  test('does not render unmapped server validation errors when form submitted', async () => {
    render(
      <Formik
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
        onSubmit={() => {
          throw createServerValidationErrorMock([
            { message: 'Global error' },
            { code: 'UnmappedCode', message: '' },
            { path: 'field1', message: 'Invalid field 1' },
            { path: 'nested.field2', message: 'Invalid field 2' },
          ]);
        }}
      >
        {({ submitCount }) => (
          <Form id="test-form">
            {submitCount > 0 ? 'The form is submitted' : 'The form'}
            <button type="submit">Submit</button>
          </Form>
        )}
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('The form is submitted')).toBeInTheDocument();

      expect(
        screen.getByText(
          'The form submission is invalid and could not be processed',
        ),
      ).toBeInTheDocument();
      expect(screen.queryByText('Global error')).not.toBeInTheDocument();
      expect(screen.queryByText('UnmappedCode')).not.toBeInTheDocument();
      expect(screen.queryByText('Invalid field 1')).not.toBeInTheDocument();
      expect(screen.queryByText('Invalid field 2')).not.toBeInTheDocument();
    });
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
        <Form id="test-form">
          <FormFieldTextInput
            id="test-form-firstName"
            label="First name"
            name="firstName"
          />

          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByTestId('errorSummary')).toHaveFocus();
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
        <Form id="test-form">
          <FormFieldTextInput
            id="test-form-firstName"
            label="First name"
            name="firstName"
          />

          <button type="submit">Submit</button>
        </Form>
      </Formik>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByTestId('errorSummary')).toHaveFocus();
    });

    const input = screen.getByLabelText('First name');

    await userEvent.type(input, 'a first name');

    await waitFor(() => {
      expect(screen.queryByText('There is a problem')).not.toBeInTheDocument();
    });

    await userEvent.clear(input);

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(screen.getByTestId('errorSummary')).not.toHaveFocus();
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
          <Form id="test-form">
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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByTestId('errorSummary')).toHaveFocus();
    });

    screen.getByLabelText('First name').focus();
    expect(screen.getByTestId('errorSummary')).not.toHaveFocus();

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByTestId('errorSummary')).toHaveFocus();
    });
  });
});
