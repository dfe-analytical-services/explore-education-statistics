import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import StatusBlock from '@admin/components/StatusBlock';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import permissionService from '@admin/services/permissionService';
import releaseService from '@admin/services/releaseService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import {
  PartialDate,
  isPartialDateEmpty,
  isValidPartialDate,
  parsePartialDateToUtcDate,
} from '@common/utils/date/partialDate';
import Yup from '@common/validation/yup';
import { endOfDay, format, formatISO, isValid } from 'date-fns';
import { Formik } from 'formik';
import React, { useState } from 'react';
import { StringSchema } from 'yup';

const errorCodeMappings = [
  errorCodeToFieldError(
    'APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE',
    'releaseStatus',
    'Enter a publish scheduled date before approving',
  ),
  errorCodeToFieldError(
    'ALL_DATAFILES_UPLOADED_MUST_BE_COMPLETE',
    'releaseStatus',
    'Check all uploaded datafiles are complete before approving',
  ),
];

interface FormValues {
  publishMethod?: 'Scheduled' | 'Immediate';
  publishScheduled?: Date;
  nextReleaseDate?: PartialDate;
  status: ReleaseApprovalStatus;
  internalReleaseNote: string;
}

const statusMap: {
  [keyof: string]: string;
} = {
  Draft: 'In Draft',
  HigherLevelReview: 'Awaiting higher review',
  Approved: 'Approved',
};

const formId = 'releaseStatusForm';

