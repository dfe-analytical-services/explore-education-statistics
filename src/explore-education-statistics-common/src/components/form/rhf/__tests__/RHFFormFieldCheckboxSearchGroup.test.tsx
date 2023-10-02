import RHFFormFieldCheckboxSearchGroup from '@common/components/form/rhf/RHFFormFieldCheckboxSearchGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import Yup from '@common/validation/yup';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

jest.mock('lodash/debounce');

describe('RHFFormFieldCheckboxSearchGroup', () => {
  test('renders with correct default ids without form', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <RHFFormFieldCheckboxSearchGroup
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
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldCheckboxSearchGroup
            name="test"
            legend="Test checkboxes"
            selectAll
            options={[
              { label: 'Checkbox 1', value: '1' },
              { label: 'Checkbox 2', value: '2' },
              { label: 'Checkbox 3', value: '3' },
            ]}
          />
        </RHFForm>
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
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldCheckboxSearchGroup
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
        </RHFForm>
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
        <RHFFormFieldCheckboxSearchGroup
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
        <RHFFormFieldCheckboxSearchGroup
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

    userEvent.click(checkbox);

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
        <RHFFormFieldCheckboxSearchGroup
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

    userEvent.click(checkbox);

    await waitFor(() => {
      expect(checkbox).not.toBeChecked();
    });
  });

  test('clicking `Select all 3 options` button checks all values', () => {
    render(
      <FormProvider
        initialValues={{
          test: [],
        }}
      >
        <RHFFormFieldCheckboxSearchGroup
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

    userEvent.click(screen.getByText('Select all 3 options'));

    expect(screen.getByLabelText('Checkbox 1')).toBeChecked();
    expect(screen.getByLabelText('Checkbox 2')).toBeChecked();
    expect(screen.getByLabelText('Checkbox 3')).toBeChecked();
  });

  test('clicking `Unselect all 3 options` button un-checks all values', () => {
    render(
      <FormProvider
        initialValues={{
          test: ['1', '2', '3'],
        }}
      >
        <RHFFormFieldCheckboxSearchGroup
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

    userEvent.click(screen.getByText('Unselect all 3 options'));

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
        <RHFFormFieldCheckboxSearchGroup
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

    userEvent.click(checkbox1);
    userEvent.click(checkbox2);
    userEvent.click(checkbox3);

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
        <RHFFormFieldCheckboxSearchGroup
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

    userEvent.click(checkbox);

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
          <RHFFormFieldCheckboxSearchGroup
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
          <RHFFormFieldCheckboxSearchGroup
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

      userEvent.click(checkbox);

      userEvent.tab();

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
          <RHFFormFieldCheckboxSearchGroup
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

      userEvent.click(checkbox);

      userEvent.tab();

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
        <RHFFormFieldCheckboxSearchGroup
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

    userEvent.type(searchInput, '2');

    await waitFor(() => {
      expect(screen.getAllByLabelText(/Checkbox/)).toHaveLength(2);
    });
    expect(screen.getByLabelText('Checkbox 1')).toBeChecked();
    expect(screen.getByLabelText('Checkbox 2')).not.toBeChecked();
  });
});
