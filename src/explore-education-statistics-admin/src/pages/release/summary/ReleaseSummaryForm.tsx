import { findTimePeriodCoverageGroup } from '@admin/pages/release/util/releaseSummaryUtil';
import service from '@admin/services/common/service';
import {
  IdTitlePair,
  TimePeriodCoverageGroup,
} from '@admin/services/common/types';
import {
  parseDate,
  validateMandatoryDayMonthYearField,
  validateOptionalPartialDayMonthYearField,
} from '@admin/validation/validation';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import { FormFieldset, Formik } from '@common/components/form';
import Form from '@common/components/form/Form';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { SelectOption } from '@common/components/form/FormSelect';
import { DayMonthYearInputs } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import Yup from '@common/validation/validation/yup';
import { endOfDay, format, isValid } from 'date-fns';
import { FormikActions, FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';
import { ObjectSchemaDefinition } from 'yup';

export interface EditFormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: string;
  releaseTypeId: string;
  scheduledPublishDate: DayMonthYearInputs;
  nextReleaseDate: DayMonthYearInputs;
}

interface Props<FormValues extends EditFormValues> {
  submitButtonText: string;
  initialValuesSupplier: (
    timePeriodCoverageGroups: TimePeriodCoverageGroup[],
  ) => FormValues;
  validationRulesSupplier?: (
    baseValidationRules: ObjectSchemaDefinition<EditFormValues>,
  ) => ObjectSchemaDefinition<FormValues>;
  onSubmitHandler: (
    values: FormValues,
    actions: FormikActions<FormValues>,
  ) => void;
  onCancelHandler: () => void;
  additionalFields?: React.ReactNode;
}

interface ReleaseSummaryFormModel {
  timePeriodCoverageGroups: TimePeriodCoverageGroup[];
  releaseTypes: IdTitlePair[];
}

const ReleaseSummaryForm = <FormValues extends EditFormValues>({
  submitButtonText,
  initialValuesSupplier,
  validationRulesSupplier,
  onSubmitHandler,
  onCancelHandler,
  additionalFields,
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

  const baseValidationRules: ObjectSchemaDefinition<EditFormValues> = {
    timePeriodCoverageCode: Yup.string().required('Choose a time period'),
    timePeriodCoverageStartYear: Yup.string().required('Enter a year'),
    releaseTypeId: Yup.string().required('Choose a release type'),
    scheduledPublishDate: validateMandatoryDayMonthYearField.test(
      'validDateIfAfterToday',
      `Schedule publish date can't be before ${format(
        new Date(),
        'do MMMM yyyy',
      )}`,
      value => {
        return (
          isValid(parseDate({ value })) &&
          endOfDay(parseDate({ value })) >= endOfDay(new Date())
        );
      },
    ),
    nextReleaseDate: validateOptionalPartialDayMonthYearField,
  };

  const formId = 'releaseSummaryForm';

  return (
    <>
      {model && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={initialValuesSupplier(model.timePeriodCoverageGroups)}
          validationSchema={Yup.object<FormValues>(
            validationRulesSupplier
              ? validationRulesSupplier(baseValidationRules)
              : (baseValidationRules as ObjectSchemaDefinition<FormValues>),
          )}
          onSubmit={onSubmitHandler}
          render={(form: FormikProps<FormValues>) => {
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
                  <FormFieldTextInput<FormValues>
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
                    type="number"
                    pattern="[0-9]*"
                  />
                </FormFieldset>
                <FormFieldDayMonthYear<FormValues>
                  formId={formId}
                  fieldName="scheduledPublishDate"
                  fieldsetLegend="Schedule publish date"
                  fieldsetLegendSize="m"
                  day={form.values.scheduledPublishDate.day}
                  month={form.values.scheduledPublishDate.month}
                  year={form.values.scheduledPublishDate.year}
                />
                <FormFieldDayMonthYear<FormValues>
                  formId={formId}
                  fieldName="nextReleaseDate"
                  fieldsetLegend="Next release expected (optional)"
                  fieldsetLegendSize="m"
                  day={form.values.nextReleaseDate.day}
                  month={form.values.nextReleaseDate.month}
                  year={form.values.nextReleaseDate.year}
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
                  {submitButtonText}
                </Button>
                <div className="govuk-!-margin-top-6">
                  <ButtonText onClick={onCancelHandler}>
                    Cancel update
                  </ButtonText>
                </div>
              </Form>
            );
          }}
        />
      )}
    </>
  );
};

export default ReleaseSummaryForm;
