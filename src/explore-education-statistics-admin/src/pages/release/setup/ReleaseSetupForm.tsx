import Link from '@admin/components/Link';
import {DayMonthYearInputs, dayMonthYearValuesToInputs, IdTitlePair,} from '@admin/services/common/types';
import {ReleaseSetupDetails} from '@admin/services/release/types';
import {
  validateMandatoryDayMonthYearField,
  validateOptionalPartialDayMonthYearField,
} from '@admin/validation/validation';
import Button from '@common/components/Button';
import {Form, FormFieldset, Formik} from '@common/components/form';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import {SelectOption} from '@common/components/form/FormSelect';
import Yup from '@common/lib/validation/yup';
import {Dictionary} from '@common/types';
import {FormikProps} from 'formik';
import React, {useEffect, useState} from 'react';
import DummyReferenceData, {TimePeriodCoverageGroup,} from '../../DummyReferenceData';

export interface FormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartYear: string;
  releaseTypeId: string;
  scheduledPublishDate: DayMonthYearInputs;
  nextReleaseExpectedDate: DayMonthYearInputs;
}

interface Props {
  releaseSetupDetails?: ReleaseSetupDetails;
  submitButtonText: string;
  onSubmitHandler: (values: FormValues) => void;
  onCancelHandler: () => void;
}

const ReleaseSetupForm = ({
  releaseSetupDetails,
  submitButtonText,
  onSubmitHandler,
  onCancelHandler,
}: Props) => {
  const [timePeriodCoverageGroups, setTimePeriodCoverageGroups] = useState<
    TimePeriodCoverageGroup[]
  >();

  const [releaseTypes, setReleaseTypes] = useState<IdTitlePair[]>();

  useEffect(() => {
    setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
    setReleaseTypes(DummyReferenceData.releaseTypeOptions);
  }, []);

  const getTimePeriodOptions = (
    timePeriodGroups: TimePeriodCoverageGroup[],
  ) => {
    const optGroups: Dictionary<SelectOption[]> = {};
    timePeriodGroups.forEach(group => {
      optGroups[group.title] = group.options.map(option => ({
        label: `${option.title} - ${option.id}`,
        value: option.id,
      }));
    });
    return optGroups;
  };

  const emptyDayMonthYear = (): DayMonthYearInputs => ({
    day: '',
    month: '',
    year: '',
  });

  const formId = 'releaseSetupForm';

  return (
    <>
      {timePeriodCoverageGroups && releaseTypes && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            timePeriodCoverageCode: releaseSetupDetails
              ? releaseSetupDetails.timePeriodCoverageCode
              : timePeriodCoverageGroups[0].options[0].id,
            timePeriodCoverageStartYear:
              releaseSetupDetails ? releaseSetupDetails.timePeriodCoverageStartYear.toString()
                : '',
            releaseTypeId: releaseSetupDetails
              ? releaseSetupDetails.releaseType.id
              : '',
            scheduledPublishDate: releaseSetupDetails
              ? dayMonthYearValuesToInputs(
                  releaseSetupDetails.scheduledPublishDate,
                )
              : emptyDayMonthYear(),
            nextReleaseExpectedDate: releaseSetupDetails
              ? dayMonthYearValuesToInputs(
                  releaseSetupDetails.nextReleaseExpectedDate,
                )
              : emptyDayMonthYear(),
          }}
          validationSchema={Yup.object<FormValues>({
            timePeriodCoverageCode: Yup.string().required(
              'Choose a time period',
            ),
            timePeriodCoverageStartYear: Yup.string().required('Enter a start year'),
            releaseTypeId: Yup.string(),
            scheduledPublishDate: validateMandatoryDayMonthYearField,
            nextReleaseExpectedDate: validateOptionalPartialDayMonthYearField,
          })}
          onSubmit={onSubmitHandler}
          render={(form: FormikProps<FormValues>) => {
            return (
              <Form id={formId}>
                <FormFieldset
                  id={`${formId}-timePeriodCoverageFieldset`}
                  legend="Select time period coverage"
                >
                  <FormFieldSelect<FormValues>
                    id={`${formId}-timePeriodCoverage`}
                    label="Type"
                    name="timePeriodCoverageCode"
                    optGroups={getTimePeriodOptions(timePeriodCoverageGroups)}
                  />
                  <FormFieldTextInput<FormValues>
                    id={`${formId}-timePeriodCoverageStartYear`}
                    name="timePeriodCoverageStartYear"
                    label={
                      DummyReferenceData.findTimePeriodCoverageGroup(
                        form.values.timePeriodCoverageCode,
                      ).startDateLabel
                    }
                    width={4}
                    type="number"
                    pattern="[0-9]*"
                  />
                </FormFieldset>
                <FormFieldDayMonthYear<FormValues>
                  formId={formId}
                  fieldName="scheduledPublishDate"
                  fieldsetLegend="Schedule publish date"
                  day={form.values.scheduledPublishDate.day}
                  month={form.values.scheduledPublishDate.month}
                  year={form.values.scheduledPublishDate.year}
                />
                <FormFieldDayMonthYear<FormValues>
                  formId={formId}
                  fieldName="nextReleaseExpectedDate"
                  fieldsetLegend="Next release expected (optional)"
                  day={form.values.nextReleaseExpectedDate.day}
                  month={form.values.nextReleaseExpectedDate.month}
                  year={form.values.nextReleaseExpectedDate.year}
                />
                <FormFieldRadioGroup<FormValues>
                  id={`${formId}-releaseTypeId`}
                  legend="Release Type"
                  name="releaseTypeId"
                  options={releaseTypes.map(type => ({
                    label: type.title,
                    value: `${type.id}`,
                  }))}
                />
                <Button type="submit" className="govuk-!-margin-top-6">
                  {submitButtonText}
                </Button>
                <div className="govuk-!-margin-top-6">
                  <Link to="#" onClick={onCancelHandler}>
                    Cancel update
                  </Link>
                </div>
              </Form>
            );
          }}
        />
      )}
    </>
  );
};

export default ReleaseSetupForm;
