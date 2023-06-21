import { Form } from '@common/components/form';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import FormFieldCheckboxSearchGroup from '../FormFieldCheckboxSearchGroup';

jest.mock('lodash/debounce');

describe('FormFieldCheckboxSearchGroup', () => {
  interface FormValues {
    test: string[];
  }

  test('renders with correct default ids without form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <FormFieldCheckboxSearchGroup<FormValues>
          name="test"
          legend="Test checkboxes"
          selectAll
          options={[
            { label: 'Checkbox 1', value: '1' },
            { label: 'Checkbox 2', value: '2' },
            { label: 'Checkbox 3', value: '3' },
          ]}
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'test');

    expect(screen.getByRole('textbox')).toHaveAttribute('id', 'test-search');

    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'test-options-1',
    );
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'test-options-2',
    );
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'test-options-3',
    );
  });

  test('renders with correct default ids with form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            legend="Test checkboxes"
            selectAll
            options={[
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
              { label: 'Checkbox 3', value: '3' },
            ]}
          />
        </Form>
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'testForm-test');

    expect(screen.getByRole('textbox')).toHaveAttribute(
      'id',
      'testForm-test-search',
    );
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'testForm-test-options-1',
    );
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'testForm-test-options-2',
    );
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'testForm-test-options-3',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldCheckboxSearchGroup<FormValues>
            id="customId"
            name="test"
            legend="Test checkboxes"
            selectAll
            options={[
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
              { id: 'customOption', label: 'Checkbox 3', value: '3' },
            ]}
          />
        </Form>
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'id',
      'testForm-customId',
    );

    expect(screen.getByRole('textbox')).toHaveAttribute(
      'id',
      'testForm-customId-search',
    );
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'testForm-customId-options-1',
    );
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'testForm-customId-options-2',
    );
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'testForm-customId-options-customOption',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <Formik<FormValues>
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        <FormFieldCheckboxSearchGroup<FormValues>
          id="customId"
          name="test"
          legend="Test checkboxes"
          selectAll
          options={[
            { label: 'Checkbox 1', value: '1' },
            { label: 'Checkbox 2', value: '2' },
            { id: 'customOption', label: 'Checkbox 3', value: '3' },
          ]}
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute('id', 'customId');

    expect(screen.getByRole('textbox')).toHaveAttribute(
      'id',
      'customId-search',
    );
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute(
      'id',
      'customId-options-1',
    );
    expect(screen.getByLabelText('Checkbox 2')).toHaveAttribute(
      'id',
      'customId-options-2',
    );
    expect(screen.getByLabelText('Checkbox 3')).toHaveAttribute(
      'id',
      'customId-options-customOption',
    );
  });

  test('checking option checks it', async () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={noop}
      >
        {() => (
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(false);

    userEvent.click(checkbox);

    await waitFor(() => {
      expect(checkbox.checked).toBe(true);
    });
  });

  test('un-checking option un-checks it', async () => {
    render(
      <Formik
        initialValues={{
          test: ['1'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);

    userEvent.click(checkbox);

    await waitFor(() => {
      expect(checkbox.checked).toBe(false);
    });
  });

  test('clicking `Select all 3 options` button checks all values', () => {
    render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
            selectAll
          />
        )}
      </Formik>,
    );

    userEvent.click(screen.getByText('Select all 3 options'));

    expect(
      (screen.getByLabelText('Checkbox 1') as HTMLInputElement).checked,
    ).toBe(true);
    expect(
      (screen.getByLabelText('Checkbox 2') as HTMLInputElement).checked,
    ).toBe(true);
    expect(
      (screen.getByLabelText('Checkbox 3') as HTMLInputElement).checked,
    ).toBe(true);
  });

  test('clicking `Unselect all 3 options` button un-checks all values', () => {
    render(
      <Formik
        initialValues={{
          test: ['1', '2', '3'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = screen.getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = screen.getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(true);
    expect(checkbox2.checked).toBe(true);
    expect(checkbox3.checked).toBe(true);

    userEvent.click(screen.getByText('Unselect all 3 options'));

    expect(checkbox1.checked).toBe(false);
    expect(checkbox2.checked).toBe(false);
    expect(checkbox3.checked).toBe(false);
  });

  test('checking all options renders the `Unselect all 3 options` button', async () => {
    const { getByLabelText, queryByText } = render(
      <Formik
        initialValues={{
          test: [],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              { id: 'checkbox-1', label: 'Checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Checkbox 3', value: '3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox1 = getByLabelText('Checkbox 1') as HTMLInputElement;
    const checkbox2 = getByLabelText('Checkbox 2') as HTMLInputElement;
    const checkbox3 = getByLabelText('Checkbox 3') as HTMLInputElement;

    expect(checkbox1.checked).toBe(false);
    expect(queryByText('Select all 3 options')).toBeInTheDocument();
    expect(queryByText('Unselect all 3 options')).not.toBeInTheDocument();

    userEvent.click(checkbox1);
    userEvent.click(checkbox2);
    userEvent.click(checkbox3);

    await waitFor(() => {
      expect(checkbox1.checked).toBe(true);
      expect(checkbox2.checked).toBe(true);
      expect(checkbox3.checked).toBe(true);

      expect(queryByText('Select all 3 options')).not.toBeInTheDocument();
      expect(queryByText('Unselect all 3 options')).toBeInTheDocument();
    });
  });

  test('un-checking any options renders the `Select all 3 options` button ', async () => {
    render(
      <Formik
        initialValues={{
          test: ['1', '2', '3'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            selectAll
            options={[
              { id: 'checkbox-1', label: 'Checkbox 1', value: '1' },
              { id: 'checkbox-2', label: 'Checkbox 2', value: '2' },
              { id: 'checkbox-3', label: 'Checkbox 3', value: '3' },
            ]}
          />
        )}
      </Formik>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

    expect(checkbox.checked).toBe(true);
    expect(screen.queryByText('Unselect all 3 options')).toBeInTheDocument();
    expect(screen.queryByText('Select all 3 options')).not.toBeInTheDocument();

    userEvent.click(checkbox);

    await waitFor(() => {
      expect(checkbox.checked).toBe(false);
      expect(
        screen.queryByText('Unselect all 3 options'),
      ).not.toBeInTheDocument();
      expect(screen.queryByText('Select all 3 options')).toBeInTheDocument();
    });
  });

  describe('error messages', () => {
    test('does not display validation message when checkboxes are untouched', async () => {
      render(
        <Formik
          initialValues={{
            test: [],
          }}
          onSubmit={() => undefined}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxSearchGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();
    });

    test('displays validation message when no checkboxes are checked', async () => {
      render(
        <Formik
          initialValues={{
            test: ['1'],
          }}
          initialTouched={{
            test: true,
          }}
          onSubmit={() => undefined}
          validationSchema={Yup.object({
            test: Yup.array()
              .min(1, 'Select at least one option')
              .required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxSearchGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);

      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      userEvent.click(checkbox);

      expect(checkbox.checked).toBe(false);

      await waitFor(() => {
        expect(
          screen.queryByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false', async () => {
      render(
        <Formik
          initialValues={{
            test: ['1'],
          }}
          initialTouched={{
            test: true,
          }}
          onSubmit={() => undefined}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          {() => (
            <FormFieldCheckboxSearchGroup<FormValues>
              name="test"
              id="checkboxes"
              legend="Test checkboxes"
              showError={false}
              options={[
                { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
                { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
                { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
              ]}
            />
          )}
        </Formik>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1') as HTMLInputElement;

      expect(checkbox.checked).toBe(true);
      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      userEvent.click(checkbox);

      await waitFor(() => {
        expect(checkbox.checked).toBe(false);
        expect(
          screen.queryByText('Select at least one option'),
        ).not.toBeInTheDocument();
      });
    });
  });

  test('providing a search term does not remove checkboxes that have already been checked', async () => {
    jest.useFakeTimers();

    render(
      <Formik
        initialValues={{
          test: ['1'],
        }}
        onSubmit={() => undefined}
      >
        {() => (
          <FormFieldCheckboxSearchGroup<FormValues>
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            searchLabel="Search options"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        )}
      </Formik>,
    );

    const searchInput = screen.getByLabelText(
      'Search options',
    ) as HTMLInputElement;

    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('checked');

    await userEvent.type(searchInput, '2');

    jest.runAllTimers();

    expect(screen.getAllByLabelText(/Checkbox/)).toHaveLength(2);
    expect(screen.getByLabelText('Checkbox 1')).toHaveAttribute('checked');
    expect(screen.getByLabelText('Checkbox 2')).not.toHaveAttribute('checked');
  });
});
