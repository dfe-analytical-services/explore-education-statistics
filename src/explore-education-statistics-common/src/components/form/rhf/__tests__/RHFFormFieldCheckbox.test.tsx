import RHFFormFieldCheckbox from '@common/components/form/rhf/RHFFormFieldCheckbox';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('RHFFormFieldCheckbox', () => {
  test('renders with correct defaults ids with form', () => {
    render(
      <FormProvider>
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldCheckbox
            name="test"
            label="Test values"
            hint="Test hint"
          />
        </RHFForm>
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-test-item-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'testForm-test',
    );
  });

  test('renders with correct defaults ids without form', () => {
    render(
      <FormProvider>
        <RHFFormFieldCheckbox
          name="test"
          label="Test values"
          hint="Test hint"
        />
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'test-item-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute('id', 'test');
  });

  test('renders with correct custom ids with form', () => {
    render(
      <FormProvider>
        <RHFForm id="testForm" onSubmit={Promise.resolve}>
          <RHFFormFieldCheckbox
            name="test"
            id="customId"
            label="Test values"
            hint="Test hint"
          />
        </RHFForm>
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-customId-item-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'testForm-customId',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <FormProvider>
        <RHFFormFieldCheckbox
          name="test"
          id="customId"
          label="Test values"
          hint="Test hint"
        />
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'customId-item-hint',
    );
    expect(screen.getByLabelText('Test values')).toHaveAttribute(
      'id',
      'customId',
    );
  });

  test('clicking the checkbox checks and unchecks it', async () => {
    const user = userEvent.setup();
    render(
      <FormProvider>
        <RHFFormFieldCheckbox name="test" id="select" label="Test values" />
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Test values');

    expect(checkbox).not.toBeChecked();

    await user.click(checkbox);

    await waitFor(() => {
      expect(checkbox).toBeChecked();
    });

    await user.click(checkbox);

    await waitFor(() => {
      expect(checkbox).not.toBeChecked();
    });
  });

  test('checks the checkbox if initial value is true', async () => {
    render(
      <FormProvider initialValues={{ test: true }}>
        <RHFFormFieldCheckbox name="test" id="select" label="Test values" />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test values')).toBeChecked();
  });

  test('does not check the checkbox if initial value is false', async () => {
    render(
      <FormProvider initialValues={{ test: false }}>
        <RHFFormFieldCheckbox name="test" id="select" label="Test values" />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test values')).not.toBeChecked();
  });
});
