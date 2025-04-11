import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import { PartialDate } from '@common/utils/date/partialDate';
import Yup from '@common/validation/yup';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';
import Button from '@common/components/Button';

describe('FormFieldDateInput', () => {
  test('renders correctly', () => {
    const { container } = render(
      <FormProvider initialValues={{}}>
        <FormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
        />
      </FormProvider>,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders an error message correctly', async () => {
    render(
      <FormProvider
        initialValues={{}}
        validationSchema={Yup.object({
          startDate: Yup.date().required('Select a date'),
        })}
      >
        <Form id="testForm" onSubmit={noop} visuallyHiddenErrorSummary>
          <FormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
          />
          <Button type="submit">Submit</Button>
        </Form>
      </FormProvider>,
    );

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    const group = within(screen.getByRole('group'));
    await waitFor(() => {
      expect(group.getByText('Select a date')).toHaveAttribute(
        'id',
        'testForm-startDate-error',
      );
    });

    expect(screen.getByRole('group')).toHaveAttribute(
      'aria-describedby',
      'testForm-startDate-error',
    );
  });

  test('renders with a hint correctly', () => {
    render(
      <FormProvider initialValues={{}}>
        <FormFieldDateInput
          legend="Start date"
          hint="Test hint"
          id="startDate"
          name="startDate"
        />
      </FormProvider>,
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

  test('renders with correct defaults ids with form', async () => {
    render(
      <FormProvider
        initialValues={{}}
        validationSchema={Yup.object({
          startDate: Yup.date().required('Select a date'),
        })}
      >
        <Form id="testForm" onSubmit={noop}>
          <FormFieldDateInput
            legend="Start date"
            hint="Test hint"
            name="startDate"
          />
          <Button type="submit">Submit</Button>
        </Form>
      </FormProvider>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-startDate-hint',
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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(group.getByText('Select a date')).toHaveAttribute(
        'id',
        'testForm-startDate-error',
      );
    });
  });

  test('renders with correct custom ids with form', async () => {
    render(
      <FormProvider
        initialValues={{}}
        validationSchema={Yup.object({
          startDate: Yup.date().required('Select a date'),
        })}
      >
        <Form id="testForm" onSubmit={noop}>
          <FormFieldDateInput
            legend="Start date"
            hint="Test hint"
            name="startDate"
            id="customId"
          />
          <Button type="submit">Submit</Button>
        </Form>
      </FormProvider>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByText('Test hint')).toHaveAttribute(
      'id',
      'testForm-customId-hint',
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

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(group.getByText('Select a date')).toHaveAttribute(
        'id',
        'testForm-customId-error',
      );
    });
  });

  test('sets a valid UTC date as a form value', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: Date }> initialValues={{}}>
        <Form id="testForm" onSubmit={handleSubmit}>
          <FormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
          />
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '10');
    await userEvent.type(screen.getByLabelText('Month'), '12');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(10);
    expect(screen.getByLabelText('Month')).toHaveValue(12);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        startDate: new Date('2020-12-10T00:00:00.000Z'),
      });
    });
  });

  test('does not set an invalid date as a form value', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: Date }> initialValues={{}}>
        <Form id="testForm" onSubmit={handleSubmit}>
          <FormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
          />
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '32');
    await userEvent.type(screen.getByLabelText('Month'), '12');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(32);
    expect(screen.getByLabelText('Month')).toHaveValue(12);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({ startDate: undefined });
    });
  });

  test('does not set a partial date as a form value', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: Date }> initialValues={{}}>
        <Form id="testForm" onSubmit={handleSubmit}>
          <FormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
          />
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '10');

    expect(screen.getByLabelText('Day')).toHaveValue(10);

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({ startDate: undefined });
    });
  });

  test('can hide day field when `type = partialDate` and `partialDateType = monthYear`', () => {
    render(
      <FormProvider<{ startDate?: PartialDate }> initialValues={{}}>
        <FormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
          type="partialDate"
          partialDateType="monthYear"
        />
      </FormProvider>,
    );

    expect(screen.queryByLabelText('Day')).not.toBeInTheDocument();
    expect(screen.getByLabelText('Month')).toBeInTheDocument();
    expect(screen.getByLabelText('Year')).toBeInTheDocument();
  });

  test('does not hide day field when `type = date` and `partialDateType = monthYear`', () => {
    render(
      <FormProvider<{ startDate?: PartialDate }> initialValues={{}}>
        <FormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
          type="date"
          partialDateType="monthYear"
        />
      </FormProvider>,
    );

    expect(screen.getByLabelText('Day')).toBeInTheDocument();
    expect(screen.getByLabelText('Month')).toBeInTheDocument();
    expect(screen.getByLabelText('Year')).toBeInTheDocument();
  });

  test('can set a full PartialDate as a form value when `type = partialDate`', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: PartialDate }> initialValues={{}}>
        <Form id="testForm" onSubmit={handleSubmit}>
          <FormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
            type="partialDate"
          />
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '15');
    await userEvent.type(screen.getByLabelText('Month'), '6');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(15);
    expect(screen.getByLabelText('Month')).toHaveValue(6);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        startDate: {
          day: 15,
          month: 6,
          year: 2020,
        },
      });
    });
  });

  test('can set only a day for a form value when `type = partialDate`', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: PartialDate }> initialValues={{}}>
        <Form id="testForm" onSubmit={handleSubmit}>
          <FormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
            type="partialDate"
          />{' '}
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '15');

    expect(screen.getByLabelText('Day')).toHaveValue(15);

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        startDate: {
          day: 15,
        },
      });
    });
  });

  test('can set an invalid PartialDate as a form value when `type = partialDate`', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: PartialDate }> initialValues={{}}>
        <Form id="testForm" onSubmit={handleSubmit}>
          <FormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
            type="partialDate"
          />{' '}
          <button type="submit">Submit</button>
        </Form>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '32');
    await userEvent.type(screen.getByLabelText('Month'), '6');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(32);
    expect(screen.getByLabelText('Month')).toHaveValue(6);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    await userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        startDate: {
          day: 32,
          month: 6,
          year: 2020,
        },
      });
    });
  });
});
