import {
  BasicMethodology,
  MethodologyPublishingStrategy,
  MethodologyStatus,
} from '@admin/services/methodologyService';
import { Release } from '@admin/services/releaseService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldRadioGroup } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import Yup from '@common/validation/yup';
import { Formik, FormikHelpers } from 'formik';
import React from 'react';
import { StringSchema } from 'yup';

export interface FormValues {
  status: MethodologyStatus;
  latestInternalReleaseNote: string;
  publishingStrategy?: MethodologyPublishingStrategy;
  withReleaseId?: string;
}

// EES-2163 Replace with real list when EES-2264 done.
const fakeUnpublishedReleases: Release[] = [
  {
    id: 'rel-1',
    title: 'Release 1',
  } as Release,
  {
    id: 'rel-2',
    title: 'Release 2',
  } as Release,
];

interface Props {
  isPublished?: string;
  methodologySummary: BasicMethodology;
  showWithRelease?: boolean; // EES-2163 - flag for showing publishing strategy sections for testing, remove when BE done.
  unPublishedReleases?: Release[];
  onCancel: () => void;
  onSubmit: (values: FormValues, actions: FormikHelpers<FormValues>) => void;
}

const MethodologyStatusForm = ({
  isPublished,
  methodologySummary,
  showWithRelease = false,
  unPublishedReleases = fakeUnpublishedReleases, // EES-2163 use fake for now.
  onCancel,
  onSubmit,
}: Props) => {
  return (
    <Formik<FormValues>
      initialValues={{
        status: methodologySummary.status,
        latestInternalReleaseNote:
          methodologySummary.latestInternalReleaseNote ?? '',
        publishingStrategy:
          methodologySummary.publishingStrategy ?? 'Immediately',
        withReleaseId: methodologySummary.withReleaseId,
      }}
      onSubmit={onSubmit}
      validationSchema={Yup.object<FormValues>({
        status: Yup.mixed().required('Choose a status'),
        latestInternalReleaseNote: Yup.string().when('status', {
          is: 'Approved',
          then: Yup.string().required('Enter an internal note'),
        }),
        publishingStrategy: Yup.string().when('status', {
          is: 'Approved',
          then: Yup.string().required('Choose when to publish'),
        }) as StringSchema<FormValues['publishingStrategy']>,
        withReleaseId: Yup.string().when('publishingStrategy', {
          is: 'WithRelease',
          then: Yup.string().required('Choose a release'),
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
            {form.values.status === 'Approved' && showWithRelease && (
              <FormFieldRadioGroup<FormValues>
                name="publishingStrategy"
                legend="When to publish"
                legendSize="m"
                hint="Do you want to publish this methodology with a specific release or immediately?"
                order={[]}
                options={[
                  {
                    label: 'With a specific release',
                    value: 'WithRelease',
                    conditional: (
                      <FormFieldSelect<FormValues>
                        label="Select release"
                        name="withReleaseId"
                        options={unPublishedReleases.map(release => {
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
