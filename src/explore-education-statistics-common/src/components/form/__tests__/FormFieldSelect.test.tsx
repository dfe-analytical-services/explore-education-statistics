import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import Yup from '@common/validation/yup';
import render from '@common-test/render';
import { waitFor } from '@testing-library/dom';
import { screen } from '@testing-library/react';
import React from 'react';

describe('FormFieldSelect', () => {
  test('renders with correct defaults ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldSelect
            name="test"
            label="Test values"
            hint="Test hint"
            options={[
              { value: '', label: '' },
              { value: '1', label: 'Option 1' },
              { value: '2', label: 'Option 2' },
              { value: '3', label: 'Option 3' },
            ]}
          />
        </Form>
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-test-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'testForm-test',
    );
  });

  test('renders with correct defaults ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <FormFieldSelect
          name="test"
          label="Test values"
          hint="Test hint"
          options={[
            { value: '', label: '' },
            { value: '1', label: 'Option 1' },
            { value: '2', label: 'Option 2' },
            { value: '3', label: 'Option 3' },
          ]}
        />
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute('id', 'test-hint');
    expect(screen.getByLabelText('Test values')).toHaveAttribute('id', 'test');
  });

  test('renders with correct custom ids with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldSelect
            name="test"
            id="customId"
            label="Test values"
            hint="Test hint"
            options={[
              { value: '', label: '' },
              { value: '1', label: 'Option 1' },
              { value: '2', label: 'Option 2' },
              { value: '3', label: 'Option 3' },
            ]}
          />
        </Form>
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-customId-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'testForm-customId',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <FormFieldSelect
          name="test"
          id="customId"
          label="Test values"
          hint="Test hint"
          options={[
            { value: '', label: '' },
            { value: '1', label: 'Option 1' },
            { value: '2', label: 'Option 2' },
            { value: '3', label: 'Option 3' },
          ]}
        />
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'customId-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'customId',
    );
  });

  test('changing options changes the select value', async () => {
    const { user } = render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <FormFieldSelect
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
      </FormProvider>,
    );

    const select = screen.getByLabelText('Test values') as HTMLInputElement;

    expect(select.value).toBe('');

    await user.selectOptions(select, '1');

    await waitFor(() => {
      expect(select.value).toBe('1');
    });
  });

  describe('error messages', () => {
    test('displays validation message when form is submitted', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          <Form
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <FormFieldSelect
              name="test"
              id="customId"
              label="Test values"
              hint="Test hint"
              options={[
                { value: '', label: '' },
                { value: '1', label: 'Option 1' },
                { value: '2', label: 'Option 2' },
                { value: '3', label: 'Option 3' },
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

    test('updates validation message when values change after form is submitted', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          <Form
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <FormFieldSelect
              name="test"
              id="customId"
              label="Test values"
              hint="Test hint"
              options={[
                { value: '', label: '' },
                { value: '1', label: 'Option 1' },
                { value: '2', label: 'Option 2' },
                { value: '3', label: 'Option 3' },
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

      await user.selectOptions(screen.getByLabelText('Test values'), '1');

      await waitFor(() => {
        expect(screen.queryByText('Select an option')).not.toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false and form is submitted', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          <Form
            id="testForm"
            showErrorSummary={false}
            onSubmit={Promise.resolve}
          >
            <FormFieldSelect
              name="test"
              id="customId"
              label="Test values"
              hint="Test hint"
              showError={false}
              options={[
                { value: '', label: '' },
                { value: '1', label: 'Option 1' },
                { value: '2', label: 'Option 2' },
                { value: '3', label: 'Option 3' },
              ]}
            />
            <button type="submit">Submit</button>
          </Form>
        </FormProvider>,
      );

      const select = screen.getByLabelText('Test values') as HTMLInputElement;

      expect(select.value).toBe('');
      expect(screen.queryByText('Select an option')).not.toBeInTheDocument();

      await user.click(screen.getByText('Submit'));

      await waitFor(() => {
        expect(select.value).toBe('');
        expect(screen.queryByText('Select an option')).not.toBeInTheDocument();
      });
    });
  });
});
