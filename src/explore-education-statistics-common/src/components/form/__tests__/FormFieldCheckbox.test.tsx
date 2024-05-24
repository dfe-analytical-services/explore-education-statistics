import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FormFieldCheckbox', () => {
  test('renders with correct defaults ids with form', () => {
    render(
      <FormProvider>
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckbox
            name="test"
            label="Test checkbox"
            hint="Test hint"
          />
        </Form>
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-test-item-hint',
    );
    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'id',
      'testForm-test',
    );
  });

  test('renders with correct defaults ids without form', () => {
    render(
      <FormProvider>
        <FormFieldCheckbox name="test" label="Test checkbox" hint="Test hint" />
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'test-item-hint',
    );
    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'id',
      'test',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <FormProvider>
        <Form id="testForm" onSubmit={Promise.resolve}>
          <FormFieldCheckbox
            name="test"
            id="customId"
            label="Test checkbox"
            hint="Test hint"
          />
        </Form>
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-customId-item-hint',
    );
    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'id',
      'testForm-customId',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <FormProvider>
        <FormFieldCheckbox
          name="test"
          id="customId"
          label="Test checkbox"
          hint="Test hint"
        />
      </FormProvider>,
    );

    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'customId-item-hint',
    );
    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'id',
      'customId',
    );
  });

  test('clicking the checkbox checks and unchecks it', async () => {
    const user = userEvent.setup();
    render(
      <FormProvider>
        <FormFieldCheckbox name="test" id="select" label="Test checkbox" />
      </FormProvider>,
    );

    const checkbox = screen.getByLabelText('Test checkbox');

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
        <FormFieldCheckbox name="test" id="select" label="Test checkbox" />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test checkbox')).toBeChecked();
  });

  test('does not check the checkbox if initial value is false', async () => {
    render(
      <FormProvider initialValues={{ test: false }}>
        <FormFieldCheckbox name="test" id="select" label="Test checkbox" />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test checkbox')).not.toBeChecked();
  });

  test('hides the conditional element when the checkbox is not checked', async () => {
    render(
      <FormProvider>
        <FormFieldCheckbox
          name="test"
          id="select"
          label="Test checkbox"
          conditional={<>I am conditional content</>}
        />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'aria-expanded',
      'false',
    );
    expect(screen.getByText('I am conditional content')).toHaveClass(
      'govuk-checkboxes__conditional--hidden',
    );
  });

  test('shows the conditional element when the checkbox is checked', async () => {
    const user = userEvent.setup();
    render(
      <FormProvider>
        <FormFieldCheckbox
          name="test"
          id="select"
          label="Test checkbox"
          conditional={<>I am conditional content</>}
        />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'aria-expanded',
      'false',
    );
    expect(screen.getByText('I am conditional content')).toHaveClass(
      'govuk-checkboxes__conditional--hidden',
    );

    await user.click(screen.getByLabelText('Test checkbox'));

    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'aria-expanded',
      'true',
    );
    expect(screen.getByText('I am conditional content')).not.toHaveClass(
      'govuk-checkboxes__conditional--hidden',
    );
  });

  test('hides the conditional element when the checkbox is initially checked', async () => {
    render(
      <FormProvider initialValues={{ test: true }}>
        <FormFieldCheckbox
          name="test"
          id="select"
          label="Test checkbox"
          conditional={<>I am conditional content</>}
        />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Test checkbox')).toHaveAttribute(
      'aria-expanded',
      'true',
    );
    expect(screen.getByText('I am conditional content')).not.toHaveClass(
      'govuk-checkboxes__conditional--hidden',
    );
  });
});
