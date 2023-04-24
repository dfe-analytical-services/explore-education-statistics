import RHFForm from '@common/components/form/rhf/RHFForm';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FormFieldRadioGroup', () => {
  test('renders with correct default ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFFormFieldRadioGroup
          name="test"
          legend="Test radios"
          options={[
            { value: '1', label: 'Radio 1' },
            { value: '2', label: 'Radio 2' },
            { value: '3', label: 'Radio 3' },
          ]}
        />
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'test');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute('id', 'test-1');
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute('id', 'test-2');
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute('id', 'test-3');
  });

  test('renders with correct default ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldRadioGroup
            name="test"
            legend="Test radios"
            options={[
              { value: '1', label: 'Radio 1' },
              { value: '2', label: 'Radio 2' },
              { value: '3', label: 'Radio 3' },
            ]}
          />
        </RHFForm>
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'testForm-test');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'testForm-test-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'testForm-test-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'testForm-test-3',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldRadioGroup
            name="test"
            id="customId"
            legend="Test radios"
            options={[
              { value: '1', label: 'Radio 1' },
              { value: '2', label: 'Radio 2' },
              { id: 'customOption', value: '3', label: 'Radio 3' },
            ]}
          />
        </RHFForm>
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'id',
      'testForm-customId',
    );
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'testForm-customId-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'testForm-customId-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'testForm-customId-customOption',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFFormFieldRadioGroup
          name="test"
          id="customId"
          legend="Test radios"
          options={[
            { value: '1', label: 'Radio 1' },
            { value: '2', label: 'Radio 2' },
            { id: 'customOption', value: '3', label: 'Radio 3' },
          ]}
        />
      </FormProvider>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'customId');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'customId-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'customId-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'customId-customOption',
    );
  });

  test('checking an option checks it', async () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <RHFFormFieldRadioGroup
          name="test"
          id="radios"
          legend="Test radios"
          options={[
            { id: 'radio-1', value: '1', label: 'Radio 1' },
            { id: 'radio-2', value: '2', label: 'Radio 2' },
            { id: 'radio-3', value: '3', label: 'Radio 3' },
          ]}
        />
      </FormProvider>,
    );

    const radio = screen.getByLabelText('Radio 1') as HTMLInputElement;

    expect(radio.checked).toBe(false);

    userEvent.click(radio);

    expect(radio.checked).toBe(true);
  });

  test('checking another option un-checks the currently checked option', async () => {
    render(
      <FormProvider
        initialValues={{
          test: '1',
        }}
      >
        <RHFFormFieldRadioGroup
          name="test"
          id="radios"
          legend="Test radios"
          options={[
            { id: 'radio-1', value: '1', label: 'Radio 1' },
            { id: 'radio-2', value: '2', label: 'Radio 2' },
            { id: 'radio-3', value: '3', label: 'Radio 3' },
          ]}
        />
      </FormProvider>,
    );

    const radio1 = screen.getByLabelText('Radio 1') as HTMLInputElement;
    const radio2 = screen.getByLabelText('Radio 2') as HTMLInputElement;

    expect(radio1.checked).toBe(true);
    expect(radio2.checked).toBe(false);

    userEvent.click(radio2);

    expect(radio1.checked).toBe(false);
    expect(radio2.checked).toBe(true);
  });

  describe('error messages', () => {
    test('displays validation message when form is submitted', async () => {
      render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().ensure().required('Select an option'),
          })}
        >
          <RHFForm
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <RHFFormFieldRadioGroup
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
          </RHFForm>
        </FormProvider>,
      );

      expect(screen.queryByText('Select an option')).toBeNull();

      userEvent.click(screen.getByRole('button', { name: 'Submit' }));

      await waitFor(() => {
        expect(screen.getByText('Select an option')).toBeInTheDocument();
      });
    });

    test('displays validation message when radios have been touched', async () => {
      render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().ensure().required('Select an option'),
          })}
        >
          <RHFFormFieldRadioGroup
            name="test"
            id="radios"
            legend="Test radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
            ]}
          />
        </FormProvider>,
      );

      userEvent.tab();
      userEvent.tab();

      await waitFor(() => {
        expect(screen.getByText('Select an option')).toBeInTheDocument();
      });
    });

    test('does not display validation message when radios are untouched', async () => {
      render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().ensure().required('Select an option'),
          })}
        >
          <RHFFormFieldRadioGroup
            name="test"
            id="radios"
            legend="Test radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
            ]}
          />
        </FormProvider>,
      );

      expect(screen.queryByText('Select an option')).toBeNull();
    });

    test('does not display validation message when `showError` is false', async () => {
      render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().ensure().required('Select an option'),
          })}
        >
          <RHFForm id="testForm" onSubmit={Promise.resolve}>
            <RHFFormFieldRadioGroup
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
          </RHFForm>
        </FormProvider>,
      );

      const radio = screen.getByLabelText('Radio 1') as HTMLInputElement;

      expect(radio.checked).toBe(false);
      expect(screen.queryByText('Select an option')).toBeNull();

      userEvent.click(screen.getByRole('button', { name: 'Submit' }));

      expect(radio.checked).toBe(false);
      expect(screen.queryByText('Select an option')).toBeNull();
    });
  });
});
