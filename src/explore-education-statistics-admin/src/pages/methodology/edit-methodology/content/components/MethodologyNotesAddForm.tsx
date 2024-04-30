import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  content: string;
}

interface Props {
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

export default function MethodologyNotesAddForm({ onCancel, onSubmit }: Props) {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      content: Yup.string().required('Methodology note must be provided'),
    });
  }, []);

  return (
    <FormProvider
      enableReinitialize
      initialValues={{ content: '' }}
      validationSchema={validationSchema}
    >
      <Form id="createMethodologyNoteForm" onSubmit={onSubmit}>
        <FormFieldTextArea<FormValues>
          label="New methodology note"
          name="content"
          rows={3}
        />

        <ButtonGroup>
          <Button type="submit">Save note</Button>
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
        </ButtonGroup>
      </Form>
    </FormProvider>
  );
}
