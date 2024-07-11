import { ApiDataSetCandidate } from '@admin/services/apiDataSetCandidateService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldSelect } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

export interface ApiDataSetCreateFormValues {
  releaseFileId: string;
}

export interface ApiDataSetCreateFormProps {
  dataSetCandidates: ApiDataSetCandidate[];
  submitText?: string;
  onCancel: () => void;
  onSubmit: (values: ApiDataSetCreateFormValues) => void;
}

export default function ApiDataSetCreateForm({
  dataSetCandidates,
  submitText = 'Confirm new API data set',
  onCancel,
  onSubmit,
}: ApiDataSetCreateFormProps) {
  const validationSchema = useMemo<
    ObjectSchema<ApiDataSetCreateFormValues>
  >(() => {
    return Yup.object({
      releaseFileId: Yup.string().required('Choose a data set'),
    });
  }, []);

  return (
    <FormProvider validationSchema={validationSchema}>
      {({ formState }) => {
        return (
          <Form id="apiDataSetCreateForm" onSubmit={onSubmit}>
            <FormFieldSelect<ApiDataSetCreateFormValues>
              name="releaseFileId"
              label="Data set"
              options={dataSetCandidates.map(candidate => ({
                label: candidate.title,
                value: candidate.releaseFileId,
              }))}
              placeholder="Choose a data set"
            />

            <ButtonGroup>
              <Button type="submit" ariaDisabled={formState.isSubmitting}>
                {submitText}
              </Button>
              <ButtonText
                ariaDisabled={formState.isSubmitting}
                onClick={onCancel}
              >
                Cancel
              </ButtonText>
              <LoadingSpinner
                alert
                hideText
                inline
                loading={formState.isSubmitting}
                size="sm"
                text="Creating new API data set"
              />
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}
