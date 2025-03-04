import FormModal from '@admin/components/FormModal';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Yup from '@common/validation/yup';
import React, { ReactNode } from 'react';
import { ObjectSchema } from 'yup';

export type ReleaseLabelFormValues = {
  label?: string;
};

interface Props {
  triggerButton: ReactNode;
  initialValues?: ReleaseLabelFormValues;
  onSubmit: (releaseDetailsFormValues: ReleaseLabelFormValues) => Promise<void>;
}

const validationSchema: ObjectSchema<ReleaseLabelFormValues> = Yup.object({
  label: Yup.string().max(
    20,
    /* eslint-disable no-template-curly-in-string */
    'Release label must be no longer than ${max} characters',
  ),
});

export default function ReleaseLabelEditModal({
  triggerButton,
  initialValues,
  onSubmit,
}: Props) {
  return (
    <FormModal
      formId="editReleaseLabelForm"
      title="Edit release label"
      triggerButton={triggerButton}
      initialValues={initialValues ?? { label: undefined }}
      validationSchema={validationSchema}
      onSubmit={onSubmit}
    >
      <FormFieldTextInput<ReleaseLabelFormValues> label="Label" name="label" />
    </FormModal>
  );
}
