import { Form } from '@common/components/form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import { PartialDate } from '@common/utils/date/partialDate';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import noop from 'lodash/noop';
import React from 'react';

describe('FormFieldDateInput', () => {
  test('renders correctly', () => {
    const { container } = render(
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
        />
      </Formik>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders with an error message correctly', () => {
    render(
      <Formik
        initialValues={{}}
        initialErrors={{
          startDate: 'The date is wrong',
        }}
        initialTouched={{
          startDate: true,
        }}
        onSubmit={noop}
      >
        <FormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'aria-describedby',
      'startDate-error',
    );
    expect(screen.getByText('The date is wrong')).toHaveAttribute(
      'id',
      'startDate-error',
    );
  });

  test('renders with a hint correctly', () => {
    render(
      <Formik initialValues={{}} onSubmit={noop}>
        <FormFieldDateInput
          legend="Start date"
          hint="Test hint"
          id="startDate"
          name="startDate"
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'aria-describedby',
      'startDate-hint',
    );
    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'startDate-hint',
    );
  });

  test('renders with a hint and error correctly', () => {
    render(
      <Formik
        initialValues={{}}
        initialErrors={{
          startDate: 'The date is wrong',
        }}
        initialTouched={{
          startDate: true,
        }}
        onSubmit={noop}
      >
        <FormFieldDateInput
          legend="Start date"
          hint="Test hint"
          id="startDate"
          name="startDate"
        />
      </Formik>,
    );

    expect(screen.getByRole('group')).toHaveAttribute(
      'aria-describedby',
      'startDate-error startDate-hint',
    );
    expect(screen.getByText('Test hint')).toHaveAttribute(
      'id',
      'startDate-hint',
    );
    expect(screen.getByText('The date is wrong')).toHaveAttribute(
      'id',
      'startDate-error',
    );
  });

  test('renders with correct defaults ids with form', () => {
    render(
      <Formik
        initialValues={{}}
        initialErrors={{
          startDate: 'The date is wrong',
        }}
        initialTouched={{
          startDate: true,
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldDateInput
            legend="Start date"
            hint="Test hint"
            name="startDate"
          />
        </Form>
      </Formik>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-startDate-hint',
    );
    expect(group.getByText('The date is wrong')).toHaveAttribute(
      'id',
      'testForm-startDate-error',
    );
    expect(group.getByLabelText('Day')).toHaveAttribute(
      'id',
      'testForm-startDate-day',
    );
    expect(group.getByLabelText('Month')).toHaveAttribute(
      'id',
      'testForm-startDate-month',
    );
    expect(group.getByLabelText('Year')).toHaveAttribute(
      'id',
      'testForm-startDate-year',
    );
  });

  test('renders with correct defaults ids without form', () => {
    render(
      <Formik
        initialValues={{}}
        initialErrors={{
          startDate: 'The date is wrong',
        }}
        initialTouched={{
          startDate: true,
        }}
        onSubmit={noop}
      >
        <FormFieldDateInput
          legend="Start date"
          hint="Test hint"
          name="startDate"
        />
      </Formik>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByText('Test hint')).toHaveAttribute(
      'id',
      'startDate-hint',
    );
    expect(group.getByText('The date is wrong')).toHaveAttribute(
      'id',
      'startDate-error',
    );
    expect(group.getByLabelText('Day')).toHaveAttribute('id', 'startDate-day');
    expect(group.getByLabelText('Month')).toHaveAttribute(
      'id',
      'startDate-month',
    );
    expect(group.getByLabelText('Year')).toHaveAttribute(
      'id',
      'startDate-year',
    );
  });

  test('renders with correct custom ids with form', () => {
    render(
      <Formik
        initialValues={{}}
        initialErrors={{
          startDate: 'The date is wrong',
        }}
        initialTouched={{
          startDate: true,
        }}
        onSubmit={noop}
      >
        <Form id="testForm">
          <FormFieldDateInput
            legend="Start date"
            hint="Test hint"
            name="startDate"
            id="customId"
          />
        </Form>
      </Formik>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-customId-hint',
    );
    expect(group.getByText('The date is wrong')).toHaveAttribute(
      'id',
      'testForm-customId-error',
    );
    expect(group.getByLabelText('Day')).toHaveAttribute(
      'id',
      'testForm-customId-day',
    );
    expect(group.getByLabelText('Month')).toHaveAttribute(
      'id',
      'testForm-customId-month',
    );
    expect(group.getByLabelText('Year')).toHaveAttribute(
      'id',
      'testForm-customId-year',
    );
  });

  test('renders with correct custom ids without form', () => {
    render(
      <Formik
        initialValues={{}}
        initialErrors={{
          startDate: 'The date is wrong',
        }}
        initialTouched={{
          startDate: true,
        }}
        onSubmit={noop}
      >
        <FormFieldDateInput
          legend="Start date"
          hint="Test hint"
          name="startDate"
          id="customId"
        />
      </Formik>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByText('Test hint')).toHaveAttribute('id', 'customId-hint');
    expect(group.getByText('The date is wrong')).toHaveAttribute(
      'id',
      'customId-error',
    );
    expect(group.getByLabelText('Day')).toHaveAttribute('id', 'customId-day');
    expect(group.getByLabelText('Month')).toHaveAttribute(
      'id',
      'customId-month',
    );
    expect(group.getByLabelText('Year')).toHaveAttribute('id', 'customId-year');
  });

  test('sets a valid UTC date as a form value', async () => {
    const onChange = jest.fn();

    render(
      <Formik<{ startDate?: Date }> initialValues={{}} onSubmit={noop}>
        {form => {
          onChange(form.values.startDate);

          return (
            <FormFieldDateInput
              legend="Start date"
              id="startDate"
              name="startDate"
            />
          );
        }}
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Day'), '10');
    userEvent.type(screen.getByLabelText('Month'), '12');
    userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(10);
    expect(screen.getByLabelText('Month')).toHaveValue(12);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    expect(onChange).toHaveBeenCalledWith(new Date('2020-12-10T00:00:00.000Z'));
  });

  test('does not set an invalid date as a form value', async () => {
    const onChange = jest.fn();

    render(
      <Formik<{ startDate?: Date }> initialValues={{}} onSubmit={noop}>
        {form => {
          onChange(form.values.startDate);

          return (
            <FormFieldDateInput
              legend="Start date"
              id="startDate"
              name="startDate"
            />
          );
        }}
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Day'), '32');
    userEvent.type(screen.getByLabelText('Month'), '12');
    userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(32);
    expect(screen.getByLabelText('Month')).toHaveValue(12);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    expect(onChange).toHaveBeenCalledWith(undefined);
  });

  test('does not set a partial date as a form value', async () => {
    const onChange = jest.fn();

    render(
      <Formik<{ startDate?: Date }> initialValues={{}} onSubmit={noop}>
        {form => {
          onChange(form.values.startDate);

          return (
            <FormFieldDateInput
              legend="Start date"
              id="startDate"
              name="startDate"
            />
          );
        }}
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Day'), '10');

    expect(screen.getByLabelText('Day')).toHaveValue(10);
    expect(onChange).toHaveBeenCalledWith(undefined);
  });

  test('can hide day field when `type = partialDate` and `partialDateType = monthYear`', () => {
    render(
      <Formik<{ startDate?: PartialDate }> initialValues={{}} onSubmit={noop}>
        <FormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
          type="partialDate"
          partialDateType="monthYear"
        />
      </Formik>,
    );

    expect(screen.queryByLabelText('Day')).not.toBeInTheDocument();
    expect(screen.getByLabelText('Month')).toBeInTheDocument();
    expect(screen.getByLabelText('Year')).toBeInTheDocument();
  });

  test('does not hide day field when `type = date` and `partialDateType = monthYear`', () => {
    render(
      <Formik<{ startDate?: PartialDate }> initialValues={{}} onSubmit={noop}>
        <FormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
          type="date"
          partialDateType="monthYear"
        />
      </Formik>,
    );

    expect(screen.getByLabelText('Day')).toBeInTheDocument();
    expect(screen.getByLabelText('Month')).toBeInTheDocument();
    expect(screen.getByLabelText('Year')).toBeInTheDocument();
  });

  test('can set a full PartialDate as a form value when `type = partialDate`', async () => {
    const onChange = jest.fn();

    render(
      <Formik<{ startDate?: PartialDate }> initialValues={{}} onSubmit={noop}>
        {form => {
          onChange(form.values.startDate);

          return (
            <FormFieldDateInput
              legend="Start date"
              id="startDate"
              name="startDate"
              type="partialDate"
            />
          );
        }}
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Day'), '15');
    userEvent.type(screen.getByLabelText('Month'), '6');
    userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(15);
    expect(screen.getByLabelText('Month')).toHaveValue(6);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    expect(onChange).toHaveBeenCalledWith({
      day: 15,
      month: 6,
      year: 2020,
    });
  });

  test('can set only a day for a form value when `type = partialDate`', async () => {
    const onChange = jest.fn();

    render(
      <Formik<{ startDate?: PartialDate }> initialValues={{}} onSubmit={noop}>
        {form => {
          onChange(form.values.startDate);

          return (
            <FormFieldDateInput
              legend="Start date"
              id="startDate"
              name="startDate"
              type="partialDate"
            />
          );
        }}
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Day'), '15');

    expect(screen.getByLabelText('Day')).toHaveValue(15);

    expect(onChange).toHaveBeenCalledWith({
      day: 15,
    });
  });

  test('can set an invalid PartialDate as a form value when `type = partialDate`', async () => {
    const onChange = jest.fn();

    render(
      <Formik<{ startDate?: PartialDate }> initialValues={{}} onSubmit={noop}>
        {form => {
          onChange(form.values.startDate);

          return (
            <FormFieldDateInput
              legend="Start date"
              id="startDate"
              name="startDate"
              type="partialDate"
            />
          );
        }}
      </Formik>,
    );

    userEvent.type(screen.getByLabelText('Day'), '32');
    userEvent.type(screen.getByLabelText('Month'), '6');
    userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(32);
    expect(screen.getByLabelText('Month')).toHaveValue(6);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    expect(onChange).toHaveBeenCalledWith({
      day: 32,
      month: 6,
      year: 2020,
    });
  });
});
