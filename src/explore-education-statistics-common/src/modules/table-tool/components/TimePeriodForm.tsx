import { Form, FormFieldSelect, FormFieldset } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import {
  SubjectMeta,
  TimePeriodQuery,
} from '@common/services/tableBuilderService';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useMemo } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  start: string;
  end: string;
}

export type TimePeriodFormSubmitHandler = (values: FormValues) => void;

const formId = 'timePeriodForm';

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

  const timePeriodOptions: SelectOption[] = [
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

  const formInitialValues = useMemo(() => {
    const { startYear, startCode, endYear, endCode } = initialValues;

    const start = startYear && startCode ? `${startYear}_${startCode}` : '';
    const end = endYear && endCode ? `${endYear}_${endCode}` : '';

    return {
      start,
      end,
    };
  }, [initialValues]);

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Choose time period
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={formInitialValues}
      validateOnBlur={false}
      validationSchema={Yup.object<FormValues>({
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
      })}
      onSubmit={async values => {
        await goToNextStep(async () => {
          await onSubmit(values);
        });
      }}
    >
      {form => {
        return isActive ? (
          <Form id={formId} showSubmitError>
            <FormFieldset id="timePeriod" legend={stepHeading}>
              <FormFieldSelect
                name="start"
                label="Start date"
                disabled={form.isSubmitting}
                options={timePeriodOptions}
                order={[]}
              />
              <FormFieldSelect
                name="end"
                label="End date"
                disabled={form.isSubmitting}
                options={timePeriodOptions}
                order={[]}
              />
            </FormFieldset>

            <WizardStepFormActions
              {...stepProps}
              isSubmitting={form.isSubmitting}
            />
          </Form>
        ) : (
          <WizardStepSummary {...stepProps} goToButtonText="Edit time period">
            {stepHeading}

            <SummaryList noBorder>
              <SummaryListItem term="Time period">
                {form.values.start &&
                  form.values.end &&
                  getDisplayTimePeriod(form.values.start, form.values.end)}
              </SummaryListItem>
            </SummaryList>

            <ResetFormOnPreviousStep {...stepProps} onReset={form.resetForm} />
          </WizardStepSummary>
        );
      }}
    </Formik>
  );
};

export default TimePeriodForm;
