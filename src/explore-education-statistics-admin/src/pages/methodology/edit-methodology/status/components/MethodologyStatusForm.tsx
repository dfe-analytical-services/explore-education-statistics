import {
  MethodologyVersion,
  MethodologyPublishingStrategy,
  MethodologyApprovalStatus,
} from '@admin/services/methodologyService';
import { MethodologyStatusPermissions } from '@admin/services/permissionService';
import { IdTitlePair } from '@admin/services/types/common';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import { FormSelect } from '@common/components/form';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import RHFFormFieldSelect from '@common/components/form/rhf/RHFFormFieldSelect';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

export interface FormValues {
  status: MethodologyApprovalStatus;
  latestInternalReleaseNote?: string;
  publishingStrategy?: MethodologyPublishingStrategy;
  withReleaseId?: string;
}

interface Props {
  isPublished?: string;
  methodology: MethodologyVersion;
  statusPermissions?: MethodologyStatusPermissions;
  unpublishedReleases: IdTitlePair[];
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

const MethodologyStatusForm = ({
  isPublished,
  methodology,
  statusPermissions,
  unpublishedReleases,
  onCancel,
  onSubmit,
}: Props) => {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
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
          <RHFForm id="methodologyStatusForm" onSubmit={onSubmit}>
            <h2>Edit methodology status</h2>

            <RHFFormFieldRadioGroup<FormValues>
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
            <RHFFormFieldTextArea<FormValues>
              name="latestInternalReleaseNote"
              className="govuk-!-width-one-half"
              label="Internal note"
              hint="Please include any relevant information"
              rows={2}
            />
            {status === 'Approved' && (
              <RHFFormFieldRadioGroup<FormValues>
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
                      <RHFFormFieldSelect<FormValues>
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
          </RHFForm>
        );
      }}
    </FormProvider>
  );
};

export default MethodologyStatusForm;
