import useFormSubmit from '@admin/hooks/useFormSubmit';
import { ReleaseStatusPermissions } from '@admin/services/permissionService';
import { Release } from '@admin/services/releaseService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import WarningMessage from '@common/components/WarningMessage';
import { ReleaseApprovalStatus } from '@common/services/publicationService';
import {
  isPartialDateEmpty,
  isValidPartialDate,
  parsePartialDateToLocalDate,
  PartialDate,
} from '@common/utils/date/partialDate';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import { endOfDay, format, isValid, parseISO } from 'date-fns';
import { Formik } from 'formik';
import React from 'react';
import { StringSchema } from 'yup';

export interface ReleaseStatusFormValues {
  publishMethod?: 'Scheduled' | 'Immediate';
  publishScheduled?: Date;
  nextReleaseDate?: PartialDate;
  status: ReleaseApprovalStatus;
  internalReleaseNote: string;
}

const formId = 'releaseStatusForm';

const errorMappings = [
  mapFieldErrors<ReleaseStatusFormValues>({
    target: 'status',
    messages: {
      APPROVED_RELEASE_MUST_HAVE_PUBLISH_SCHEDULED_DATE:
        'Enter a publish scheduled date before approving',
      ALL_DATAFILES_UPLOADED_MUST_BE_COMPLETE:
        'Check all uploaded datafiles are complete before approving',
      PUBLISHED_RELEASE_CANNOT_BE_UNAPPROVED:
        'Release has already been published and cannot be un-approved',
      META_GUIDANCE_MUST_BE_POPULATED:
        'All public metadata guidance must be populated before release can be approved',
      DATA_REPLACEMENT_IN_PROGRESS:
        'Pending data file replacements that are in progress must be completed or cancelled before release can be approved',
      METHODOLOGY_MUST_BE_APPROVED_OR_PUBLISHED:
        "The publication's methodology must be approved before release can be approved",
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
  const handleSubmit = useFormSubmit<ReleaseStatusFormValues>(
    async ({ status, publishMethod, publishScheduled, ...values }) => {
      const isApproved = status === 'Approved';

      await onSubmit({
        ...values,
        status,
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
        status: release.status,
        internalReleaseNote: release.internalReleaseNote ?? '',
        publishMethod: release.publishScheduled ? 'Scheduled' : undefined,
        publishScheduled: release.publishScheduled
          ? parseISO(release.publishScheduled)
          : undefined,
        nextReleaseDate: release.nextReleaseDate,
      }}
      onSubmit={handleSubmit}
      validationSchema={Yup.object<ReleaseStatusFormValues>({
        status: Yup.string().required('Choose a status') as StringSchema<
          ReleaseStatusFormValues['status']
        >,
        internalReleaseNote: Yup.string().when('status', {
          is: value => ['Approved', 'HigherLevelReview'].includes(value),
          then: Yup.string().required('Enter an internal release note'),
        }),
        publishMethod: Yup.string().when('status', {
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
        <Form id={formId}>
          <h2>Edit release status</h2>

          <FormFieldRadioGroup<ReleaseStatusFormValues>
            legend="Status"
            name="status"
            id={`${formId}-status`}
            orderDirection={[]}
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
            name="internalReleaseNote"
            className="govuk-!-width-one-half"
            id={`${formId}-internalReleaseNote`}
            label="Internal release note"
            rows={3}
          />

          {form.values.status === 'Approved' && (
            <FormFieldRadioGroup<ReleaseStatusFormValues>
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
                    <FormFieldDateInput<ReleaseStatusFormValues>
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
                  conditional: (
                    <WarningMessage className="govuk-!-width-two-thirds">
                      This will start the release process immediately and make
                      statistics available to the public. Make sure this is okay
                      before continuing.
                    </WarningMessage>
                  ),
                },
              ]}
            />
          )}

          <FormFieldDateInput<ReleaseStatusFormValues>
            id={`${formId}-nextReleaseDate`}
            name="nextReleaseDate"
            legend="Next release expected (optional)"
            legendSize="m"
            type="partialDate"
            partialDateType="monthYear"
          />

          <ButtonGroup>
            <Button type="submit" disabled={form.isSubmitting}>
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
      )}
    </Formik>
  );
};

export default ReleaseStatusForm;
