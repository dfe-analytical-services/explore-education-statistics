import {
  Form,
  FormFieldSelect,
  FormFieldset,
  Formik,
} from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import { PublicationSubjectMeta } from '@common/services/tableBuilderService';
import TimePeriod from '@common/services/types/TimePeriod';
import { Comparison } from '@common/types/util';
import useResetFormOnPreviousStep from '@frontend/modules/table-tool/components/hooks/useResetFormOnPreviousStep';
import { FormikProps } from 'formik';
import React, { useRef } from 'react';
import { InjectedWizardProps } from './Wizard';
import WizardStepFormActions from './WizardStepFormActions';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  start: string;
  end: string;
}

export type TimePeriodFormSubmitHandler = (values: FormValues) => void;

interface Props {
  options: PublicationSubjectMeta['timePeriod']['options'];
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
  } = props;

  const formikRef = useRef<Formik<FormValues>>(null);
  const formId = 'timePeriodForm';

  useResetFormOnPreviousStep(formikRef, currentStep, stepNumber);

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

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose time period
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      enableReinitialize
      ref={formikRef}
      onSubmit={async values => {
        await onSubmit(values);
        goToNextStep();
      }}
      initialValues={{
        start: '',
        end: '',
      }}
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

              const end: string = this.resolve(Yup.ref('end'));

              if (!end) {
                return true;
              }

              const startTime = TimePeriod.fromString(value);
              const endTime = TimePeriod.fromString(end);

              const comparison = startTime.compare(endTime);

              return (
                comparison === Comparison.LessThan ||
                comparison === Comparison.EqualTo
              );
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

              const start: string = this.resolve(Yup.ref('start'));

              if (!start) {
                return true;
              }

              const endTime = TimePeriod.fromString(value);
              const startTime = TimePeriod.fromString(start);

              const comparison = endTime.compare(startTime);

              return (
                comparison === Comparison.GreaterThan ||
                comparison === Comparison.EqualTo
              );
            },
          ),
      })}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form id={formId}>
            <FormFieldset id={`${formId}-timePeriod`} legend={stepHeading}>
              <FormFieldSelect
                name="start"
                id={`${formId}-start`}
                label="Start date"
                options={timePeriodOptions}
              />
              <FormFieldSelect
                name="end"
                id={`${formId}-end`}
                label="End date"
                options={timePeriodOptions}
              />
            </FormFieldset>

            <WizardStepFormActions {...props} form={form} formId={formId} />
          </Form>
        ) : (
          <>
            {stepHeading}
            <SummaryList noBorder>
              <SummaryListItem term="Start date">
                {form.values.start &&
                  TimePeriod.fromString(form.values.start).label}
              </SummaryListItem>
              <SummaryListItem term="End date">
                {form.values.end &&
                  TimePeriod.fromString(form.values.end).label}
              </SummaryListItem>
            </SummaryList>
          </>
        );
      }}
    />
  );
};

export default TimePeriodForm;
