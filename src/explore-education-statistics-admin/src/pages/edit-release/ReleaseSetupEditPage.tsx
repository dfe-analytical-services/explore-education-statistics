import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import DummyReferenceData, {
  DateType,
  TimePeriodCoverageGroup,
} from '@admin/pages/DummyReferenceData';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import Button from '@common/components/Button';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { SelectOption } from '@common/components/form/FormSelect';
import Yup from '@common/lib/validation/yup';
import { Dictionary } from '@common/types';
import { FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { setupRoute } from '@admin/routes/releaseRoutes';
import {
  shapeOfDayMonthYearField,
  validateMandatoryDayMonthYearField,
  validateOptionalPartialDayMonthYearField,
} from '@admin/validation/validation';
import Link from '@admin/components/Link';
import {
  dateToDayMonthYear,
  dayMonthYearToDate,
  DayMonthYearValues,
  IdLabelPair,
  ReleaseSetupDetails,
} from '@admin/services/api/common/types/types';

interface MatchProps {
  releaseId: string;
}

interface FormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate?: DayMonthYearValues;
  timePeriodCoverageStartDateYearOnly?: number;
  releaseTypeId: string;
  scheduledPublishDate: DayMonthYearValues;
  nextReleaseExpectedDate: DayMonthYearValues;
}

const ReleaseSetupEditPage = ({
  match,
  history,
}: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSetupDetails, setReleaseSetupDetails] = useState<
    ReleaseSetupDetails
  >();

  const [timePeriodCoverageGroups, setTimePeriodCoverageGroups] = useState<
    TimePeriodCoverageGroup[]
  >();

  const [releaseTypes, setReleaseTypes] = useState<IdLabelPair[]>();

  useEffect(() => {
    const release = DummyPublicationsData.getReleaseSetupDetails(releaseId);

    setReleaseSetupDetails(release);
    setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
    setReleaseTypes(DummyReferenceData.releaseTypeOptions);
  }, [releaseId]);

  const selectedTimePeriodCoverageGroup =
    releaseSetupDetails &&
    DummyReferenceData.findTimePeriodCoverageGroup(
      releaseSetupDetails.timePeriodCoverageCode,
    );

  const getTimePeriodOptions = (
    timePeriodGroups: TimePeriodCoverageGroup[],
  ) => {
    const optGroups: Dictionary<SelectOption[]> = {};
    timePeriodGroups.forEach(group => {
      optGroups[group.label] = group.options.map(option => ({
        label: `${option.label} - ${option.id}`,
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
    <ReleasePageTemplate
      releaseId={releaseId}
      publicationTitle={
        releaseSetupDetails ? releaseSetupDetails.publicationTitle : ''
      }
    >
      <h2 className="govuk-heading-m">Edit release setup</h2>

      {releaseSetupDetails && timePeriodCoverageGroups && releaseTypes && (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            timePeriodCoverageCode: releaseSetupDetails.timePeriodCoverageCode,
            timePeriodCoverageStartDate: isDayMonthYearDateTypeSelected(
              selectedTimePeriodCoverageGroup,
            )
              ? dateToDayMonthYear(
                  releaseSetupDetails.timePeriodCoverageStartDate,
                )
              : undefined,
            timePeriodCoverageStartDateYearOnly: isYearOnlyDateTypeSelected(
              selectedTimePeriodCoverageGroup,
            )
              ? releaseSetupDetails.timePeriodCoverageStartDate.getFullYear()
              : undefined,
            releaseTypeId: releaseSetupDetails.releaseType.id,
            scheduledPublishDate: releaseSetupDetails.scheduledPublishDate,
            nextReleaseExpectedDate: dateToDayMonthYear(
              releaseSetupDetails.nextReleaseExpectedDate,
            ),
          }}
          validationSchema={Yup.object<FormValues>({
            timePeriodCoverageCode: Yup.string().required(
              'Choose a time period',
            ),
            timePeriodCoverageStartDate: Yup.object<DayMonthYearValues>().when(
              'timePeriodCoverageCode',
              {
                is: (val: string) => isDayMonthYearDateTypeCodeSelected(val),
                then: validateMandatoryDayMonthYearField,
                otherwise: shapeOfDayMonthYearField,
              },
            ),
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
          onSubmit={async (values: FormValues) => {
            const release = DummyPublicationsData.getReleaseById(releaseId);

            release.timePeriodCoverage.code = values.timePeriodCoverageCode;

            if (values.timePeriodCoverageStartDate) {
              release.timePeriodCoverage.startDate = dayMonthYearToDate(
                values.timePeriodCoverageStartDate,
              );
            } else if (values.timePeriodCoverageStartDateYearOnly) {
              release.timePeriodCoverage.startDate = new Date(
                values.timePeriodCoverageStartDateYearOnly,
                1,
                1,
              );
            }

            release.scheduledPublishDate = values.scheduledPublishDate;

            release.nextReleaseExpectedDate = dayMonthYearToDate(
              values.nextReleaseExpectedDate,
            );
            release.releaseType = DummyReferenceData.findReleaseType(
              values.releaseTypeId,
            );
            history.push(setupRoute.generateLink(releaseId));
          }}
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
                    onChange={event => {
                      setReleaseSetupDetails({
                        ...releaseSetupDetails,
                        timePeriodCoverageCode: event.target.value,
                      });
                    }}
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
                    label: type.label,
                    value: `${type.id}`,
                  }))}
                />

                <Button type="submit" className="govuk-!-margin-top-6">
                  Update release setup
                </Button>

                <div className="govuk-!-margin-top-6">
                  <Link to={setupRoute.generateLink(releaseId)}>
                    Cancel update
                  </Link>
                </div>
              </Form>
            );
          }}
        />
      )}
    </ReleasePageTemplate>
  );
};

export default ReleaseSetupEditPage;
