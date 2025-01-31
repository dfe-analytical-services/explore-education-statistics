import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextArea } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';

export interface ApiDataSetGuidanceNotesFormValues {
  notes: string;
}

export interface ApiDataSetCreateFormProps {
  notes?: string;
  onSubmit: (values: ApiDataSetGuidanceNotesFormValues) => void;
}

export default function ApiDataSetGuidanceNotesForm({
  notes,
  onSubmit,
}: ApiDataSetCreateFormProps) {
  return (
    <FormProvider initialValues={{ notes }}>
      {({ formState }) => {
        return (
          <Form id="guidanceNotesForm" onSubmit={onSubmit}>
            <FormFieldTextArea
              name="notes"
              label="Public guidance notes"
              hint="Highlight any extra information that may not be apparent in the automated changelog below."
            />

            <ButtonGroup>
              <Button type="submit" ariaDisabled={formState.isSubmitting}>
                Save public guidance notes
              </Button>

              <LoadingSpinner
                alert
                hideText
                inline
                loading={formState.isSubmitting}
                size="sm"
                text="Saving notes"
              />
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}
