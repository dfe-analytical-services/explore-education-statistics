import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import RHFForm from '@common/components/form/rhf/RHFForm';
import FormProvider from '@common/components/form/rhf/FormProvider';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';
import Yup from '@common/validation/yup';

describe('RHFFormFieldTextArea', () => {
  test('renders correctly', () => {
    const { container } = render(
      <FormProvider initialValues={{}}>
        <RHFFormFieldTextArea name="testField" label="Test field" />
      </FormProvider>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with a hint correctly', () => {
    render(
      <FormProvider initialValues={{}}>
        <RHFFormFieldTextArea
          name="testField"
          label="Test field"
          hint="Test hint"
        />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'aria-describedby',
      'testField-hint',
    );
    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testField-hint',
    );
  });

  test('renders with correct defaults ids with form', () => {
    render(
      <FormProvider initialValues={{}}>
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldTextArea
            name="testField"
            label="Test field"
            hint="Test hint"
          />
        </RHFForm>
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'id',
      'testForm-testField',
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'aria-describedby',
      'testForm-testField-hint',
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-testField-hint',
    );
  });

  test('renders with correct defaults ids without form', () => {
    render(
      <FormProvider initialValues={{}}>
        <RHFFormFieldTextArea
          name="testField"
          label="Test field"
          hint="Test hint"
        />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'id',
      'testField',
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'aria-describedby',
      'testField-hint',
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testField-hint',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <FormProvider initialValues={{}}>
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldTextArea
            id="customId"
            name="testField"
            label="Test field"
            hint="Test hint"
          />
        </RHFForm>
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'id',
      'testForm-customId',
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'aria-describedby',
      'testForm-customId-hint',
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-customId-hint',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <FormProvider initialValues={{}}>
        <RHFFormFieldTextArea
          id="customId"
          name="testField"
          label="Test field"
          hint="Test hint"
        />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'id',
      'customId',
    );

    expect(screen.getByLabelText('Test field')).toHaveAttribute(
      'aria-describedby',
      'customId-hint',
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'customId-hint',
    );
  });

  test('trims the value by default when the form is submitted', async () => {
    const handleSubmit = jest.fn();
    render(
      <FormProvider initialValues={{}}>
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldTextArea name="testField" label="Test field" />{' '}
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.type(screen.getByLabelText('Test field'), '  trim me  ');
    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({ testField: 'trim me' });
    });
  });

  test('does not trim the value when the form is submitted when `trimInput` is false', async () => {
    const handleSubmit = jest.fn();
    render(
      <FormProvider initialValues={{}}>
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldTextArea
            name="testField"
            label="Test field"
            trimInput={false}
          />
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    userEvent.type(screen.getByLabelText('Test field'), '  do not trim me  ');
    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        testField: '  do not trim me  ',
      });
    });
  });

  describe('error messages', () => {
    test('does not display validation message when untouched', () => {
      render(
        <FormProvider
          initialValues={{
            testField: '',
          }}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <RHFFormFieldTextArea name="testField" label="Test field" />
        </FormProvider>,
      );

      expect(screen.queryByText('Field required')).not.toBeInTheDocument();
    });

    test('displays validation message on blur', async () => {
      render(
        <FormProvider
          initialValues={{
            testField: '',
          }}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <RHFFormFieldTextArea name="testField" label="Test field" />
        </FormProvider>,
      );

      userEvent.click(screen.getByLabelText('Test field'));
      userEvent.tab();

      await waitFor(() => {
        expect(screen.queryByText('Field required')).toBeInTheDocument();
      });
    });

    test('displays validation message when form is submitted', async () => {
      render(
        <FormProvider
          initialValues={{ testField: '' }}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <RHFForm
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <RHFFormFieldTextArea name="testField" label="Test field" />

            <button type="submit">Submit</button>
          </RHFForm>
        </FormProvider>,
      );

      expect(screen.queryByText('Field required')).not.toBeInTheDocument();

      userEvent.click(screen.getByText('Submit'));

      await waitFor(() => {
        expect(screen.queryByText('Field required')).toBeInTheDocument();
      });
    });

    test('does not display validation message on blur when `showError` is false', async () => {
      render(
        <FormProvider
          initialValues={{
            testField: '',
          }}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <RHFFormFieldTextArea
            name="testField"
            label="Test field"
            showError={false}
          />
        </FormProvider>,
      );

      userEvent.click(screen.getByLabelText('Test field'));
      userEvent.tab();

      await waitFor(() => {
        expect(screen.queryByText('Field required')).not.toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false and form is submitted', async () => {
      render(
        <FormProvider
          initialValues={{
            testField: '',
          }}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <RHFForm id="testForm" onSubmit={Promise.resolve}>
            <RHFFormFieldTextArea
              name="testField"
              label="Test field"
              showError={false}
            />

            <button type="submit">Submit</button>
          </RHFForm>
        </FormProvider>,
      );

      expect(screen.queryByText('Field required')).not.toBeInTheDocument();

      userEvent.click(screen.getByText('Submit'));

      await waitFor(() => {
        expect(screen.queryByText('Field required')).not.toBeInTheDocument();
      });
    });
  });
});
