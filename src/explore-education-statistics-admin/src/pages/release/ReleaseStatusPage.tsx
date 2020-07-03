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
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import {
  PartialDate,
  isPartialDateEmpty,
  isValidPartialDate,
  parsePartialDateToUtcDate,
  formatPartialDate,
} from '@common/utils/date/partialDate';
import Yup from '@common/validation/yup';
import { endOfDay, format, formatISO, isValid } from 'date-fns';
import { Formik } from 'formik';
import React, { useState } from 'react';
import { StringSchema } from 'yup';

const errorCodeMappings = [
  errorCodeToFieldError(
    'APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE',
    'status',
    'Enter a publish scheduled date before approving',
  ),
  errorCodeToFieldError(
    'ALL_DATAFILES_UPLOADED_MUST_BE_COMPLETE',
    'status',
    'Check all uploaded datafiles are complete before approving',
  ),
  errorCodeToFieldError(
    'PUBLISHED_RELEASE_CANNOT_BE_UNAPPROVED',
    'status',
    'Release has already been published and cannot be un-approved',
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

  const { value: release, setState: setRelease } = useAsyncHandledRetry(
    () => releaseService.getRelease(releaseId),
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
    if (!release) {
      throw new Error('Could not update missing release');
    }

    const nextRelease = await releaseService.updateRelease(releaseId, {
      ...release,
      typeId: release.type.id,
      ...values,
      publishScheduled:
        values.status === 'Approved' &&
        values.publishScheduled &&
        values.publishMethod === 'Scheduled'
          ? formatISO(values.publishScheduled, {
              representation: 'date',
            })
          : undefined,
    });

    setRelease({
      isLoading: false,
      value: nextRelease,
    });

    setShowForm(false);

    onChangeReleaseStatus();
  }, errorCodeMappings);

  if (!release) {
    return <LoadingSpinner />;
  }

  const isEditable = statusOptions?.some(option => !option.disabled);

  return (
    <>
      {!showForm ? (
        <>
          <h2>Release status</h2>

          <SummaryList>
            <SummaryListItem term="Current status">
              <StatusBlock
                text={statusMap[release.status]}
                id={`CurrentReleaseStatus-${statusMap[release.status]}`}
              />
            </SummaryListItem>
            {release.status === 'Approved' && (
              <SummaryListItem term="Release process status">
                <ReleaseServiceStatus releaseId={releaseId} />
              </SummaryListItem>
            )}
            <SummaryListItem term="Scheduled release">
              {release.publishScheduled ? (
                <FormattedDate>{release.publishScheduled}</FormattedDate>
              ) : (
                'Not scheduled'
              )}
            </SummaryListItem>
            <SummaryListItem term="Next release expected">
              {isValidPartialDate(release.nextReleaseDate) ? (
                <time>{formatPartialDate(release.nextReleaseDate)}</time>
              ) : (
                'Not set'
              )}
            </SummaryListItem>
          </SummaryList>

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
            status: release.status,
            internalReleaseNote: release.internalReleaseNote ?? '',
            publishMethod: release.publishScheduled ? 'Scheduled' : undefined,
            publishScheduled: release.publishScheduled
              ? new Date(release.publishScheduled)
              : undefined,
            nextReleaseDate: release.nextReleaseDate,
          }}
          onSubmit={handleSubmit}
          validationSchema={Yup.object<FormValues>({
            status: Yup.string().required('Choose a status') as StringSchema<
              FormValues['status']
            >,
            internalReleaseNote: Yup.string().when('status', {
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
                <h2>Edit release status</h2>

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
