import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldDateInput from '@common/components/form/rhf/RHFFormFieldDateInput';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';

export interface ReleaseNoteFormValues {
  on?: Date;
  reason: string;
}

interface Props {
  id: string;
  initialValues: ReleaseNoteFormValues;
  onCancel: () => void;
  onSubmit: (values: ReleaseNoteFormValues) => void;
}

export default function ReleaseNoteForm({
  id,
  initialValues,
  onCancel,
  onSubmit,
}: Props) {
  const isEditing = !!initialValues.on;

  const validationSchema = useMemo<ObjectSchema<ReleaseNoteFormValues>>(() => {
    const schema = Yup.object({
      on: Yup.date(),
      reason: Yup.string().required('Release note must be provided'),
    });

    if (isEditing) {
      return schema.shape({
        on: Yup.date().required('Enter a valid edit date'),
      });
    }

    return schema;
  }, [isEditing]);

  return (
    <FormProvider
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      <RHFForm id={id} onSubmit={onSubmit}>
        {isEditing && (
          <RHFFormFieldDateInput<ReleaseNoteFormValues>
            name="on"
            legend="Edit date"
            legendSize="s"
          />
        )}
        <RHFFormFieldTextArea<ReleaseNoteFormValues>
          label={isEditing ? 'Edit release note' : 'New release note'}
          name="reason"
          rows={3}
        />

        <ButtonGroup>
          <Button type="submit">
            {isEditing ? 'Update note' : 'Save note'}
          </Button>
          <Button variant="secondary" onClick={onCancel}>
            Cancel
          </Button>
        </ButtonGroup>
      </RHFForm>
    </FormProvider>
  );
}
