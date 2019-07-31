import Link from "@admin/components/Link";
import {setupRoute} from "@admin/routes/edit-release/routes";
import {DayMonthYearValues, IdTitlePair} from "@admin/services/common/types";
import service from "@admin/services/edit-release/setup/service";
import {ReleaseSetupDetails} from "@admin/services/edit-release/setup/types";
import {
  shapeOfDayMonthYearField,
  validateMandatoryDayMonthYearField,
  validateOptionalPartialDayMonthYearField
} from "@admin/validation/validation";
import Button from "@common/components/Button";
import FormFieldDayMonthYear from "@common/components/form/FormFieldDayMonthYear";
import FormFieldRadioGroup from "@common/components/form/FormFieldRadioGroup";
import FormFieldSelect from "@common/components/form/FormFieldSelect";
import FormFieldTextInput from "@common/components/form/FormFieldTextInput";
import {SelectOption} from "@common/components/form/FormSelect";
import {Form, FormFieldset, Formik} from "@common/components/form/index";
import Yup from "@common/lib/validation/yup/index";
import {Dictionary} from "@common/types";
import {FormikProps} from "formik";
import React, {useEffect, useState} from "react";
import DummyReferenceData, {DateType, TimePeriodCoverageGroup} from "../../DummyReferenceData";

export interface FormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate?: DayMonthYearValues;
  timePeriodCoverageStartDateYearOnly?: number;
  releaseTypeId: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

interface Props {
  releaseSetupDetails: ReleaseSetupDetails;
  onSubmitHandler: (values: FormValues) => void;
}

const ReleaseSetupForm = ({releaseSetupDetails, onSubmitHandler}: Props) => {

  const [timePeriodCoverageGroups, setTimePeriodCoverageGroups] = useState<
    TimePeriodCoverageGroup[]
    >();

  const [releaseTypes, setReleaseTypes] = useState<IdTitlePair[]>();

  useEffect(() => {
    service.getReleaseSetupDetails(releaseSetupDetails.id).then(release => {
      setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
      setReleaseTypes(DummyReferenceData.releaseTypeOptions);
    });
  }, []);

  const selectedTimePeriodCoverageGroup =
    DummyReferenceData.findTimePeriodCoverageGroup(
      releaseSetupDetails.timePeriodCoverageCode,
    );

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

  const isDayMonthYearDateTypeSelected = (
    timePeriodGroup?: TimePeriodCoverageGroup,
  ) =>
    timePeriodGroup
      ? DateType.DayMonthYear === timePeriodGroup.startDateType
      : false;

  const isYearOnlyDateTypeSelected = (
    timePeriodGroup?: TimePeriodCoverageGroup,
  ) =>
    timePeriodGroup ? DateType.Year === timePeriodGroup.startDateType : false;

  const isDayMonthYearDateTypeCodeSelected = (timePeriodGroupCode?: string) =>
    timePeriodGroupCode
      ? isDayMonthYearDateTypeSelected(
      DummyReferenceData.findTimePeriodCoverageGroup(timePeriodGroupCode),
      )
      : false;

  const isYearOnlyDateTypeCodeSelected = (timePeriodGroupCode?: string) =>
    timePeriodGroupCode
      ? isYearOnlyDateTypeSelected(
      DummyReferenceData.findTimePeriodCoverageGroup(timePeriodGroupCode),
      )
      : false;

  const formId = 'releaseSetupForm';

  return (
    <>
      {timePeriodCoverageGroups && releaseTypes && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            timePeriodCoverageCode:
            releaseSetupDetails.timePeriodCoverageCode,
            timePeriodCoverageStartDate: isDayMonthYearDateTypeSelected(
              selectedTimePeriodCoverageGroup,
            )
              ? releaseSetupDetails.timePeriodCoverageStartDate
              : undefined,
            timePeriodCoverageStartDateYearOnly: isYearOnlyDateTypeSelected(
              selectedTimePeriodCoverageGroup,
            )
              ? releaseSetupDetails.timePeriodCoverageStartDate.year
              : undefined,
            releaseTypeId: releaseSetupDetails.releaseType.id,
            scheduledPublishDate: releaseSetupDetails.scheduledPublishDate,
            nextReleaseExpectedDate:
            releaseSetupDetails.nextReleaseExpectedDate,
          }}
          validationSchema={Yup.object<FormValues>({
            timePeriodCoverageCode: Yup.string().required(
              'Choose a time period',
            ),
            timePeriodCoverageStartDate: Yup.object<
              DayMonthYearValues
              >().when('timePeriodCoverageCode', {
              is: (val: string) => isDayMonthYearDateTypeCodeSelected(val),
              then: validateMandatoryDayMonthYearField,
              otherwise: shapeOfDayMonthYearField,
            }),
            timePeriodCoverageStartDateYearOnly: Yup.number().when(
              'timePeriodCoverageCode',
              {
                is: (val: string) => isYearOnlyDateTypeCodeSelected(val),
                then: Yup.number().required('Enter a start year'),
                otherwise: Yup.number(),
              },
            ),
            releaseTypeId: Yup.string(),
            scheduledPublishDate: validateOptionalPartialDayMonthYearField,
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
                  {isYearOnlyDateTypeCodeSelected(
                    form.values.timePeriodCoverageCode,
                  ) && (
                    <FormFieldTextInput<FormValues>
                      id={`${formId}-timePeriodCoverageStartYearOnly`}
                      name="timePeriodCoverageStartDateYearOnly"
                      label={
                        DummyReferenceData.findTimePeriodCoverageGroup(
                          form.values.timePeriodCoverageCode,
                        ).startDateLabel
                      }
                      width={4}
                      type="number"
                      pattern="[0-9]*"
                    />
                  )}
                  {isDayMonthYearDateTypeCodeSelected(
                    form.values.timePeriodCoverageCode,
                  ) && (
                    <FormFieldDayMonthYear<FormValues>
                      formId={formId}
                      fieldName="timePeriodCoverageStartDate"
                      fieldsetLegend={
                        DummyReferenceData.findTimePeriodCoverageGroup(
                          form.values.timePeriodCoverageCode,
                        ).startDateLabel
                      }
                      day={
                        form.values.timePeriodCoverageStartDate &&
                        form.values.timePeriodCoverageStartDate.day
                      }
                      month={
                        form.values.timePeriodCoverageStartDate &&
                        form.values.timePeriodCoverageStartDate.month
                      }
                      year={
                        form.values.timePeriodCoverageStartDate &&
                        form.values.timePeriodCoverageStartDate.year
                      }
                    />
                  )}
                </FormFieldset>

                <FormFieldDayMonthYear<FormValues>
                  formId={formId}
                  fieldName="scheduledPublishDate"
                  fieldsetLegend="Schedule publish date (optional)"
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
                  Update release status
                </Button>

                <div className="govuk-!-margin-top-6">
                  <Link to={setupRoute.generateLink(releaseSetupDetails.id)}>
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