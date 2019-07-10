import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import DummyReferenceData, {
  DateType,
  TimePeriodCoverageGroup,
} from '@admin/pages/DummyReferenceData';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldDayMonthYear from '@common/components/form/FormFieldDayMonthYear';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { SelectOption } from '@common/components/form/FormSelect';
import { isValid } from 'date-fns';
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
  const month = dmy.month ? parseInt(dmy.month, 10) - 1 : 0;
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
    const release = DummyPublicationsData.getReleaseSetupDetails(releaseId);

    setReleaseSetupDetails(release);
    setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
    setReleaseTypes(DummyReferenceData.releaseTypeOptions);
  }, [releaseId]);

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

  const selectedTimePeriodCoverageGroup =
    releaseSetupDetails &&
    DummyReferenceData.findTimePeriodCoverageGroup(
      releaseSetupDetails.timePeriodCoverageCode,
    );

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
                is: val =>
                  DateType.DayMonthYear ===
                  DummyReferenceData.findTimePeriodCoverageGroup(val)
                    .startDateType,
                then: Yup.object({
                  day: Yup.string().required('Enter a start day'),
                  month: Yup.string().required('Enter a start month'),
                  year: Yup.string().required('Enter a start year'),
                }),
                otherwise: Yup.object({
                  day: Yup.string(),
                  month: Yup.string(),
                  year: Yup.string(),
                }),
              },
            ),
            timePeriodCoverageStartDateYearOnly: Yup.string().when(
              'timePeriodCoverageCode',
              {
                is: val =>
                  DateType.Year ===
                  DummyReferenceData.findTimePeriodCoverageGroup(val)
                    .startDateType,
                then: Yup.string().required('Enter a start year'),
                otherwise: Yup.string(),
              },
            ),
            releaseTypeId: Yup.string(),
            scheduledReleaseDate: Yup.object({
              day: Yup.string().required('Enter a day'),
              month: Yup.string().required('Enter a month'),
              year: Yup.string().required('Enter a year'),
            }).test('validDate', 'Enter a valid date', value =>
              isValid(new Date(value.year, value.month, value.day)),
            ),
            nextReleaseExpectedDate: Yup.object({
              day: Yup.string(),
              month: Yup.string(),
              year: Yup.string(),
            }).test('validDate', 'Enter a valid date', value =>
              isValid(new Date(value.year, value.month, value.day)),
            ),
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
                  {form.values.timePeriodCoverageCode &&
                    DateType.Year ===
                      DummyReferenceData.findTimePeriodCoverageGroup(
                        form.values.timePeriodCoverageCode,
                      ).startDateType && (
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
                  {form.values.timePeriodCoverageCode &&
                    DateType.DayMonthYear ===
                      DummyReferenceData.findTimePeriodCoverageGroup(
                        form.values.timePeriodCoverageCode,
                      ).startDateType && (
                      <FormFieldDayMonthYear
                        fieldName="timePeriodCoverageStartDate"
                        fieldsetLegend={
                          DummyReferenceData.findTimePeriodCoverageGroup(
                            form.values.timePeriodCoverageCode,
                          ).startDateLabel
                        }
                        day={
                          form.values.timePeriodCoverageStartDate
                            ? form.values.timePeriodCoverageStartDate.day
                            : ''
                        }
                        month={
                          form.values.timePeriodCoverageStartDate
                            ? form.values.timePeriodCoverageStartDate.month
                            : ''
                        }
                        year={
                          form.values.timePeriodCoverageStartDate
                            ? form.values.timePeriodCoverageStartDate.year
                            : ''
                        }
                      />
                    )}
                </FormFieldset>

                <FormFieldDayMonthYear
                  fieldName="scheduledReleaseDate"
                  fieldsetLegend="Schedule publish date"
                  day={form.values.scheduledReleaseDate.day}
                  month={form.values.scheduledReleaseDate.month}
                  year={form.values.scheduledReleaseDate.year}
                />

                <FormFieldDayMonthYear
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
