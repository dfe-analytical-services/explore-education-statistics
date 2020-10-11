import { Form, FormFieldSelect, FormFieldset } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ResetFormOnPreviousStep from '@common/modules/table-tool/components/ResetFormOnPreviousStep';
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

interface Props {
  options: SubjectMeta['timePeriod']['options'];
  initialValues?: { timePeriod?: TimePeriodQuery };
  onSubmit: TimePeriodFormSubmitHandler;
}

const TimePeriodForm = (props: Props & InjectedWizardProps) => {
  const {
    options,
    onSubmit,
    isActive,
    goToNextStep,
    currentStep,
    stepNumber,
    initialValues = { timePeriod: undefined },
  } = props;

  const formId = 'timePeriodForm';

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

  const formInitialValues = useMemo(() => {
    let start = '';
    let end = '';

    if (initialValues && initialValues.timePeriod) {
      start = `${initialValues.timePeriod.startYear}_${initialValues.timePeriod.startCode}`;
      end = `${initialValues.timePeriod.endYear}_${initialValues.timePeriod.endCode}`;
    }

    return {
      start,
      end,
    };
  }, [initialValues]);

  const getOptionLabel = (optionValue: string) => {
    const matchingOption = timePeriodOptions.find(
      option => option.value === optionValue,
    );

    return matchingOption ? matchingOption.label : '';
  };

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
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
        await onSubmit(values);
        goToNextStep();
      }}
    >
      {form => {
        return isActive ? (
          <Form id={formId} showSubmitError>
            <FormFieldset id={`${formId}-timePeriod`} legend={stepHeading}>
              <FormFieldSelect
                name="start"
                id={`${formId}-start`}
                label="Start date"
                disabled={form.isSubmitting}
                options={timePeriodOptions}
                order={[]}
              />
              <FormFieldSelect
                name="end"
                id={`${formId}-end`}
                label="End date"
                disabled={form.isSubmitting}
                options={timePeriodOptions}
                order={[]}
              />
            </FormFieldset>

            <WizardStepFormActions {...props} formId={formId} />
          </Form>
        ) : (
          <>
            {stepHeading}

            <ResetFormOnPreviousStep
              currentStep={currentStep}
              stepNumber={stepNumber}
            />

            <SummaryList noBorder>
              <SummaryListItem term="Start date">
                {form.values.start && getOptionLabel(form.values.start)}
              </SummaryListItem>
              <SummaryListItem term="End date">
                {form.values.end && getOptionLabel(form.values.end)}
              </SummaryListItem>
            </SummaryList>
          </>
        );
      }}
    </Formik>
  );
};

export default TimePeriodForm;
