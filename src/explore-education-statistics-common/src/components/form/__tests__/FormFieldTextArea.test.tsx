import { Form } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { waitFor } from '@testing-library/dom';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';
import Yup from '@common/validation/yup';

describe('FormFieldTextArea', () => {
  test('renders correctly', () => {
    const { container } = render(
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldTextArea name="testField" label="Test field" />
      </Formik>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with a hint correctly', () => {
    render(
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldTextArea
          name="testField"
          label="Test field"
          hint="Test hint"
        />
      </Formik>,
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
      <Formik initialValues={{}} onSubmit={noop}>
        <Form id="testForm">
          <FormFieldTextArea
            name="testField"
            label="Test field"
            hint="Test hint"
          />
        </Form>
      </Formik>,
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
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldTextArea
          name="testField"
          label="Test field"
          hint="Test hint"
        />
      </Formik>,
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
      <Formik initialValues={{}} onSubmit={noop}>
        <Form id="testForm">
          <FormFieldTextArea
            id="customId"
            name="testField"
            label="Test field"
            hint="Test hint"
          />
        </Form>
      </Formik>,
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
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldTextArea
          id="customId"
          name="testField"
          label="Test field"
          hint="Test hint"
        />
      </Formik>,
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

  test('trims the value on blur by default', () => {
    render(
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldTextArea name="testField" label="Test field" />
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Test field'), '  trim me  ');
    userEvent.tab();
    expect(screen.getByLabelText('Test field')).toHaveValue('trim me');
  });

  test('does not trim the value on blur when `trimInput` is false', () => {
    render(
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldTextArea
          name="testField"
          label="Test field"
          trimInput={false}
        />
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Test field'), '  do not trim me  ');
    userEvent.tab();
    expect(screen.getByLabelText('Test field')).toHaveValue(
      '  do not trim me  ',
    );
  });

  describe('error messages', () => {
    test('does not display validation message when untouched', () => {
      render(
        <Formik
          initialValues={{
            testField: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <FormFieldTextArea name="testField" label="Test field" />
        </Formik>,
      );

      expect(screen.queryByText('Field required')).not.toBeInTheDocument();
    });

    test('displays validation message on blur', async () => {
      render(
        <Formik
          initialValues={{
            testField: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <FormFieldTextArea name="testField" label="Test field" />
        </Formik>,
      );

      userEvent.click(screen.getByLabelText('Test field'));
      userEvent.tab();

      await waitFor(() => {
        expect(screen.queryByText('Field required')).toBeInTheDocument();
      });
    });

    test('displays validation message when form is submitted', async () => {
      render(
        <Formik
          initialValues={{ testField: '' }}
          onSubmit={noop}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          {props => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldTextArea name="testField" label="Test field" />

              <button type="submit">Submit</button>
            </form>
          )}
        </Formik>,
      );

      expect(screen.queryByText('Field required')).not.toBeInTheDocument();

      userEvent.click(screen.getByText('Submit'));

      await waitFor(() => {
        expect(screen.queryByText('Field required')).toBeInTheDocument();
      });
    });

    test('does not display validation message on blur when `showError` is false', async () => {
      render(
        <Formik
          initialValues={{
            testField: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          <FormFieldTextArea
            name="testField"
            label="Test field"
            showError={false}
          />
        </Formik>,
      );

      userEvent.click(screen.getByLabelText('Test field'));
      userEvent.tab();

      await waitFor(() => {
        expect(screen.queryByText('Field required')).not.toBeInTheDocument();
      });
    });

    test('does not display validation message when `showError` is false and form is submitted', async () => {
      render(
        <Formik
          initialValues={{
            testField: '',
          }}
          onSubmit={noop}
          validationSchema={Yup.object({
            testField: Yup.string().required('Field required'),
          })}
        >
          {props => (
            <form onSubmit={props.handleSubmit}>
              <FormFieldTextArea
                name="testField"
                label="Test field"
                showError={false}
              />

              <button type="submit">Submit</button>
            </form>
          )}
        </Formik>,
      );

      expect(screen.queryByText('Field required')).not.toBeInTheDocument();

      userEvent.click(screen.getByText('Submit'));

      await waitFor(() => {
        expect(screen.queryByText('Field required')).not.toBeInTheDocument();
      });
    });
  });
});
