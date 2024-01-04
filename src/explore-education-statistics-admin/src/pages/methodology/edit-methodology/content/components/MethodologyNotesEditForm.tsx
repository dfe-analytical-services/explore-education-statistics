import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import RHFFormFieldDateInput from '@common/components/form/rhf/RHFFormFieldDateInput';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  id: string;
  content: string;
  displayDate: Date;
}

interface Props {
  initialValues: FormValues;
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

export default function MethodologyNotesEditForm({
  initialValues,
  onCancel,
  onSubmit,
}: Props) {
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      id: Yup.string().required(),
      displayDate: Yup.date().required('Enter a valid edit date'),
      content: Yup.string().required('Methodology note must be provided'),
    });
  }, []);

  return (
    <FormProvider
      enableReinitialize
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      <RHFForm id="editMethodologyNoteForm" onSubmit={onSubmit}>
        <RHFFormFieldDateInput<FormValues>
          name="displayDate"
          legend="Edit date"
          legendSize="s"
        />
        <RHFFormFieldTextArea<FormValues>
          label="Edit methodology note"
          name="content"
          rows={3}
        />

        <ButtonGroup>
          <Button type="submit">Update note</Button>
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
        </ButtonGroup>
      </RHFForm>
    </FormProvider>
  );
}
