import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import { Release } from '@admin/services/releaseService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import WarningMessage from '@common/components/WarningMessage';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import FormattedDate from '@common/components/FormattedDate';
import {
  isPartialDateEmpty,
  isValidPartialDate,
  parsePartialDateToLocalDate,
  PartialDate,
} from '@common/utils/date/partialDate';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import ModalConfirm from '@common/components/ModalConfirm';
import { endOfDay, format, isValid, parseISO } from 'date-fns';
import { Formik } from 'formik';
import React, { useState } from 'react';
import { StringSchema } from 'yup';

export interface ReleaseStatusFormValues {
  publishMethod?: 'Scheduled' | 'Immediate';
  publishScheduled?: Date;
  nextReleaseDate?: PartialDate;
  approvalStatus: ReleaseApprovalStatus;
  latestInternalReleaseNote: string;
}

export const formId = 'releaseStatusForm';

const errorMappings = [
  mapFieldErrors<ReleaseStatusFormValues>({
    target: 'approvalStatus',
    messages: {
      APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE:
        'Enter a publish scheduled date before approving',
      PUBLISHED_RELEASE_CANNOT_BE_UNAPPROVED:
        'Release has already been published and cannot be un-approved',
      PUBLIC_META_GUIDANCE_REQUIRED:
        'All public data guidance must be populated before the release can be approved',
      PUBLIC_PRE_RELEASE_ACCESS_LIST_REQUIRED:
        'Public pre-release access list is required before the release can be approved',
      DATA_FILE_REPLACEMENTS_MUST_BE_COMPLETED:
        'Pending data file replacements that are in progress must be completed or cancelled before the release can be approved',
      DATA_FILE_IMPORTS_MUST_BE_COMPLETED:
        'All data file imports must be completed before the release can be approved',
      METHODOLOGY_MUST_BE_APPROVED:
        "The publication's methodology must be approved before the release can be approved",
      RELEASE_NOTE_REQUIRED:
        'A public release note must be added for this amendment before it can be approved',
    },
  }),
  mapFieldErrors<ReleaseStatusFormValues>({
    target: 'nextReleaseDate',
    messages: {
      PARTIAL_DATE_NOT_VALID: 'Enter a valid date',
    },
  }),
];

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
  const [showScheduledConfirmModal, setShowScheduledConfirmModal] = useState<
    boolean
  >(false);

  const handleSubmit = useFormSubmit<ReleaseStatusFormValues>(
    async ({ approvalStatus, publishMethod, publishScheduled, ...values }) => {
      const isApproved = approvalStatus === 'Approved';

      await onSubmit({
        ...values,
        approvalStatus,
        publishMethod: isApproved ? publishMethod : undefined,
        publishScheduled:
          isApproved && publishScheduled && publishMethod === 'Scheduled'
            ? publishScheduled
            : undefined,
      });
    },
    errorMappings,
  );

  return (
    <Formik<ReleaseStatusFormValues>
      enableReinitialize
      initialValues={{
        approvalStatus: release.approvalStatus,
        latestInternalReleaseNote: release.latestInternalReleaseNote ?? '',
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
        latestInternalReleaseNote: Yup.string().when('approvalStatus', {
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
                  label: 'Ready for higher review',
                  value: 'HigherLevelReview',
                  disabled: !statusPermissions?.canMarkHigherLevelReview,
                },
                {
                  label: 'Approved for publication',
                  value: 'Approved',
                  disabled: !statusPermissions?.canMarkApproved,
                },
              ]}
            />

            <FormFieldTextArea<ReleaseStatusFormValues>
              name="latestInternalReleaseNote"
              className="govuk-!-width-one-half"
              label="Internal note"
              hint="Please include any relevant information"
              rows={3}
            />

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
                onClick={e => {
                  e.preventDefault();
                  if (
                    form.values.approvalStatus === 'Approved' &&
                    form.values.publishMethod === 'Scheduled' &&
                    form.values.publishScheduled
                  ) {
                    return setShowScheduledConfirmModal(true);
                  }
                  return form.submitForm();
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
            onConfirm={() => {
              form.submitForm();
              setShowScheduledConfirmModal(false);
            }}
            onExit={() => setShowScheduledConfirmModal(false)}
            onCancel={() => setShowScheduledConfirmModal(false)}
            open={showScheduledConfirmModal}
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
        </>
      )}
    </Formik>
  );
};

export default ReleaseStatusForm;
