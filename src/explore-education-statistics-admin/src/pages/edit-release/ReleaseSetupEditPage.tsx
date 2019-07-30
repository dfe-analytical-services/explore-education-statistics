import Link from '@admin/components/Link';
import DummyReferenceData, {
  DateType,
  TimePeriodCoverageGroup,
} from '@admin/pages/DummyReferenceData';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import { setupRoute } from '@admin/routes/releaseRoutes';
import { DayMonthYearValues, IdLabelPair } from '@admin/services/common/types';
import service from '@admin/services/edit-release/setup/service';
import {
  ReleaseSetupDetails,
  ReleaseSetupDetailsUpdateRequest,
} from '@admin/services/edit-release/setup/types';
import {
  shapeOfDayMonthYearField,
  validateMandatoryDayMonthYearField,
  validateOptionalPartialDayMonthYearField,
} from '@admin/validation/validation';
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
    service.getReleaseSetupDetails(releaseId).then(release => {
      setReleaseSetupDetails(release);
      setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
      setReleaseTypes(DummyReferenceData.releaseTypeOptions);
    });
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
    <>
      {releaseSetupDetails && timePeriodCoverageGroups && releaseTypes && (
        <ReleasePageTemplate
          releaseId={releaseId}
          publicationTitle={releaseSetupDetails.publicationTitle}
        >
          <h2 className="govuk-heading-m">Edit release setup</h2>

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
            onSubmit={async (values: FormValues) => {
              const updatedReleaseDetails: ReleaseSetupDetailsUpdateRequest = {
                id: releaseId,
                timePeriodCoverageCode: values.timePeriodCoverageCode,
                timePeriodCoverageStartDate:
                  isDayMonthYearDateTypeCodeSelected(
                    values.timePeriodCoverageCode,
                  ) && values.timePeriodCoverageStartDate
                    ? values.timePeriodCoverageStartDate
                    : {
                        year: values.timePeriodCoverageStartDateYearOnly,
                      },
                scheduledPublishDate: values.scheduledPublishDate,
                nextReleaseExpectedDate: values.nextReleaseExpectedDate,
                releaseType: DummyReferenceData.findReleaseType(
                  values.releaseTypeId,
                ),
              };
              service
                .updateReleaseSetupDetails(updatedReleaseDetails)
                .then(_ => history.push(setupRoute.generateLink(releaseId)));
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
                    Update release status
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
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseSetupEditPage;
