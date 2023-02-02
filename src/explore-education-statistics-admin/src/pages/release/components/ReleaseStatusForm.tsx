import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import {
  Release,
  ReleaseChecklistErrorCode,
} from '@admin/services/releaseService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import {
  Form,
  FormFieldCheckbox,
  FormFieldRadioGroup,
} from '@common/components/form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useFormSubmit from '@common/hooks/useFormSubmit';
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
import { Formik } from 'formik';
import { keyBy, mapValues } from 'lodash';
import React from 'react';
import { StringSchema } from 'yup';

export interface ReleaseStatusFormValues {
  publishMethod?: 'Scheduled' | 'Immediate';
  publishScheduled?: Date;
  nextReleaseDate?: PartialDate;
  approvalStatus: ReleaseApprovalStatus;
  notifySubscribers?: boolean;
  internalReleaseNote?: string;
}

export const formId = 'releaseStatusForm';

const errorMappings = [
  mapFieldErrors<ReleaseStatusFormValues>({
    target: 'approvalStatus',
    messages: {
      PublishedReleaseCannotBeUnapproved:
        'Release has already been published and cannot be un-approved',
      ...mapValues(
        keyBy(ReleaseChecklistErrorCode, value => value),
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
  release: Release;
  statusPermissions: ReleaseStatusPermissions;
  onCancel: () => void;
  onSubmit: (values: ReleaseStatusFormValues) => void;
}

const ReleaseStatusForm = ({
  release,
  statusPermissions,
  onCancel,
  onSubmit,
}: Props) => {
  const [showConfirmScheduleModal, toggleConfirmScheduleModal] = useToggle(
    false,
  );
  const [showScheduleErrorModal, toggleScheduleErrorModal] = useToggle(false);

  const handleSubmit = useFormSubmit<ReleaseStatusFormValues>(
    async ({ approvalStatus, publishMethod, publishScheduled, ...values }) => {
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
    },
    errorMappings,
    fallbackErrorMapping,
  );

  return (
    <Formik<ReleaseStatusFormValues>
      enableReinitialize
      initialValues={{
        approvalStatus: release.approvalStatus,
        notifySubscribers: release.notifySubscribers,
        internalReleaseNote: release.latestInternalReleaseNote,
        publishMethod: release.publishScheduled ? 'Scheduled' : undefined,
        publishScheduled: release.publishScheduled
          ? parseISO(release.publishScheduled)
          : undefined,
        nextReleaseDate: release.nextReleaseDate,
      }}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<ReleaseStatusFormValues>({
        approvalStatus: Yup.string().required(
          'Choose a status',
        ) as StringSchema<ReleaseStatusFormValues['approvalStatus']>,
        notifySubscribers: Yup.boolean().when('approvalStatus', {
          is: value => value === 'Approved',
          then: Yup.boolean().required(),
        }),
        internalReleaseNote: Yup.string().when('approvalStatus', {
          is: value => ['Approved', 'HigherLevelReview'].includes(value),
          then: Yup.string().required('Enter an internal note'),
        }),
        publishMethod: Yup.string().when('approvalStatus', {
          is: 'Approved',
          then: Yup.string().required('Choose when to publish'),
        }) as StringSchema<ReleaseStatusFormValues['publishMethod']>,
        publishScheduled: Yup.date().when('publishMethod', {
          is: 'Scheduled',
          then: Yup.date()
            .required('Enter a valid publish date')
            .test({
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

              return isValid(parsePartialDateToLocalDate(value));
            },
          }),
      })}
    >
      {form => (
        <>
          <Form id={formId}>
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
                  label: 'Ready for higher review (this will notify approvers)',
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
                if (element.target.value === 'Approved') {
                  form.setFieldValue('notifySubscribers', true);
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

            {form.values.approvalStatus === 'Approved' && release.amendment && (
              <FormFieldCheckbox
                name="notifySubscribers"
                label="Notify subscribers by email"
              />
            )}

            {form.values.approvalStatus === 'Approved' && (
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
                      <FormFieldDateInput<ReleaseStatusFormValues>
                        name="publishScheduled"
                        legend="Publish date"
                        legendSize="s"
                      />
                    ),
                  },
                  {
                    label: 'Immediately',
                    value: 'Immediate',
                    conditional: (
                      <WarningMessage className="govuk-!-width-two-thirds">
                        The time taken by the release process will vary. Contact
                        us if the release has not been published within one
                        hour.
                      </WarningMessage>
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
              <Button
                type="submit"
                disabled={form.isSubmitting}
                onClick={async e => {
                  e.preventDefault();

                  if (form.values.approvalStatus !== 'Approved') {
                    form.setFieldValue('notifySubscribers', undefined);
                  }

                  if (
                    form.values.approvalStatus === 'Approved' &&
                    form.values.publishMethod === 'Scheduled' &&
                    form.values.publishScheduled
                  ) {
                    // Ensure validation has been run as form state
                    // may not be up-to-date (seems to only affect tests).
                    const errors = await form.validateForm();

                    if (Object.keys(errors).length === 0) {
                      toggleConfirmScheduleModal.on();
                      return;
                    }
                  }

                  await form.submitForm();
                }}
              >
                Update status
              </Button>
              <ButtonText
                onClick={() => {
                  form.resetForm();
                  onCancel();
                }}
              >
                Cancel
              </ButtonText>
            </ButtonGroup>
          </Form>

          <ModalConfirm
            title="Confirm publish date"
            open={showConfirmScheduleModal}
            onConfirm={async () => {
              await form.submitForm();
              toggleConfirmScheduleModal.off();
            }}
            onExit={toggleConfirmScheduleModal.off}
          >
            <p>
              This release will be published at 09:30 on{' '}
              <FormattedDate format="EEEE d MMMM yyyy">
                {form.values.publishScheduled || ''}
              </FormattedDate>
              .
            </p>
            <p>Are you sure?</p>
          </ModalConfirm>

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
      )}
    </Formik>
  );
};

export default ReleaseStatusForm;
