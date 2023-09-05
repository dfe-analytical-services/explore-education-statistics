import { FormFieldset } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import {
  SubjectMeta,
  TimePeriodQuery,
} from '@common/services/tableBuilderService';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  start: string;
  end: string;
}

export type TimePeriodFormSubmitHandler = (values: FormValues) => void;

interface Props extends InjectedWizardProps {
  initialValues?: Partial<TimePeriodQuery>;
  options: SubjectMeta['timePeriod']['options'];
  onSubmit: TimePeriodFormSubmitHandler;
}

const TimePeriodForm = ({
  initialValues = {},
  options,
  onSubmit,
  ...stepProps
}: Props) => {
  const { isActive, goToNextStep } = stepProps;

  const timePeriodOptions: SelectOption[] = useMemo(() => {
    return [
      {
        label: 'Please select',
        value: '',
      },
      ...options.map(option => {
        return {
          label: option.label,
          value: `${option.year}_${option.code}`,
        };
      }),
    ];
  }, [options]);

  const getOptionLabel = (optionValue: string) => {
    const matchingOption = timePeriodOptions.find(
      option => option.value === optionValue,
    );

    return matchingOption ? matchingOption.label : '';
  };

  const getDisplayTimePeriod = (
    startValue: string,
    endValue: string,
  ): string => {
    if (startValue === endValue) {
      return getOptionLabel(startValue);
    }
    return `${getOptionLabel(startValue)} to ${getOptionLabel(endValue)}`;
  };

  const initialFormValues = useMemo(() => {
    const { startYear, startCode, endYear, endCode } = initialValues;

    const defaultTimePeriod =
      options.length === 1 ? `${options[0].year}_${options[0].code}` : '';

    const start =
      startYear && startCode ? `${startYear}_${startCode}` : defaultTimePeriod;
    const end =
      endYear && endCode ? `${endYear}_${endCode}` : defaultTimePeriod;

    return {
      start,
      end,
    };
  }, [initialValues, options]);

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Choose time period
    </WizardStepHeading>
  );

  const handleSubmit = async (values: FormValues) => {
    await goToNextStep(async () => {
      await onSubmit(values);
    });
  };

  const validationSchema: ObjectSchema<FormValues> = useMemo(() => {
    return Yup.object({
      start: Yup.string()
        .required('Start date required')
        .test(
          'lessThanOrEqual',
          'Start date must be before or same as end date',
          function lessThanOrEqual(value: string) {
            if (!value) {
              return true;
            }

            // eslint-disable-next-line react/no-this-in-sfc
            const end: string = this.resolve(Yup.ref('end'));

            if (!end) {
              return true;
            }

            const startIndex = timePeriodOptions.findIndex(
              option => option.value === value,
            );
            const endIndex = timePeriodOptions.findIndex(
              option => option.value === end,
            );

            return startIndex <= endIndex;
          },
        ),
      end: Yup.string()
        .required('End date required')
        .test(
          'moreThanOrEqual',
          'End date must be after or same as start date',
          function moreThanOrEqual(value: string) {
            if (!value) {
              return true;
            }

            // eslint-disable-next-line react/no-this-in-sfc
            const start: string = this.resolve(Yup.ref('start'));

            if (!start) {
              return true;
            }

            const endIndex = timePeriodOptions.findIndex(
              option => option.value === value,
            );
            const startIndex = timePeriodOptions.findIndex(
              option => option.value === start,
            );

            return endIndex >= startIndex;
          },
        ),
    });
  }, [timePeriodOptions]);

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialFormValues}
      validationSchema={validationSchema}
    >
      {({ formState, getValues, reset }) => {
        if (isActive) {
          return (
            <RHFForm
              id="timePeriodForm"
              showSubmitError
              onSubmit={handleSubmit}
            >
              <FormFieldset
                id="timePeriod"
                hint={options.length === 1 && 'Only one time period available.'}
                legend={stepHeading}
              >
                <RHFFormFieldSelect<FormValues>
                  name="start"
                  label="Start date"
                  disabled={formState.isSubmitting}
                  options={timePeriodOptions}
                  order={RHFFormFieldSelect.unordered}
                />
                <RHFFormFieldSelect<FormValues>
                  name="end"
                  label="End date"
                  disabled={formState.isSubmitting}
                  options={timePeriodOptions}
                  order={RHFFormFieldSelect.unordered}
                />
              </FormFieldset>

              <WizardStepFormActions
                {...stepProps}
                isSubmitting={formState.isSubmitting}
              />
            </RHFForm>
          );
        }

        const values = getValues();

        return (
          <WizardStepSummary {...stepProps} goToButtonText="Edit time period">
            {stepHeading}

            <SummaryList noBorder>
              <SummaryListItem term="Time period">
                {values.start &&
                  values.end &&
                  getDisplayTimePeriod(values.start, values.end)}
              </SummaryListItem>
            </SummaryList>

            <ResetFormOnPreviousStep {...stepProps} onReset={reset} />
          </WizardStepSummary>
        );
      }}
    </FormProvider>
  );
};

export default TimePeriodForm;