const ReleaseStatusPage = () => {
  const [showForm, setShowForm] = useState(false);

  const { releaseId, onChangeReleaseStatus } = useManageReleaseContext();

  const { value: summary, setState: setSummary } = useAsyncHandledRetry(
    () => releaseService.getReleaseSummary(releaseId),
    [showForm],
  );

  const { value: statusOptions = [] } = useAsyncRetry<RadioOption[]>(
    async () => {
      const [canMarkAsDraft, canSubmit, canApprove] = await Promise.all([
        permissionService.canMarkReleaseAsDraft(releaseId),
        permissionService.canSubmitReleaseForHigherLevelReview(releaseId),
        permissionService.canApproveRelease(releaseId),
      ]);

      return [
        {
          label: 'In draft',
          value: 'Draft',
          disabled: !canMarkAsDraft,
        },
        {
          label: 'Ready for higher review',
          value: 'HigherLevelReview',
          disabled: !canSubmit,
        },
        {
          label: 'Approved for publication',
          value: 'Approved',
          disabled: !canApprove,
        },
      ];
    },
  );

  const handleSubmit = useFormSubmit<FormValues>(async values => {
    const nextSummary = await releaseService.updateReleaseStatus(releaseId, {
      ...values,
      publishScheduled:
        values.status === 'Approved' &&
        values.publishScheduled &&
        values.publishMethod === 'Scheduled'
          ? formatISO(values.publishScheduled, {
              representation: 'date',
            })
          : '',
    });

    setSummary({
      isLoading: false,
      value: nextSummary,
    });

    setShowForm(false);

    onChangeReleaseStatus();
  }, errorCodeMappings);

  if (!summary) {
    return <LoadingSpinner />;
  }

  const isEditable = statusOptions?.some(option => !option.disabled);

  return (
    <>
      <h2>Release status</h2>

      {!showForm ? (
        <>
          <div className="govuk-!-margin-bottom-6">
            The current release status is:{' '}
            <StatusBlock
              text={statusMap[summary.status]}
              id={`CurrentReleaseStatus-${statusMap[summary.status]}`}
            />
            {summary.status === 'Approved' && (
              <div className="govuk-!-margin-top-1">
                Release process status:{' '}
                <ReleaseServiceStatus releaseId={releaseId} />
              </div>
            )}
          </div>

          {isEditable && (
            <Button
              className="govuk-!-margin-top-2"
              onClick={() => setShowForm(true)}
            >
              Edit release status
            </Button>
          )}
        </>
      ) : (
        <Formik<FormValues>
          enableReinitialize
          initialValues={{
            status: summary.status,
            internalReleaseNote: summary.internalReleaseNote,
            publishMethod: summary.publishScheduled ? 'Scheduled' : undefined,
            publishScheduled: summary.publishScheduled
              ? new Date(summary.publishScheduled)
              : undefined,
            nextReleaseDate: summary.nextReleaseDate,
          }}
          onSubmit={handleSubmit}
          validationSchema={Yup.object<FormValues>({
            status: Yup.string().required('Choose a status') as StringSchema<
              FormValues['status']
            >,
            internalReleaseNote: Yup.string().when('releaseStatus', {
              is: value => ['Approved', 'HigherLevelReview'].includes(value),
              then: Yup.string().required('Provide an internal release note'),
            }),
            publishMethod: Yup.string().when('status', {
              is: 'Approved',
              then: Yup.string().required('Choose when to publish'),
            }) as StringSchema<FormValues['publishMethod']>,
            publishScheduled: Yup.date().when('publishMethod', {
              is: 'Scheduled',
              then: Yup.date()
                .required('Enter a valid publish date')
                .test({
                  name: 'validDateIfAfterToday',
                  message: `Publish date can't be before ${format(
                    new Date(),
                    'do MMMM yyyy',
                  )}`,
                  test(value) {
                    return endOfDay(value) >= endOfDay(new Date());
                  },
                }),
            }),
            nextReleaseDate: Yup.object<PartialDate>({
              day: Yup.number().notRequired(),
              month: Yup.number(),
              year: Yup.number(),
            })
              .notRequired()
              .test({
                name: 'validDate',
                message: 'Enter a valid next release date',
                test(value: PartialDate) {
                  if (isPartialDateEmpty(value)) {
                    return true;
                  }

                  if (!isValidPartialDate(value)) {
                    return false;
                  }

                  return isValid(parsePartialDateToUtcDate(value));
                },
              }),
          })}
        >
          {form => {
            return (
              <Form id={formId}>
                <FormFieldRadioGroup<FormValues>
                  legend="Status"
                  name="status"
                  id={`${formId}-status`}
                  options={statusOptions}
                  orderDirection={[]}
                />
                <FormFieldTextArea
                  name="internalReleaseNote"
                  className="govuk-!-width-one-half"
                  id={`${formId}-internalReleaseNote`}
                  label="Internal release note"
                  rows={3}
                />

                {form.values.status === 'Approved' && (
                  <FormFieldRadioGroup<FormValues>
                    id={`${formId}-publishMethod`}
                    name="publishMethod"
                    legend="When to publish"
                    legendSize="m"
                    hint="Do you want to publish this release on a specific date or as soon as possible?"
                    order={[]}
                    options={[
                      {
                        label: 'On a specific date',
                        value: 'Scheduled',
                        conditional: (
                          <FormFieldDateInput<FormValues>
                            id={`${formId}-publishScheduled`}
                            name="publishScheduled"
                            legend="Publish date"
                            legendSize="s"
                          />
                        ),
                      },
                      {
                        label: 'As soon as possible',
                        value: 'Immediate',
                      },
                    ]}
                  />
                )}

                <FormFieldDateInput<FormValues>
                  id={`${formId}-nextReleaseDate`}
                  name="nextReleaseDate"
                  legend="Next release expected (optional)"
                  legendSize="m"
                  type="partialDate"
                  partialDateType="monthYear"
                />

                <ButtonGroup>
                  <Button type="submit">Update status</Button>
                  <ButtonText
                    onClick={() => {
                      form.resetForm();
                      setShowForm(false);
                    }}
                  >
                    Cancel
                  </ButtonText>
                </ButtonGroup>
              </Form>
            );
          }}
        </Formik>
      )}
    </>
  );
};

export default ReleaseStatusPage;
