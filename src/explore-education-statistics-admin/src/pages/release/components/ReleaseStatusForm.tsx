import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import {
  ReleaseVersion,
  ReleaseVersionChecklistErrorCode,
} from '@admin/services/releaseVersionService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import {
  isPartialDateEmpty,
  isValidPartialDate,
  parsePartialDateToLocalDate,
  PartialDate,
} from '@common/utils/date/partialDate';
import {
  hasErrorMessage,
  isServerValidationError,
  mapFallbackFieldError,
  mapFieldErrors,
} from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { endOfDay, format, isValid, parseISO } from 'date-fns';
import keyBy from 'lodash/keyBy';
import mapValues from 'lodash/mapValues';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

export interface ReleaseStatusFormValues {
  publishMethod?: 'Scheduled' | 'Immediate';
  publishScheduled?: Date;
  nextReleaseDate?: PartialDate;
  approvalStatus: ReleaseApprovalStatus;
  notifySubscribers?: boolean;
  internalReleaseNote?: string;
  updatePublishedDate?: boolean;
}

export const formId = 'releaseStatusForm';

const errorMappings = [
  mapFieldErrors<ReleaseStatusFormValues>({
    target: 'approvalStatus',
    messages: {
      PublishedReleaseCannotBeUnapproved:
        'Release has already been published and cannot be un-approved',
      ...mapValues(
        keyBy(ReleaseVersionChecklistErrorCode, value => value),
        _ => 'Resolve all errors in the publishing checklist',
      ),
    },
  }),
  mapFieldErrors<ReleaseStatusFormValues>({
    target: 'publishScheduled',
    messages: {
      PublishDateCannotBeEmpty: 'Enter a publish date before approving',
      PublishDateCannotBeScheduled:
        'Release must be scheduled at least one day in advance of the publishing day',
    },
  }),
  mapFieldErrors<ReleaseStatusFormValues>({
    target: 'nextReleaseDate',
    messages: {
      PartialDateNotValid: 'Enter a valid date',
    },
  }),
];

const fallbackErrorMapping = mapFallbackFieldError<ReleaseStatusFormValues>({
  target: 'approvalStatus',
  fallbackMessage:
    'There was a problem updating the approval status of this release',
});

interface Props {
  releaseVersion: ReleaseVersion;
  statusPermissions: ReleaseStatusPermissions;
  onCancel: () => void;
  onSubmit: (values: ReleaseStatusFormValues) => Promise<void> | void;
}

