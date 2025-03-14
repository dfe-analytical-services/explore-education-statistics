import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import Yup from '@common/validation/yup';
import render from '@common-test/render';
import { waitFor } from '@testing-library/dom';
import { screen } from '@testing-library/react';

import noop from 'lodash/noop';
import React from 'react';

jest.mock('lodash/debounce');

describe('FormFieldRadioSearchGroup', () => {
  test('renders with correctly without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <FormFieldRadioSearchGroup
          id="test-group"
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

    expect(screen.getByRole('group')).toHaveAttribute('id', 'test-group');
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'test-group-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'test-group-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'test-group-3',
    );
  });

  test('renders with correctly with form', () => {
    render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <Form id="testForm" onSubmit={noop}>
          <FormFieldRadioSearchGroup
            id="test-group"
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

    expect(screen.getByRole('group')).toHaveAttribute(
      'id',
      'testForm-test-group',
    );
    expect(screen.getByLabelText('Radio 1')).toHaveAttribute(
      'id',
      'testForm-test-group-1',
    );
    expect(screen.getByLabelText('Radio 2')).toHaveAttribute(
      'id',
      'testForm-test-group-2',
    );
    expect(screen.getByLabelText('Radio 3')).toHaveAttribute(
      'id',
      'testForm-test-group-3',
    );
  });

  test('checking an option checks it', async () => {
    const { user } = render(
      <FormProvider
        initialValues={{
          test: '',
        }}
      >
        <FormFieldRadioSearchGroup
          id="test-group"
          name="test"
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
        <FormFieldRadioSearchGroup
          id="test-group"
          name="test"
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
    const radio3 = screen.getByLabelText('Radio 3') as HTMLInputElement;

    expect(radio1.checked).toBe(true);
    expect(radio2.checked).toBe(false);
    expect(radio3.checked).toBe(false);

    await user.click(radio2);

    expect(radio1.checked).toBe(false);
    expect(radio2.checked).toBe(true);
    expect(radio3.checked).toBe(false);
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
          <Form id="testForm" onSubmit={noop}>
            <FormFieldRadioSearchGroup
              id="test-group"
              name="test"
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

    test('updates validation message when change values after form is submitted', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          <Form id="testForm" onSubmit={noop}>
            <FormFieldRadioSearchGroup
              id="test-group"
              name="test"
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

      await waitFor(() => {
        expect(screen.queryByText('Select an option')).not.toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
          validationSchema={Yup.object({
            test: Yup.string().required('Select an option'),
          })}
        >
          <Form id="testForm" showErrorSummary={false} onSubmit={noop}>
            <FormFieldRadioSearchGroup
              id="test-group"
              name="test"
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

  describe('search', () => {
    test('providing a search term filters the radios', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
        >
          <FormFieldRadioSearchGroup
            name="test"
            id="test-group"
            legend="Test radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
              { id: 'radio-22', value: '22', label: 'Radio 22' },
            ]}
          />
        </FormProvider>,
      );

      const searchInput = screen.getByLabelText('Search') as HTMLInputElement;

      await user.type(searchInput, '2');

      await waitFor(() =>
        expect(screen.queryByLabelText('Radio 3')).not.toBeInTheDocument(),
      );

      const radios = screen.getAllByRole('radio');
      expect(radios).toHaveLength(2);
      expect(radios[0]).toHaveAttribute('value', '2');
      expect(radios[1]).toHaveAttribute('value', '22');
    });

    test('providing a search term does not remove a radio that has already been checked', async () => {
      const { user } = render(
        <FormProvider
          initialValues={{
            test: '',
          }}
        >
          <FormFieldRadioSearchGroup
            name="test"
            id="test-group"
            legend="Test radios"
            options={[
              { id: 'radio-1', value: '1', label: 'Radio 1' },
              { id: 'radio-2', value: '2', label: 'Radio 2' },
              { id: 'radio-3', value: '3', label: 'Radio 3' },
            ]}
          />
        </FormProvider>,
      );

      const searchInput = screen.getByLabelText('Search') as HTMLInputElement;

      const radio1 = screen.getByLabelText('Radio 1') as HTMLInputElement;
      const radio2 = screen.getByLabelText('Radio 2') as HTMLInputElement;

      await user.click(radio1);
      expect(radio1.checked).toBe(true);

      await user.type(searchInput, '2');

      await waitFor(() =>
        expect(screen.queryByLabelText('Radio 3')).not.toBeInTheDocument(),
      );

      expect(screen.getAllByRole('radio')).toHaveLength(2);
      expect(radio1.checked).toBe(true);
      expect(radio2.checked).toBe(false);
    });
  });
});
