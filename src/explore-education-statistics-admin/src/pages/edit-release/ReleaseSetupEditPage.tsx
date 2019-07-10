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
  dateToDayMonthYear,
  dayMonthYearToDate,
  DayMonthYearValues,
  IdLabelPair,
  ReleaseSetupDetails,
} from '../../services/types/types';

interface MatchProps {
  releaseId: string;
}

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
        label: `${option.label} - ${option.id}`,
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

    release.scheduledReleaseDate = values.scheduledReleaseDate;

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

  const isDayMonthYearDateTypeSelected = (
    timePeriodGroup?: TimePeriodCoverageGroup,
  ) =>
    timePeriodGroup && DateType.DayMonthYear === timePeriodGroup.startDateType;

  const isYearOnlyDateTypeSelected = (
    timePeriodGroup?: TimePeriodCoverageGroup,
  ) => timePeriodGroup && DateType.Year === timePeriodGroup.startDateType;

  const validateMandatoryDayMonthYearField = Yup.object({
    day: Yup.number().required('Enter a day'),
    month: Yup.number().required('Enter a month'),
    year: Yup.number().required('Enter a year'),
  }).test('validDate', 'Enter a valid date', value =>
    isValid(new Date(value.year, value.month, value.day)),
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
          onSubmit={async values => {
            await handleFormSubmission(values);
            history.push(setupRoute.generateLink(releaseId));
          }}
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
              ? releaseSetupDetails.timePeriodCoverageStartDate
                  .getFullYear()
                  .toString()
              : undefined,
            releaseTypeId: releaseSetupDetails.releaseType.id,
            scheduledReleaseDate: releaseSetupDetails.scheduledReleaseDate,
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
                then: validateMandatoryDayMonthYearField,
                otherwise: Yup.object({
                  day: Yup.mixed()
                    .transform(val =>
                      val === '' ? undefined : parseInt(val, 10),
                    )
                    .nullable(),
                  month: Yup.mixed()
                    .transform(val =>
                      val === '' ? undefined : parseInt(val, 10),
                    )
                    .nullable(),
                  year: Yup.mixed()
                    .transform(val =>
                      val === '' ? undefined : parseInt(val, 10),
                    )
                    .nullable(),
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
              day: Yup.mixed()
                .transform(val => (val === '' ? undefined : parseInt(val, 10)))
                .nullable(),
              month: Yup.mixed()
                .transform(val => (val === '' ? undefined : parseInt(val, 10)))
                .nullable(),
              year: Yup.mixed()
                .transform(val => (val === '' ? undefined : parseInt(val, 10)))
                .nullable(),
            }).test(
              'validDateWithAllFieldsDefined',
              'Enter a valid date',
              value => {
                if (!value.year || !value.month || !value.day) {
                  return true;
                }
                return isValid(new Date(value.year, value.month, value.day));
              },
            ),
            nextReleaseExpectedDate: Yup.object({
              day: Yup.number(),
              month: Yup.number(),
              year: Yup.number(),
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
                  fieldName="scheduledReleaseDate"
                  fieldsetLegend="Schedule publish date (optional)"
                  day={form.values.scheduledReleaseDate.day}
                  month={form.values.scheduledReleaseDate.month}
                  year={form.values.scheduledReleaseDate.year}
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
