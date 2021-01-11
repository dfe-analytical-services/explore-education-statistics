import metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import { IdTitlePair } from '@admin/services/types/common';
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
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik, FormikHelpers } from 'formik';
import React, { ReactNode } from 'react';
import { ObjectSchema } from 'yup';

export interface ReleaseSummaryFormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: string;
  releaseTypeId: string;
}

const formId = 'releaseSummaryForm';

interface Model {
  releaseTypes: IdTitlePair[];
  timePeriodCoverageGroups: TimePeriodCoverageGroup[];
}

interface Props<FormValues extends ReleaseSummaryFormValues> {
  additionalFields?: ReactNode;
  submitText: string;
  initialValues: (
    timePeriodCoverageGroups: TimePeriodCoverageGroup[],
  ) => FormValues;
  validationSchema?: (
    baseSchema: ObjectSchema<ReleaseSummaryFormValues>,
  ) => ObjectSchema<FormValues>;
  onSubmit: (values: FormValues, actions: FormikHelpers<FormValues>) => void;
  onCancel: () => void;
}

const ReleaseSummaryForm = <
  FormValues extends ReleaseSummaryFormValues = ReleaseSummaryFormValues
>({
  additionalFields,
  submitText,
  initialValues,
  validationSchema,
  onSubmit,
  onCancel,
}: Props<FormValues>) => {
  const { value: model, isLoading } = useAsyncRetry<Model>(async () => {
    const [releaseTypes, timePeriodCoverageGroups] = await Promise.all([
      metaService.getReleaseTypes(),
      metaService.getTimePeriodCoverageGroups(),
    ]);

    return {
      releaseTypes,
      timePeriodCoverageGroups,
    };
  });

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (!model) {
    return <WarningMessage>Could not load release summary</WarningMessage>;
  }

  const { timePeriodCoverageGroups, releaseTypes } = model;

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

  const baseSchema = Yup.object<ReleaseSummaryFormValues>({
    timePeriodCoverageCode: Yup.string().required('Choose a time period'),
    timePeriodCoverageStartYear: Yup.string()
      .required('Enter a year')
      .length(4, 'Year must be exactly 4 characters'),
    releaseTypeId: Yup.string().required('Choose a release type'),
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
              id={`${formId}-timePeriodCoverageFieldset`}
              legend="Select time period coverage"
              legendSize="m"
            >
              <FormFieldSelect<ReleaseSummaryFormValues>
                id={`${formId}-timePeriodCoverage`}
                label="Type"
                name="timePeriodCoverageCode"
                optGroups={timePeriodOptions}
              />
              <FormFieldNumberInput<ReleaseSummaryFormValues>
                id={`${formId}-timePeriodCoverageStartYear`}
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
              id={`${formId}-releaseTypeId`}
              legend="Release Type"
              name="releaseTypeId"
              options={releaseTypes.map(type => ({
                label: type.title,
                value: `${type.id}`,
              }))}
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
