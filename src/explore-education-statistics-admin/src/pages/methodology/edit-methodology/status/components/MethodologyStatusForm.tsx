import {
  MethodologyVersion,
  MethodologyPublishingStrategy,
  MethodologyApprovalStatus,
  UpdateMethodology,
} from '@admin/services/methodologyService';
import { MethodologyStatusPermissions } from '@admin/services/permissionService';
import { IdTitlePair } from '@admin/services/types/common';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import { FormSelect } from '@common/components/form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

export type MethodologyStatusFormValues = Omit<UpdateMethodology, 'title'>;

interface Props {
  isPublished?: string;
  methodology: MethodologyVersion;
  statusPermissions?: MethodologyStatusPermissions;
  unpublishedReleases: IdTitlePair[];
  onCancel: () => void;
  onSubmit: (values: MethodologyStatusFormValues) => void;
}

const MethodologyStatusForm = ({
  isPublished,
  methodology,
  statusPermissions,
  unpublishedReleases,
  onCancel,
  onSubmit,
}: Props) => {
  const validationSchema = useMemo<
    ObjectSchema<MethodologyStatusFormValues>
  >(() => {
    return Yup.object({
      status: Yup.string()
        .oneOf<MethodologyApprovalStatus>([
          'Draft',
          'HigherLevelReview',
          'Approved',
        ])
        .required('Choose a status'),
      latestInternalReleaseNote: Yup.string().when('status', {
        is: 'Approved',
        then: s => s.required('Enter an internal note'),
      }),
      publishingStrategy: Yup.string()
        .oneOf<MethodologyPublishingStrategy>(['WithRelease', 'Immediately'])
        .when('status', {
          is: 'Approved',
          then: s => s.required('Choose when to publish'),
        }),
      withReleaseId: Yup.string().when('publishingStrategy', {
        is: 'WithRelease',
        then: s => s.required('Choose a release'),
      }),
    });
  }, []);

  return (
    <FormProvider
      enableReinitialize
      initialValues={{
        status: methodology.status,
        latestInternalReleaseNote: methodology.internalReleaseNote ?? '',
        publishingStrategy: methodology.publishingStrategy ?? 'Immediately',
        withReleaseId: methodology.scheduledWithRelease?.id,
      }}
      validationSchema={validationSchema}
    >
      {({ watch }) => {
        const status = watch('status');
        return (
          <Form id="methodologyStatusForm" onSubmit={onSubmit}>
            <h2>Edit methodology status</h2>

            <FormFieldRadioGroup<MethodologyStatusFormValues>
              legend="Status"
              hint={
                isPublished &&
                'Once approved, changes will be available to the public immediately.'
              }
              name="status"
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
              order={[]}
            />
            <FormFieldTextArea<MethodologyStatusFormValues>
              name="latestInternalReleaseNote"
              className="govuk-!-width-one-half"
              label="Internal note"
              hint="Please include any relevant information"
              rows={2}
            />
            {status === 'Approved' && (
              <FormFieldRadioGroup<MethodologyStatusFormValues>
                name="publishingStrategy"
                legend="When to publish"
                legendSize="m"
                hint="Do you want to publish this methodology with a specific release or immediately?"
                order={FormSelect.unordered}
                options={[
                  {
                    label: 'With a specific release',
                    value: 'WithRelease',
                    conditional: (
                      <FormFieldSelect<MethodologyStatusFormValues>
                        label="Select release"
                        name="withReleaseId"
                        order={FormSelect.unordered}
                        options={unpublishedReleases.map(release => {
                          return {
                            label: release.title,
                            value: release.id,
                          };
                        })}
                        placeholder="Choose a release"
                      />
                    ),
                  },
                  {
                    label: 'Immediately',
                    value: 'Immediately',
                  },
                ]}
              />
            )}

            <ButtonGroup>
              <Button type="submit">Update status</Button>
              <ButtonText onClick={onCancel}>Cancel</ButtonText>
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default MethodologyStatusForm;
