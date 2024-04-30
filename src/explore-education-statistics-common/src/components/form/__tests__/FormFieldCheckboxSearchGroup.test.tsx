import FormFieldCheckboxSearchGroup from '@common/components/form/FormFieldCheckboxSearchGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('lodash/debounce');

describe('FormFieldCheckboxSearchGroup', () => {
  test('renders with correct default ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxSearchGroup
          name="test"
          legend="Test checkboxes"
          selectAll
          options={[
            { label: 'Checkbox 1', value: '1' },
            { label: 'Checkbox 2', value: '2' },
            { label: 'Checkbox 3', value: '3' },
          ]}
        />
      </FormProvider>,
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
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
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
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
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
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
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
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxSearchGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1');

    expect(checkbox).not.toBeChecked();

    await userEvent.click(checkbox);

    await waitFor(() => {
      expect(checkbox).toBeChecked();
    });
  });

  test('un-checking option un-checks it', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1'],
        }}
      >
        <FormFieldCheckboxSearchGroup
          name="test"
          id="checkboxes"
          legend="Test checkboxes"
          options={[
            { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
            { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
            { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
          ]}
        />
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1');

    expect(checkbox).toBeChecked();

    await userEvent.click(checkbox);

    await waitFor(() => {
      expect(checkbox).not.toBeChecked();
    });
  });

  test('clicking `Select all 3 options` button checks all values', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
    );

    await userEvent.click(screen.getByText('Select all 3 options'));

    expect(screen.getByLabelText('Checkbox 1')).toBeChecked();
    expect(screen.getByLabelText('Checkbox 2')).toBeChecked();
    expect(screen.getByLabelText('Checkbox 3')).toBeChecked();
  });

  test('clicking `Unselect all 3 options` button un-checks all values', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1', '2', '3'],
        }}
      >
        <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1');
    const checkbox2 = screen.getByLabelText('Checkbox 2');
    const checkbox3 = screen.getByLabelText('Checkbox 3');

    expect(checkbox1).toBeChecked();
    expect(checkbox2).toBeChecked();
    expect(checkbox3).toBeChecked();

    await userEvent.click(screen.getByText('Unselect all 3 options'));

    expect(checkbox1).not.toBeChecked();
    expect(checkbox2).not.toBeChecked();
    expect(checkbox3).not.toBeChecked();
  });

  test('checking all options renders the `Unselect all 3 options` button', async () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
    );

    const checkbox1 = screen.getByLabelText('Checkbox 1');
    const checkbox2 = screen.getByLabelText('Checkbox 2');
    const checkbox3 = screen.getByLabelText('Checkbox 3');

    expect(checkbox1).not.toBeChecked();
    expect(screen.queryByText('Select all 3 options')).toBeInTheDocument();
    expect(
      screen.queryByText('Unselect all 3 options'),
    ).not.toBeInTheDocument();

    await userEvent.click(checkbox1);
    await userEvent.click(checkbox2);
    await userEvent.click(checkbox3);

    await waitFor(() => {
      expect(checkbox1).toBeChecked();
      expect(checkbox2).toBeChecked();
      expect(checkbox3).toBeChecked();

      expect(
        screen.queryByText('Select all 3 options'),
      ).not.toBeInTheDocument();
      expect(screen.queryByText('Unselect all 3 options')).toBeInTheDocument();
    });
  });

  test('un-checking any options renders the `Select all 3 options` button ', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1', '2', '3'],
        }}
      >
        <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Checkbox 1');

    expect(checkbox).toBeChecked();
    expect(screen.queryByText('Unselect all 3 options')).toBeInTheDocument();
    expect(screen.queryByText('Select all 3 options')).not.toBeInTheDocument();

    await userEvent.click(checkbox);

    await waitFor(() => {
      expect(checkbox).not.toBeChecked();
      expect(
        screen.queryByText('Unselect all 3 options'),
      ).not.toBeInTheDocument();
      expect(screen.queryByText('Select all 3 options')).toBeInTheDocument();
    });
  });

  describe('error messages', () => {
    test('does not display validation message when checkboxes are untouched', async () => {
      render(
        <FormProvider
          initialValues={{
            test: [],
          }}
          validationSchema={Yup.object({
            test: Yup.array().required('Select at least one option'),
          })}
        >
          <FormFieldCheckboxSearchGroup
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        </FormProvider>,
      );

      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();
    });

    test('displays validation message when no checkboxes are checked', async () => {
      render(
        <FormProvider
          initialValues={{
            test: ['1'],
          }}
          validationSchema={Yup.object({
            test: Yup.array()
              .min(1, 'Select at least one option')
              .required('Select at least one option'),
          })}
        >
          <FormFieldCheckboxSearchGroup
            name="test"
            id="checkboxes"
            legend="Test checkboxes"
            options={[
              { id: 'checkbox-1', value: '1', label: 'Checkbox 1' },
              { id: 'checkbox-2', value: '2', label: 'Checkbox 2' },
              { id: 'checkbox-3', value: '3', label: 'Checkbox 3' },
            ]}
          />
        </FormProvider>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1');

      expect(checkbox).toBeChecked();
      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      await userEvent.click(checkbox);

      await userEvent.tab();

      await waitFor(() => {
        expect(checkbox).not.toBeChecked();
        expect(
          screen.queryByText('Select at least one option'),
        ).toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false', async () => {
      render(
        <FormProvider
          initialValues={{
            test: ['1'],
          }}
          validationSchema={Yup.object({
            test: Yup.array()
              .min(1, 'Select at least one option')
              .required('Select at least one option'),
          })}
        >
          <FormFieldCheckboxSearchGroup
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
        </FormProvider>,
      );

      const checkbox = screen.getByLabelText('Checkbox 1');

      expect(checkbox).toBeChecked();
      expect(
        screen.queryByText('Select at least one option'),
      ).not.toBeInTheDocument();

      await userEvent.click(checkbox);

      await userEvent.tab();

      await waitFor(() => {
        expect(checkbox).not.toBeChecked();
        expect(
          screen.queryByText('Select at least one option'),
        ).not.toBeInTheDocument();
      });
    });
  });

  test('providing a search term does not remove checkboxes that have already been checked', async () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1'],
        }}
      >
        <FormFieldCheckboxSearchGroup
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
      </FormProvider>,
    );

    const searchInput = screen.getByLabelText('Search options');

    expect(screen.getByLabelText('Checkbox 1')).toBeChecked();

    await userEvent.type(searchInput, '2');

    await waitFor(() => {
      expect(screen.getAllByLabelText(/Checkbox/)).toHaveLength(2);
    });
    expect(screen.getByLabelText('Checkbox 1')).toBeChecked();
    expect(screen.getByLabelText('Checkbox 2')).not.toBeChecked();
  });
});
