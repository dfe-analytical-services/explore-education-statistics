import Form from '@common/components/form/Form';
import FormProvider from '@common/components/form/FormProvider';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import Yup from '@common/validation/yup';
import render from '@common-test/render';
import { waitFor } from '@testing-library/dom';
import { screen } from '@testing-library/react';
import React from 'react';

describe('FormFieldRadioGroup', () => {
  test('renders with correct default ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <FormFieldRadioGroup
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
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldRadioGroup
            name="test"
            legend="Test radios"
            options={[
              { value: '1', label: 'Radio 1' },
              { value: '2', label: 'Radio 2' },
              { value: '3', label: 'Radio 3' },
            ]}
          />
        </Form>
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
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldRadioGroup
            name="test"
            id="customId"
            legend="Test radios"
            options={[
              { value: '1', label: 'Radio 1' },
              { value: '2', label: 'Radio 2' },
              { id: 'customOption', value: '3', label: 'Radio 3' },
            ]}
          />
        </Form>
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
        <FormFieldRadioGroup
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
    const { user } = render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <FormFieldRadioGroup
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

    await user.click(radio);

    expect(radio.checked).toBe(true);
  });

  test('checking another option un-checks the currently checked option', async () => {
    const { user } = render(
      <FormProvider
        initialValues={{
          test: '1',
        }}
      >
        <FormFieldRadioGroup
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

    await user.click(radio2);

    expect(radio1.checked).toBe(false);
    expect(radio2.checked).toBe(true);
  });

  describe('error messages', () => {
    test('displays validation message when form is submitted', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().ensure().required('Select an option'),
          })}
        >
          <Form
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <FormFieldRadioGroup
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
          </Form>
        </FormProvider>,
      );

      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Submit' }));

      await waitFor(() => {
        expect(screen.getByText('Select an option')).toBeInTheDocument();
      });
    });

    test('updates validation message when update values after form is submitted', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().ensure().required('Select an option'),
          })}
        >
          <Form
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <FormFieldRadioGroup
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
          </Form>
        </FormProvider>,
      );

      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Submit' }));

      await waitFor(() => {
        expect(screen.getByText('Select an option')).toBeInTheDocument();
      });

      await user.click(screen.getByLabelText('Radio 3'));

      await waitFor(() =>
        expect(screen.queryByText('Select an option')).not.toBeInTheDocument(),
      );
    });

    test('does not display validation message when `showError` is false', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().ensure().required('Select an option'),
          })}
        >
          <Form
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <FormFieldRadioGroup
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
          </Form>
        </FormProvider>,
      );

      const radio = screen.getByLabelText('Radio 1') as HTMLInputElement;

      expect(radio.checked).toBe(false);
      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: 'Submit' }));

      expect(radio.checked).toBe(false);
      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();
    });
  });
});
