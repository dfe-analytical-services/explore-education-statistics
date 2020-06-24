import metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import { IdTitlePair } from '@admin/services/types/common';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { SelectOption } from '@common/components/form/FormSelect';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/yup';
import { Formik, FormikHelpers } from 'formik';
import React, { ReactNode, useEffect, useState } from 'react';
import { ObjectSchema } from 'yup';

export interface ReleaseSummaryFormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: string;
  releaseTypeId: string;
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
  ) => ObjectSchema<FormValues>;
  onSubmit: (values: FormValues, actions: FormikHelpers<FormValues>) => void;
  onCancel: () => void;
}

interface ReleaseSummaryFormModel {
  timePeriodCoverageGroups: TimePeriodCoverageGroup[];
  releaseTypes: IdTitlePair[];
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
  const [model, setModel] = useState<ReleaseSummaryFormModel>();

  useEffect(() => {
    Promise.all([
      metaService.getReleaseTypes(),
      metaService.getTimePeriodCoverageGroups(),
    ]).then(([releaseTypesResult, timePeriodGroupsResult]) => {
      setModel({
        releaseTypes: releaseTypesResult,
        timePeriodCoverageGroups: timePeriodGroupsResult,
      });
    });
  }, []);

  const getTimePeriodOptions = (
    timePeriodGroups: TimePeriodCoverageGroup[],
  ) => {
    const optGroups: Dictionary<SelectOption[]> = {};
    timePeriodGroups.forEach(group => {
      optGroups[group.category.label] = group.timeIdentifiers.map(
        ({ identifier }) => identifier,
      );
    });
    return optGroups;
  };

  const findTimePeriodCoverageGroup = (
    code: string,
    timePeriodCoverageGroups: TimePeriodCoverageGroup[],
  ) => {
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
    timePeriodCoverageStartYear: Yup.string().required('Enter a year'),
    releaseTypeId: Yup.string().required('Choose a release type'),
  });

  return (
    <>
      {model && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={initialValues(model.timePeriodCoverageGroups)}
          validationSchema={
            validationSchema ? validationSchema(baseSchema) : baseSchema
          }
          onSubmit={onSubmit}
        >
          {form => {
            const timePeriodLabel = findTimePeriodCoverageGroup(
              form.values.timePeriodCoverageCode,
              model.timePeriodCoverageGroups,
            ).category.label;

            return (
              <Form id={formId}>
                <FormFieldset
                  className="govuk-!-margin-bottom-9"
                  id={`${formId}-timePeriodCoverageFieldset`}
                  legend="Select time period coverage"
                  legendSize="m"
                >
                  <FormFieldSelect<FormValues>
                    id={`${formId}-timePeriodCoverage`}
                    label="Type"
                    name="timePeriodCoverageCode"
                    optGroups={getTimePeriodOptions(
                      model.timePeriodCoverageGroups,
                    )}
                  />
                  <FormFieldNumberInput<FormValues>
                    id={`${formId}-timePeriodCoverageStartYear`}
                    name="timePeriodCoverageStartYear"
                    label={`
                      ${
                        ['Month', 'Term', 'Week', 'Other'].includes(
                          timePeriodLabel,
                        )
                          ? 'Year'
                          : timePeriodLabel
                      }
                    `}
                    width={4}
                  />
                </FormFieldset>
                <div className="govuk-!-margin-top-9">
                  <FormFieldRadioGroup<FormValues>
                    id={`${formId}-releaseTypeId`}
                    legend="Release Type"
                    name="releaseTypeId"
                    options={model.releaseTypes.map(type => ({
                      label: type.title,
                      value: `${type.id}`,
                    }))}
                  />
                </div>
                {additionalFields}
                <Button type="submit" className="govuk-!-margin-top-9">
                  {submitText}
                </Button>
                <div className="govuk-!-margin-top-6">
                  <ButtonText onClick={onCancel}>Cancel</ButtonText>
                </div>
              </Form>
            );
          }}
        </Formik>
      )}
    </>
  );
};

export default ReleaseSummaryForm;
