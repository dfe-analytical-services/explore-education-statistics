import metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { SelectOption } from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik, FormikHelpers } from 'formik';
import React, { ReactNode } from 'react';
import { ObjectSchema } from 'yup';

export interface ReleaseSummaryFormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: string;
  releaseType?: ReleaseType;
}

const formId = 'releaseSummaryForm';

interface Props<FormValues extends ReleaseSummaryFormValues> {
  additionalFields?: ReactNode;
  submitText: string;
  initialValues: (
    timePeriodCoverageGroups: TimePeriodCoverageGroup[],
  ) => FormValues;
  validationSchema?: (
    baseSchema: ObjectSchema<ReleaseSummaryFormValues>,
  ) => ObjectSchema<FormValues | ReleaseSummaryFormValues>;
  onSubmit: (values: FormValues, actions: FormikHelpers<FormValues>) => void;
  onCancel: () => void;
}

const ReleaseSummaryForm = <
  FormValues extends ReleaseSummaryFormValues = ReleaseSummaryFormValues,
>({
  additionalFields,
  submitText,
  initialValues,
  validationSchema,
  onSubmit,
  onCancel,
}: Props<FormValues>) => {
  const { value: timePeriodCoverageGroups, isLoading } = useAsyncRetry<
    TimePeriodCoverageGroup[]
  >(async () => metaService.getTimePeriodCoverageGroups());

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!timePeriodCoverageGroups) {
    return <WarningMessage>Could not load release summary</WarningMessage>;
  }

  const timePeriodOptions = timePeriodCoverageGroups.reduce<
    Dictionary<SelectOption[]>
  >((acc, group) => {
    acc[group.category.label] = group.timeIdentifiers.map(
      ({ identifier }) => identifier,
    );

    return acc;
  }, {});

  const findTimePeriodCoverageGroup = (
    code: string,
  ): TimePeriodCoverageGroup | undefined => {
    return (
      timePeriodCoverageGroups.find(group =>
        group.timeIdentifiers
          .map(option => option.identifier.value)
          .some(id => id === code),
      ) || timePeriodCoverageGroups[0]
    );
  };

  const baseSchema: ObjectSchema<ReleaseSummaryFormValues> = Yup.object({
    timePeriodCoverageCode: Yup.string().required('Choose a time period'),
    timePeriodCoverageStartYear: Yup.string()
      .required('Enter a year')
      .length(4, 'Year must be exactly 4 characters'),

    releaseType: Yup.string<ReleaseType>()
      .required('Choose a release type')
      .oneOf(Object.keys(releaseTypes) as ReleaseType[]),
  });

  return (
    <Formik<FormValues>
      enableReinitialize
      initialValues={initialValues(timePeriodCoverageGroups)}
      validationSchema={
        validationSchema ? validationSchema(baseSchema) : baseSchema
      }
      onSubmit={onSubmit}
    >
      {form => {
        const timePeriodLabel =
          findTimePeriodCoverageGroup(form.values.timePeriodCoverageCode)
            ?.category.label ?? '';

        return (
          <Form id={formId}>
            <FormFieldset
              hint="For 6 digit years, enter only the first four digits in this box e.g. for 2017/18, only enter 2017."
              id="timePeriodCoverage"
              legend="Select time period coverage"
              legendSize="m"
            >
              <FormFieldSelect<ReleaseSummaryFormValues>
                label="Type"
                name="timePeriodCoverageCode"
                optGroups={timePeriodOptions}
              />
              <FormFieldNumberInput<ReleaseSummaryFormValues>
                name="timePeriodCoverageStartYear"
                label={`
                      ${
                        [
                          'Month',
                          'Term',
                          'Week',
                          'Other',
                          'Financial year part',
                        ].includes(timePeriodLabel)
                          ? 'Year'
                          : timePeriodLabel
                      }
                    `}
                width={4}
              />
            </FormFieldset>

            <FormFieldRadioGroup<ReleaseSummaryFormValues>
              legend="Release type"
              name="releaseType"
              options={(Object.keys(releaseTypes) as ReleaseType[]).map(
                type => ({
                  label: releaseTypes[type],
                  value: type,
                }),
              )}
            />

            {additionalFields}

            <ButtonGroup>
              <Button type="submit">{submitText}</Button>
              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </Form>
        );
      }}
    </Formik>
  );
};

export default ReleaseSummaryForm;
