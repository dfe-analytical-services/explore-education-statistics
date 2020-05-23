import { findTimePeriodCoverageGroup } from '@admin/pages/release/util/releaseSummaryUtil';
import service from '@admin/services/common/service';
import {
  IdTitlePair,
  TimePeriodCoverageGroup,
} from '@admin/services/common/types';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldNumberInput from '@common/components/form/FormFieldNumberInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { SelectOption } from '@common/components/form/FormSelect';
import { Dictionary } from '@common/types';
import {
  DayMonthYear,
  dayMonthYearIsComplete,
  dayMonthYearIsEmpty,
  dayMonthYearToDate,
} from '@common/utils/date/dayMonthYear';
import Yup from '@common/validation/yup';
import { endOfDay, format, isValid } from 'date-fns';
import { Formik, FormikHelpers } from 'formik';
import React, { ReactNode, useEffect, useState } from 'react';
import { ObjectSchemaDefinition } from 'yup';

export interface ReleaseSummaryFormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: string;
  releaseTypeId: string;
  scheduledPublishDate: Date;
  nextReleaseDate?: DayMonthYear;
}

const formId = 'releaseSummaryForm';

interface Props<FormValues extends ReleaseSummaryFormValues> {
  additionalFields?: ReactNode;
  submitText: string;
  initialValues: (
    timePeriodCoverageGroups: TimePeriodCoverageGroup[],
  ) => FormValues;
  validationRules?: (
    baseValidationRules: ObjectSchemaDefinition<ReleaseSummaryFormValues>,
  ) => ObjectSchemaDefinition<FormValues>;
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
  validationRules,
  onSubmit,
  onCancel,
}: Props<FormValues>) => {
  const [model, setModel] = useState<ReleaseSummaryFormModel>();

  useEffect(() => {
    Promise.all([
      service.getReleaseTypes(),
      service.getTimePeriodCoverageGroups(),
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

  const baseValidationRules: ObjectSchemaDefinition<ReleaseSummaryFormValues> = {
    timePeriodCoverageCode: Yup.string().required('Choose a time period'),
    timePeriodCoverageStartYear: Yup.string().required('Enter a year'),
    releaseTypeId: Yup.string().required('Choose a release type'),
    scheduledPublishDate: Yup.date()
      .required('Enter a valid scheduled publish date')
      .test({
        name: 'validDateIfAfterToday',
        message: `Scheduled publish date can't be before ${format(
          new Date(),
          'do MMMM yyyy',
        )}`,
        test(value) {
          return endOfDay(value) >= endOfDay(new Date());
        },
      }),
    nextReleaseDate: Yup.object({
      day: Yup.number().nullable(),
      month: Yup.number().nullable(),
      year: Yup.number().nullable(),
    }).test({
      name: 'validDate',
      message: 'Enter a valid next release date',
      test(value: DayMonthYear) {
        if (dayMonthYearIsEmpty(value)) {
          return true;
        }

        if (value.year && value.month && !value.day) {
          return true;
        }

        if (dayMonthYearIsComplete(value)) {
          return isValid(dayMonthYearToDate(value));
        }

        return false;
      },
    }),
  };

  return (
    <>
      {model && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={initialValues(model.timePeriodCoverageGroups)}
          validationSchema={Yup.object<FormValues>(
            validationRules
              ? validationRules(baseValidationRules)
              : (baseValidationRules as ObjectSchemaDefinition<FormValues>),
          )}
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
                        ['Month', 'Term', 'Other'].includes(timePeriodLabel)
                          ? 'Year'
                          : timePeriodLabel
                      }
                    `}
                    width={4}
                  />
                </FormFieldset>
                <FormFieldDateInput<FormValues>
                  id={`${formId}-scheduledPublishDate`}
                  name="scheduledPublishDate"
                  legend="Schedule publish date"
                  legendSize="m"
                />
                <FormFieldDateInput<FormValues>
                  id={`${formId}-nextReleaseDate`}
                  name="nextReleaseDate"
                  legend="Next release expected (optional)"
                  legendSize="m"
                  type="dayMonthYear"
                />
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
