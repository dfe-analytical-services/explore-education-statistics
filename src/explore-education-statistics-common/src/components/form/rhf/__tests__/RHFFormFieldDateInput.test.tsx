import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldDateInput from '@common/components/form/rhf/RHFFormFieldDateInput';
import { PartialDate } from '@common/utils/date/partialDate';
import Yup from '@common/validation/yup';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('RHFFormFieldDateInput', () => {
  test('renders correctly', () => {
    const { container } = render(
      <FormProvider initialValues={{}}>
        <RHFFormFieldDateInput
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
        <RHFFormFieldDateInput
          legend="Start date"
          id="startDate"
          name="startDate"
        />
      </FormProvider>,
    );

    userEvent.tab();
    userEvent.tab();

    await waitFor(() => {
      expect(screen.getByText('Select a date')).toHaveAttribute(
        'id',
        'startDate-error',
      );
    });

    expect(screen.getByRole('group')).toHaveAttribute(
      'aria-describedby',
      'startDate-error',
    );
  });

  test('renders with a hint correctly', () => {
    render(
      <FormProvider initialValues={{}}>
        <RHFFormFieldDateInput
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
        <RHFForm id="testForm" onSubmit={noop}>
          <RHFFormFieldDateInput
            legend="Start date"
            hint="Test hint"
            name="startDate"
          />
        </RHFForm>
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

    userEvent.tab();
    userEvent.tab();

    await waitFor(() => {
      expect(group.getByText('Select a date')).toHaveAttribute(
        'id',
        'testForm-startDate-error',
      );
    });
  });

  test('renders with correct defaults ids without form', async () => {
    render(
      <FormProvider
        initialValues={{}}
        validationSchema={Yup.object({
          startDate: Yup.date().required('Select a date'),
        })}
      >
        <RHFFormFieldDateInput
          legend="Start date"
          hint="Test hint"
          name="startDate"
        />
      </FormProvider>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByText('Test hint')).toHaveAttribute(
      'id',
      'startDate-hint',
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

    userEvent.tab();
    userEvent.tab();

    await waitFor(() => {
      expect(group.getByText('Select a date')).toHaveAttribute(
        'id',
        'startDate-error',
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
        <RHFForm id="testForm" onSubmit={noop}>
          <RHFFormFieldDateInput
            legend="Start date"
            hint="Test hint"
            name="startDate"
            id="customId"
          />
        </RHFForm>
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

    userEvent.tab();
    userEvent.tab();

    await waitFor(() => {
      expect(group.getByText('Select a date')).toHaveAttribute(
        'id',
        'testForm-customId-error',
      );
    });
  });

  test('renders with correct custom ids without form', async () => {
    render(
      <FormProvider
        initialValues={{}}
        validationSchema={Yup.object({
          startDate: Yup.date().required('Select a date'),
        })}
      >
        <RHFFormFieldDateInput
          legend="Start date"
          hint="Test hint"
          name="startDate"
          id="customId"
        />
      </FormProvider>,
    );

    const group = within(screen.getByRole('group'));

    expect(group.getByLabelText('Day')).toHaveAttribute('id', 'customId-day');
    expect(group.getByLabelText('Month')).toHaveAttribute(
      'id',
      'customId-month',
    );
    expect(group.getByLabelText('Year')).toHaveAttribute('id', 'customId-year');

    userEvent.tab();
    userEvent.tab();

    await waitFor(() => {
      expect(group.getByText('Select a date')).toHaveAttribute(
        'id',
        'customId-error',
      );
    });
  });

  test('sets a valid UTC date as a form value', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: Date }> initialValues={{}}>
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
          />
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '10');
    await userEvent.type(screen.getByLabelText('Month'), '12');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(10);
    expect(screen.getByLabelText('Month')).toHaveValue(12);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
          />
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '32');
    await userEvent.type(screen.getByLabelText('Month'), '12');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(32);
    expect(screen.getByLabelText('Month')).toHaveValue(12);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({ startDate: undefined });
    });
  });

  test('does not set a partial date as a form value', async () => {
    const handleSubmit = jest.fn();

    render(
      <FormProvider<{ startDate?: Date }> initialValues={{}}>
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
          />
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '10');

    expect(screen.getByLabelText('Day')).toHaveValue(10);

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({ startDate: undefined });
    });
  });

  test('can hide day field when `type = partialDate` and `partialDateType = monthYear`', () => {
    render(
      <FormProvider<{ startDate?: PartialDate }> initialValues={{}}>
        <RHFFormFieldDateInput
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
        <RHFFormFieldDateInput
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
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
            type="partialDate"
          />
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '15');
    await userEvent.type(screen.getByLabelText('Month'), '6');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(15);
    expect(screen.getByLabelText('Month')).toHaveValue(6);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
            type="partialDate"
          />{' '}
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '15');

    expect(screen.getByLabelText('Day')).toHaveValue(15);

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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
        <RHFForm id="testForm" onSubmit={handleSubmit}>
          <RHFFormFieldDateInput
            legend="Start date"
            id="startDate"
            name="startDate"
            type="partialDate"
          />{' '}
          <button type="submit">Submit</button>
        </RHFForm>
      </FormProvider>,
    );

    await userEvent.type(screen.getByLabelText('Day'), '32');
    await userEvent.type(screen.getByLabelText('Month'), '6');
    await userEvent.type(screen.getByLabelText('Year'), '2020');

    expect(screen.getByLabelText('Day')).toHaveValue(32);
    expect(screen.getByLabelText('Month')).toHaveValue(6);
    expect(screen.getByLabelText('Year')).toHaveValue(2020);

    userEvent.click(screen.getByRole('button', { name: 'Submit' }));

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