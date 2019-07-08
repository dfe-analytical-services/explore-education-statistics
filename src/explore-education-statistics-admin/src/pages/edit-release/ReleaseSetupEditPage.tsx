import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import DummyReferenceData, {
  DateType,
  TimePeriodCoverageGroup,
} from '@admin/pages/DummyReferenceData';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { SelectOption } from '@common/components/form/FormSelect';
import Yup from '@common/lib/validation/yup';
import { Dictionary } from '@common/types';
import { FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { setupRoute } from '@admin/routes/releaseRoutes';
import Link from '../../components/Link';
import {
  IdLabelPair,
  ReleaseSetupDetails,
} from '../../services/publicationService';

interface MatchProps {
  releaseId: string;
}

interface DayMonthYearValues {
  day: string;
  month: string;
  year: string;
}

export const dateToDayMonthYear = (date?: Date) => {
  return {
    day: `${date ? date.getDate() : ''}`,
    month: `${date ? date.getMonth() + 1 : ''}`,
    year: `${date ? date.getFullYear() : ''}`,
  };
};

export const dayMonthYearToDate = (dmy: DayMonthYearValues) => {
  const date = dmy.day ? parseInt(dmy.day, 10) : 1;
  const month = dmy.month ? parseInt(dmy.month, 10) : 1;
  const year = dmy.year ? parseInt(dmy.year, 10) : 1971;
  return new Date(year, month, date);
};

interface FormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate?: DayMonthYearValues;
  timePeriodCoverageStartDateYearOnly?: string;
  releaseTypeId: string;
  scheduledReleaseDate: DayMonthYearValues;
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
    setReleaseSetupDetails(
      DummyPublicationsData.getReleaseSetupDetails(releaseId),
    );
    setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
    setReleaseTypes(DummyReferenceData.releaseTypeOptions);
  }, [releaseId]);

  const selectedTimePeriodCoverageGroup =
    releaseSetupDetails && releaseSetupDetails.timePeriodCoverageCode
      ? DummyReferenceData.findTimePeriodCoverageGroup(
          releaseSetupDetails.timePeriodCoverageCode,
        )
      : null;

  const getTimePeriodOptions = (
    timePeriodGroups: TimePeriodCoverageGroup[],
  ) => {
    const optGroups: Dictionary<SelectOption[]> = {};
    timePeriodGroups.forEach(group => {
      optGroups[group.label] = group.options.map(option => ({
        label: option.label,
        value: option.id,
      }));
    });
    return optGroups;
  };

  const formId = 'releaseSetupForm';

  const handleFormSubmission = async (values: FormValues) => {
    const release = DummyPublicationsData.getReleaseById(releaseId);

    release.timePeriodCoverage.code = values.timePeriodCoverageCode;

    if (values.timePeriodCoverageStartDate) {
      release.timePeriodCoverage.startDate = dayMonthYearToDate(
        values.timePeriodCoverageStartDate,
      );
    } else if (values.timePeriodCoverageStartDateYearOnly) {
      release.timePeriodCoverage.startDate = new Date(
        parseInt(values.timePeriodCoverageStartDateYearOnly, 10),
        1,
        1,
      );
    }

    release.scheduledReleaseDate = dayMonthYearToDate(
      values.scheduledReleaseDate,
    );
    release.nextReleaseExpectedDate = dayMonthYearToDate(
      values.nextReleaseExpectedDate,
    );
    release.releaseType = DummyReferenceData.findReleaseType(
      values.releaseTypeId,
    );
  };

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
          // ref={formikRef}
          onSubmit={async values => {
            await handleFormSubmission(values);
            history.push(setupRoute.generateLink(releaseId));
          }}
          initialValues={{
            timePeriodCoverageCode: releaseSetupDetails.timePeriodCoverageCode,
            timePeriodCoverageStartDate:
              selectedTimePeriodCoverageGroup &&
              DateType.DayMonthYear ===
                selectedTimePeriodCoverageGroup.startDateType
                ? dateToDayMonthYear(
                    releaseSetupDetails.timePeriodCoverageStartDate,
                  )
                : undefined,
            timePeriodCoverageStartDateYearOnly:
              selectedTimePeriodCoverageGroup &&
              DateType.Year === selectedTimePeriodCoverageGroup.startDateType
                ? releaseSetupDetails.timePeriodCoverageStartDate
                    .getFullYear()
                    .toString()
                : undefined,
            releaseTypeId: releaseSetupDetails.releaseType.id,
            scheduledReleaseDate: dateToDayMonthYear(
              releaseSetupDetails.scheduledReleaseDate,
            ),
            nextReleaseExpectedDate: dateToDayMonthYear(
              releaseSetupDetails.nextExpectedReleaseDate,
            ),
          }}
          validationSchema={Yup.object<FormValues>({
            timePeriodCoverageCode: Yup.string(),
            timePeriodCoverageStartDate: Yup.object({
              day: Yup.string(),
              month: Yup.string(),
              year: Yup.string(),
            }),
            releaseTypeId: Yup.string(),
            scheduledReleaseDate: Yup.object({
              day: Yup.string(),
              month: Yup.string(),
              year: Yup.string(),
            }),
            nextReleaseExpectedDate: Yup.object({
              day: Yup.string(),
              month: Yup.string(),
              year: Yup.string(),
            }),
          })}
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
                  {selectedTimePeriodCoverageGroup &&
                    DateType.Year ===
                      selectedTimePeriodCoverageGroup.startDateType && (
                      <FormFieldTextInput<FormValues>
                        id={`${formId}-timePeriodCoverageStartYearOnly`}
                        name="timePeriodCoverageStartDateYearOnly"
                        label={selectedTimePeriodCoverageGroup.startDateLabel}
                        width={4}
                      />
                    )}
                  {selectedTimePeriodCoverageGroup &&
                    form.values.timePeriodCoverageStartDate &&
                    DateType.DayMonthYear ===
                      selectedTimePeriodCoverageGroup.startDateType && (
                      <FormFieldDayMonthYear
                        fieldsetId={`${formId}-timePeriodCoverageStartDateFieldset`}
                        fieldsetLegend={
                          selectedTimePeriodCoverageGroup.startDateLabel
                        }
                        fieldIdPrefix={`${formId}-timePeriodCoverageStartDate`}
                        day={form.values.timePeriodCoverageStartDate.day}
                        month={form.values.timePeriodCoverageStartDate.month}
                        year={form.values.timePeriodCoverageStartDate.year}
                      />
                    )}
                </FormFieldset>

                <FormFieldDayMonthYear
                  fieldsetId={`${formId}-scheduledReleaseDateFieldset`}
                  fieldsetLegend="Schedule publish date"
                  fieldIdPrefix={`${formId}-scheduledReleaseDate`}
                  day={form.values.scheduledReleaseDate.day}
                  month={form.values.scheduledReleaseDate.month}
                  year={form.values.scheduledReleaseDate.year}
                />

                <FormFieldDayMonthYear
                  fieldsetId={`${formId}-nextExpectedReleaseDateFieldset`}
                  fieldsetLegend="Next release expected (optional)"
                  fieldIdPrefix={`${formId}-nextExpectedReleaseDate`}
                  day={form.values.nextReleaseExpectedDate.day}
                  month={form.values.nextReleaseExpectedDate.month}
                  year={form.values.nextReleaseExpectedDate.year}
                />

                <button
                  type="submit"
                  className="govuk-button govuk-!-margin-top-6"
                >
                  Update release setup
                </button>

                <div className="govuk-!-margin-top-6">
                  <Link to="/prototypes/publication-create-new-absence-config">
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
