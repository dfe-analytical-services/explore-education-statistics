import metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import RHFFormFieldNumberInput from '@common/components/form/rhf/RHFFormFieldNumberInput';
import { SelectOption } from '@common/components/form/FormSelect';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { ReleaseType, releaseTypes } from '@common/services/types/releaseType';
import { Dictionary } from '@common/types';
import { IdTitlePair } from '@admin/services/types/common';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import { mapFieldErrors } from '@common/validation/serverValidations';

export interface ReleaseSummaryFormValues {
  releaseType?: ReleaseType;
  templateReleaseId?: string;
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: string;
}

const formId = 'releaseSummaryForm';

const errorMappings = [
  mapFieldErrors<ReleaseSummaryFormValues>({
    target: 'timePeriodCoverageStartYear',
    messages: {
      SlugNotUnique:
        'Choose a unique combination of time period and start year',
    },
  }),
];

interface Props {
  submitText: string;
  templateRelease?: IdTitlePair;
  initialValues: ReleaseSummaryFormValues;
  onSubmit: (values: ReleaseSummaryFormValues) => Promise<void> | void;
  onCancel: () => void;
}

export default function ReleaseSummaryForm({
  initialValues,
  submitText,
  templateRelease,
  onSubmit,
  onCancel,
}: Props) {
  const { value: timePeriodCoverageGroups, isLoading } = useAsyncRetry<
    TimePeriodCoverageGroup[]
  >(() => metaService.getTimePeriodCoverageGroups());

  // Can't create new releases with type Experimental statistics.
  const permittedReleaseTypes = useMemo(
    () =>
      (Object.keys(releaseTypes) as ReleaseType[]).filter(type =>
        type === 'ExperimentalStatistics'
          ? initialValues.releaseType === 'ExperimentalStatistics'
          : true,
      ),
    [initialValues.releaseType],
  );

  const validationSchema = useMemo<
    ObjectSchema<ReleaseSummaryFormValues>
  >(() => {
    return Yup.object({
      timePeriodCoverageCode: Yup.string().required('Choose a time period'),
      timePeriodCoverageStartYear: Yup.string()
        .required('Enter a year')
        .length(4, 'Year must be exactly 4 characters'),
      releaseType: Yup.string()
        .required('Choose a release type')
        .oneOf(permittedReleaseTypes),
      templateReleaseId: Yup.string(),
    });
  }, [permittedReleaseTypes]);

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

  return (
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      {({ getValues }) => {
        const timePeriodLabel =
          findTimePeriodCoverageGroup(getValues('timePeriodCoverageCode'))
            ?.category.label ?? '';

        return (
          <RHFForm id={formId} onSubmit={onSubmit}>
            <FormFieldset
              hint="For 6 digit years, enter only the first four digits in this box e.g. for 2017/18, only enter 2017."
              id="timePeriodCoverage"
              legend="Select time period coverage"
              legendSize="m"
            >
              <RHFFormFieldSelect<ReleaseSummaryFormValues>
                label="Type"
                name="timePeriodCoverageCode"
                optGroups={timePeriodOptions}
              />
              <RHFFormFieldNumberInput<ReleaseSummaryFormValues>
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
            <RHFFormFieldRadioGroup<ReleaseSummaryFormValues>
              legend="Release type"
              name="releaseType"
              options={permittedReleaseTypes.map(type => ({
                label: releaseTypes[type],
                value: type,
              }))}
            />

            {templateRelease && (
              <RHFFormFieldRadioGroup<ReleaseSummaryFormValues>
                legend="Select template"
                name="templateReleaseId"
                options={[
                  {
                    label: 'Create new template',
                    value: 'new',
                  },
                  {
                    label: `Copy existing template (${templateRelease.title})`,
                    value: `${templateRelease.id}`,
                  },
                ]}
              />
            )}
            <ButtonGroup>
              <Button type="submit">{submitText}</Button>
              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
}