const ReleaseStatusForm = ({
  releaseVersion,
  statusPermissions,
  onCancel,
  onSubmit,
}: Props) => {
  const [showConfirmScheduleModal, toggleConfirmScheduleModal] =
    useToggle(false);
  const [showScheduleErrorModal, toggleScheduleErrorModal] = useToggle(false);

  const handleSubmitForm = async ({
    approvalStatus,
    publishMethod,
    publishScheduled,
    notifySubscribers,
    updatePublishedDate,
    ...values
  }: ReleaseStatusFormValues) => {
    const isApproved = approvalStatus === 'Approved';

    try {
      await onSubmit({
        ...values,
        approvalStatus,
        publishMethod: isApproved ? publishMethod : undefined,
        publishScheduled:
          isApproved && publishScheduled && publishMethod === 'Scheduled'
            ? publishScheduled
            : undefined,
        notifySubscribers:
          isApproved && releaseVersion.amendment
            ? notifySubscribers
            : undefined,
        updatePublishedDate:
          isApproved && releaseVersion.amendment
            ? updatePublishedDate
            : undefined,
      });
    } catch (err) {
      if (
        isServerValidationError(err) &&
        hasErrorMessage(err, ['PublishDateCannotBeScheduled'])
      ) {
        toggleScheduleErrorModal.on();
      }

      throw err;
    }
  };

  const validationSchema = useMemo<
    ObjectSchema<ReleaseStatusFormValues>
  >(() => {
    const schema = Yup.object({
      approvalStatus: Yup.string()
        .oneOf(['Draft', 'HigherLevelReview', 'Approved'])
        .required('Choose a status'),
      internalReleaseNote: Yup.string().when('approvalStatus', {
        is: (value: string) =>
          ['Approved', 'HigherLevelReview'].includes(value),
        then: s => s.required('Enter an internal note'),
      }),
      publishMethod: Yup.string()
        .oneOf(['Scheduled', 'Immediate'])
        .when('approvalStatus', {
          is: 'Approved',
          then: s => s.required('Choose when to publish'),
        }),
      publishScheduled: Yup.date().when('publishMethod', {
        is: 'Scheduled',
        then: s =>
          s.required('Enter a valid publish date').test({
            name: 'validDateIfAfterToday',
            message: `Publish date cannot be before ${format(
              new Date(),
              'do MMMM yyyy',
            )}`,
            test(value) {
              return endOfDay(value) >= endOfDay(new Date());
            },
          }),
      }),

      nextReleaseDate: Yup.object({
        month: Yup.number(),
        year: Yup.number().required(),
      })
        .default(undefined)
        .test({
          name: 'validDate',
          message: 'Enter a valid next release date',
          test: value => {
            if (!value || isPartialDateEmpty(value as PartialDate)) {
              return true;
            }

            if (!isValidPartialDate(value)) {
              return false;
            }

            return isValid(parsePartialDateToLocalDate(value));
          },
        }),
      notifySubscribers: Yup.boolean(),
      updatePublishedDate: Yup.boolean(),
    });

    if (releaseVersion.amendment) {
      return schema.shape({
        notifySubscribers: Yup.boolean().when('approvalStatus', {
          is: (value: string) => value === 'Approved',
          then: s => s.required(),
        }),
        updatePublishedDate: Yup.boolean().when('approvalStatus', {
          is: (value: ReleaseApprovalStatus) => value === 'Approved',
          then: s => s.required(),
        }),
      });
    }

    return schema;
  }, [releaseVersion.amendment]);

  return (
    <FormProvider
      errorMappings={errorMappings}
      fallbackErrorMapping={fallbackErrorMapping}
      initialValues={{
        approvalStatus: releaseVersion.approvalStatus,
        notifySubscribers: releaseVersion.amendment
          ? releaseVersion.notifySubscribers
          : undefined,
        updatePublishedDate: releaseVersion.amendment
          ? releaseVersion.updatePublishedDate
          : undefined,
        internalReleaseNote: releaseVersion.latestInternalReleaseNote,
        publishMethod: releaseVersion.publishScheduled
          ? 'Scheduled'
          : undefined,
        publishScheduled: releaseVersion.publishScheduled
          ? parseISO(releaseVersion.publishScheduled)
          : undefined,
        nextReleaseDate: releaseVersion.nextReleaseDate,
      }}
      validationSchema={validationSchema}
    >
      {({ formState, getValues, handleSubmit, reset, setValue, watch }) => {
        const approvalStatus = watch('approvalStatus');
        return (
          <>
            <Form id={formId} onSubmit={handleSubmitForm}>
              <FormFieldRadioGroup<ReleaseStatusFormValues>
                legend="Status"
                name="approvalStatus"
                order={[]}
                options={[
                  {
                    label: 'In draft',
                    value: 'Draft',
                    disabled: !statusPermissions?.canMarkDraft,
                  },
                  {
                    label:
                      'Ready for higher review (this will notify approvers)',
                    value: 'HigherLevelReview',
                    disabled: !statusPermissions?.canMarkHigherLevelReview,
                  },
                  {
                    label: 'Approved for publication',
                    value: 'Approved',
                    disabled: !statusPermissions?.canMarkApproved,
                  },
                ]}
                onChange={element => {
                  if (
                    releaseVersion.amendment &&
                    element.target.value === 'Approved'
                  ) {
                    setValue('notifySubscribers' as const, true);
                    setValue('updatePublishedDate' as const, false);
                  }
                }}
              />

              <FormFieldTextArea<ReleaseStatusFormValues>
                name="internalReleaseNote"
                className="govuk-!-width-one-half"
                label="Internal note"
                hint="Please include any relevant information"
                rows={3}
              />

              {approvalStatus === 'Approved' && releaseVersion.amendment && (
                <>
                  <FormFieldCheckbox
                    name="notifySubscribers"
                    label="Notify subscribers by email"
                  />

                  <FormFieldCheckbox<ReleaseStatusFormValues>
                    name="updatePublishedDate"
                    label="Update published date"
                    conditional={
                      <WarningMessage className="govuk-!-width-two-thirds">
                        The release's published date in the public view will be
                        updated once the publication is complete.
                      </WarningMessage>
                    }
                  />
                </>
              )}

              {approvalStatus === 'Approved' && (
                <FormFieldRadioGroup<ReleaseStatusFormValues>
                  name="publishMethod"
                  legend="When to publish"
                  legendSize="m"
                  hint="Do you want to publish this release on a specific date or immediately?"
                  order={[]}
                  options={[
                    {
                      label: 'On a specific date',
                      value: 'Scheduled',
                      conditional: (
                        <>
                          {releaseVersion.preReleaseUsersOrInvitesAdded && (
                            <WarningMessage className="govuk-!-width-two-thirds">
                              Pre-release users will have access to a preview of
                              the release 24 hours before the scheduled publish
                              date.
                            </WarningMessage>
                          )}
                          <FormFieldDateInput<ReleaseStatusFormValues>
                            name="publishScheduled"
                            legend="Publish date"
                            legendSize="s"
                          />
                        </>
                      ),
                    },
                    {
                      label: 'Immediately',
                      value: 'Immediate',
                      conditional: (
                        <>
                          <p className="govuk-!-width-two-thirds">
                            The time taken by the release process will vary.
                            Contact us if the release has not been published
                            within one hour.
                          </p>
                          {releaseVersion.preReleaseUsersOrInvitesAdded && (
                            <WarningMessage className="govuk-!-width-two-thirds">
                              Pre-release users will not have access to a
                              preview of the release if it is published
                              immediately.
                            </WarningMessage>
                          )}
                        </>
                      ),
                    },
                  ]}
                />
              )}

              <FormFieldDateInput<ReleaseStatusFormValues>
                name="nextReleaseDate"
                legend="Next release expected (optional)"
                legendSize="m"
                type="partialDate"
                partialDateType="monthYear"
              />

              <ButtonGroup>
                <ModalConfirm
                  title="Confirm publish date"
                  open={showConfirmScheduleModal}
                  onConfirm={async () => {
                    await handleSubmit(handleSubmitForm)();
                    toggleConfirmScheduleModal.off();
                  }}
                  onCancel={toggleConfirmScheduleModal.off}
                  onExit={toggleConfirmScheduleModal.off}
                  triggerButton={
                    <Button
                      type="submit"
                      disabled={formState.isSubmitting}
                      onClick={async e => {
                        e.preventDefault();

                        if (
                          approvalStatus === 'Approved' &&
                          getValues('publishMethod') === 'Scheduled' &&
                          getValues('publishScheduled')
                        ) {
                          if (formState.isValid) {
                            toggleConfirmScheduleModal.on();
                            return;
                          }
                        }

                        await handleSubmit(handleSubmitForm)();
                      }}
                    >
                      Update status
                    </Button>
                  }
                >
                  <p>
                    This release will be published at 09:30 on{' '}
                    <FormattedDate format="EEEE d MMMM yyyy">
                      {getValues('publishScheduled') || ''}
                    </FormattedDate>
                    .
                  </p>
                  <p>
                    Once confirmed, if you need to change or cancel the
                    publishing for any reason, you must come back to this page
                    to change it yourself. If you need any support, please
                    contact{' '}
                    <a href="mailto:explore.statistics@education.gov.uk">
                      explore.statistics@education.gov.uk
                    </a>
                    .
                  </p>
                  <p>Are you sure?</p>
                </ModalConfirm>

                <ButtonText
                  onClick={() => {
                    reset();
                    onCancel();
                  }}
                >
                  Cancel
                </ButtonText>
              </ButtonGroup>
            </Form>

            <ModalConfirm
              title="Publish date cannot be scheduled"
              confirmText="Back to form"
              showCancel={false}
              open={showScheduleErrorModal}
              onConfirm={toggleScheduleErrorModal.off}
              onExit={toggleScheduleErrorModal.off}
            >
              <WarningMessage>
                Release must be scheduled at least one day in advance of the
                publishing day. Please speak to{' '}
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>{' '}
                if this is an issue.
              </WarningMessage>
            </ModalConfirm>
          </>
        );
      }}
    </FormProvider>
  );
};

export default ReleaseStatusForm;
