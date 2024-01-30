import { createServerValidationErrorMock } from '@common-test/createAxiosErrorMock';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import SubmitError from '@common/components/form/util/SubmitError';
import Yup from '@common/validation/yup';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('Form', () => {
  test('renders error summary from form errors when form is submitted', async () => {
    const { container } = render(
      <FormProvider
        initialValues={{
          firstName: '',
          lastName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required('First name is required'),
          lastName: Yup.string().required('Last name is required'),
        })}
      >
        <RHFForm id="test-form" onSubmit={jest.fn()}>
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
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
      <FormProvider
        initialValues={{
          firstName: '',
          lastName: 'Lastname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required('First name is required'),
          lastName: Yup.string().required('Last name is required'),
        })}
      >
        <RHFForm id="test-form" onSubmit={jest.fn()}>
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
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
      <FormProvider
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
      >
        <RHFForm id="test-form" onSubmit={jest.fn()}>
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
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
      <FormProvider
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
      >
        <RHFForm id="test-form" onSubmit={jest.fn()}>
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
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
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        <RHFForm id="test-form" onSubmit={handleSubmit}>
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledTimes(1);
    });
  });

  test('renders submit error with default message when error thrown', async () => {
    const { container } = render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        <RHFForm
          id="test-form"
          onSubmit={() => {
            throw new Error();
          }}
        >
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.getByText('Something went wrong whilst submitting the form'),
      ).toHaveAttribute('href', '#test-form-submit');
    });

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders submit error with custom message when `SubmitError` thrown', async () => {
    const { container } = render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        <RHFForm
          id="test-form"
          onSubmit={() => {
            throw new SubmitError('Custom submit error message');
          }}
        >
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        <RHFForm
          id="test-form"
          onSubmit={() => {
            throw new SubmitError('Custom submit error message', {
              field: 'firstName',
            });
          }}
        >
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        <RHFForm
          id="test-form"
          onSubmit={() => {
            throw new SubmitError('Custom submit error message', {
              field: 'invalidField',
            });
          }}
        >
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
      >
        <RHFForm id="test-form" onSubmit={onSubmit}>
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.getByText('Something went wrong whilst submitting the form'),
      ).toBeInTheDocument();
    });

    // Stop the onSubmit from throwing error
    onSubmit.mockImplementation();

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong whilst submitting the form'),
      ).not.toBeInTheDocument();
    });
  });

  test('removes submit error when form is reset', async () => {
    render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required(),
        })}
      >
        {({ reset }) => (
          <RHFForm
            id="test-form"
            onSubmit={() => {
              throw new Error();
            }}
          >
            The form
            <button type="submit">Submit</button>
            <button
              type="button"
              onClick={() => {
                reset();
              }}
            >
              Reset form
            </button>
          </RHFForm>
        )}
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(
        screen.getByText('Something went wrong whilst submitting the form'),
      ).toBeInTheDocument();
    });

    userEvent.click(screen.getByText('Reset form'));

    await waitFor(() => {
      expect(
        screen.queryByText('Something went wrong whilst submitting the form'),
      ).not.toBeInTheDocument();
    });
  });

  test('renders mapped server validation errors when form submitted', async () => {
    render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        <RHFForm
          id="test-form"
          onSubmit={() => {
            throw createServerValidationErrorMock([
              { path: 'firstName', message: 'Invalid first name' },
            ]);
          }}
        >
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toBeInTheDocument();
    });
  });

  test('removes mapped server validation errors when form can be submitted successfully', async () => {
    const onSubmit = jest.fn(() => {
      throw createServerValidationErrorMock([
        { path: 'firstName', message: 'Invalid first name' },
      ]);
    });

    render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        <RHFForm id="test-form" onSubmit={onSubmit}>
          The form
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toBeInTheDocument();
    });

    // Stop the onSubmit from throwing error
    onSubmit.mockImplementation();

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.queryByText('Invalid first name')).not.toBeInTheDocument();
    });
  });

  test('removes mapped server validation errors when form is reset', async () => {
    render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        {({ reset }) => (
          <RHFForm
            id="test-form"
            onSubmit={() => {
              throw createServerValidationErrorMock([
                { path: 'firstName', message: 'Invalid first name' },
              ]);
            }}
          >
            The form
            <button type="submit">Submit</button>
            <button
              type="button"
              onClick={() => {
                reset();
              }}
            >
              Reset form
            </button>
          </RHFForm>
        )}
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toBeInTheDocument();
    });

    userEvent.click(screen.getByText('Reset form'));

    await waitFor(() => {
      expect(screen.queryByText('Invalid first name')).not.toBeInTheDocument();
    });
  });

  test('removes mapped server validation errors when field values are changed', async () => {
    render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        {({ register }) => (
          <RHFForm
            id="test-form"
            onSubmit={() => {
              throw createServerValidationErrorMock([
                { path: 'firstName', message: 'Invalid first name' },
              ]);
            }}
          >
            The form
            <label htmlFor="firstName">Firstname</label>
            <input
              type="text"
              id="firstName"
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...register('firstName')}
            />
            <button type="submit">Submit</button>
          </RHFForm>
        )}
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid first name')).toBeInTheDocument();
    });

    userEvent.type(screen.getByLabelText('Firstname'), 'Another firstname');

    await waitFor(() => {
      expect(screen.queryByText('Invalid first name')).not.toBeInTheDocument();
    });
  });

  test('does not render unmapped server validation errors when form submitted', async () => {
    render(
      <FormProvider
        initialValues={{
          firstName: 'Firstname',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().defined(),
        })}
      >
        {({ formState }) => (
          <RHFForm
            id="test-form"
            onSubmit={() => {
              throw createServerValidationErrorMock([
                { message: 'Global error' },
                { code: 'UnmappedCode', message: '' },
                { path: 'field1', message: 'Invalid field 1' },
                { path: 'nested.field2', message: 'Invalid field 2' },
              ]);
            }}
          >
            {formState.isSubmitted ? 'The form is submitted' : 'The form'}
            <button type="submit">Submit</button>
          </RHFForm>
        )}
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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
      <FormProvider
        initialValues={{
          firstName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required('First name is required'),
        })}
      >
        {({ register }) => (
          <RHFForm id="test-form" onSubmit={jest.fn()}>
            <input
              id="test-form-firstName"
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...register('firstName')}
            />
            <button type="submit">Submit</button>
          </RHFForm>
        )}
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(screen.getByTestId('errorSummary')).toHaveFocus();
  });

  test('does not re-focus error summary when changing input', async () => {
    render(
      <FormProvider
        initialValues={{
          firstName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required('First name is required'),
        })}
      >
        {({ register }) => (
          <RHFForm id="test-form" onSubmit={jest.fn()}>
            <label htmlFor="firstName">First name</label>
            <input
              id="firstName"
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...register('firstName')}
            />
            <button type="submit">Submit</button>
          </RHFForm>
        )}
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(screen.getByTestId('errorSummary')).toHaveFocus();

    const input = screen.getByLabelText('First name');

    userEvent.type(input, 'a first name');

    await waitFor(() => {
      expect(screen.queryByText('There is a problem')).not.toBeInTheDocument();
    });

    userEvent.clear(input);

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(screen.getByTestId('errorSummary')).not.toHaveFocus();
  });

  test('re-focuses error summary on re-submit', async () => {
    render(
      <FormProvider
        initialValues={{
          firstName: '',
        }}
        validationSchema={Yup.object({
          firstName: Yup.string().required('First name is required'),
        })}
      >
        {({ register }) => (
          <RHFForm id="test-form" onSubmit={jest.fn()}>
            <label htmlFor="firstName">First name</label>
            <input
              id="firstName"
              // eslint-disable-next-line react/jsx-props-no-spreading
              {...register('firstName')}
            />
            <button type="submit">Submit</button>
          </RHFForm>
        )}
      </FormProvider>,
    );

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(screen.getByTestId('errorSummary')).toHaveFocus();

    screen.getByLabelText('First name').focus();
    expect(screen.getByTestId('errorSummary')).not.toHaveFocus();

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(screen.getByText('There is a problem')).toBeInTheDocument();
    });

    expect(screen.getByTestId('errorSummary')).toHaveFocus();
  });
});
