import metaService, {
  TimePeriodCoverageGroup,
} from '@admin/services/metaService';
import organisationService from '@admin/services/organisationService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import {
  FormFieldCheckboxGroup,
  FormFieldset,
  FormFieldTextInput,
} from '@common/components/form';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import { SelectOption } from '@common/components/form/FormSelect';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { Organisation } from '@common/services/types/organisation';
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
  releaseLabel?: string;
  publishingOrganisations?: string[];
}

const formId = 'releaseSummaryForm';

const errorMappings = [
  mapFieldErrors<ReleaseSummaryFormValues>({
    target: 'releaseLabel',
    messages: {
      SlugNotUnique:
        'Choose a unique combination of type, start year and label',
      ReleaseSlugUsedByRedirect: `The requested release label has previously been used either for this or another related release and can not be re-used without a manual check. Please contact the Explore Education Statistics team with the details of the requested label name change.`,
    },
  }),
];

interface Props {
  submitText: string;
  templateRelease?: IdTitlePair;
  initialValues: ReleaseSummaryFormValues;
  releaseVersion: number;
  onSubmit: (values: ReleaseSummaryFormValues) => Promise<void> | void;
  onCancel: () => void;
}

export default function ReleaseSummaryForm({
  initialValues,
  releaseVersion,
  submitText,
  templateRelease,
  onSubmit,
  onCancel,
}: Props) {
  const {
    value: timePeriodCoverageGroups,
    isLoading: timePeriodCoverageGroupsLoading,
  } = useAsyncRetry<TimePeriodCoverageGroup[]>(() =>
    metaService.getTimePeriodCoverageGroups(),
  );

  const { value: organisations, isLoading: organisationsLoading } =
    useAsyncRetry<Organisation[]>(() =>
      organisationService.listOrganisations(),
    );

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
      releaseLabel: Yup.string().max(
        20,
        /* eslint-disable no-template-curly-in-string */
        'Release label must be no longer than ${max} characters',
      ),
      publishingOrganisations: Yup.array()
        .of(Yup.string().required().uuid())
        .transform((value, originalValue) => {
          // If the original value is falsy (e.g., false), transform it to undefined
          // needed as react-hook-form casts no checkbox selection to false
          if (!originalValue) {
            return undefined;
          }
          return value;
        })
        .optional(),
    });
  }, [permittedReleaseTypes]);

  if (timePeriodCoverageGroupsLoading || organisationsLoading) {
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

  const disableReleaseSlugChange = releaseVersion > 0;

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
          <Form id={formId} onSubmit={onSubmit}>
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
                disabled={disableReleaseSlugChange}
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
                disabled={disableReleaseSlugChange}
              />
            </FormFieldset>
            <FormFieldTextInput
              id="releaseLabel"
              name="releaseLabel"
              label="Release label"
              labelSize="m"
              hint="Unique label for the release, use if needed to distinguish it from other releases that share the same period."
              width={20}
              disabled={disableReleaseSlugChange}
            />
            <FormFieldRadioGroup<ReleaseSummaryFormValues>
              legend="Release type"
              name="releaseType"
              options={permittedReleaseTypes.map(type => ({
                label: releaseTypes[type],
                value: type,
              }))}
            />

            {templateRelease && (
              <FormFieldRadioGroup<ReleaseSummaryFormValues>
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

            {organisations && (
              <FormFieldCheckboxGroup<ReleaseSummaryFormValues>
                hint="Optional - select which organisations are responsible for publishing this release"
                legend="Publishing Organisations"
                legendSize="m"
                name="publishingOrganisations"
                options={organisations.map(org => {
                  return {
                    label: org.title,
                    value: org.id,
                  };
                })}
                small
              />
            )}

            <ButtonGroup>
              <Button type="submit">{submitText}</Button>
              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}
