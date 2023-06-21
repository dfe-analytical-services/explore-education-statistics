import {
  MethodologyVersion,
  MethodologyPublishingStrategy,
  MethodologyStatus,
} from '@admin/services/methodologyService';
import { IdTitlePair } from '@admin/services/types/common';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup, FormSelect } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import Yup from '@common/validation/yup';
import useFormSubmit from '@common/hooks/useFormSubmit';
import { Formik } from 'formik';
import React from 'react';

export interface FormValues {
  status: MethodologyStatus;
  latestInternalReleaseNote: string;
  publishingStrategy?: MethodologyPublishingStrategy;
  withReleaseId?: string;
}

interface Props {
  isPublished?: string;
  methodology: MethodologyVersion;
  unpublishedReleases: IdTitlePair[];
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

const MethodologyStatusForm = ({
  isPublished,
  methodology,
  unpublishedReleases,
  onCancel,
  onSubmit,
}: Props) => {
  return (
    <Formik<FormValues>
      initialValues={{
        status: methodology.status,
        latestInternalReleaseNote: methodology.internalReleaseNote ?? '',
        publishingStrategy: methodology.publishingStrategy ?? 'Immediately',
        withReleaseId: methodology.scheduledWithRelease?.id,
      }}
      onSubmit={useFormSubmit<FormValues>(onSubmit)}
      validationSchema={Yup.object<FormValues>({
        status: Yup.mixed().required('Choose a status'),
        latestInternalReleaseNote: Yup.string().when('status', {
          is: 'Approved',
          then: s => s.required('Enter an internal note'),
        }),
        publishingStrategy: Yup.string().when('status', {
          is: 'Approved',
          then: s => s.required('Choose when to publish'),
        }),
        withReleaseId: Yup.string().when('publishingStrategy', {
          is: 'WithRelease',
          then: s => s.required('Choose a release'),
        }),
      })}
    >
      {form => {
        return (
          <Form id="methodologyStatusForm">
            <h2>Edit methodology status</h2>

            <FormFieldRadioGroup<FormValues, MethodologyStatus>
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
                },
                {
                  label: 'Approved for publication',
                  value: 'Approved',
                  conditional: (
                    <FormFieldTextArea<FormValues>
                      name="latestInternalReleaseNote"
                      className="govuk-!-width-one-half"
                      label="Internal note"
                      hint="Please include any relevant information"
                      rows={2}
                    />
                  ),
                },
              ]}
              order={[]}
            />
            {form.values.status === 'Approved' && (
              <FormFieldRadioGroup<FormValues>
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
                      <FormFieldSelect<FormValues>
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
        );
      }}
    </Formik>
  );
};

export default MethodologyStatusForm;
