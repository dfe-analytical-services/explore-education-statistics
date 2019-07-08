import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import DummyReferenceData, {
  DateType,
  TimePeriodCoverageGroup,
} from '@admin/pages/DummyReferenceData';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import {
  Form,
  FormFieldset,
  FormGroup,
  Formik,
  FormTextInput,
} from '@common/components/form';
import FormFieldDayMonthYear, {
  dateToDayMonthYear,
} from '@common/components/form/FormFieldDayMonthYear';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import { SelectOption } from '@common/components/form/FormSelect';
import Yup from '@common/lib/validation/yup';
import { Dictionary } from '@common/types';
import { FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
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

interface FormValues {
  timePeriodCoverageCode: string;
  timePeriodCoverageStartDate: DayMonthYearValues;
  releaseTypeId: string;
  scheduledReleaseDate: DayMonthYearValues;
  nextExpectedReleaseDate: DayMonthYearValues;
}

const ReleaseSetupEditPage = ({ match }: RouteComponentProps<MatchProps>) => {
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
            // await onSubmit(values);
            // goToNextStep();
          }}
          initialValues={{
            timePeriodCoverageCode: releaseSetupDetails.timePeriodCoverageCode,
            timePeriodCoverageStartDate: dateToDayMonthYear(
              releaseSetupDetails.timePeriodCoverageStartDate,
            ),
            releaseTypeId: releaseSetupDetails.releaseType.id,
            scheduledReleaseDate: dateToDayMonthYear(
              releaseSetupDetails.scheduledReleaseDate,
            ),
            nextExpectedReleaseDate: dateToDayMonthYear(
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
            nextExpectedReleaseDate: Yup.object({
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
                  <FormFieldSelect
                    id={`${formId}-timePeriodCoverage`}
                    label="Type"
                    name="time-period"
                    optGroups={getTimePeriodOptions(timePeriodCoverageGroups)}
                    value={form.values.timePeriodCoverageCode}
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
                      <FormGroup>
                        <FormTextInput
                          id={`${formId}-timePeriodCoverageStartYear`}
                          name="release-year"
                          label={selectedTimePeriodCoverageGroup.startDateLabel}
                          value={form.values.timePeriodCoverageStartDate.year}
                          width={4}
                        />
                      </FormGroup>
                    )}
                  {selectedTimePeriodCoverageGroup &&
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
                  day={form.values.nextExpectedReleaseDate.day}
                  month={form.values.nextExpectedReleaseDate.month}
                  year={form.values.nextExpectedReleaseDate.year}
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
