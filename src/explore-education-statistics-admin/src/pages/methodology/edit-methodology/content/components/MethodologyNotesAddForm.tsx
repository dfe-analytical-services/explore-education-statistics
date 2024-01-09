import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
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
      <RHFForm id="createMethodologyNoteForm" onSubmit={onSubmit}>
        <RHFFormFieldTextArea<FormValues>
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
      </RHFForm>
    </FormProvider>
  );
}
