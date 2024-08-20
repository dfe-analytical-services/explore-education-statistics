import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextArea } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';

export interface ApiDataSetNotesFormValues {
  notes: string;
}

export interface ApiDataSetCreateFormProps {
  notes?: string;
  onSubmit: (values: ApiDataSetNotesFormValues) => void;
}

export default function ApiDataSetNotesForm({
  notes,
  onSubmit,
}: ApiDataSetCreateFormProps) {
  return (
    <FormProvider initialValues={{ notes }}>
      {({ formState }) => {
        return (
          <Form id="apiDataSetCreateForm" onSubmit={onSubmit}>
            <FormFieldTextArea
              name="notes"
              label="Public guidance notes"
              hint="Use the public guidance notes to highlight any extra information to your end users that may not be apparent in the automated changelog below."
            />

            <ButtonGroup>
              <Button type="submit" ariaDisabled={formState.isSubmitting}>
                {`${notes ? 'Update' : 'Add'} public guidance notes`}
              </Button>

              <LoadingSpinner
                alert
                hideText
                inline
                loading={formState.isSubmitting}
                size="sm"
                text="Updating notes"
              />
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}
