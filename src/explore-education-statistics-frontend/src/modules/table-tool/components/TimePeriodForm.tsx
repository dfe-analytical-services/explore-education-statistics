import Button from '@common/components/Button';
import {
  Form,
  FormFieldSelect,
  FormFieldset,
  FormGroup,
} from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Yup from '@common/lib/validation/yup';
import TimePeriod from '@common/services/types/TimePeriod';
import { Comparison } from '@common/types/util';
import { Formik, FormikProps } from 'formik';
import React from 'react';
import { MetaSpecification } from './meta/initialSpec';
import { InjectedWizardProps } from './Wizard';
import WizardStepHeading from './WizardStepHeading';

interface FormValues {
  start: string;
  end: string;
}

interface Props {
  specification: MetaSpecification;
  onSubmit: (values: FormValues) => void;
}

const TimePeriodForm = (props: Props & InjectedWizardProps) => {
  const {
    specification,
    onSubmit,
    isActive,
    goToNextStep,
    goToPreviousStep,
  } = props;
  const startEndDateValues = specification.timePeriod.options.map(
    ({ code, year }) => `${year}_${code}`,
  );

  const timePeriodOptions: SelectOption[] = specification.timePeriod.options.map(
    option => {
      return {
        label: option.label,
        value: `${option.year}_${option.code}`,
      };
    },
  );

  const stepHeading = (
    <WizardStepHeading {...props} fieldsetHeading>
      Choose time period
    </WizardStepHeading>
  );

  return (
    <Formik<FormValues>
      onSubmit={values => {
        onSubmit(values);
        goToNextStep();
      }}
      initialValues={{
        start: '2012_AY',
        end: '2016_AY',
      }}
      validationSchema={Yup.object<FormValues>({
        end: Yup.string()
          .required('End date is required')
          .oneOf(startEndDateValues, 'Must be one of provided dates')
          .test(
            'moreThanOrEqual',
            'Must be after or same as start date',
            function moreThanOrEqual(value: string) {
              const start: string = this.resolve(Yup.ref('start'));

              if (!start) {
                return false;
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
        start: Yup.string()
          .required('Start date is required')
          .oneOf(startEndDateValues, 'Must be one of provided dates')
          .test(
            'lessThanOrEqual',
            'Must be before or same as end date',
            function lessThanOrEqual(value: string) {
              const end: string = this.resolve(Yup.ref('end'));

              if (!end) {
                return false;
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
      })}
      render={(form: FormikProps<FormValues>) => {
        return isActive ? (
          <Form {...form} id="timePeriodForm">
            <FormFieldset id="timePeriodForm-timePeriod" legend={stepHeading}>
              <FormFieldSelect
                name="start"
                id="timePeriodForm-timePeriod-start"
                label="Start date"
                options={timePeriodOptions}
              />
              <FormFieldSelect
                name="end"
                id="timePeriodForm-timePeriod-end"
                label="End date"
                options={timePeriodOptions}
              />
            </FormFieldset>

            <FormGroup>
              <Button type="submit">Next step</Button>

              <Button
                type="button"
                variant="secondary"
                onClick={goToPreviousStep}
              >
                Previous step
              </Button>
            </FormGroup>
          </Form>
        ) : (
          <>
            {stepHeading}
            <SummaryList noBorder>
              <SummaryListItem term="Start date">
                {TimePeriod.fromString(form.values.start).label}
              </SummaryListItem>
              <SummaryListItem term="End date">
                {TimePeriod.fromString(form.values.end).label}
              </SummaryListItem>
            </SummaryList>
          </>
        );
      }}
    />
  );
};

export default TimePeriodForm;
